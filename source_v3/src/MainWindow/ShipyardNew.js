import * as fsSync from 'fs';
import fs from 'fs/promises';
import path from 'path';
import chokidar from 'chokidar';
import { EventEmitter } from 'events';
import { ipcMain } from 'electron';
import EventBus from '../EventBus.js';
import settingsHelper from '../Helpers/SettingsHelper.js';
import fileHelper from '../Helpers/FileHelper.js';
import KeySender from '../Helpers/KeySender.js';
import FrontierApi from '../Helpers/FrontierApi.js';
import {
    mergeFrontierShips,
    normalizeFrontierShips,
    upsertJournalShip,
} from '../Helpers/FrontierFleet.mjs';
import { getDefaultPlayerJournal } from '../Helpers/PlatformDefaults.mjs';

/*         
* The Player Journal is a log file that contains information about the player's actions and events in the game.
* Each line of the Player Journal is a full JSON object, so we can parse it and get the data we need.
* https://elite-journal.readthedocs.io/en/latest/
*/

class Shipyard extends EventEmitter {
    constructor(mainWindow) {
        super();

        this.LOG_DIRECTORY = ''; //- Linux Steam (Proton):  '~/.local/share/Steam/steamapps/compatdata/359320/pfx/drive_c/users/steamuser/Saved Games/Frontier Developments/Elite Dangerous'
        this.DATA_DIRECTORY = '';

        this.mainWindow = mainWindow;
        this.currentLogFile = null;
        this.dirWatcher = null;
        this.fileWatcher = null;
        this.isMonitoring = false;

        this.shipyardData = null;
        this.shipListData = null;
        this.PreviousShip = null;
        this.frontierApi = new FrontierApi();

        this.linesProcessed = 0;
        this.currentlyMonitoredFile = null;

        this.setupIPC();
        this.initialize();
    }

    async initialize() {
        try {
            this.LOG_DIRECTORY = fileHelper.resolveEnvVariables(
                settingsHelper.readSetting(
                    'PlayerJournal',
                    getDefaultPlayerJournal()
                )
            ); console.log('Shipyard Journal Dir: ', this.LOG_DIRECTORY);

            this.DATA_DIRECTORY = fileHelper.resolveEnvVariables(
                settingsHelper.readSetting('UserDataFolder', '%USERPROFILE%\\EDHM_UI')
            ); console.log('Shipyard Data Dir: ', this.DATA_DIRECTORY);
            fileHelper.ensureDirectoryExists(this.DATA_DIRECTORY);

            this.ShipyardFilePath = path.join(this.DATA_DIRECTORY, 'Shipyard_v3.json');
            this.ShipListFilePath = fileHelper.getAssetPath('data/Ship_List.json');
            
            if (fileHelper.checkFileExists(this.ShipyardFilePath)) {
                //- File exists, read it:
                this.shipyardData = await fileHelper.loadJsonFile(this.ShipyardFilePath);
                console.log('Shipyard loaded from file:', this.ShipyardFilePath);
            } else {
                //- Shipyard file doesnt exists
                this.shipyardData = {
                    enabled: false,
                    player_name: '',
                    ships: []
                };
                // Save the initial Shipyard data to the JSON file
                fileHelper.writeJsonFile(this.ShipyardFilePath, this.shipyardData, true);
                console.log('Shipyard file created:', this.ShipyardFilePath);
            }

            this.shipListData = await fileHelper.loadJsonFile(this.ShipListFilePath);
            EventBus.emit('shipyard:configLoaded', {
                shipyard: this.shipyardData,
                shipList: this.shipListData,
            });
        } catch (err) {
            console.error(err);
            this.emit('error', { type: 'config', error: err });
            EventBus.emit('shipyard:error', { type: 'config', error: err });
        }
    }

    SaveShipyard() {
        try {
            fileHelper.writeJsonFile(this.ShipyardFilePath, this.shipyardData, true);
            console.log('Ship data saved.');
            return true;
        } catch (error) {
            console.error('Error saving ship data:', error);
            return false;
        }
    }

    getFrontierShips(profile) {
        return normalizeFrontierShips(profile, this.shipListData);
    }

    async mergeFrontierFleet(profile, galaxy = 'live') {
        if (!profile || typeof profile !== 'object') {
            throw new Error('Frontier returned an invalid Commander profile.');
        }

        const existingShips = Array.isArray(this.shipyardData?.ships) ? this.shipyardData.ships : [];
        // Frontier is authoritative for records imported by Frontier. The
        // helper preserves themes/images and retains unmatched Journal/V2 data.
        const frontierShips = this.getFrontierShips(profile);
        if (frontierShips.length === 0) {
            throw new Error(
                'Frontier returned a Commander profile without any ships. Existing Shipyard data was left unchanged.'
            );
        }
        const mergedShips = mergeFrontierShips(frontierShips, existingShips);
        const commanderName = String(profile.commander?.name || this.shipyardData?.player_name || '').trim();
        this.shipyardData = {
            ...this.shipyardData,
            player_name: commanderName,
            ships: mergedShips,
            frontier: {
                last_sync_at: new Date().toISOString(),
                ship_count: frontierShips.length,
                galaxy,
            },
        };

        if (!this.SaveShipyard()) {
            throw new Error('The Frontier fleet was received but could not be saved to Shipyard_v3.json.');
        }
        await this.frontierApi.recordFleetSync(commanderName, frontierShips.length);

        const payload = {
            shipyard: this.shipyardData,
            shipList: this.shipListData,
            frontierStatus: await this.frontierApi.getStatus(),
        };
        this.mainWindow.webContents.send('shipyard:fleetUpdated', payload);
        return payload;
    }

    async syncFrontierFleet(authorize = false) {
        if (!this.shipListData || !this.shipyardData) await this.initialize();
        if (authorize) await this.frontierApi.authorize();
        const activeInstance = settingsHelper.getActiveInstance();
        const galaxy = activeInstance?.key === 'ED_Horizons' ? 'legacy' : 'live';
        const profile = await this.frontierApi.fetchProfile(galaxy);
        return this.mergeFrontierFleet(profile, galaxy);
    }

    handleFrontierAuthCallback(callbackUrl) {
        return this.frontierApi.handleAuthorizationCallback(callbackUrl);
    }

    async startMonitoring() {
        const DirExists = this.LOG_DIRECTORY && fileHelper.checkFileExists(this.LOG_DIRECTORY);
        if (!this.LOG_DIRECTORY || !DirExists) {
            const error = { type: 'monitoring', error: '404 - JOURNAL_DIRECTORY not found.' };
            console.error(error);
            this.emit('error', error);
            EventBus.emit('shipyard:error', error);
            return;
        }
        if (this.shipyardData.enabled) {
            console.log('Shipyard is Enabled.');
            console.log('Starting Journal monitoring...');

            this.isMonitoring = true;
            this.emit('monitoring:started', this.LOG_DIRECTORY);
            EventBus.emit('shipyard:monitoringStarted', this.LOG_DIRECTORY);

            await this.checkAndSwitchLogFile();

            this.dirWatcher = chokidar.watch(this.LOG_DIRECTORY, {
                ignoreInitial: true,
                depth: 0,
                awaitWriteFinish: true,
            });

            this.dirWatcher.on('add', () => this.checkAndSwitchLogFile());
            this.dirWatcher.on('unlink', () => this.checkAndSwitchLogFile());
        }
        else {
            console.log('Shipyard is Disabled.');
        }
    }

    stopMonitoring() {
        this.isMonitoring = false;
        if (this.dirWatcher) this.dirWatcher.close();
        if (this.fileWatcher) this.fileWatcher.close();
        this.emit('monitoring:stopped');
        EventBus.emit('shipyard:monitoringStopped');
    }

    async checkAndSwitchLogFile() {
        const latest = await this.getLatestLogFile();
        if (!latest || latest === this.currentlyMonitoredFile) return;

        this.currentLogFile = latest;
        this.emit('log:fileChanged', latest);
        EventBus.emit('shipyard:logFileChanged', latest);

        EventBus.emit('shipyard:logLoaded', {
            file: latest,
            timestamp: Date.now()
        });

        await this.startMonitoringFile(latest);
    }

    /** Returns the latest log file in the journal directory. */
    async getLatestLogFile() {
        try {
            const files = await fs.readdir(this.LOG_DIRECTORY);
            const logFiles = files.filter(file => file.endsWith('.log') || file.endsWith('.txt'));

            if (logFiles.length === 0) return null;

            const filesWithStats = await Promise.all(
                logFiles.map(async (file) => {
                    const filePath = path.join(this.LOG_DIRECTORY, file);
                    const stats = await fs.stat(filePath);
                    return { path: filePath, created: stats.birthtimeMs };
                })
            );

            filesWithStats.sort((a, b) => b.created - a.created);
            return filesWithStats[0].path;
        } catch (err) {
            this.emit('error', { type: 'latestLog', error: err });
            EventBus.emit('shipyard:error', { type: 'latestLog', error: err });
            return null;
        }
    }

    async startMonitoringFile(filePath) {
        if (filePath === this.currentlyMonitoredFile) return;

        this.currentlyMonitoredFile = filePath;
        this.linesProcessed = 0;

        if (this.fileWatcher) this.fileWatcher.close();

        const initialParsed = await this.analyzeLogFile(filePath, 0);
        this.linesProcessed = initialParsed.length;

        this.fileWatcher = chokidar.watch(filePath, {
            persistent: true,
            ignoreInitial: true,
        }).on('change', async () => {
            try {
                const content = await fs.readFile(filePath, 'utf-8');
                const currentLineCount = content.trim().split('\n').length;

                if (currentLineCount > this.linesProcessed) {
                    const newParsed = await this.analyzeLogFile(filePath, this.linesProcessed);
                    this.linesProcessed = currentLineCount;
                } else if (currentLineCount < this.linesProcessed) {
                    const allParsed = await this.analyzeLogFile(filePath, 0);
                    this.linesProcessed = allParsed.length;
                }
            } catch (error) {
                this.emit('error', { type: 'fileChange', error });
            }
        });
    }

    async analyzeLogFile(filePath, startLine = 0) {
        try {
            const content = await fs.readFile(filePath, 'utf-8');
            const lines = content.trim().split('\n');
            const newLines = lines.slice(startLine);

            const parsedLines = newLines.map(line => {
                try {
                    return this.OnShipyardEvent(line, startLine > 0);
                } catch (error) {
                    console.warn('Error parsing JSON line:', line, error);
                    return null;
                }
            }).filter(line => line !== null);

            EventBus.emit('shipyard:logAnalyzed', {
                file: filePath,
                newLines: parsedLines.length,
                totalLines: lines.length,
                timestamp: Date.now()
            });

            return parsedLines;
        } catch (error) {
            this.emit('error', { type: 'analyze', error });
            return [];
        }
    }

    /** Process a Journal Event catching only those we are interested in.
     * @param {*} line - The line from the Journal file to process. */
    OnShipyardEvent(line, isNewLine = false) {
        const _json = JSON.parse(line);
        try {
            let _data = {};
            switch (_json.event) {
                case 'Commander':
                    _data = {
                        event: 'PlayerName',
                        data: { player_name: _json.Name }
                    };
                    this.shipyardData.player_name = _json.Name;
                    this.SaveShipyard();
                    this.emit('log:entry', _data);
                    EventBus.emit('shipyard:logEntry', _data);
                    break;

                case 'Loadout':
                    _data = {
                        event: 'ShipLoadout',
                        data: this.GetShip({
                            frontier_id: _json.ShipID,
                            kind_short: _json.Ship,
                            name: _json.ShipName?.trim(),
                            plate: _json.ShipIdent
                        })
                    };
                    this.PlayerJournal_ShipChanged(_data, isNewLine);
                    break;

                case 'LaunchSRV':
                    _data = {
                        event: 'LaunchSRV',
                        data: this.GetShip({
                            kind_short: _json.SRVType,
                            name: _json.SRVType_Localised,
                            plate: "SRV" + (_json.ID ? `_${_json.ID}` : '')
                        })
                    };
                    this.PlayerJournal_ShipChanged(_data, isNewLine);
                    break;

                case 'DockSRV':
                    _data = {
                        event: 'DockSRV',
                        data: this.GetShip({
                            kind_short: _json.SRVType,
                            name: _json.SRVType_Localised,
                            plate: "SRV" + (_json.ID ? `_${_json.ID}` : '')
                        })
                    };
                    if (this.PreviousShip) {
                        this.PlayerJournal_ShipChanged(this.PreviousShip, isNewLine);
                    }
                    break;

                case 'Embark':
                    _data = {
                        event: 'Embark',
                        data: this.GetShip({
                            kind_short: _json.SRVType,
                            name: _json.SRVType_Localised,
                            plate: "SRV" + (_json.ID ? `_${_json.ID}` : '')
                        })
                    };
                    if (this.PreviousShip) {
                        this.PlayerJournal_ShipChanged(this.PreviousShip, isNewLine);
                    }
                    break;

                default:
                    break;
            }
            this.mainWindow.webContents.send('shipyard:logEntry', _data);
        } catch (error) {
            console.error('Error in OnShipyardEvent:', error);
        }
        return _json;
    }

    /*** Player Journal Ship Changed Event
     * This function is called when the ship changes in the game.
     * @param {*} event Data of the new ship
     * @param {boolean} _ApplyTheme 'true' to apply the theme to the ship, 'false' otherwise. */
    async PlayerJournal_ShipChanged(event, _ApplyTheme = true) {
        /* OCURRE CUANDO SE CAMBIA LA NAVE 
        - Guarda la Nave en el Historial de Naves
        - Si el Juego está abierto, Aplica el Tema seleccionado para la nave
        TODO:   - Registrar el ID de la nave para el CPM
    */
        try {
            if (event) {
                event.data = this.AddShip(event.data);
                this.PreviousShip = event;

                if (_ApplyTheme && event.data.theme !== 'Current Settings') {
                    console.log('Applying Theme:', event.data.theme);
                    const tApply = await settingsHelper.ApplyTheme(event.data.theme);
                    /*if (tApply) {
                        setTimeout(() => {
                            // Simulate pressing the "F11" key to refresh colors in the game
                            KeySender.SendKey({
                                targetProcess: 'EliteDangerous64', //<- Exe name
                                targetWindow: 'Elite - Dangerous', //<- Window title
                                keyBindings: ['F11','F11']    //<- Keys to send
                            });
                            //console.log('F11 key sent to game!');
                        }, 2000); // Sends after 2 seconds
                    }*/
                }
            }
        } catch (error) {
            console.log(error);
        }
    }

    /** Get the ship data from the ShipList based on the given parameters. 
     * @param {*} shipData - Parameters containing ship information.
     * @param {string|number} shipData.frontier_id - Stable owned-ship ID shared by the journal and CAPI.
     * @param {string} shipData.kind_short - [Required] The ship kind (e.g., "python", "cutter").
     * @param {string} shipData.name -  The ship name (e.g., "NORMANDY").
     * @param {string} shipData.plate - The ship plate (e.g., "SR-03"). */
    GetShip(shipData) {
        try {
            if (this.shipListData && shipData && shipData.kind_short) {
                const sKindShort = shipData.kind_short.toLowerCase();
                const ship = this.shipListData.find(ship => ship.ed_short.toLowerCase() === sKindShort);
                if (ship) {
                    return {
                        frontier_id: shipData.frontier_id,
                        ship_id: ship.ship_id,
                        kind_short: ship.ed_short.toLowerCase(),
                        kind_full: ship.ship_full_name,
                        name: shipData.name,
                        plate: shipData.plate,
                        theme: "Current Settings",
                        image: `${ship.ed_short.toLowerCase()}.jpg`,
                    };
                } else {
                    console.warn(`Ship not found in ShipList: ${shipData.kind_short}`);
                    return null;
                }
            }
            return null;
        } catch (error) {
            console.log(error);
        }
    }

    AddShip(shipData) {
        if (typeof shipData !== 'object' || shipData === null) {
            console.error('AddShip: shipData is not a valid object:', shipData);
            return shipData;
        }

        const result = upsertJournalShip(shipData, this.shipyardData.ships);
        this.shipyardData.ships = result.ships;

        if (result.changed) {
            this.SaveShipyard();

            this.mainWindow.webContents.send('shipyard-ShipAdded', {
                shipyard: this.shipyardData,
                shipList: this.shipListData,
            });

            console.log(
                `${result.added ? 'New ship added' : 'Ship identity updated'}: ` +
                `${result.ship.kind_short}, file saved.`
            );
        }
        return result.ship;
    }

    setupIPC() {
        ipcMain.handle('shipyard:start', async () => {
            await this.initialize();
            await this.startMonitoring();
            return this.getStatus();
        });

        ipcMain.handle('shipyard:stop', () => {
            this.stopMonitoring();
            return this.getStatus();
        });
        ipcMain.handle('shipyard:restart', async () => {
            this.stopMonitoring();
            await this.startMonitoring();
            return this.getStatus();
        });

        ipcMain.handle('shipyard:getStatus', () => this.getStatus());
        ipcMain.handle('shipyard:getConfig', () => {
            return {
                shipyard: this.shipyardData,
                shipList: this.shipListData
            };
        });

        ipcMain.handle('shipyard:reloadConfig', async () => {
            await this.initialize();
            return {
                shipyard: this.shipyardData,
                shipList: this.shipListData
            };
        });

        ipcMain.handle('shipyard:updateConfig', async (_, newConfig) => {
            try {
                if (typeof newConfig !== 'object' || newConfig === null) {
                    throw new Error('Invalid configuration payload');
                }

                this.shipyardData = { ...this.shipyardData, ...newConfig };
                const success = this.SaveShipyard();

                if (success) {
                    EventBus.emit('shipyard:configUpdated', this.shipyardData);
                    return { success: true, updated: this.shipyardData };
                } else {
                    return { success: false, error: 'Failed to save configuration' };
                }

            } catch (err) {
                console.error('Error updating config:', err);
                return { success: false, error: err.message };
            }
        });

        ipcMain.handle('shipyard:frontierStatus', () => this.frontierApi.getStatus());
        ipcMain.handle('shipyard:frontierTokenPath', () => this.frontierApi.tokenFilePath);
        ipcMain.handle('shipyard:frontierLogin', () => this.syncFrontierFleet(true));
        ipcMain.handle('shipyard:frontierCancelLogin', () => this.frontierApi.cancelAuthorization());
        ipcMain.handle('shipyard:frontierRefresh', () => this.syncFrontierFleet(false));
        ipcMain.handle('shipyard:frontierLogout', async () => {
            await this.frontierApi.clearTokens();
            return this.frontierApi.getStatus();
        });
    }

    getStatus() {
        return {
            monitoring: this.isMonitoring,
            currentLogFile: this.currentLogFile,
            logDirectory: this.LOG_DIRECTORY,
            shipyardFile: this.ShipyardFilePath,
            shipListFile: this.ShipListFilePath,
        };
    }
}

export default Shipyard;
