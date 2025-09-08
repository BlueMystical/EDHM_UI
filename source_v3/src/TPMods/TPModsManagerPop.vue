<template class="h-100">
    <div v-show="visible" class="modal fade show" style="display: block;" data-bs-theme="dark">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content bg-dark text-light">
                <div class="modal-header">
                    <h5 class="modal-title">TPMods Manager</h5>
                    <button type="button" class="btn-close btn-close-white" aria-label="Close" @click="close"></button>
                </div>
                <div class="modal-body" style="height: 600px;">

                    <!-- Main Content -->
                    <div class="container-fluid h-100">

                        <div class="row h-100">
                            <div class="col h-100">
                                <div class="row h-100">

                                    <!-- List of available Mods -->
                                    <div class="left-column border content-scrollable">



                                        <div class="accordion accordion-flush" id="thumbnailAccordion" >
                                            <div v-for="(mod, index) in TPmods" :key="mod.mod_name" class="accordion-item" data-bs-toggle="true">
                                                <h2 class="accordion-header" :id="'heading-' + index">
                                                    <button class="accordion-button"
                                                        :class="{ 'collapsed': index !== 0 }" type="button"
                                                        data-bs-toggle="collapse" :data-bs-target="'#collapse-' + index"
                                                        @click="onSelectMod(mod)"
                                                        :aria-expanded="index === 0 ? 'true' : 'false'"
                                                        :aria-controls="'collapse-' + index">
                                                        <img :src="mod.isActive ? mod.thumbnail_url : getGrayscaleImage(mod)"
                                                            :alt="mod.mod_name" class="img-thumbnail"
                                                            :style="{ filter: mod.isActive ? 'none' : 'grayscale(100%)' }"
                                                            aria-label="Thumbnail of {{ mod.mod_name }}" />
                                                        &nbsp;&nbsp;
                                                        <span class="image-label">{{ mod.mod_name }}</span>
                                                    </button>
                                                </h2>
                                                <div :id="'collapse-' + index" class="accordion-collapse collapse"
                                                    :class="{ 'show': index === 0 }"
                                                    :aria-labelledby="'heading-' + index"
                                                    data-bs-parent="#thumbnailAccordion" data-bs-toggle="true">
                                                    <div class="accordion-body">
                                                        <ul>
                                                            <li v-for="child in mod.childs" :key="child.mod_name"
                                                                :id="'mod-' + child.mod_name" class="image-container"
                                                                :class="{ 'selected': child.mod_name === selectedModBasename }"
                                                                @click="onSelectMod(child)"
                                                                @contextmenu="onRightClick($event, child)">                                                                >
                                                                <img :src="child.isActive ? child.thumbnail_url : getGrayscaleImage(child)"
                                                                    :alt="child.mod_name" class="img-thumbnail"
                                                                    :style="{ filter: child.isActive ? 'none' : 'grayscale(100%)' }"
                                                                    aria-label="Thumbnail of {{ child.mod_name }}" />
                                                                &nbsp;&nbsp;
                                                                <span class="image-label">{{ child.mod_name }}</span>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- Properties of the selected Mod -->
                                    <div class="right-column border justify-content-center content-scrollable">

                                        <!-- Progress bar for Mod Installing and Updating -->
                                        <span v-show="showProgressBar" class="progress" role="progressbar"
                                            aria-label="Downloading.." :aria-valuenow="progressValue" aria-valuemin="0"
                                            aria-valuemax="100">
                                            <div class="progress-bar progress-bar-striped progress-bar-animated text-bg-warning"
                                                :style="{ width: progressValue + '%' }">{{ progressText }}</div>
                                        </span>

                                        <!-- Loading Spinner -->
                                        <div v-if="showSpinner"
                                            class="d-flex justify-content-center align-items-center w-100 h-100">
                                            <div class="spinner-border text-warning m-5" role="status">
                                                <span class="visually-hidden">Loading...</span>
                                            </div>
                                        </div>

                                        <!-- Alert to show when the mod is not installed or needs an update -->
                                        <div v-show="showAlert" class="alert alert-dark alert-dismissible" role="alert">
                                            <button type="button" class="btn-close" aria-label="Close"
                                                @click="closeAlert"></button>
                                            <h4 class="alert-heading"><i class="bi bi-info-circle"></i>&nbsp;{{
                                                alert.title }}</h4>
                                            <p>{{ alert.message }}&nbsp;<a href="#" class="link-warning"
                                                    @click="OnModDownload_Click">Click Here</a>&nbsp;to install it.</p>
                                            <hr>
                                            <p v-html="alert.detail" class="mb-0"></p>
                                            <hr>
                                            <h5>Version {{ alert.version }}</h5>
                                            <p v-html="alert.changes" class="mb-0"></p>
                                            <br><img :src="alert.thumbnail" width="200" height="60" alt="...">
                                        </div>

                                        <!-- Alert to show the 'Read Me' information of the selected mod -->
                                        <div v-show="showInfo" class="alert alert-light alert-dismissible" role="alert">
                                            <button type="button" class="btn-close" aria-label="Close"
                                                @click="closeInfo"></button>
                                            <div v-html="infoMessage"></div>
                                        </div>

                                        <!-- The actual Properties of the selected Mod -->
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
        TPModProperties
    },
    data() {
        return {
            visible: false,
            showSpinner: false,        //<- Flag to Show/hide the Loading Spinner            
            statusText: 'Ready.',

            ActiveInstance: null,
            TPmods: [],
            ModsCounter: 0,
            TEMP_FOLDER: '',

            selectedMod: null,
            selectedModBasename: null, // Para rastrear el mod seleccionado
            
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

            showProgressBar: false,    //<- Flag to Show/hide the Progress bar
            download: {
                progressValue: 0,
                progressText: '',
                downloadSpeed: 0.0,
                averageSpeed: 0.0,
                startTime: 0,
                totalDownloadedBytes: 0
            },
            progressListener: null,

        };
    },
    methods: {

        async Initialize() {
            try {
                console.log('Initializing 3PMods Manager..');                
                this.showInfo = false;
                this.showAlert = false;
                this.showSpinner = true;
                this.statusText = 'Initializing..';

                this.TEMP_FOLDER = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\Temp\\EDHM_UI'); //console.log(this.TEMP_FOLDER);
                await window.api.ensureDirectoryExists(this.TEMP_FOLDER);
                const destFile = window.api.joinPath(this.TEMP_FOLDER, 'tpmods_list.json');

                const availableMods = await window.api.downloadAsset(TPMODS_URL, destFile);             //console.log('Available Mods:', modsList);
                const installedMods = await window.api.GetInstalledTPMods(this.ActiveInstance.path);    //console.log('Installed Mods:', installedMods);

                this.TPmods = [];
                this.TPmods = await this.LoadTPMods(availableMods, installedMods); //console.log(this.TPmods);
                this.statusText = this.ModsCounter + ' Detected Mods';

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

        async LoadTPMods(availableMods, installedMods) {
            let _ret = [];
            try {

                this.ModsCounter = 0;
                if (availableMods && availableMods.length > 0) {
                    this.TPmods = [];

                    for (const mod of availableMods) {

                        let listedMod = {
                            mod_name: mod.mod_name,
                            description: mod.description,
                            author: mod.author,
                            mod_version: mod.mod_version,
                            download_url: mod.download_url,
                            thumbnail_url: mod.thumbnail_url,
                            changelog: mod.changelog,

                            file_json: null,
                            file_ini: null,
                            data: null,
                            data_ini: null,

                            path: null,
                            basename: null,
                            isActive: false,
                            isUpdateAvaliable: false,
                            childs: []
                        };

                        if (installedMods && installedMods.mods) {
                            //- Check if the mod is installed:
                            const found = installedMods.mods.findIndex((item) => item && item.data.mod_name === mod.mod_name);
                            if (found >= 0) {
                                //- Mod is installed    
                                const fMod = installedMods.mods[found];
                                fMod.isActive = true;

                                listedMod.isUpdateAvaliable = Util.compareVersions(mod.mod_version, fMod.data.version);
                                listedMod.isActive = true;

                                listedMod.file_json = fMod.file_json;
                                listedMod.file_ini = fMod.file_ini;

                                listedMod.data = await this.applyIniData(fMod.data, fMod.data_ini),
                                    listedMod.data_ini = fMod.data_ini;

                                listedMod.path = fMod.path;
                                listedMod.basename = window.api.getBaseName(fMod.file_json, '.json');

                                this.ModsCounter++;
                            }
                        }

                        _ret.push(listedMod);
                    };

                    //-- Add any non-list mod that is installed:
                    if (installedMods && installedMods.mods) {

                        let prevMod = null;
                        for (const iMod of installedMods.mods) {
                            prevMod = {
                                mod_name: iMod.data.mod_name,
                                description: iMod.data.description,
                                author: iMod.data.author,
                                mod_version: "1.0",
                                download_url: "",
                                thumbnail_url: iMod.file_thumb,
                                isActive: true,
                                file_json: iMod.file_json,
                                file_ini: iMod.file_ini,
                                data: await this.applyIniData(iMod.data, iMod.data_ini),
                                data_ini: iMod.data_ini,
                                path: iMod.path,
                                basename: window.api.getBaseName(iMod.file_json, '.json'),
                                childs: []
                            };

                            //- Check if the mod is a child of the previous one:
                            const found = _ret.findIndex((item) => item && item.path === prevMod.path);
                            if (found >= 0) {
                                //- Mod is installed 
                                if (prevMod.mod_name != _ret[found].mod_name) {
                                    _ret[found].childs.push(prevMod);
                                }
                            } else {
                                _ret.push(prevMod);
                                this.ModsCounter++;
                            }
                        }
                    }
                    if (installedMods && installedMods.errors && installedMods.errors.length > 0) {
                        console.log('There was some errors: ', installedMods.errors);
                        var msg = '';
                        installedMods.errors.forEach(err => {
                            msg += err.msg + '<br>';
                        });
                        EventBus.emit('ShowError', new Error(msg));
                    }
                }

            } catch (error) {
                console.error(error);
                EventBus.emit('ShowError', error);
            }
            return _ret;
        },

        /** Reads the data from the ini file and applies it to the JSON data.
         * @param data Json Data
         * @param ini Vlues from the Ini file         */
        async applyIniData(data, ini) {
            if (data && ini) {
                if (Array.isArray(data.sections)) {
                    for (const section of data.sections) {
                        const ini_sec = section.ini_section;
                        if (section.keys && Array.isArray(section.keys)) {
                            for (const key of section.keys) {
                                try {
                                    const colorKeys = key.key.split('|');  //<- iniKey === "x159|y159|z159" or "x159|y155|z153|w200"                                    

                                    if (Array.isArray(colorKeys) && colorKeys.length > 2) {
                                        //- Multi Key: Colors

                                        let colorComponents = []; //console.log('colorKeys', colorKeys);
                                        for (const [index, rgbKey] of colorKeys.entries()) {
                                            const iniValue = await window.api.getIniKey(ini, ini_sec, rgbKey);  //console.log('rgbKey', rgbKey);
                                            colorComponents.push(iniValue); //<- colorComponents: [ '0.063', '0.7011', '1' ]
                                        }
                                        //console.log('colorComponents:', colorComponents);
                                        if (colorComponents != undefined && !colorComponents.includes(undefined)) {
                                            const color = Util.reverseGammaCorrectedList(colorComponents); //<- color: { r: 81, g: 220, b: 255, a: 255 }
                                            //console.log('color', color);
                                            key.value = parseFloat(Util.rgbaToInt(color).toFixed(1)); //console.log('value', key.value);
                                        }
                                    } else {
                                        //- Single Key: Text, Numbers, etc.
                                        key.value = await window.api.getIniKey(ini, ini_sec, key.key);
                                    }
                                } catch (error) {
                                    console.log(error);
                                }
                            }
                        }
                    }
                }
            }
            return data;
        },
        
        // #region Utility Methods

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

        /** Replaces the <image> tags with standard tags and fix the url of the image. 
         * @param htmlString Original HTML string
         * @param imageBasePath Path where the image should be located         */
        convertImageTags(htmlString, imageBasePath) {
            const customImageRegex = /<image=([^;>]+)(;size=([^;>]+))?(;align=([^>]+))?>/g;
            const standardImageRegex = /<img[^>]*>/g; // Matches standard <img> tags

            // Process custom <image> tags
            let modifiedHtml = htmlString.replace(customImageRegex, (match, filename, sizeMatch, size, alignMatch, align) => {
                const imagePath = window.api.joinPath(imageBasePath, 'assets', filename);
                let imgTag = `<img src="file://${imagePath}"`;

                if (size) {
                    const [width, height] = size.split(',');
                    imgTag += ` width="${width}" height="${height}"`;
                }

                if (align) {
                    imgTag += ` style="vertical-align: ${align};"`;
                }

                imgTag += '>';
                return imgTag;
            });

            // Process standard <img> tags (optional: you can modify them if needed)
            // If you don't want to modify them, you can remove this part.
            modifiedHtml = modifiedHtml.replace(standardImageRegex, (match) => {
                return match; // Return the original <img> tag
            });

            return modifiedHtml;
        },

        // #endregion

        // #region Control Events
        
        async onSelectMod(mod) {
            this.selectedMod = mod;
            this.selectedModBasename = mod.mod_name;
            
            if (mod.isActive) {
                this.showInfo = false;
                this.closeAlert();

                if (mod.isUpdateAvaliable) {
                    this.showUpdateAlert('Update Available,', this.selectedMod );
                }
                else {
                    this.$refs.ModProps.OnInitialize(mod);
                }  
                console.log('Ini:', mod.data_ini);             

            } else {
                this.$refs.ModProps.clearProps();
                this.showUpdateAlert('The mod is not Installed,', this.selectedMod );
            }
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
            let _retIni = true;
            if (e.data.mod_type === 'INIConfig' ) {
                _retIni = await window.api.SaveIniFile(e.file_ini, e.data_ini);
            }
            console.log('Mod Changes Saved?:', _retJsn, _retIni);
        },

        showUpdateAlert(message, pModData) {
            this.alert = {
                title:      pModData.mod_name,
                message:    message,
                detail:     pModData.description,
                changes:    pModData.changelog,
                version:    pModData.mod_version,
                thumbnail:  pModData.thumbnail_url
            };
            this.showInfo = false;
            this.showAlert = true;
        },
        closeAlert() {
            this.showAlert = false;
        },
        closeInfo() {
            this.showInfo = false;
        },  


        // #endregion

        // #region TOOLBAR BUTTON EVENTS
        
        cmdReadMe_Click(e){
            if (this.selectedMod && this.selectedMod.data.read_me) {  
                this.showAlert = false;
                this.showInfo = true; console.log('showInfo',this.showInfo );
                this.infoMessage = this.convertImageTags(this.selectedMod.data.read_me, this.selectedMod.path);              
            }
        },
        cmdReloadMods_Click(e) {
            this.Initialize();
        },
        async cmdImportMod_Click(e) {
            try {
                const homeDir = await window.api.resolveEnvVariables('%USERPROFILE%');
                const LastFolderUsed = await window.api.readSetting('LastFolderUsed', homeDir);
                const options = {
                    title: 'Select a Mod ZIP file:',
                    filters: [
                        { name: 'Zip Files', extensions: ['zip'] },
                        { name: 'All Files', extensions: ['*'] },
                    ],
                    defaultPath: LastFolderUsed,
                    properties: ['openFile', 'showHiddenFiles', 'dontAddToRecent']
                };

                const filePath = await window.api.ShowOpenDialog(options); //console.log(filePath)
                if (filePath && filePath.length > 0) {

                    await window.api.writeSetting('LastFolderUsed', window.api.getParentFolder(filePath[0]));
                    const folderPath = filePath[0];
                    const gamePath = this.ActiveInstance.path;

                    //- Unzip the file:
                    await window.api.decompressFile(folderPath, gamePath);
                    await this.Initialize(); //<- Re-load the mods list
                    this.statusText = 'Mod Installed.';
                }
            } catch (error) {
                console.error('Error during file import:', error);
                EventBus.emit('ShowError', error);
            }
        },
        async cmdDeleteMod_Click(e) {
            if (this.selectedMod && this.selectedMod.isActive) {
                console.log(this.selectedMod);
                const options = {
                    type: 'warning',
                    buttons: ['Cancel', 'Yes, Delete it', 'No, keep it'],
                    defaultId: 1,
                    title: 'Delete : ' + this.selectedMod.mod_name,
                    message: 'Are you sure you want to delete this mod?',
                    cancelId: 0,
                };

                try {
                    const result = await window.api.ShowMessageBox(options);
                    if (result && result.response === 1) {
                        const gamePath = window.api.joinPath(this.ActiveInstance.path, 'EDHM-ini', '3rdPartyMods');

                        if (this.selectedMod.path !== gamePath) {
                            //- Delete Mod's Folder:
                            console.log('Delete Mod Folder:', this.selectedMod.path);
                            await window.api.deleteFolderRecursive(this.selectedMod.path);
                        } else {
                            //- Delete all mod's files:
                            const deleteCommand = window.api.joinPath(this.selectedMod.path, this.selectedMod.basename + '.*');
                            console.log('Deleting all mod files:', deleteCommand);
                            await window.api.deleteFilesByWildcard(deleteCommand); 
                        }

                        //- Delete all dependencies: 
                        console.log('Deleting dependencies for mod:', this.selectedMod.mod_name);
                        if (this.selectedMod.data.dependencies && this.selectedMod.data.dependencies.length > 0) {                               
                            for (const dep of this.selectedMod.data.dependencies) {
                                const depPath = window.api.joinPath(this.ActiveInstance.path, dep);
                                console.log('Deleting dependency:', depPath);
                                await window.api.deleteFileByAbsolutePath(depPath);
                            }
                        }

                        //- Remove the mod from the list:
                        try { this.$refs.ModProps.clearProps(); } catch {}
                        this.selectedMod = null;
                        this.selectedModBasename = null;
                        
                        this.Initialize(); //<- Reload mod list
                    }
                } catch (error) {
                    console.error(error);
                    EventBus.emit('ShowError', error);
                }
            }
        },
        cmdEditJSON_Click(e) {
            if (this.selectedMod && this.selectedMod.data) {
                window.api.openFile(this.selectedMod.file_json);
            }
        },
        cmdEditIni_Click(e) {
            if (this.selectedMod && this.selectedMod.data) {
                window.api.openFile(this.selectedMod.file_ini);
            }
        },
        cmdEditOpenFolder_Click(e) {
            if (this.selectedMod && this.selectedMod.data) {
                const folder = window.api.getParentFolder(this.selectedMod.file_json);
                window.api.openPathInExplorer(folder);
            }
        },
        async cmdEditReinstall_Click(e) {
            if (this.selectedMod && this.selectedMod.data) {
                this.showSpinner = true;
                const fileName = Util.trimAllSpaces(this.selectedMod.mod_name) + '_v' + this.selectedMod.mod_version + '.zip';
                const fileSavePath = window.api.joinPath(this.TEMP_FOLDER, fileName);

                await this.DownloadAndInstallUpdate({ 
                    url:        this.selectedMod.download_url, 
                    game_path:  this.ActiveInstance.path,
                    save_to:    fileSavePath,
                    mod_name:   fileName              
                });
            }
        },
        cmdThemeExport_Click(e) {

        },
        cmdThemeImport_Click(e) {

        },

        // #endregion

        // #region Mod Updates - Downloads

        async OnModDownload_Click(event) {
            event.preventDefault(); // Prevent default link behavior
            if (this.selectedMod) {
                //console.log(this.selectedMod);                
                this.showSpinner = true;
                this.closeAlert(); 

                const fileName = Util.trimAllSpaces(this.selectedMod.mod_name) + '_v' + this.selectedMod.mod_version + '.zip';
                const fileSavePath = window.api.joinPath(this.TEMP_FOLDER, fileName);

                await this.DownloadAndInstallUpdate({ 
                    url:        this.selectedMod.download_url, 
                    game_path:  this.ActiveInstance.path,
                    save_to:    fileSavePath,
                    mod_name:   fileName                 
                });
            }            
        },
        
        async DownloadAndInstallUpdate(Options) {
            try {
                //console.log('Downloading file:', Options);
                this.statusText = 'Downloading..';

                let filePath = Options.save_to;
                if (!filePath) return;

                const destDir = await window.api.getParentFolder(filePath);
                await window.api.ensureDirectoryExists(destDir);
                await window.api.deleteFilesByType(destDir, '.zip');

                //- Setup Progress Control Variables:
                this.showProgressBar = true;  //<- Shows/Hides the Progressbar
                this.download.progressValue = 0;       //<- Progress Value in Percentage %
                this.download.downloadSpeed = 0;       //<- Download Speend in bytes/s
                this.download.averageSpeed = 0;        //<- Average Download Speend in KB/s
                this.download.totalDownloadedBytes = 0; //<- Bytes Downloaded
                this.download.startTime = Date.now();

                //- Setup a Progress Listener
                this.progressListener = (event, data) => {
                    //- This will fillup the Progress Bar:          
                    this.download.downloadSpeed = data.speed;
                    this.download.progressValue = data.progress;
                    this.download.totalDownloadedBytes += data.speed;
                    this.download.progressText = `${data.progress.toFixed(1)}%`;
                };

                //- Start the Progress Listener:
                window.api.onDownloadProgress(this.progressListener);

                //- Start the Download and wait till it finishes..
                await window.api.downloadFile(Options.url, filePath);

                //- When the Download Finishes: 
                console.log('Download complete!', filePath);
                this.statusText = 'Installing..';

                //- Some Cleanup:
                window.api.removeDownloadProgressListener(this.progressListener);

                //- Unzip the file:
                await window.api.decompressFile(filePath, Options.game_path);
                await this.Initialize(); //<- Re-load the mods list
                this.showSpinner = false;
                this.showProgressBar = false;
                this.statusText = 'Mod Installed.';
                this.selectedMod = null;
                this.selectedModBasename = null;

                EventBus.emit('RoastMe', { type: 'Accent', accent: 'warning', background: 'success', title: 'Success:',
                    message: `The plugin '${Options.mod_name}' is installed!<br>You may need to re-start the game.` });

            } catch (error) {
                console.error('Download failed:', error);
                this.showProgressBar = false;
                this.showSpinner = false;
                window.api.removeDownloadProgressListener(this.progressListener);
                EventBus.emit('ShowError', new Error(error.message + error.stack));
            }
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



.accordion-header-thumbnail,
.list-group-thumbnail,


.accordion-header-thumbnail {
  vertical-align: middle;
  margin-right: 0px !important;
}
.accordion-button {
  padding: 0rem 0; 
}
.accordion-button:not(.collapsed) {
  padding: 0rem 0rem;
}

.left-column {
  width: 35%;
  height: 100%;
  float: left;
}
.right-column {
  width: 65%;
  height: 100%;
  float: left;
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

.progress {
  margin-top: 10px;
  margin-bottom: 4px;
}

ul {
  list-style: none; /* Remove list markers */
  padding: 0; /* Remove padding */
  margin: 0; /* Remove margin */
}

.image-container {
  width: 180px;
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
  padding: 0; /* Remove padding */
  border-radius: 1px;
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
  border-radius: 1px;
  font-size: 0.8em; /* Reduce el tamaño de la fuente */
}
</style>
