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

                        <!-- TODO: Dynamically load the Ships -->
                        <div class="row row-cols-1 row-cols-md-3 g-4">
                            <div v-for="ship in ships" :key="ship.ship_id" class="col">
                                <div class="card bg-secondary text-light">
                                    <img :src="ship.thumbnail.replace('${ship.ed_short}', ship.ship_kind.toLowerCase())"
                                        class="card-img-top" :alt="ship.ship_name">
                                    <div class="card-body">
                                        <h5 class="card-title">{{ ship.ship_name }}</h5>
                                        <p class="card-text">{{ ship.ship_kind_name }} ({{ ship.ship_plate }})</p>
                                    </div>
                                    <div class="card-footer bg-transparent">
                                        <select class="form-select form-select-sm" aria-label="Theme Selection" v-model="ship.theme">
                                            <option v-for="option in themes" :key="option.id" :value="option.id">
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
                        <div class="btn-toolbar" role="toolbar" aria-label="Toolbar with button groups">
                            <span id="lblStatus" class="navbar-text mx-3 text-nowrap ml-auto"
                                style="padding-top: -4px;">
                                {{ statusText }}
                            </span>
                            <div class="btn-group me-3" role="group" aria-label="First group">
                                <button id="cmdReadMe" class="btn btn-success text-light btn-outline-secondary"
                                    data-bs-title="Mod Information" @mousedown="cmdReadMe_Click"
                                    @mouseover="updateStatus('Mod Information')" @mouseleave="clearStatus">
                                    <i class="bi bi-book"></i> Read me
                                </button>
                                <button id="cmdReloadMods" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdReloadMods_Click" @mouseover="updateStatus('Reload Mod List')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-arrow-clockwise"></i>
                                </button>
                                <button id="cmdImportMod" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdImportMod_Click" @mouseover="updateStatus('Import Mod')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-download"></i>
                                </button>
                                <button id="cmdDeleteMod" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdDeleteMod_Click" @mouseover="updateStatus('Delete Mod')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                            <div class="btn-group me-3" role="group" aria-label="Themes">
                                <button id="cmdEditJSON" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdEditJSON_Click" @mouseover="updateStatus('Edit Json Configuration')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-pencil-square"></i>
                                </button>
                                <button id="cmdEditIni" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdEditIni_Click" @mouseover="updateStatus('Edit Ini Configuration')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-gear"></i>
                                </button>
                                <button id="cmdEditOpenFolder" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdEditOpenFolder_Click"
                                    @mouseover="updateStatus('Open Container Folder')" @mouseleave="clearStatus">
                                    <i class="bi bi-folder2-open"></i>
                                </button>
                                <button id="cmdEditReinstall" class="btn btn-outline-secondary" type="button"
                                    @mousedown="cmdEditReinstall_Click" @mouseover="updateStatus('Re-Install Mod')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-arrow-repeat"></i>
                                </button>
                            </div>
                            <div class="btn-group me-3" role="group" aria-label="Themes">
                                <button id="cmdThemeExport" class="btn btn-outline-secondary disabled" type="button"
                                    @mousedown="cmdThemeExport_Click" @mouseover="updateStatus('Export Theme')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-arrow-bar-up"></i>
                                </button>
                                <button id="cmdThemeImport" class="btn btn-outline-secondary disabled" type="button"
                                    @mousedown="cmdThemeImport_Click" @mouseover="updateStatus('Import Theme')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-arrow-bar-down"></i>
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
//import Shipyard from './Shipyard.js';

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
                this.showSpinner = true;
                this.statusText = 'Initializing..';

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
            const ShipyardFilePath = await window.api.joinPath(this.DATA_DIRECTORY, 'Shipyard_v3.json'); console.log("Shipyard file path:", ShipyardFilePath); // For debugging
            this.shipData = await window.api.getJsonFile(ShipyardFilePath); console.log("Shipyard data loaded:", this.shipData); // For debugging
            if (this.shipData) {
                this.ships = this.shipData.ships;   console.log("Loaded ships:", this.ships); // For debugging
                this.themes = themes;               console.log("Loaded themes:", this.themes); // For debugging
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