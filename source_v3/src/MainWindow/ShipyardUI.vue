<template class="h-100">
    <div v-show="visible" class="modal fade show" style="display: block;" data-bs-theme="dark">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content bg-dark text-light">
                <div class="modal-header">
                    <h5 class="modal-title">The Shipyard</h5>
                    <button type="button" class="btn-close btn-close-white" aria-label="Close" @click="close"></button>
                </div>
                <div class="modal-body" style="height: 600px;">

                    <!-- Main Content -->
                    <div class="container-fluid h-100">
                        <div class="frontier-panel d-flex flex-wrap align-items-center justify-content-between gap-2 mb-3 p-3">
                            <div>
                                <div class="fw-semibold">Frontier Fleet</div>
                                <div class="small" :class="frontierStatus.authorized ? 'text-success' : 'text-warning'">
                                    {{ frontierStatusText }}
                                </div>
                            </div>
                            <div class="btn-group" role="group" aria-label="Frontier fleet controls">
                                <button ref="frontierDataButton" type="button" class="btn btn-sm btn-outline-info"
                                    @click="openFrontierDataInfo">
                                    <i class="bi bi-shield-lock me-1" aria-hidden="true"></i>
                                    About my data
                                </button>
                                <template v-if="!frontierStatus.authorized">
                                    <button v-if="frontierBusy" type="button" class="btn btn-sm btn-outline-warning"
                                        :disabled="frontierCancelRequested" @click="cancelFrontierConnection">
                                        <span v-if="frontierCancelRequested" class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>
                                        {{ frontierCancelRequested ? 'Cancelling...' : 'Cancel Connection' }}
                                    </button>
                                    <button v-else type="button" class="btn btn-sm btn-primary" @click="connectFrontier">
                                        Connect Frontier
                                    </button>
                                </template>
                                <template v-else>
                                    <button type="button" class="btn btn-sm btn-success" :disabled="frontierBusy"
                                        @click="refreshFrontierFleet">
                                        <span v-if="frontierBusy" class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>
                                        Refresh Fleet
                                    </button>
                                    <button type="button" class="btn btn-sm btn-outline-danger" :disabled="frontierBusy"
                                        @click="disconnectFrontier">Disconnect</button>
                                </template>
                            </div>
                        </div>

                        <!-- Alert to show the 'Read Me' information of the selected mod -->
                        <div v-show="showInfo" class="alert alert-warning alert-dismissible" role="alert">
                            <button type="button" class="btn-close" aria-label="Close" @click="closeInfo"></button>
                            <div v-html="infoMessage"></div>
                        </div>

                        <div class="row row-cols-1 row-cols-md-3 g-4">
                            <div v-for="ship in filteredShips" :key="ship.record_id || ship.frontier_id || `${ship.kind_short}:${ship.name}:${ship.plate}`" class="col"
                                :class="{ 'selected-card': selectedShip === ship }" @click="selectCard(ship)">

                                <div class="card bg-secondary text-light">
                                    <img :src="ship.imageUrl" class="card-img-top" :alt="ship.name"
                                        @click.stop="changeShipImage(ship)" style="cursor: pointer;">
                                    <div class="card-body">
                                        <p class="card-title"><b>{{ ship.kind_full }}</b></p>
                                        <p class="card-text">{{ ship.name }} ({{ ship.plate }})</p>
                                    </div>
                                    <div class="card-footer">
                                        <select class="form-select form-select-sm" aria-label="Theme Selection"
                                            v-model="ship.theme" @change="onThemeChange(ship)">
                                            <option v-for="option in themes" :key="option.id" :value="option.name">
                                                {{ option.name }}
                                            </option>
                                        </select>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                </div> <!-- modal-body -->
                <div class="modal-footer">

                    <!-- Bottom NavBar -->
                    <nav class="navbar navbar-expand-lg navbar-dark bg-dark navbar-thin" data-bs-theme="dark">
                        <!-- Cuadro de búsqueda alineado a la izquierda -->
                        <form class="d-flex me-auto" role="search" @submit.prevent>
                            <input class="form-control form-control-sm" type="search" placeholder="Search Ship..."
                                aria-label="Search" v-model="searchQuery">
                        </form>
                        <span id="lblStatus" class="navbar-text mx-3 text-nowrap ml-auto text-warning"
                            style="padding-top: -4px;">
                            {{ statusText }}
                        </span>&nbsp;
                        <div class="form-check form-switch">
                            <input class="form-check-input" type="checkbox" value="" id="checkNativeSwitch" checked
                                v-model="shipData.enabled" switch>
                            <label class="form-check-label" for="checkNativeSwitch">
                                Journal Monitor {{ shipData.enabled ? 'Enabled' : 'Disabled' }}
                            </label>&nbsp;&nbsp;
                        </div>
                        <div class="btn-toolbar" role="toolbar" aria-label="Toolbar with button groups">

                            <div class="btn-group me-3" role="group" aria-label="First group">
                                <button id="cmdReloadShips" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdReloadShips_Click" @mouseover="updateStatus('Reload Journal Ships')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-arrow-clockwise"></i>
                                </button>
                                <button id="cmdImportFromV2" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdImportFromV2_Click" @mouseover="updateStatus('Import V2 Ships')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-arrow-bar-down"></i>
                                </button>
                                <button id="cmdDeleteShip" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdDeleteShip_Click" @mouseover="updateStatus('Delete Ship')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                            <div class="btn-group me-3" role="group" aria-label="Themes">
                                <button id="cmdShowSomeInfo" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdShowSomeInfo_Click" @mouseover="updateStatus('Info')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-info-circle"></i>
                                </button>
                                <button id="cmdSaveChanges" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdSaveChanges_Click" @mouseover="updateStatus('Save Changes')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-floppy-fill"></i>
                                </button>
                            </div>
                        </div>
                    </nav>

                </div>
            </div>
        </div>

        <div v-if="showFrontierDataInfo" class="frontier-data-overlay" role="presentation"
            @click.self="closeFrontierDataInfo">
            <section ref="frontierDataDialog" class="frontier-data-dialog bg-dark text-light" role="dialog"
                aria-modal="true" aria-labelledby="frontierDataTitle" tabindex="-1"
                @keydown.esc="closeFrontierDataInfo">
                <div class="frontier-data-header">
                    <h5 id="frontierDataTitle" class="mb-0">About your Frontier data</h5>
                    <button type="button" class="btn-close btn-close-white" aria-label="Close"
                        @click="closeFrontierDataInfo"></button>
                </div>

                <div class="frontier-data-body">
                    <div class="frontier-local-notice mb-3">
                        <strong>Your Frontier fleet data stays on your computer.</strong>
                        EDHM-UI does not upload your Commander or fleet information to an EDHM-UI server or collect it
                        anywhere else. Your profile is requested directly from Frontier's Companion API and processed
                        locally only to populate the Shipyard and keep your owned ships matched with their theme and
                        image settings.
                    </div>

                    <h6>What EDHM-UI keeps</h6>
                    <ul>
                        <li>Your Commander name.</li>
                        <li>Each owned ship's Frontier ID, model, custom name, and ship identifier.</li>
                        <li>The time, ship count, and active galaxy from the latest fleet refresh.</li>
                        <li>Your local per-ship theme and image choices.</li>
                    </ul>

                    <h6>What EDHM-UI does not keep</h6>
                    <p>
                        Frontier returns a broader Commander profile, but EDHM-UI does not save the raw response or
                        retain credits, ranks, location, cargo, modules, loadouts, or market data. The saved fleet data
                        remains in your local EDHM-UI user-data folder and is not sent to an EDHM-UI server.
                    </p>

                    <h6>Authorization and privacy</h6>
                    <p>
                        Frontier provides the <strong>auth</strong> and <strong>capi</strong> permissions used here.
                        Frontier does not provide a fleet-only permission, so EDHM-UI limits its own use of the returned
                        profile to the Shipyard information listed above. Access and refresh tokens are encrypted using
                        your operating system's secure storage and are never written into the Shipyard data file.
                        Disconnecting removes the locally stored Frontier authorization tokens but keeps your Shipyard
                        records and customizations.
                    </p>

                    <h6>Where authorization is stored</h6>
                    <p class="mb-2">Encrypted Frontier access and refresh tokens:</p>
                    <div class="frontier-storage-path mb-2"><code>{{ frontierTokenFilePath }}</code></div>
                    <button type="button" class="btn btn-sm btn-outline-light"
                        @click="openFrontierStorageFolder(frontierTokenFilePath)">
                        <i class="bi bi-folder2-open me-1" aria-hidden="true"></i>
                        Open authorization data folder
                    </button>
                </div>

                <div class="frontier-data-footer">
                    <button type="button" class="btn btn-outline-info" @click="openFrontierDeveloperDocs">
                        Frontier API authentication documentation
                        <i class="bi bi-box-arrow-up-right ms-1" aria-hidden="true"></i>
                    </button>
                    <button type="button" class="btn btn-secondary" @click="closeFrontierDataInfo">Close</button>
                </div>
            </section>
        </div>
    </div>
</template>
<script>
import EventBus from '../EventBus.js';
import Util from '../Helpers/Utils.js';

// Enable Dropdown for the Context Menus:
const dropdownElementList = document.querySelectorAll('.dropdown-toggle');
const dropdownList = [...dropdownElementList].map(dropdownToggleEl => new bootstrap.Dropdown(dropdownToggleEl));

//Enable Tooltips:
const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

export default {
    name: 'ShipyardUI',
    components: {

    },
    props: {
        themesData: {
            type: Object, // Or Array, or whatever the type of your themes data is
            required: false, // Or true, depending on whether the prop is mandatory
        },
    },
    emits: ['shipSelected'], // Emit an event when a ship is selected
    watch: {
        themesData(newValue, oldValue) {
            // This function will be called whenever the 'themesData' prop changes
            console.log('themesData prop changed!');
            // Perform any actions you need to take when the prop updates
            this.loadShipData(newValue);
        },
    },
    data() {
        return {
            visible: false,
            showSpinner: false,        //<- Flag to Show/hide the Loading Spinner            
            shipyardMessage: '',
            frontierBusy: false,
            frontierCancelRequested: false,
            showFrontierDataInfo: false,
            frontierTokenFilePath: '',
            frontierStatus: {
                authorized: false,
                commanderName: '',
                expiresAt: null,
                lastSyncAt: null,
                shipCount: 0,
            },
            removeFleetUpdatedListener: null,

            ActiveInstance: null,
            showAlert: false,       //<- Flag to Show/hide the Install/Update Alert
            alert: {
                title: '',
                message: '',
                detail: '',
                changes: '',
                version: '',
                thumbnail: ''
            },

            showInfo: false,        //<- Flag to Show/hide the mod's 'Read Me' information
            infoMessage: '',

            shipData: {}, // { enabled: false, player_name: '', ships: [] }, //<- Shipyard Data
            ships: [],
            themes: [],
            DATA_DIRECTORY: '',
            selectedShip: null,
            searchQuery: ''
        };
    },
    computed: {
        filteredShips() {
            if (!this.searchQuery) return this.ships;
            const normalize = str => str.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
            const q = normalize(this.searchQuery.toLowerCase());
            return this.ships.filter(ship =>
                normalize(ship.name || '').toLowerCase().includes(q) ||
                normalize(ship.kind_full || '').toLowerCase().includes(q) ||
                normalize(ship.plate || '').toLowerCase().includes(q)
            );
        },
        statusText() {
            if (this.shipyardMessage) return this.shipyardMessage;
            return `${this.filteredShips.length} of ${this.ships.length} ships`;
        },
        frontierStatusText() {
            if (this.frontierBusy) return 'Waiting for Frontier...';
            if (!this.frontierStatus.authorized) return 'Not connected';
            const commander = this.frontierStatus.commanderName
                ? `Connected as CMDR ${this.frontierStatus.commanderName}`
                : 'Connected';
            if (!this.frontierStatus.lastSyncAt) return commander;
            return `${commander} · ${this.frontierStatus.shipCount} ships · Last refreshed ${new Date(this.frontierStatus.lastSyncAt).toLocaleString()}`;
        }
    },
    methods: {

        async Initialize() {
            try {
                console.log('Initializing Shipyard UI..');
                this.showInfo = false;
                this.showAlert = false;

                this.DATA_DIRECTORY = await window.api.GetProgramDataDirectory();
                await Promise.all([
                    this.loadFrontierStatus(),
                    this.loadFrontierTokenPath(),
                ]);

            } catch (error) {
                this.showSpinner = false;
                console.error(error);
                EventBus.emit('ShowError', error);
            } finally {
                setTimeout(() => {
                    this.showSpinner = false;
                }, 1000);
            }
        },
        async loadShipData(themes = this.themes) {
            this.showSpinner = true;
            //const ShipyardFilePath = await window.api.joinPath(this.DATA_DIRECTORY, 'Shipyard_v3.json');
            const ShyardData = await window.shipyardAPI.getConfig(); //console.log('shipData', this.shipData);
            this.shipData = ShyardData.shipyard;
            if (this.shipData) {
                this.ships = await Promise.all(this.shipData.ships.map(async ship => ({
                    ...ship,
                    imageUrl: await this.getShipImage(ship.image) // Resolve the promise here
                })));
                this.themes = themes || this.themes;
                this.shipyardMessage = `${this.ships.length} Ships Loaded.`;
            }
            this.showSpinner = false;
        },
        async saveShipData() {
            const dataToSave = {
                enabled: this.shipData.enabled,
                player_name: this.shipData.player_name,
                ships: this.ships.map(ship => {
                    const { imageUrl, ...shipWithoutImageUrl } = ship;
                    return shipWithoutImageUrl;
                })
            };
            try {
                //await window.api.writeJsonFile(ShipyardFilePath, dataToSave, true); //<- path, data, beautify
                const result = await window.shipyardAPI.updateConfig(dataToSave);
                if (result.success) {
                    console.log('Config actualizada:', result.updated);
                } else {
                    console.error('Error al actualizar config:', result.error);
                }
                
                this.updateStatus('Ship data saved.');
                return true; // Indicate success

            } catch (error) {
                console.error('Error saving ship data:', error);
                return false; // Indicate failure
            }
        },

        /* Pops up this Component */
        async open(data) {
            this.visible = true;
            this.ActiveInstance = data;
            await this.Initialize();
            await this.loadShipData(this.themesData || this.themes);
            console.log('Poping Shipyard UI..');
        },
        close() {
            this.showFrontierDataInfo = false;
            this.visible = false;
        },
        openFrontierDataInfo() {
            this.showFrontierDataInfo = true;
            this.$nextTick(() => this.$refs.frontierDataDialog?.focus());
        },
        closeFrontierDataInfo() {
            this.showFrontierDataInfo = false;
            this.$nextTick(() => this.$refs.frontierDataButton?.focus());
        },
        openFrontierDeveloperDocs() {
            window.api.openUrlInBrowser('https://user.frontierstore.net/developer/docs');
        },
        async loadFrontierTokenPath() {
            this.frontierTokenFilePath = await window.shipyardAPI.frontierTokenPath();
        },
        async openFrontierStorageFolder(filePath) {
            if (!filePath) {
                EventBus.emit('ShowError', new Error('The local Frontier storage path is not available.'));
                return;
            }
            try {
                await window.api.openPathInExplorer(window.api.getParentFolder(filePath));
            } catch (error) {
                EventBus.emit('ShowError', error);
            }
        },
        sanitizeId(id) {
            /* Cleans html tags */
            return id.replace(/\s/g, '');
        },
        updateStatus(message) {
            this.shipyardMessage = message;
        },
        clearStatus() {
            this.shipyardMessage = '';
        },
        closeInfo() {
            this.showInfo = false;
        },
        async getShipImage(ship_image) {
            /* Returns the ship image path */
            //const img_path = await window.api.getAssetFileUrl(`images/Ships/${ship_image}`); //            console.log("Ship image path:", img_path); // For debugging
            const img_path = await window.api.joinPath(this.DATA_DIRECTORY, `images/Ships/${ship_image}`);
            return img_path;
        },
        async changeShipImage(ship) {
            const options = {
                title: 'Select an Image for the Ship:',
                defaultPath: '', //Absolute directory path, absolute file path, or file name to use by default. 
                filters: [
                    { name: 'Images', extensions: ['jpg', 'jpeg', 'png', 'gif'] },
                    { name: 'All Files', extensions: ['*'] }
                ],
                properties: ['openFile', 'showHiddenFiles', 'createDirectory', 'promptToCreate', 'dontAddToRecent'],
            };
            const filePath = await window.api.ShowOpenDialog(options);
            if (filePath) {
                const selectedImagePath = filePath[0];
                const originalFilename = selectedImagePath.split('/').pop(); // Get the filename
                const newFilename = `${ship.kind_short}_${Date.now()}.${originalFilename.split('.').pop()}`; // Create a unique filename
                
                //const destinationPath = await window.api.getAssetPath(`images/Ships/${newFilename}`);
                const destinationPath = await window.api.joinPath(this.DATA_DIRECTORY, `images/Ships/${newFilename}`);

                try {
                    await window.api.copyFile(selectedImagePath, destinationPath);
                    const index = this.ships.findIndex(s => this.isSameShip(s, ship));
                    if (index !== -1) {
                        // Update the ship object
                        this.ships[index] = {
                            ...this.ships[index],
                            image: newFilename,
                            imageUrl: await this.getShipImage(newFilename) // Update imageUrl
                        };
                        // Optionally, you might want to trigger a save of your shipData here
                        this.saveShipData();
                    }
                } catch (error) {
                    console.error('Error copying image:', error);
                    EventBus.emit('ShowError', 'Failed to change ship image.');
                }
            }
        },
        async onThemeChange(ship) {
            console.log(`Theme changed for ${ship.name} to: ${ship.theme}`);
            try {
                await this.saveShipData();
                this.shipyardMessage = `Theme for ${ship.name} updated and saved.`;
                // Optionally, you can emit an event or show a notification to the user
            } catch (error) {
                console.error('Error saving ship data:', error);
                this.shipyardMessage = 'Error saving ship data.';
                EventBus.emit('ShowError', 'Failed to save ship data.');
            }
        },

        selectCard(ship) {
            this.selectedShip = ship;
            console.log('Selected ship:', ship);
            this.$emit('shipSelected', ship); // Emitimos el evento usando this.$emit
        },

        isSameShip(left, right) {
            if (left.record_id && right.record_id) return left.record_id === right.record_id;
            if (left.frontier_id != null && right.frontier_id != null) {
                return String(left.frontier_id) === String(right.frontier_id);
            }
            return left.kind_short === right.kind_short && left.name === right.name && left.plate === right.plate;
        },

        async loadFrontierStatus() {
            this.frontierStatus = await window.shipyardAPI.frontierStatus();
        },

        async connectFrontier() {
            this.frontierBusy = true;
            this.frontierCancelRequested = false;
            this.shipyardMessage = 'Waiting for Frontier authorization...';
            EventBus.emit('RoastMe', {
                type: 'Info',
                title: 'Connect Frontier',
                message: 'Your browser will open Frontier authorization.<br>' +
                    'After approval, allow the browser to open EDHM-UI to complete the connection. ' +
                    'No localhost certificate exception is required.',
                autoHide: false,
                width: '520px',
            });
            try {
                const result = await window.shipyardAPI.frontierLogin();
                this.frontierStatus = result.frontierStatus;
                await this.loadShipData(this.themes);
                this.shipyardMessage = `${result.frontierStatus.shipCount} Frontier ships refreshed.`;
                EventBus.emit('RoastMe', {
                    type: 'Success',
                    message: `Frontier connected. ${result.frontierStatus.shipCount} ships loaded into the Shipyard.`,
                });
            } catch (error) {
                if (this.frontierCancelRequested) {
                    this.shipyardMessage = 'Frontier connection cancelled.';
                } else {
                    EventBus.emit('ShowError', error);
                }
            } finally {
                EventBus.emit('closeToast', 'Info');
                this.frontierBusy = false;
                this.frontierCancelRequested = false;
                await this.loadFrontierStatus().catch(() => {});
            }
        },

        async cancelFrontierConnection() {
            if (!this.frontierBusy || this.frontierCancelRequested) return;

            this.frontierCancelRequested = true;
            this.shipyardMessage = 'Cancelling Frontier connection...';
            try {
                const cancelled = await window.shipyardAPI.frontierCancelLogin();
                if (!cancelled) {
                    this.frontierCancelRequested = false;
                    this.shipyardMessage = 'Completing Frontier connection...';
                }
            } catch (error) {
                this.frontierCancelRequested = false;
                EventBus.emit('ShowError', error);
            }
        },

        async refreshFrontierFleet() {
            this.frontierBusy = true;
            this.shipyardMessage = 'Refreshing fleet from Frontier...';
            try {
                const result = await window.shipyardAPI.frontierRefresh();
                this.frontierStatus = result.frontierStatus;
                await this.loadShipData(this.themes);
                this.shipyardMessage = `${result.frontierStatus.shipCount} Frontier ships refreshed.`;
                EventBus.emit('RoastMe', {
                    type: 'Success',
                    message: `${result.frontierStatus.shipCount} ships refreshed from Frontier.`,
                });
            } catch (error) {
                EventBus.emit('ShowError', error);
            } finally {
                this.frontierBusy = false;
                await this.loadFrontierStatus().catch(() => {});
            }
        },

        async disconnectFrontier() {
            this.frontierBusy = true;
            try {
                this.frontierStatus = await window.shipyardAPI.frontierLogout();
                this.shipyardMessage = 'Frontier account disconnected. Stored ships were retained.';
                EventBus.emit('RoastMe', {
                    type: 'Success',
                    message: 'Frontier authorization removed. Existing Shipyard data was retained.',
                });
            } catch (error) {
                EventBus.emit('ShowError', error);
            } finally {
                this.frontierBusy = false;
            }
        },

        async cmdReloadShips_Click(e) {
            this.saveShipData();            

            const shipyardEnabled = await window.shipyardAPI.start();
            console.log('Shipyard Status:', await window.shipyardAPI.getStatus());

            EventBus.emit('shypyard-load-ships', null);
            this.loadShipData(this.themes);
        },
         cmdImportFromV2_Click(e) {
            this.ImportShipyardV2(); // Call the method to import ships from V2
            this.loadShipData(this.themes);
            this.shipyardMessage = 'Ships imported from V2.';
        },
        async cmdDeleteShip_Click(e) {
            if (this.selectedShip) {

                const options = {
                    type: 'question', //<- none, info, error, question, warning
                    buttons: ['Cancel', "Yes, Delete it", 'No, thanks.'],
                    defaultId: 1,
                    title: 'Delete Ship?',
                    message: `Are you sure you want to delete '${this.selectedShip.name}' ? `,
                    detail: '',
                    cancelId: 0
                };
                const result = await window.api.ShowMessageBox(options);
                if (result && result.response === 1) {
                    const index = this.ships.findIndex(s => this.isSameShip(s, this.selectedShip));
                    if (index !== -1) {
                        this.ships.splice(index, 1); // Remove the ship from the array
                        this.selectedShip = null; // Clear the selected ship
                        this.shipyardMessage = 'Ship deleted.';
                        this.saveShipData(); // Save changes to the file
                    }
                }
            } else {
                EventBus.emit('ShowError', 'No ship selected for deletion.');
            }
        },
        cmdShowSomeInfo_Click(e) {
            this.showInfo = !this.showInfo;
            if (this.showInfo) {
                this.infoMessage = 'The <b>Shipyard</b> allows you to set a theme for each ship.<br>' +
                    'Connect Frontier to import your complete owned fleet without reading the Player Journal.<br>' +
                    'The optional Journal Monitor detects when you embark a ship and automatically applies its assigned theme.';
            } else {
                this.infoMessage = '';
            }
        },
        cmdSaveChanges_Click(e) {
            this.saveShipData();
        },

        /** This will Import a Shipyard V2 File.
         * Converted from the old Shipyard V2 format to the new V2 format.
         * Moves the file to the EDHM_UI Data Directory and deletes the old file.     */
        async ImportShipyardV2() {
            try {
                const DATA_DIRECTORY = await window.api.GetProgramDataDirectory(); //<- Get the Data Directory: %USERPROFILE%\EDHM_UI
                const v2Location = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\edhm_ui');
                const v2ShipyardFile = window.api.joinPath(v2Location, 'Data', 'shipyard.json');
                const v3ShipyardFile = window.api.joinPath(DATA_DIRECTORY, 'Shipyard_v3.json');
                const v2FileExists = await window.api.fileExists(v2ShipyardFile);

                if (v2FileExists) {
                    // Read the old Shipyard V2 file
                    const shipyardV2Data = await window.api.getJsonFile(v2ShipyardFile);
                    console.log('Shipyard V2 Data:', shipyardV2Data);

                    // Convert the data to the new format
                    const convertedData = shipyardV2Data.ships.map(item => ({
                        ship_id:        item.Ship.ship_id,
                        kind_short:     item.Ship.ed_short,
                        kind_full:      item.Ship.ship_full_name,
                        custom_name:    item.ship_name,
                        plate:          item.ship_plate || '',
                        theme:          item.theme || 'Current Settings',
                        image:          item.Ship.ed_short + '.jpg',
                    }));

                    var Shipyard = {
                        enabled: shipyardV2Data.enabled,
                        player_name: shipyardV2Data.player_name,
                        ships: convertedData
                    };

                    // Save the converted data to the new file
                    const _ret = await window.api.writeJsonFile(v3ShipyardFile, Shipyard);
                    if (_ret) {
                        //console.log('Converted Shipyard V2 to V3:', Shipyard);
                        console.log('Converted Shipyard V2 to V3 and saved to:', v3ShipyardFile);

                        // Delete the old Shipyard V2 file
                        //await window.api.deleteFileByAbsolutePath(v2ShipyardFile);
                        //console.log('Deleted old Shipyard V2 file:', v2ShipyardFile);

                        EventBus.emit('RoastMe', { type: 'Success', message: 'Shipyard V2 Imported Successfully!' });
                    }
                } else {
                    console.log('No Shipyard V2 file found at:', v2ShipyardFile);
                    EventBus.emit('RoastMe', { type: 'Error', message: 'No Shipyard V2 file found!' });
                }

            } catch (error) {
                console.log('Error @ImportShipyardV2():', error);
                EventBus.emit('ShowError', error);
            }
        },

        OnShipAdded(data) {
            console.log('@shipyard-ShipAdded', data);
            this.shipData = data.shipyard; 
            this.loadShipData(this.themes);
        },

        async OnFleetUpdated(data) {
            this.frontierStatus = data.frontierStatus || this.frontierStatus;
            await this.loadShipData(this.themes);
        }


    },
    mounted() {
        /* LISTENING EVENTS:   */
        EventBus.on('open-ShipyardUI', this.open);
        EventBus.on('ShipyardUI-Initialize', this.Initialize);
        window.shipyardAPI.onShipAdded(this.OnShipAdded);
        this.removeFleetUpdatedListener = window.shipyardAPI.onFleetUpdated(this.OnFleetUpdated);
    },
    beforeUnmount() {
        // Clean up the event listener:
        EventBus.off('open-ShipyardUI', this.open);
        EventBus.off('ShipyardUI-Initialize', this.Initialize);
        window.shipyardAPI?.onShipAdded(() => {});
        this.removeFleetUpdatedListener?.();
    }
};
</script>
<style scoped>
#ShipyardUI {
    background-color: #1F1F1F;
    color: #ffffff;
}

.card-img-top {
    max-height: 200px;
    /* Adjust as needed */
    object-fit: cover;
    /* Ensure images fill the container without distortion */
}

.selected-card .card {
    border: 2px solid orange !important;
    /* Puedes personalizar el estilo de resaltado */
    box-shadow: 0 0 10px orange(0, 0, 255, 0.5);
    /* Ejemplo de sombra */
}

.frontier-panel {
    background: #252a30;
    border: 1px solid #495057;
    border-radius: 0.375rem;
}

.frontier-data-overlay {
    position: fixed;
    inset: 0;
    z-index: 1080;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 1rem;
    background: rgba(0, 0, 0, 0.72);
}

.frontier-data-dialog {
    display: flex;
    flex-direction: column;
    width: min(700px, 100%);
    max-height: calc(100vh - 2rem);
    border: 1px solid #495057;
    border-radius: 0.5rem;
    box-shadow: 0 1rem 3rem rgba(0, 0, 0, 0.55);
}

.frontier-data-header,
.frontier-data-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.75rem;
    padding: 1rem 1.25rem;
}

.frontier-data-header {
    border-bottom: 1px solid #495057;
}

.frontier-data-body {
    overflow-y: auto;
    padding: 1rem 1.25rem;
}

.frontier-data-body h6 {
    color: #6edff6;
}

.frontier-local-notice {
    padding: 0.9rem 1rem;
    color: #d9f7ff;
    background: rgba(13, 202, 240, 0.12);
    border: 1px solid rgba(110, 223, 246, 0.55);
    border-left-width: 4px;
    border-radius: 0.375rem;
}

.frontier-storage-path {
    overflow-wrap: anywhere;
    padding: 0.55rem 0.7rem;
    background: #191c1f;
    border: 1px solid #495057;
    border-radius: 0.375rem;
}

.frontier-storage-path code {
    color: #9eeaf9;
}

.frontier-data-footer {
    flex-wrap: wrap;
    border-top: 1px solid #495057;
}

@media (max-width: 575.98px) {
    .frontier-data-footer > .btn {
        width: 100%;
    }
}
</style>
