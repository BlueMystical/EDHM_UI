<template class="h-100">
    <div v-if="visible" class="modal fade show" style="display: block;" data-bs-theme="dark">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content bg-dark text-light">
                <div class="modal-header">
                    <h5 class="modal-title">TPMods Manager</h5>
                    <button type="button" class="btn-close btn-close-white" aria-label="Close" @click="close"></button>
                </div>
                <div class="modal-body" style="height: 600px;">

                    <Notifications ref="notif" />

                    <!-- Main Content -->
                    <div class="container-fluid h-100">

                        <!-- Loading Spinner -->
                        <div v-if="showSpinner"
                            class="d-flex justify-content-center align-items-center position-fixed top-10 left-50 w-50 h-50 bg-dark bg-opacity-75 z-index-999">
                            <div class="spinner-border text-light" role="status">
                                <span class="visually-hidden">Loading...</span>
                            </div>
                        </div>

                        <div class="row h-100">
                            <div class="col h-100">
                                <div class="row h-100">
                                    <!-- List of available Mods -->
                                    <div class="left-column border content-scrollable">
                                        <ul>
                                            <li v-for="mod in TPmods" :key="mod.mod_name" :id="'mod-' + mod.mod_name"
                                                class="image-container"
                                                :class="{ 'selected': mod.mod_name === selectedModBasename }"
                                                @click="onSelectMod(mod)" @contextmenu="onRightClick($event, mod)">
                                                <img :src="mod.isActive ? mod.thumbnail_url : getGrayscaleImage(mod)"
                                                    :alt="mod.mod_name" class="img-thumbnail"
                                                    :style="{ filter: mod.isActive ? 'none' : 'grayscale(100%)' }"
                                                    aria-label="Thumbnail of {{ mod.mod_name }}" />
                                                <span class="image-label">{{ mod.mod_name }}</span>
                                            </li>
                                        </ul>
                                    </div>
                                    <!-- Properties of the selected Mod -->
                                    <div class="right-column border  content-scrollable">
                                    
                                        <!-- Alert Notification -->
                                        <div v-if="showAlert"
                                            class="alert alert-warning alert-dismissible fade show d-flex align-items-center" role="alert">
                                            <i class="bi bi-info-circle"></i> &nbsp;{{ alertMessage }}&nbsp;
                                            <a href="#" class="alert-link" @click="handleDownloadClick">Click Here</a>&nbsp;to install it.
                                            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close" @click="closeAlert"></button>
                                        </div>

                                        <TPModProperties ref="ModProps" @OnValuesChanged="OnValuesChanged" />
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
                                    type="button" data-bs-toggle="tooltip" data-bs-placement="top"
                                    data-bs-title="Mod Information" @mousedown="addNewTheme_Click"
                                    @mouseover="updateStatus('Mod Information')" @mouseleave="clearStatus">
                                    <i class="bi bi-book"></i> Read me
                                </button>
                                <button id="cmdReloadMods" class="btn btn-outline-secondary" type="button"
                                    data-bs-toggle="tooltip" data-bs-placement="top" data-bs-title="Reload Mod List"
                                    @mousedown="editTheme_Click" @mouseover="updateStatus('Reload Mod List')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-arrow-clockwise"></i>
                                </button>
                                <button id="cmdImportMod" class="btn btn-outline-secondary" type="button"
                                    data-bs-toggle="tooltip" data-bs-placement="top" data-bs-title="Import Mod"
                                    @mousedown="exportTheme_Click" @mouseover="updateStatus('Import Mod')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-download"></i>
                                </button>
                                <button id="cmdDeleteMod" class="btn btn-outline-secondary" type="button"
                                    data-bs-toggle="tooltip" data-bs-placement="top" data-bs-title="Delete Mod"
                                    @mousedown="saveTheme_Click" @mouseover="updateStatus('Delete Mod')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                            <div class="btn-group me-3" role="group" aria-label="Themes">
                                <button id="cmdEditJSON" class="btn btn-outline-secondary" type="button"
                                    data-bs-toggle="tooltip" data-bs-placement="top"
                                    data-bs-title="Edit Json Configuration" @mousedown="reloadThemes_Click"
                                    @mouseover="updateStatus('Edit Json Configuration')" @mouseleave="clearStatus">
                                    <i class="bi bi-pencil-square"></i>
                                </button>
                                <button id="cmdEditIni" class="btn btn-outline-secondary" type="button"
                                    data-bs-toggle="tooltip" data-bs-placement="top"
                                    data-bs-title="Edit Ini Configuration" @mousedown="reloadThemes_Click"
                                    @mouseover="updateStatus('Edit Ini Configuration')" @mouseleave="clearStatus">
                                    <i class="bi bi-gear"></i>
                                </button>
                                <button id="cmdEditOpenFolder" class="btn btn-outline-secondary" type="button"
                                    data-bs-toggle="tooltip" data-bs-placement="top"
                                    data-bs-title="Open Container Folder" @mousedown="reloadThemes_Click"
                                    @mouseover="updateStatus('Open Container Folder')" @mouseleave="clearStatus">
                                    <i class="bi bi-folder2-open"></i>
                                </button>
                                <button id="cmdEditReinstall" class="btn btn-outline-secondary" type="button"
                                    data-bs-toggle="tooltip" data-bs-placement="top" data-bs-title="Re-Install Mod"
                                    @mousedown="reloadThemes_Click" @mouseover="updateStatus('Re-Install Mod')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-arrow-repeat"></i>
                                </button>
                            </div>
                            <div class="btn-group me-3" role="group" aria-label="Themes">
                                <button id="cmdThemeExport" class="btn btn-outline-secondary" type="button"
                                    data-bs-toggle="tooltip" data-bs-placement="top" data-bs-title="Export Theme"
                                    @mousedown="reloadThemes_Click" @mouseover="updateStatus('Export Theme')"
                                    @mouseleave="clearStatus">
                                    <i class="bi bi-arrow-bar-up"></i>
                                </button>
                                <button id="cmdThemeImport" class="btn btn-outline-secondary" type="button"
                                    data-bs-toggle="tooltip" data-bs-placement="top" data-bs-title="Import Theme"
                                    @mousedown="reloadThemes_Click" @mouseover="updateStatus('Import Theme')"
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
import Notifications from '../MainWindow/Components/Notifications.vue';
import TPModProperties from './TPProperties.vue';
import Util from '../Helpers/Utils.js';



// Enable Dropdown for the Context Menus:
const dropdownElementList = document.querySelectorAll('.dropdown-toggle');
const dropdownList = [...dropdownElementList].map(dropdownToggleEl => new bootstrap.Dropdown(dropdownToggleEl));

//Enable Tooltips:
const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

const TPMODS_URL = "https://raw.githubusercontent.com/psychicEgg/EDHM/main/Odyssey/3rdPartyMods/mod_list.json";

export default {
    name: 'TPModsManager',
    components: {
        Notifications,
        TPModProperties
    },
    data() {
        return {
            visible: false,
            showSpinner: false,        //<- Flag to Show/hide the Loading Spinner
            statusText: 'Ready.',
            ActiveInstance: null,
            TPmods: [],
            TEMP_FOLDER: '',

            selectedMod: null,
            selectedModBasename: null, // Para rastrear el mod seleccionado
            
            alertMessage: '',
            showAlert: false,
        };
    },
    methods: {

        /** This is the Start Point of the Program **/
        async Initialize() {
            try {
                console.log('Initializing 3PMods Manager..');
                this.showSpinner = true;
                this.statusText = 'Initializing..';
                this.TEMP_FOLDER = await window.api.resolveEnvVariables('$TMPDIR\\EDHM_UI');
                const destFile = window.api.joinPath(this.TEMP_FOLDER, 'tpmods_list.json');
                const modsList = await window.api.downloadAsset(TPMODS_URL, destFile);                  //console.log('Available Mods:', modsList);
                const installedMods = await window.api.GetInstalledTPMods(this.ActiveInstance.path);    //console.log('Installed Mods:', installedMods);
                
                let installConter = 0;
                if (modsList && modsList.length > 0) {
                    this.TPmods = [];
                    modsList.forEach(mod => {
                        const found = installedMods.findIndex((item) => item && item.data.mod_name === mod.mod_name);
                        if (found >= 0) {
                            //- Mod is installed    
                            const fMod = installedMods[found];                    
                            mod.isActive  = true;

                            mod.file_json = fMod.file_json;
                            mod.file_ini = fMod.file_ini;

                            mod.data = fMod.data;
                            mod.data_ini = fMod.data_ini;
                            
                            mod.path = fMod.path;                            
                            installConter++;
                        } else {
                            //- Mod is NOT installed
                            mod.isActive  = false;
                        }
                        this.TPmods.push(mod);
                    });
                }
                this.statusText = installConter + ' Detected Mods';
                //console.log('Detected Mods', this.TPmods);

            } catch (error) {
                console.error(error);
            } finally {
                setTimeout(() => {
                    this.showSpinner = false;
                }, 1000);
            }
        },

        /* Pops up this Component */
        async open(data) {
            this.visible = true;
            this.ActiveInstance = data;
            this.Initialize();
            console.log('Poping TPMods..');
        },
        close() {
            this.visible = false;
        },

        // #region Utility Methods
        
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

        getGrayscaleImage(mod) {
            if (mod.grayscaleBase64) {
                return mod.grayscaleBase64;
            } else if (mod.thumbnail_url) {
                // If grayscaleBase64 isn't available, dynamically generate it
                return this.convertToGrayscale(mod.thumbnail_url, mod);
            } else {
                return null; // Or a placeholder image
            }
        },
        convertToGrayscale(imageUrl, mod) {
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            const img = new Image();

            img.onload = () => {
                canvas.width = img.width;
                canvas.height = img.height;
                ctx.drawImage(img, 0, 0);

                const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                const data = imageData.data;

                for (let i = 0; i < data.length; i += 4) {
                    const avg = (data[i] + data[i + 1] + data[i + 2]) / 3;
                    data[i] = avg; // red
                    data[i + 1] = avg; // green
                    data[i + 2] = avg; // blue
                }

                ctx.putImageData(imageData, 0, 0);
                const grayscaleBase64 = canvas.toDataURL();
                mod.grayscaleBase64 = grayscaleBase64; // store for later use.
                this.$forceUpdate();
            };

            img.src = imageUrl;

            return imageUrl; // return the original url until the conversion is finished.
        },

        // #endregion

        // #region Control Events
        
        async onSelectMod(mod) {
            this.selectedMod = mod;
            this.selectedModBasename = mod.mod_name;
            
            if (mod.isActive) {
                //console.log('Mod seleccionado:', mod);
                this.closeAlert();

                this.$refs.ModProps.OnInitialize(mod);

            } else {
                this.$refs.ModProps.clearProps();
                this.showUpdateAlert('The mod is not Installed,');
            }
            
            this.$emit("mod-selected", mod); //<- Ejemplo de como pasar el objeto mod al padre.
        },
        onRightClick(event, mod) {
            event.preventDefault();
            // Aquí puedes agregar la lógica para el menú contextual
            console.log('Click derecho en mod:', mod);
        },
        async OnValuesChanged(e) {
            //console.log('OnValuesChanged event received.', e);
            this.selectedMod = e;
            const _retJsn = await window.api.writeJsonFile(e.file_json, e.data, true);
            const _retIni = await window.api.SaveIniFile(e.file_ini, e.data_ini);

            console.log('Mod Changes Saved?:', _retJsn, _retIni);
        },
        
        showUpdateAlert(message) {
            this.alertMessage = message;
            this.showAlert = true;
        },
        closeAlert() {
            this.showAlert = false;
        },
        handleDownloadClick(event) {
            event.preventDefault(); // Prevent default link behavior
            // Your download logic here
            console.log('Download link clicked!');
            if (this.selectedMod) {
                console.log(this.selectedMod);
            }
            // ... your download code ...
            this.closeAlert(); // Optional: close the alert after download
        },
        
        // #endregion
    },
    mounted() {
        /* LISTENING EVENTS:   */
        EventBus.on('On_Initialize3PMods', this.Initialize);
        EventBus.on('open-3PMods', this.open);
    },
    beforeUnmount() {
        // Clean up the event listener:
        EventBus.off('On_Initialize3PMods', this.Initialize);
        EventBus.off('open-3PMods', this.open);
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
  width: 100%;
}

.content-scrollable {
  overflow-y: auto;
  scrollbar-width: thin; /* Firefox */
}
/* For Chrome, Edge, and Safari */
.content-scrollable::-webkit-scrollbar {
  width: 8px; /* Adjust thickness as needed */
}

.left-column {
  width: 31%;
  height: 100%;
  float: left;
}

.right-column {
  width: 69%;
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

ul {
  list-style: none; /* Remove list markers */
  padding: 0; /* Remove padding */
  margin: 0; /* Remove margin */
}

.image-container {
  width: 200px;
  position: relative;
  background-color: transparent;
  color: #f8f9fa;
  padding: 0; /* Remove padding */
  margin: 1px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.image-container:hover .img-thumbnail {
  border-color: #00bfff;
}

.img-thumbnail {
  width: 100%;
  height: auto;
  background-color: transparent;
  margin: 0; /* Remove margin */
  padding: 1; /* Remove padding */
}

.selected .img-thumbnail {
  border: 3px solid #00bfff;
  box-shadow: 0 0 10px #00bfff;
}

.image-label {
  position: absolute;
  bottom: 5px;
  left: 5px;
  background-color: rgba(0, 0, 0, 0.3);
  padding: 1px;
  border-radius: 3px;
  font-size: 0.8em; /* Reduce el tamaño de la fuente */
}
</style>
