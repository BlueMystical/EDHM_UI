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

                        <!-- Dynamically load the Ships on Cards -->
                        <div class="row row-cols-1 row-cols-md-3 g-4">
                            <div v-for="ship in ships" :key="ship.ship_id" class="col">
                                <div class="card bg-secondary text-light">
                                    <img :src="ship.imageUrl" class="card-img-top" :alt="ship.name"
                                        @click="changeShipImage(ship)" style="cursor: pointer;">
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
                        <div class="form-check form-switch">
                            <input class="form-check-input" type="checkbox" value="" id="checkNativeSwitch"  checked v-model="shipData.enabled" switch>
                            <label class="form-check-label" for="checkNativeSwitch">Shipyard Enabled</label>
                        </div>
                        <div class="btn-toolbar" role="toolbar" aria-label="Toolbar with button groups">
                            <span id="lblStatus" class="navbar-text mx-3 text-nowrap ml-auto"
                                style="padding-top: -4px;">
                                {{ statusText }}
                            </span>

                            <div class="btn-group me-3" role="group" aria-label="First group">
                                <button id="cmdReloadShips" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdReloadShips_Click" @mouseover="updateStatus('Reload Ships')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-arrow-clockwise"></i>
                                </button>
                                <button id="cmdDeleteShip" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdDeleteShip_Click" @mouseover="updateStatus('Delete Ship')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                            <div class="btn-group me-3" role="group" aria-label="Themes">
                                <button id="cmdInfo" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdThemeExport_Click" @mouseover="updateStatus('Info')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-info-circle"></i>
                                </button>
                                <button id="cmdSaveChanges" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdThemeImport_Click" @mouseover="updateStatus('Save Changes')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-floppy-fill"></i>
                                </button>
                            </div>
                        </div>
                    </nav>

                </div>
            </div>
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
            // You can also provide a default value if needed:
            // default: () => ({})
        },
    },
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
            statusText: 'Ready.',

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

            shipData: {},
            ships: [],
            themes: [],
            DATA_DIRECTORY: '',

        };
    },
    methods: {

        async Initialize() {
            try {
                console.log('Initializing Shipyard UI..');
                this.showInfo = false;
                this.showAlert = false;                

                this.DATA_DIRECTORY = await window.api.GetProgramDataDirectory();

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
        async loadShipData(themes) {
            this.showSpinner = true;
            const ShipyardFilePath = await window.api.joinPath(this.DATA_DIRECTORY, 'Shipyard_v3.json');
            this.shipData = await window.api.getJsonFile(ShipyardFilePath);
            if (this.shipData) {
                this.ships = await Promise.all(this.shipData.ships.map(async ship => ({
                    ...ship,
                    imageUrl: await this.getShipImage(ship.image) // Resolve the promise here
                })));
                this.themes = themes;
                this.statusText = `${this.ships.length} Ships Loaded.`;
            }
            this.showSpinner = false;
        },
        async saveShipData() {
            const ShipyardFilePath = await window.api.joinPath(this.DATA_DIRECTORY, 'Shipyard_v3.json');
            const dataToSave = {
                enabled: this.shipData.enabled,
                player_name: this.shipData.player_name,
                ships: this.ships.map(ship => {
                    const { imageUrl, ...shipWithoutImageUrl } = ship;
                    return shipWithoutImageUrl;
                })
            };
            try {
                await window.api.writeJsonFile(ShipyardFilePath, dataToSave, true); //<- path, data, beautify
                console.log('Ship data saved.');
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
            this.Initialize();
            console.log('Poping Shipyard UI..');
        },
        close() {
            this.visible = false;
        },
        sanitizeId(id) {
            /* Cleans html tags */
            return id.replace(/\s/g, '');
        },
        updateStatus(message) {
            this.statusText = message;
        },
        clearStatus() {
            this.statusText = '';
        },
        async getShipImage(ship_image) {
            /* Returns the ship image path */
            const img_path = await window.api.getAssetFileUrl(`images/Ships/${ship_image}`); //            console.log("Ship image path:", img_path); // For debugging
            return img_path;
        },
        async changeShipImage(ship) {
            const options = {
                title: 'Import Theme from a ZIP file:',
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
                const destinationPath = await window.api.joinPath(this.DATA_DIRECTORY, 'images', 'Ships', newFilename);

                try {
                    await window.api.copyFile(selectedImagePath, destinationPath);
                    const index = this.ships.findIndex(s => s.ship_id === ship.ship_id);
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
                this.statusText = `Theme for ${ship.name} updated and saved.`;
                // Optionally, you can emit an event or show a notification to the user
            } catch (error) {
                console.error('Error saving ship data:', error);
                this.statusText = 'Error saving ship data.';
                EventBus.emit('ShowError', 'Failed to save ship data.');
            }
        },
    },
    mounted() {
        this.Initialize();
        /* LISTENING EVENTS:   */
        EventBus.on('open-ShipyardUI', this.open);
    },
    beforeUnmount() {
        // Clean up the event listener:
        EventBus.off('open-ShipyardUI', this.open);
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
</style>