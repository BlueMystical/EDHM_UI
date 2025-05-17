import { ipcMain } from 'electron'; 
import fs from 'node:fs/promises'; // For asynchronous operations (like in getLatestLogFile)
import fsSync from 'node:fs';      // For synchronous operations in Initialize
import path from 'node:path';
import chokidar from 'chokidar';
import { execFile } from 'child_process';
import settingsHelper from '../Helpers/SettingsHelper.js';
import fileHelper from '../Helpers/FileHelper.js';
import EventBus from '../EventBus.js';

// #region Declarations

// Configuration Files:
const LOG_DIRECTORY = fileHelper.resolveEnvVariables(
    settingsHelper.readSetting('PlayerJournal',
        '%USERPROFILE%\\Saved Games\\Frontier Developments\\Elite Dangerous')
);
const DATA_DIRECTORY = fileHelper.resolveEnvVariables(
    settingsHelper.readSetting('UserDataFolder', '%USERPROFILE%\\EDHM_UI')
);
const ShipyardFilePath = path.join(DATA_DIRECTORY, 'Shipyard_v3.json');

let ShipList = null;
let Shipyard = null;
let currentlyMonitoredFile = null;
let fileWatcher = null;
let directoryWatcher = null;
let mainWindowInstance = null; // To hold the mainWindow instance
let PreviousShip = null;
let AutoApplyTheme = true;
let linesProcessed = 0;

// #endregion

/*         
* The Player Journal is a log file that contains information about the player's actions and events in the game.
* Each line of the Player Journal is a full JSON object, so we can parse it and get the data we need.
* https://elite-journal.readthedocs.io/en/latest/
*/  

/** Shipyard Initialization 
 * @param {*} mainWindow Reference to the Electron's Main Window, for comunication with the renderer process */
export const Initialize = (mainWindow) => {
    try {
        mainWindowInstance = mainWindow;

        // Load the Ships List:
        ShipList = JSON.parse(
            fsSync.readFileSync(
                fileHelper.getAssetPath('data/Ship_List.json'), 'utf8')
        );

        // Load the Shipyard data from the JSON file, creates it if it doesn't exist:
        fileHelper.ensureDirectoryExists(DATA_DIRECTORY);
        if (fileHelper.checkFileExists(DATA_DIRECTORY)) {
            if (fileHelper.checkFileExists(ShipyardFilePath)) {
                //- File exists, read it:
                Shipyard = JSON.parse(
                    fsSync.readFileSync(ShipyardFilePath, 'utf8')
                );
            } else { 
                //- Shipyard file doesnt exists
                Shipyard = {
                    enabled: false,
                    player_name: '',
                    ships: []
                };
                // Save the initial Shipyard data to the JSON file
                fileHelper.writeJsonFile(ShipyardFilePath, Shipyard, true);
            }
        }
    } catch (error) {
        console.log(error);
    }
    console.log('Shipyard Initialized, enabled:', Shipyard.enabled);
    return Shipyard.enabled;
};



/*** Player Journal Ship Changed Event
 * This function is called when the ship changes in the game.
 * @param {*} event Data of the new ship
 * @param {boolean} _ApplyTheme 'true' to apply the theme to the ship, 'false' otherwise. */
async function PlayerJournal_ShipChanged(event, _ApplyTheme = true) {
    /* OCURRE CUANDO SE CAMBIA LA NAVE 
        - Guarda la Nave en el Historial de Naves
        - Si el Juego está abierto, Aplica el Tema seleccionado para la nave 
TODO:   - Registrar el ID de la nave para el CPM 
    */
    try {
        if (event) {    
            console.log('Event:', event);        
            event.data = AddShip(event.data); 
            console.log('--------------------------------------');

            if (_ApplyTheme) {  //&& event.data.theme !== 'Current Settings'
                console.log('Applying Theme:', event.data.theme); 
                const tApply = await settingsHelper.ApplyTheme(event.data.theme);
                if (tApply) {
                    const _ret = RunSendKeyScript(); //<- Send F11 key to the game
                    console.log('F11 key sent to game:', _ret);
                }
                console.log('--------------------------------------');
            }
        }
    } catch (error) {
        console.log(error);
    }
}

/** Process a Journal Event catching only those we are interested in.
 * @param {*} line - The line from the Journal file to process. */
function OnShipyardEvent(line, isNewLine = false) {
    const _json = JSON.parse(line);
    try {
     
        // #region Player Journal Events examples
        /*
        "event":"Commander", "Name":"Blue mystic"
        "event":"LoadGame", "Commander":"Blue mystic", "Horizons":true, "Odyssey":true, "Ship":"Python", "ShipID":6, "ShipName":"NORMANDY", "ShipIdent":"SR-03", "GameMode":"Solo", "gameversion":"4.0.0.700"
        "event":"Loadout", "Ship":"cutter", "ShipName":"NORMANDY",  "ShipIdent":"SR-04"
        "event":"Rank", "Combat":3, "Trade":6, "Explore":5, "Soldier":0, "Exobiologist":0, "Empire":12, "Federation":3, "CQC":0 }
        "event":"Shutdown" ..

        "event": "interdicted", "Submitted": false, "Interdictor": "Dread Pirate Roberts", "IsPlayer": false, "Faction": "Timocani Purple Posse"
        "event":"ShipTargeted", "TargetLocked":true, "Ship":"anaconda", "ScanStage":3, "PilotName":"$cmdr_decorate:#name=ChloeDB;", "PilotName_Localised":"CMDR ChloeDB", "PilotRank":"Deadly", "SquadronID":"NERC", "ShieldHealth":100.000000, "HullHealth":100.000000, "LegalStatus":"Clean", "Power":"Aisling Duval" }
        "event":"ShipTargeted", "TargetLocked":false }
        "event":"UnderAttack" ...

        "event":"FSDTarget", "Name":"Pleiades Sector GW-W c1-13", "StarClass":"K", "RemainingJumpsInRoute":1 }
        "event":"StartJump", "JumpType":"Hyperspace", "StarSystem":"Pleiades Sector GW-W c1-13", "SystemAddress":3657130971786, "StarClass":"K" }
        "event":"FSDJump", "StarSystem":"Pleiades Sector GW-W c1-13", "SystemAllegiance":"Independent", "SystemEconomy_Localised":"Military", "SystemSecondEconomy_Localised":"None", 
                        "SystemGovernment_Localised":"Cooperative", "SystemSecurity_Localised":"Medium Security", "Population":1200000, 
                        "Body":"Pleiades Sector GW-W c1-13 A", "BodyID":1, "BodyType":"Star", "JumpDist":14.956, "FuelUsed":0.154522, "FuelLevel":15.845478, 
                        "Factions":[ { "Name":"Anti Xeno Initiative", "FactionState":"None", "Government":"Patronage", "Influence":0.075000, "Allegiance":"Independent", 
                                                "Happiness_Localised":"Happy", "MyReputation":100.000000 }, 
                                        { "Name":"The Hive", "FactionState":"Boom", "Government":"Cooperative", "Influence":0.645000, "Allegiance":"Independent",  
                                                "Happiness_Localised":"Happy", "MyReputation":96.040001, 
                                                "RecoveringStates":[ { "State":"PublicHoliday", "Trend":0 } ], "ActiveStates":[ { "State":"Boom" } ] }
    
        "event":"LaunchFighter", "Loadout":"one", "ID":163, "PlayerControlled":true }	//<-  when launching a fighter
        "event":"Dockfighter

        "event":"LaunchSRV", "SRVType":"testbuggy", "SRVType_Localised":"SRV Scarab", "Loadout":"starter", "ID":220, "PlayerControlled":true } //<- deploying the SRV from a ship onto planet surface
        "event":"LaunchSRV", "SRVType":"combat_multicrew_srv_01", "SRVType_Localised":"SRV Scorpion", "Loadout":"default", "ID":204, "PlayerControlled":true }

        "event":"DockSRV", "SRVType":"testbuggy", "SRVType_Localised":"SRV Scarab", "ID":220 }		//<- when docking an SRV with the ship
        "event":"Embark", "SRV": false, "Taxi": false, "Multicrew": false, "ID": 36					//<- when a player (on foot) gets into a ship or SRV
        
        */

        // #endregion

        //- Here we will analyse the Events from the Player Journal catching only those we need
        if (mainWindowInstance && mainWindowInstance.webContents) {
            let _data = {};
            switch (_json.event) {
                //1. Get Player's Name:
                case 'Commander':
                    _data = { 
                        event: 'PlayerName', 
                        data: {
                            player_name: _json.Name 
                        }
                    };  
                    Shipyard.player_name = _json.Name; //<- Save Player Name to the Shipyard      
                    SaveShipyard(); // Save the updated Shipyard data              
                    mainWindowInstance.webContents.send('OnShipyardEvent', _data ); //<- Send data to renderer
                    break;

                //- 2. When loading from main menu, or switching ships, or after changing the ship in Outfitting, or when docking SRV back in mothership	:
                case 'Loadout':
                    	/*	"timestamp": "2021-08-06T01:56:55Z", "event": "Loadout", 
                        "Ship": "cutter", "ShipID": 7, "ShipName": "NORMANDY", "ShipIdent": "SR-04", "HullValue": 180435872,
                        "ModulesValue": 128249820, "HullHealth": 1.0, "UnladenMass": 1803.399902, "CargoCapacity": 320, "MaxJumpRange": 31.563942, .... */
                    _data = {
                        event: 'ShipLoadout', 
                        data: GetShip({
                            kind_short: _json.Ship,
                            name: _json.ShipName?.trim(),
                            plate: _json.ShipIdent
                        })
                    };
                    //mainWindowInstance.webContents.send('OnShipyardEvent',  _data );
                    PlayerJournal_ShipChanged(_data, isNewLine) //<- Add ship to the Shipyard if not there, apply its Theme, send F11 key to game                    
                    break;

                //- 3. When deploying the SRV from a ship onto planet surface
                case 'LaunchSRV':
                    /* "timestamp":"2023-07-30T18:28:01Z", "event":"LaunchSRV", 
					  "SRVType":"combat_multicrew_srv_01", 
					  "SRVType_Localised":"SRV Scorpion", 
					  "Loadout":"default", "ID":10, "PlayerControlled":true } */
                    _data = {
                        event: 'LaunchSRV', 
                        data: GetShip({
                            kind_short: _json.SRVType,
                            name: _json.SRVType_Localised,
                            plate: "SRV" + (_json.ID ? `_${_json.ID}` : '')
                        })
                    };
                    //mainWindowInstance.webContents.send('OnShipyardEvent', _data);
                    PlayerJournal_ShipChanged(_data, isNewLine);

                //- 4. when docking an SRV with the ship
                case 'DockSRV':
                    /* { "timestamp":"2023-07-30T18:48:56Z", "event":"DockSRV", 
					 	"SRVType":"combat_multicrew_srv_01", "SRVType_Localised":"SRV Scorpion", "ID":10 } */
                    _data = {
                        event: 'DockSRV', 
                        data: GetShip({
                            kind_short: _json.SRVType,
                            name: _json.SRVType_Localised,
                            plate: "SRV" + (_json.ID ? `_${_json.ID}` : '')
                        })
                    };
                    if (PreviousShip) {
                        PlayerJournal_ShipChanged(PreviousShip, isNewLine);
                    }
                    //We dont use this event at the moment.
                    break;

                //- when a player (on foot) gets into a ship or SRV
                case 'Embark':
                    /*	{	"timestamp":"2023-11-27T08:23:44Z","event":"Embark",
                                    "SRV":false,"Taxi":false, "Multicrew":false, "ID":72, 
                                    "StarSystem":"Bleae Thua WH-G b38-5",
                                    "SystemAddress":11653454440777,
                                    "Body":"Bleae Thua WH-G b38-5 A", "BodyID":2,
                                    "OnStation":false,
                                    "OnPlanet":false
                               } */
                    _data = {
                        event: 'Embark',
                        data: GetShip({
                            kind_short: _json.SRVType,
                            name: _json.SRVType_Localised,
                            plate: "SRV" + (_json.ID ? `_${_json.ID}` : '')
                        })
                    };
                    // Since the Embark doesnt tell the ship, we use the Previously embarked ship
                    // *: it may be a problem if the player logs out while on foot and tries to embark, we wont know what ship they had
                    if (PreviousShip) {
                        PlayerJournal_ShipChanged(PreviousShip, isNewLine);
                    }
                    break;
                case '':
                    break;
                default:
                    break;
            }
        }
    } catch (error) {
        console.error('Error in OnShipyard_Event:', error);
    }
    return _json;
}

/** Get the ship data from the ShipList based on the given parameters. 
 * @param {*} shipData - Parameters containing ship information.
 * @param {string} shipData.kind_short - [Required] The ship kind (e.g., "python", "cutter").
 * @param {string} shipData.name -  The ship name (e.g., "NORMANDY").
 * @param {string} shipData.plate - The ship plate (e.g., "SR-03"). */
function GetShip(shipData) {
    try {
        //console.log('GetShip', shipData);
        if (ShipList && shipData && shipData.kind_short) {
            const sKindShort = shipData.kind_short.toLowerCase();
            const ship = ShipList.find(ship => ship.ed_short.toLowerCase() === sKindShort);
            if (ship) {
                return {
                    ship_id:    ship.ship_id,
                    kind_short: ship.ed_short.toLowerCase(),
                    kind_full:  ship.ship_full_name,
                    name:   shipData.name,
                    plate:  shipData.plate,         
                    theme:  "Current Settings",
                    image:  `${ship.ed_short.toLowerCase()}.jpg`,
                    //image:  fileHelper.getAssetUrl(`images/Ships/${ship.ed_short}.jpg`),
                 };
            } else {
                console.warn(`Ship not found in ShipList: ${shipData.kind_short}`);
                return null;
            }
        }
        return null; // Return null if ShipList is not loaded or shipData is invalid
    } catch (error) {
        console.log(error);
    }
}

function AddShip(shipData) {
    //console.log('AddShip', shipData);

    // Check if shipData is defined and is an object
    if (typeof shipData !== 'object' || shipData === null) {
        console.error('AddShip: shipData is not a valid object:', shipData);
        return shipData; // Indicate failure: invalid input
    }

    // Use optional chaining and nullish coalescing to safely access properties
    const kind_short = shipData.kind_short?.toLowerCase().trim() ?? '';
    const name = shipData.name;
    const plate = shipData.plate;
    let exShip = null;

    // Check for pre-existence based on a combination of identifying properties
    const shipExists = Shipyard.ships.some(existingShip => {
        // Safely access properties of existingShip as well
        const existingKindShort = existingShip.kind_short?.toLowerCase().trim() ?? '';
        const existingName = existingShip.name;
        const existingPlate = existingShip.plate;
        exShip = existingShip;
        return existingKindShort === kind_short && existingName === name && existingPlate === plate;
    });

    if (shipExists) {
        console.log(`Ship not added: A ship with kind_short '${kind_short}', name '${name}', and plate '${plate}' already exists.`);
        return exShip;
    } else {
        // Create a *copy* of the shipData object before pushing it
        const newShip = { ...shipData };
        Shipyard.ships.push(newShip);
        SaveShipyard(); // Save the updated Shipyard data
        console.log(`Ship added to Shipyard: ${newShip.kind_short}, file Saved.`);

        return shipData;
    }    
}

function SaveShipyard() {
    try {
        fileHelper.writeJsonFile(ShipyardFilePath, Shipyard, true);
        console.log('Ship data saved.');
        return true; // Indicate success
    } catch (error) {
        console.error('Error saving ship data:', error);
        Shipyard.ships.pop(); // Remove the added ship on error.
        return false; // Indicate failure
    }
}

/** Returns the latest log file in the journal directory. */
async function getLatestLogFile() {
    try {
        console.log('Getting latest log file from:', LOG_DIRECTORY);
        const files = await fs.readdir(LOG_DIRECTORY); // Use await with the promise-based readdir
        const logFiles = files.filter(file => file.endsWith('.log') || file.endsWith('.txt')); // Adjust filter as needed

        if (logFiles.length === 0) {
            return null;
        }

        const filesWithStats = await Promise.all(
            logFiles.map(async (file) => {
                const filePath = path.join(LOG_DIRECTORY, file);
                const stats = await fs.stat(filePath);
                return { path: filePath, created: stats.birthtimeMs };
            })
        );

        filesWithStats.sort((a, b) => b.created - a.created); // Sort by creation time (newest first)
        return filesWithStats[0].path;
    } catch (error) {
        console.error('Error getting latest log file:', error);
        return null;
    }
}

/** Reads the journal file and parses each line as JSON object.
 * @param {*} filePath full path to the Journal file 
 * @returns an array of parsed lines */
async function analyzeLogFile(filePath, startLine = 0) {
    try {
        const content = await fs.readFile(filePath, 'utf-8');
        const lines = content.trim().split('\n');
        const newLines = lines.slice(startLine); // Only process new lines

        const parsedLines = newLines.map(line => {
            try {
                return OnShipyardEvent(line, startLine > 0); // Call the event handler to process the line
            } catch (error) {
                console.warn('Error parsing JSON line:', line, error);
                return null;
            }
        }).filter(line => line !== null);

        if (parsedLines.length > 0) {
            console.log(`Analyzed ${parsedLines.length} new lines from:`, filePath);
            if (mainWindowInstance && mainWindowInstance.webContents) {
                mainWindowInstance.webContents.send('log-analysis-update', parsedLines); // Send data to renderer
            } else {
                console.warn('mainWindowInstance not available to send log analysis update.');
            }
        } else if (startLine > 0) {
            console.log('No new lines to analyze.');
        } else {
            console.log(`Initial analysis of ${lines.length} lines from:`, filePath);
        }

        return parsedLines;
    } catch (error) {
        console.error('Error analyzing log file:', error);
        return []; // Return an empty array in case of error
    }
}

/** Watch for changes in the log file and re-analyze it. 
 * @param {*} filePath full path to the Journal file */
async function startMonitoringFile(filePath) {
    if (filePath === currentlyMonitoredFile) {
        return; // Already monitoring this file
    }

    console.log('Starting to monitor:', path.basename(filePath));
    currentlyMonitoredFile = filePath;
    linesProcessed = 0; // Reset processed lines count for a new file

    // Stop previous watcher if any
    if (fileWatcher) {
        fileWatcher.close();
    }

    // Initial analysis of the entire file
    const initialParsedLines = await analyzeLogFile(filePath, 0);
    linesProcessed = initialParsedLines.length; // Update the number of lines processed

    // Watch for changes in the file
    fileWatcher = chokidar.watch(filePath, {
        persistent: true,
        ignoreInitial: true,
    }).on('change', async () => {
        console.log('File changed:', filePath);
        try {
            const content = await fs.readFile(filePath, 'utf-8');
            const currentLineCount = content.trim().split('\n').length;

            if (currentLineCount > linesProcessed) {
                // Analyze only the new lines
                const newParsedLines = await analyzeLogFile(filePath, linesProcessed);
                linesProcessed = currentLineCount; // Update the number of lines processed
            } else if (currentLineCount < linesProcessed) {
                // Handle file truncation (optional, depending on your needs)
                console.warn('Log file truncated, re-analyzing from the beginning.');
                const allParsedLines = await analyzeLogFile(filePath, 0);
                linesProcessed = allParsedLines.length;
            }
        } catch (error) {
            console.error('Error reading file during change detection:', error);
        }
    }).on('error', error => console.error('Error watching file:', error));
}

/** Start monitoring the directory for new log files.
 * * If a new file is added, it will start monitoring that file for changes. 
 * returns 'true' if Shipyard is enabled and monitoring is sucsefull */
function startDirectoryMonitoring() {
    try {
        if (Shipyard && Shipyard.enabled) {
            console.log('Starting Journal monitoring...');
            
            // Ensure the log directory exists
            fileHelper.ensureDirectoryExists(LOG_DIRECTORY);

            // Initial check for the latest file
            checkAndSwitchLogFile();

            // Watch the directory for new files
            directoryWatcher = chokidar.watch(LOG_DIRECTORY, {
                persistent: true,
                ignoreInitial: true,
            }).on('add', async (filePath) => {
                console.log('New file created:', filePath);
                checkAndSwitchLogFile();
            }).on('unlink', async (filePath) => {
                console.log('File removed:', filePath);
                checkAndSwitchLogFile(); // Re-evaluate in case the monitored file was removed
            }).on('error', error => console.error('Error watching directory:', error));
            return true; // Indicate success
        } 
        else { 
            console.log('** Shipyard is disabled, not monitoring directory. **');
            return false; // Indicate failure: Shipyard is disabled
        }
    } catch (error) {
        console.error(error);
    }
}

function RunSendKeyScript() {
    if (process.platform === 'win32') {
        const scriptPath = fileHelper.getAssetPath('data/etc/SendF11.vbs'); // Path to the VBScript file
        execFile('wscript.exe', ['//nologo', scriptPath], (error, stdout, stderr) => {
            if (error) {
                console.error('Error executing VBScript:', error);
                // You might want to send an error message back to the renderer process
            } else {
                console.log('VBScript executed successfully');
                // You might want to send a success message back to the renderer process
            }
            if (stderr) {
                console.error('VBScript stderr:', stderr);
            }
            return  stdout; // The VBScript likely won't produce much stdout
        });
    } else {
        //- Running on  Linux
        const scriptPath = fileHelper.getAssetPath('data/etc/SendF11.sh'); 
        return fileHelper.runProgram(scriptPath); 
    }
}

//- Utility method:
async function checkAndSwitchLogFile() {
    const latestFile = await getLatestLogFile();
    if (latestFile && latestFile !== currentlyMonitoredFile) {
        startMonitoringFile(latestFile);
    }
}

ipcMain.handle('start-log-monitoring', (event) => {
    try {
        return startDirectoryMonitoring();
    } catch (error) {
        console.error(error);
        return null;
    }
});
ipcMain.handle('shypyard-load-ships', (event) => {
    try {
        return Initialize(mainWindowInstance);
    } catch (error) {
        console.error(error);
        return null;
    }
});

export default { 
    getLatestLogFile, analyzeLogFile, 
    startMonitoringFile, checkAndSwitchLogFile, 
    startDirectoryMonitoring, 
    Initialize,
    OnShipyardEvent, PlayerJournal_ShipChanged,
    RunSendKeyScript,
};
