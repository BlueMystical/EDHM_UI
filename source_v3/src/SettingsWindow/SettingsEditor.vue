<template>
    <div class="container-fluid bg-dark text-light p-3" data-bs-theme="dark">
        <!-- Nav tabs -->
        <ul class="nav nav-tabs" id="settingsTabs" role="tablist">
            <li class="nav-item" role="presentation">
                <button class="nav-link active" id="general-tab" data-bs-toggle="tab" data-bs-target="#general"
                    type="button" role="tab" aria-controls="general" aria-selected="true">
                    General Settings
                </button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link" id="graphics-tab" data-bs-toggle="tab" data-bs-target="#graphics" type="button"
                    role="tab" aria-controls="graphics" aria-selected="false">
                    Graphic Settings
                </button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link" id="hud-tab" data-bs-toggle="tab" data-bs-target="#hud" type="button"
                    role="tab" aria-controls="hud" aria-selected="false">
                    HUD Settings
                </button>
            </li>
        </ul>

        <!-- Tab panes -->
        <div class="tab-content mt-3">
            <!-- General Settings -->
            <div class="tab-pane fade show active" id="general" role="tabpanel" aria-labelledby="general-tab">
                <!-- CONTENIDO ORIGINAL -->
                <div class="input-group mb-3">
                    <!-- List of Game Publishers -->
                    <div class="form-floating">
                        <select class="form-select" id="cboGamePublisher" aria-label="Game Publisher:"
                            v-model="selectedPublisher" @change="OnGamePublisherChange">
                            <option v-for="(publisher, index) in publishers" :key="index" :value="index">
                                {{ publisher.instance }}
                            </option>
                        </select>
                        <label for="cboGamePublisher">Game Publisher:</label>
                    </div>
                    <!-- List of Game Versions -->
                    <div class="form-floating">
                        <select class="form-select" id="cboGameVersion" aria-label="Game Version:"
                            v-model="selectedVersion" @change="OnGameVersionChange">
                            <option v-for="(version, index) in versions" :key="index" :value="index">
                                {{ version.name }}
                            </option>
                        </select>
                        <label for="cboGameVersion">Game Version:</label>
                    </div>
                </div>

                <!-- Game Path Box -->
                <label for="txtFullGamePath" class="form-label">Full path to the Game's Executable:</label>
                <div class="input-group mb-3">
                    <input type="text" class="form-control form-control-sm"
                        placeholder="Manually select the game location or use the Localization Wizard below"
                        aria-label="Pick a location for " id="txtFullGamePath" v-model="selectedGamePath"
                        @change="OnGamePathChange" @input="OnGamePathChange" />
                    <button class="btn btn-outline-secondary" type="button" @click="browseGamePath()">
                        Browse
                    </button>
                </div>

                <hr />

                <!-- Other Settings as Card -->
                <div class="card text-light mb-3">
                    <div class="card-header">
                        <h5 class="mb-0">Other Settings</h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col">
                                <label for="playerJournal">Player's Journal Location:</label>
                                <div id="playerJournal" class="input-group mb-3">
                                    <input type="text" class="form-control form-control-sm"
                                        placeholder="Pick a Location" aria-label="Pick a Location"
                                        aria-describedby="button-addon2" v-model="config.PlayerJournal" />
                                    <button class="btn btn-outline-secondary" type="button" id="button-addon2"
                                        @click="browseJournalFolder">
                                        Browse
                                    </button>
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col">
                                <label for="quantity">Saves To Remember:</label>
                                <input type="number" class="form-control" id="quantity" min="1" max="50"
                                    v-model="config.SavesToRemember" />
                            </div>
                            <div class="col">
                                <label for="userDataLocation">Themes & User's Data:</label>
                                <div id="userDataLocation" class="input-group mb-3" disabled>
                                    <input type="text" class="form-control form-control-sm"
                                        placeholder="Pick a Location" aria-label="Pick a Location"
                                        aria-describedby="button-addon2" v-model="config.UserDataFolder" />
                                    <button class="btn btn-outline-secondary" type="button" id="button-addon2"
                                        @click="browseUserDataFolder">
                                        Browse
                                    </button>
                                </div>
                            </div>
                        </div>

                        <!-- Custom Icon + Tray Options + Preview -->
                        <div class="row">
                            <!-- Columna izquierda: Custom Icon input -->
                            <div class="col">
                                <label for="customIconPath">Custom Icon:</label>
                                <div id="customIconPath" class="input-group mb-3" disabled>
                                    <input type="text" class="form-control form-control-sm"
                                        placeholder="Pick a Location" aria-label="Pick a Location"
                                        aria-describedby="button-addon4" v-model="config.CustomIcon" />
                                    <button class="btn btn-outline-secondary" type="button" id="button-addon4"
                                        @click="browseCustomIcon">
                                        Browse
                                    </button>
                                </div>
                            </div>

                            <!-- Columna derecha: switches + preview -->
                            <div class="col d-flex flex-column align-items-start">
                                <div class="form-check form-switch mb-2">
                                    <input class="form-check-input" type="checkbox" role="switch"
                                        id="flexSwitchCheckTray" v-model="config.HideToTray" />
                                    <label class="form-check-label" for="flexSwitchCheckTray">
                                        Hide to Tray on close
                                    </label>
                                </div>

                                <div class="form-check form-switch mb-2">
                                    <input class="form-check-input" type="checkbox" role="switch"
                                        id="flexSwitchStartMinimized" v-model="config.StartMinimizedToTray" />
                                    <label class="form-check-label" for="flexSwitchStartMinimized">
                                        Start minimized to Tray
                                    </label>
                                </div>

                                <img :src="config.CustomIcon" class="img-thumbnail" alt="..." width="64" height="64" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Footer buttons -->
                <div class="mt-4">
                    <div class="btn-group" role="group" aria-label="Default button group">
                        <button type="button" class="btn btn-outline-secondary" @click="CleanInstall">
                            Clean Install
                        </button>
                        <button type="button" class="btn btn-success" @click="runGameLocationAssistant">
                            Game Localization Wizard
                        </button>
                        <button type="button" class="btn btn-primary" @click="save">
                            Save Changes
                        </button>
                    </div>
                </div>
            </div>

            <!-- Graphic Settings -->
            <div class="tab-pane fade" id="graphics" role="tabpanel" aria-labelledby="graphics-tab">
                <div class="card text-light mb-3">
                    <div class="card-body">
                        <!-- Checkboxes -->
                        <div class="form-check" v-for="(label, key) in gpuCheckboxes" :key="key">
                            <input class="form-check-input" type="checkbox" :id="key" v-model="config[key]"
                                :disabled="!config.GpuAcceleration && (key === 'GpuSmoothScrolling')" />
                            <label class="form-check-label" :for="key">{{ label }}</label>
                        </div>

                        <!-- Renderer Select -->
                        <div class="mt-3">
                            <label for="gpuRenderer" class="form-label">GPU Renderer:</label>
                            <select id="gpuRenderer" class="form-select" v-model="config.GpuRenderer"
                                :disabled="!config.GpuAcceleration">
                                <option value="Vulkan">Vulkan</option>
                                <option value="OpenGL">OpenGL</option>
                                <option value="Direct3D">Direct3D</option>
                            </select>
                        </div>

                        <!-- Font Size Select -->
                        <div class="mt-3">
                            <label for="fontSize" class="form-label">Font Size:</label>
                            <select id="fontSize" class="form-select" v-model="config.FontSize">
                                <option value="8px">8 px</option>
                                <option value="12px">12 px</option>
                                <option value="14px">14 px</option>
                                <option value="16px">16 px</option>
                                <option value="18px">18 px</option>
                                <option value="20px">20 px</option>
                                <option value="24px">24 px</option>
                                <option value="26px">26 px</option>
                                <option value="28px">28 px</option>
                            </select>
                        </div>

                         <!-- UI Scale Slider -->
                        <div class="mt-3">
                            <label for="uiScale" class="form-label">UI Scale (0 = Automatic):</label>
                            <input type="range" class="form-range" id="uiScale" min="0" max="3" step="0.25"
                                v-model.number="config.UiScaleFactor" @input="onUiScaleChange" />
                            <div class="small text-muted">
                                Value: {{ config.UiScaleFactor === 0 ? 'Automatic' : config.UiScaleFactor }}
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- HUD Settings -->
            <div class="tab-pane fade" id="hud" role="tabpanel" aria-labelledby="hud-tab">
                <div class="card text-light mb-3">
                    <div class="card-header">
                        <h5 class="mb-0">HUD Settings</h5>
                    </div>
                    <div class="card-body">
                        <p>Soon.</p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import EventBus from '../EventBus';
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap';

export default {
    name: 'SettingsEditor',
    props: {
        initData: { type: Object, default: () => ({}) }
    },
    data() {
        return {
            visible: false,
            config: {},
            ActiveInstance: {},
            selectedPublisher: 0,
            selectedVersion: 0,
            selectedGamePath: '',
            publishers: [],
            versions: [],
            DATA_DIRECTORY: '',
            gpuCheckboxes: {
                GpuSmoothScrolling: 'Smooth Scrolling',
                GpuAcceleration: 'Hardware Acceleration'
            }
        };
    },
    created() {
    },
    methods: {


        async Initialize() {
            try {
                if (this.config) {
                    console.log('Settings Loaded:', this.config);
                    // Ensure Default Values are set:
                    if (!this.config.CustomIcon) {
                        this.config.CustomIcon = await window.api.getAssetPath('images/Icon_v3_a0.ico');
                    }
                    this.DATA_DIRECTORY = await window.api.resolveEnvVariables(this.config.UserDataFolder); //console.log('DATA_DIRECTORY:', DATA_DIRECTORY);

                    const instanceName = this.config.ActiveInstance; //<- "Steam (Odyssey (Live))"
                    const pubName = instanceName.split('(')[0];     //<- "Steam "
                    this.ActiveInstance = this.config.GameInstances
                        .flatMap(instance => instance.games)
                        .find(game => game.instance === instanceName);
                    console.log('ActiveInstance:', instanceName);

                    this.selectedGamePath = this.ActiveInstance.path;
                    this.selectedVersion = this.getGameVersionIndex(this.ActiveInstance.name);
                    this.selectedPublisher = this.getGameInstanceIndex(pubName);
                    this.publishers = this.config.GameInstances;
                    this.loadVersions();
                }
            } catch (error) {
                console.log(error);
            }
        },
        /* Button Click: Save the Settings */
        async save() {

            let selected = this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion];
            //- Sets the path for the Active instance:
            if (selected.path != this.selectedGamePath) {
                selected.path = this.selectedGamePath;
            }

            //- Sets the Active Instance:
            this.config.ActiveInstance = selected.instance;

            window.api.events.sendEvent('SettingsChanged', JSON.parse(JSON.stringify(this.config))); //<- this event will be heard in 'App.vue'  
            window.api.settings.close();
        },

        loadVersions() {
            const selectedPublisherData = this.config.GameInstances[this.selectedPublisher];
            if (selectedPublisherData) {
                this.versions = selectedPublisherData.games;
                if (this.versions.length > 0) {
                    this.selectedVersion = 0;
                }
            } else {
                this.versions = [];
            }
        },

        OnGamePublisherChange(e) {
            const publisher = this.config.GameInstances[this.selectedPublisher];    //console.log(publisher);
            if (publisher) {
                this.ActiveInstance = publisher.games[0]; //console.log(this.ActiveInstance);
                this.config.ActiveInstance = this.ActiveInstance.instance;
                this.selectedGamePath = this.ActiveInstance.path;
                this.loadVersions();
            }
        },
        OnGameVersionChange(e) {
            const publisher = this.config.GameInstances[this.selectedPublisher];
            if (publisher) {
                this.ActiveInstance = publisher.games[this.selectedVersion];
                this.config.ActiveInstance = this.ActiveInstance.instance;
                this.selectedGamePath = this.ActiveInstance.path;
            }
        },
        async OnGamePathChange(e) {
            console.log('Selected Game Path:', this.selectedGamePath);
            // Check if EliteDangerous64.exe exists in the selected folder
            const filesInFolder = await window.api.readDirectory(this.selectedGamePath);
            const hasEliteExe = filesInFolder.includes('EliteDangerous64.exe');

            if (hasEliteExe) {
                console.log('[GamePath] Valid path selected:', this.selectedGamePath);
            } else {
                console.warn('[GamePath] EliteDangerous64.exe not found in selected folder:', this.selectedGamePath);
                window.api.events.sendEvent('RoastMe', {
                    type: 'Error',
                    message: 'EliteDangerous64.exe not found in the selected folder.<br>You can do it manually in the Game Instances...<br>or just click the Green Button.',
                    delay: 10000
                });
                return;
            }

            this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].path = this.selectedGamePath;
            this.config.ActiveInstance = this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].instance;

            console.log('ActiveInstance:', this.config.ActiveInstance);
            try {
                await window.api.terminateProgram('EliteDangerous64.exe');
            } catch (error) {
                console.error('Error:', error);
            }
        },

        onUiScaleChange() {
            // Broadcast al proceso principal o a otras ventanas
            window.api.settings.sendUiScale(this.config.UiScaleFactor);
        },

        getGameInstanceIndex(name) {
            switch (name.trim()) {
                case 'Steam': return 0;
                case 'Epic Games': return 1;
                case 'Frontier': return 2;
                default: return -1;
            }
        },
        getGameVersionIndex(name) {
            switch (name.trim()) {
                case 'Odyssey (Live)': return 0;
                case 'Horizons (Live)': return 1;
                case 'Horizons (Legacy)': return 2;
                default: return -1;
            }
        },

        /* Manually Browse for the Game Executable */
        async browseGamePath(params) {
            const platform = await window.api.getPlatform();
            const winDir = await window.api.resolveEnvVariables('%PROGRAMFILES%');
            const linuxDir = await window.api.resolveEnvVariables('%USERPROFILE%');
            const defaultLocation = this.selectedGamePath ? this.selectedGamePath :
                platform === 'win32' ? winDir : linuxDir;

            const options = {
                title: 'Select the Game Executable',
                defaultPath: defaultLocation,
                filters: [
                    { name: 'Game Executable', extensions: ['exe'] }
                ],
                properties: ['openFile', 'showHiddenFiles', 'dontAddToRecent'],
                message: 'Select the Game Executable',
            };

            const filePath = await window.api.ShowOpenDialog(options);

            if (filePath && filePath.length > 0) {
                const selectedFile = filePath[0];
                const parentFolder = await window.api.getParentFolder(selectedFile);
                this.selectedGamePath = parentFolder;

                this.OnGamePathChange(null); // Trigger the change event to update the config

            } else {
                console.log('[GamePath] No file selected.');
            }
        },
        /* Browse for the location to store User's data and Themes */
        async browseUserDataFolder() {
            try {
                const DATA_DIRECTORY = await window.api.resolveEnvVariables(this.config.UserDataFolder); //<- %USERPROFILE%\EDHM_UI
                const options = {
                    title: 'Select Where to Store User Data',
                    defaultPath: DATA_DIRECTORY,
                    properties: ['openDirectory', 'createDirectory', 'promptToCreate', 'dontAddToRecent'],
                    message: 'Select Where to Store User Data',
                };
                const filePath = await window.api.ShowOpenDialog(options);
                if (filePath) {
                    this.config.UserDataFolder = filePath[0];
                    if (DATA_DIRECTORY != this.config.UserDataFolder) {
                        const PrimeSettings = { DataFolder: this.config.UserDataFolder };
                        const primaryPath = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\EDHM-UI-V3');
                        await window.api.ensureDirectoryExists(primaryPath);
                        await window.api.writeJsonFile(window.api.joinPath(primaryPath, 'Settings.json'), PrimeSettings, true);
                        await window.api.copyDirectory(DATA_DIRECTORY, this.config.UserDataFolder);
                        if (await window.api.fileExists(window.api.joinPath(this.config.UserDataFolder, 'Settings.json'))) {
                            console.log('Primary Settings updated at:', this.config.UserDataFolder);
                        }
                        window.api.events.sendEvent('RoastMe', { type: 'Info', message: 'You might want to restart the App for changes to take effect.', delay: 10000 });
                    }
                }
            } catch (error) {
                console.error('Error browsing user data folder:', error);
                window.api.events.sendEvent('ShowError', error);
            }
        },
        /* Browse for the location where the ED Player Journal is located */
        async browseJournalFolder() {
            var defaultLocation = this.config.PlayerJournal ? this.config.PlayerJournal :
                '%USERPROFILE%\\Saved Games\\Frontier Developments\\Elite Dangerous';
            defaultLocation = await window.api.resolveEnvVariables(defaultLocation); //<- '%USERPROFILE%\\Saved Games\\Frontier Developments\\Elite Dangerous'
            console.log('defaultLocation:', defaultLocation);

            const options = {
                title: 'Select Where Game Stores Journal Files',
                defaultPath: defaultLocation,
                properties: ['openDirectory', 'createDirectory', 'promptToCreate', 'dontAddToRecent'],
                message: 'Select Where Game Stores Journal Files',
                filters: null // No specific filters for directories
            };
            const filePath = await window.api.ShowOpenDialog(options);
            if (filePath) {
                this.config.PlayerJournal = filePath[0];
            }
        },

        /* Browse for the location of a Custom Icon for the App */
        async browseCustomIcon() {
            //const DefaultLocation = await window.api.getAssetPath('images/Icon_v3_a0.ico');
            const DATA_DIRECTORY = await window.api.resolveEnvVariables(this.config.UserDataFolder); //console.log('DATA_DIRECTORY:', DATA_DIRECTORY);
            const DefaultLocation = window.api.joinPath(DATA_DIRECTORY, 'images');//console.log('DefaultLocation:', DefaultLocation);
            const options = {
                title: 'Select a Custom Icon',
                defaultPath: DefaultLocation,
                properties: ['openFile', 'showHiddenFiles', 'createDirectory', 'dontAddToRecent'],
                message: 'Select a Custom Icon',
            };
            const filePath = await window.api.ShowOpenDialog(options);
            if (filePath) {
                this.config.CustomIcon = filePath[0];
            }
        },

        /* Cleans html tags */
        sanitizeId(id) {
            return id.replace(/\s/g, '');
        },
        async InstallGameInstance(FolderPath) {
            try {
                await window.api.terminateProgram('EliteDangerous64.exe');
            } catch { }
        },

        /* Attempts to Detect the running Game Process and then sets the Paths */
        async runGameLocationAssistant() {
            window.api.events.sendEvent('RoastMe', { type: 'Info', message: 'Waiting for Game to Start...<br>Leave the game running at menus and return here.', delay: 10000, autoHide: false });

            // 1. Check if the Game is already Running:
            const fullPath = await window.api.detectProgram('EliteDangerous64.exe');

            if (fullPath) {
                console.log('Process found at:', fullPath);
                window.api.events.sendEvent('RoastMe', { type: 'Success', message: `Process found!<br>Game will now Close<br>Don't Panic..` });

                await window.api.terminateProgram('EliteDangerous64.exe');
                const FolderPath = await window.api.getParentFolder(fullPath);

                this.selectedGamePath = FolderPath; console.log('selectedGamePath', this.selectedGamePath);
                console.log('Selected Game Path:', this.selectedGamePath);

                this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].path = this.selectedGamePath;
                this.config.ActiveInstance = this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].instance;

                console.log('ActiveInstance:', this.config.ActiveInstance);
                this.InstallGameInstance(this.selectedGamePath);

            } else {
                console.log('Process not found.');
                window.api.events.sendEvent('RoastMe', { type: 'Info', message: 'Waiting for Game to Start...<br>Leave the game running at menus and return here.' });

                window.api.startMonitoring('EliteDangerous64.exe');

                // Event listener for program detection
                window.api.onProgramDetected(async (event, exePath) => {
                    console.log(`Executable Path: ${exePath}`);
                    window.api.events.sendEvent('RoastMe', { type: 'Success', message: `Process found!<br>Game will now Close<br>Don't Panic..` });

                    await window.api.terminateProgram('EliteDangerous64.exe');
                    const FolderPath = await window.api.getParentFolder(exePath);

                    //this.addNewGameInstance(String(FolderPath));
                    this.selectedGamePath = FolderPath; console.log(this.selectedGamePath);
                    console.log('Selected Game Path:', this.selectedGamePath);

                    this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].path = this.selectedGamePath;
                    this.config.ActiveInstance = this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].instance;

                    console.log('ActiveInstance:', this.config.ActiveInstance);
                    this.InstallGameInstance(this.selectedGamePath);
                });
            }
        },

        /** Button Click: Clean Install
         * Deletes the Settings File for a Clean Install         */
        async CleanInstall() {
            try {
                const options = {
                    type: 'warning', //<- none, info, error, question, warning
                    buttons: ['Cancel', "Yes, I'm Sure", 'No! i was just checking what this button does..'],
                    defaultId: 1,
                    title: 'Question',
                    message: 'Do you want to proceed?',
                    detail: "This will wipe all the settings to their default state.",
                    cancelId: 0,
                };
                const result = await window.api.ShowMessageBox(options); console.log(result);
                if (result.response === 1) {
                    const FilePath = await window.api.joinPath(this.config.UserDataFolder, 'Settings.json');
                    const ResolvedPath = await window.api.resolveEnvVariables(FilePath);
                    const _ret = await window.api.deleteFileByAbsolutePath(ResolvedPath);
                    if (_ret) {
                        window.api.events.sendEvent('RoastMe', { type: 'Success', message: 'EDHM Settings got wiped!<br>You should re-start the App now.' });
                    }
                }
            } catch (error) {
                window.api.events.sendEvent('ShowError', error);
            }
        },

        async checkInstall(InstallStatus) {
            this.visible = true
            if (InstallStatus === 'existingInstall') {
                this.config = await window.api.getSettings()
                this.Initialize()
            } else {
                // FRESH INSTALL:
                this.config = await window.api.getDefaultSettings();
                this.Initialize();
                setTimeout(() => {
                    window.api.events.sendEvent('RoastMe', { type: 'Info', message: 'You can do it manually in the Game Instances..<br> or just click the Green Button.', delay: 10000 });
                }, 2000);
            }
            
            if (typeof this.config.UiScaleFactor === 'undefined') {
                this.config.UiScaleFactor = 0;
            }
            console.log('uiScaleFactor:', this.config.UiScaleFactor)
        }
    },
    mounted() {
        window.api.settings.onInit((InstallStatus) => {
            console.log('initData@SettingsEditor.vue', InstallStatus);
            this.checkInstall(InstallStatus);
        })
    },
    beforeDestroy() {
        EventBus.off('open-settings-editor', this.open);
    }
}
</script>

<style scoped>
.modal-content {
    background-color: #343a40;
    color: #f8f9fa;
}

.accordion-button {
    background-color: #343a40;
    color: #f8f9fa;
}

.accordion-body {
    background-color: #343a40;
    color: #f8f9fa;
}

.form-control {
    background-color: #222;
    color: #f8f9fa;
    border-color: #666;
}
</style>
