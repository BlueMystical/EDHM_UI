<template>
    <div id="TPModsManager" class="vh-100 vw-100 p-0 bg-dark text-light " data-bs-theme="dark">
        <Notifications ref="notif" />

        <div class="content-container">

            <!-- Loading Spinner -->
            <div v-if="showSpinner"
                class="d-flex justify-content-center align-items-center position-fixed top-0 left-0 w-100 h-100 bg-dark bg-opacity-75 z-index-999">
                <div class="spinner-border text-light" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>

            <!-- Main Content -->
            <div class="container-fluid h-100">
                <div class="row h-100">
                    <div class="col h-100">
                        <div class="row h-100">
                            <!-- List of available Mods -->
                            <div class="left-column border  content-scrollable">
                                Left Column (30%)
                            </div>
                            <!-- Properties of the selected Mod -->
                            <div class="right-column border  content-scrollable">
                                <Properties ref="ModProps" @OnValueChanged="TODO" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Bottom NavBar -->
        <nav class="navbar navbar-expand-lg navbar-dark bg-dark fixed-bottom navbar-thin" data-bs-theme="dark">
            <div class="btn-toolbar" role="toolbar" aria-label="Toolbar with button groups">
                <span id="lblStatus" class="navbar-text mx-3 text-nowrap ml-auto" style="padding-top: -4px;">{{
                    statusText }}</span>
                <div class="btn-group me-2" role="group" aria-label="First group">
                    <button id="cmdAddNewTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip"
                        data-bs-placement="bottom" data-bs-title="Add New Theme" @mousedown="addNewTheme_Click">
                        <i class="bi bi-plus-circle"></i>
                    </button>
                    <button id="cmdEditTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip"
                        data-bs-placement="bottom" data-bs-title="Edit Theme" @mousedown="editTheme_Click">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                    <button id="cmdExportTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip"
                        data-bs-placement="bottom" data-bs-title="Export Theme" @mousedown="exportTheme_Click">
                        <i class="bi bi-save"></i>
                    </button>
                    <button id="cmdSaveTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip"
                        data-bs-placement="bottom" data-bs-title="Save Theme" @mousedown="saveTheme_Click">
                        <i class="bi bi-floppy"></i>
                    </button>
                    <button id="cmdReloadThemes" class="btn btn-outline-secondary" type="button"
                        data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-title="Reload Themes"
                        @mousedown="reloadThemes_Click">
                        <i class="bi bi-arrow-clockwise"></i>
                    </button>
                </div>
                <div class="btn-group me-2 dropup" role="group" aria-label="Second group">
                    <button type="button" class="btn btn-outline-secondary dropdown-toggle" data-bs-toggle="dropdown"
                        aria-expanded="false">
                        Dropdown
                    </button>
                    <ul class="dropdown-menu">
                        <li><a class="dropdown-item" href="#">Dropdown link</a></li>
                        <li><a class="dropdown-item" href="#">Dropdown link</a></li>
                    </ul>
                </div>
            </div>
        </nav>
    </div>
</template>

<script>
import EventBus from '../EventBus.js';
import Notifications from '../MainWindow/Components/Notifications.vue';
import Properties from './TPProperties.vue';
import Util from '../Helpers/Utils.js';

// Enable Dropdown for the Context Menus:
const dropdownElementList = document.querySelectorAll('.dropdown-toggle');
const dropdownList = [...dropdownElementList].map(dropdownToggleEl => new bootstrap.Dropdown(dropdownToggleEl));

export default {
    name: 'TPModsManager',
    components: {
        Notifications,
        Properties
    },
    data() {
        return {
            showSpinner: false,        //<- Flag to Show/hide the Loading Spinner
            statusText: 'Ready.',
        };
    },
    methods: {

        /** This is the Start Point of the Program **/
        async Initialize() {
            try {
                console.log('Initializing 3PMods Manager..');
                this.showSpinner = true;
                this.statusText = 'Initializing..';

                const notiOptions = {
                    type: 'Info', //<- Info, Success, Warning, Error
                    title: 'TPMods:',
                    message: 'Hello World!',
                    autoHide: true,
                    delay: 5000  //<- Auto-hide delay in milliseconds
                };
                this.$refs.notif.showToast(notiOptions);

            } catch (error) {
                console.error(error);
            } finally {
                this.loading = false;
            }
        },

        OnValueChanged(e) {
            console.log('OnValueChanged event received.', e);
        }

    },
    mounted() {
        this.Initialize();
        /* LISTENING EVENTS:   */
        EventBus.on('On_Initialize3PMods', this.Initialize);
    },
    beforeUnmount() {
        // Clean up the event listener
        EventBus.off('On_Initialize3PMods', this.Initialize);
    }
};
</script>

<style scoped>
#TPModsManager {
    background-color: #1F1F1F;
    color: #ffffff;
}

.content-container {
    height: calc(100vh - 56px);
    /* Adjust 56px to your navbar height */
    width: 100%;
}

.content-scrollable {
    overflow-y: auto;
}

.left-column {
    width: 30%;
    height: 100%;
    float: left;
}

.right-column {
    width: 70%;
    height: 100%;
    float: left;
}

.row>div {
    box-sizing: border-box;
}

.spinner-container {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
}

.visually-hidden {
    color: #ffffff;
}
</style>
