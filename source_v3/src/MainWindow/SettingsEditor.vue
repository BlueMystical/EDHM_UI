<template>
    <div v-if="visible" class="modal fade show" style="display: block;" data-bs-theme="dark">
        <div class="modal-dialog modal-lg">
            <div class="modal-content bg-dark text-light">
                <div class="modal-header">
                    <h5 class="modal-title">EDHM Settings</h5>
                    <button type="button" class="btn-close btn-close-white" aria-label="Close" @click="close"></button>
                </div>
                <div class="modal-body">

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
                    <label for='txtFullGamePath' class="form-label">Full path to the Game's Executable:</label>
                    <div class="input-group mb-3">
                        <input type="text" class="form-control form-control-sm"
                            placeholder='Manually select the game location or use the Localization Wizard below'
                            aria-label='Pick a location for ' id='txtFullGamePath' v-model="selectedGamePath"
                            @change="OnGamePathChange">
                        <button class="btn btn-outline-secondary" type="button"
                            @click="browseGamePath()">Browse</button>
                    </div>

                    <hr>

                    <!-- Other Settings -->
                    <div class="accordion" id="accordionExample">
                        <div class="accordion-item">
                            <h2 class="accordion-header" id="headingOtherSettings">
                                <button class="accordion-button" type="button" data-bs-toggle="collapse"
                                    data-bs-target="#collapseOtherSettings" aria-expanded="true"
                                    aria-controls="collapseOtherSettings">Other Settings
                                </button>
                            </h2>
                            <div id="collapseOtherSettings" class="accordion-collapse collapse show"
                                aria-labelledby="headingOtherSettings" data-bs-parent="#accordionExample">
                                <div class="accordion-body">
                                    <div class="row">
                                        <div class="col">
                                            <div class="form-check form-switch">
                                                <input class="form-check-input" type="checkbox" role="switch"
                                                    id="flexSwitchCheckDefault" v-model="config.GreetMe">
                                                <label class="form-check-label" for="flexSwitchCheckDefault">Greet me on Startup</label>
                                            </div>
                                        </div>
                                        <div class="col">
                                            <label for="playerJournal">Player's Journal Location:</label>
                                            <div id="playerJournal" class="input-group mb-3">
                                                <input type="text" class="form-control form-control-sm"
                                                    placeholder="Pick a Location" aria-label="Pick a Location"
                                                    aria-describedby="button-addon2" v-model="config.PlayerJournal">
                                                <button class="btn btn-outline-secondary" type="button"
                                                    id="button-addon2" @click="browseJournalFolder">Browse</button>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col">
                                            <div class="form-check form-switch">
                                                <input class="form-check-input" type="checkbox" role="switch"
                                                    id="flexSwitchCheckTray" checked v-model="config.HideToTray">
                                                <label class="form-check-label" for="flexSwitchCheckTray">Hide to Tray on close</label>
                                            </div>
                                        </div>
                                        <div class="col">
                                            <label for="userDataLocation">Themes & User's Data:</label>
                                            <div id="userDataLocation" class="input-group mb-3" disabled>
                                                <input type="text" class="form-control form-control-sm"
                                                    placeholder="Pick a Location" aria-label="Pick a Location"
                                                    aria-describedby="button-addon2" v-model="config.UserDataFolder" disabled>
                                                <button class="btn btn-outline-secondary" type="button"
                                                    id="button-addon2" @click="browseUserDataFolder" disabled>Browse</button>
                                            </div>
                                        </div>

                                    </div>
                                    <div class="row">
                                        <div class="col">
                                        </div>
                                        <div class="col">
                                            <label for="quantity">Saves To Remember:</label>
                                            <input type="number" class="form-control" id="quantity" min="1" max="50"
                                                v-model="config.SavesToRemember">
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                </div> <!-- modal-body -->
                <div class="modal-footer">
                    <div class="btn-group" role="group" aria-label="Default button group">
                        <button type="button" class="btn btn-outline-secondary" @click="CleanInstall">Clean Install</button>
                        <button type="button" class="btn btn-success" @click="runGameLocationAssistant">Game Localization Wizard</button>
                        <button type="button" class="btn btn-primary" @click="save">Save Changes</button>
                    </div>

                </div>
            </div>
        </div>
    </div>
</template>

<script>
import EventBus from '../EventBus';

export default {
    name: 'SettingsEditor',
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
        };
    },
    created() {
        // Emit the 'open-settings-editor' Event to open this Window
        EventBus.on('open-settings-editor', this.open);
    },
    methods: {

        Initialize() {
            try {
                if (this.config) {
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

        /* Pops up this Component */
        async open(InstallStatus) {
            this.visible = true;
            if (InstallStatus === 'existingInstall') {
                this.config = await window.api.getSettings();
                this.Initialize();
            } else {
                // FRESH INSTALL:
                this.config = await window.api.getDefaultSettings();
                this.Initialize();
                setTimeout(() => {
                    EventBus.emit('RoastMe', { type: 'Info', message: 'You can do it manually in the Game Instances..<br> or just click the Green Button.', delay: 10000 });    
                }, 2000);
            }
            //console.log(this.config);        
        },
        close() {
            this.visible = false;
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

            EventBus.emit('SettingsChanged', JSON.parse(JSON.stringify(this.config))); //<- this event will be heard in 'App.vue'  
            this.close();
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
                this.ActiveInstance = publisher.games[this.selectedVersion]; //console.log(this.ActiveInstance);
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

            this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].path = this.selectedGamePath;
            this.config.ActiveInstance = this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].instance;

            console.log('ActiveInstance:', this.config.ActiveInstance);
            await window.api.terminateProgram('EliteDangerous64.exe');
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

            const options = {
                title: 'Select the Game Executable',
                defaultPath: platform === 'win32' ? winDir : linuxDir,
                filters: [
                    { name: 'Game Exe', extensions: ['exe'] }
                ],
                properties: ['openFile', 'multiSelections', 'showHiddenFiles', 'dontAddToRecent'],
                message: 'Select the Game Executable',
            };
            const filePath = await window.api.ShowOpenDialog(options); //console.log(filePath);
            if (filePath) {
                this.selectedGamePath = window.api.getParentFolder(filePath[0]);
                this.OnGamePathChange(null);
            }
        },
        /* Browse for the location to store User's data and Themes */
        async browseUserDataFolder() {
            const DefaultLocation = await window.api.resolveEnvVariables('%USERPROFILE%\\EDHM_UI');
            const options = {
                title: 'Select Where to Store User Data',
                defaultPath: DefaultLocation,
                properties: ['openDirectory', 'createDirectory', 'promptToCreate', 'dontAddToRecent'],
                message: 'Select Where to Store User Data',
            };
            const filePath = await window.api.ShowOpenDialog(options);
            if (filePath) {
                this.config.UserDataFolder = filePath[0];
            }
        },
        /* Browse for the location where the ED Player Journal is located */
        async browseJournalFolder() {
            const DefaultLocation = await window.api.resolveEnvVariables('%USERPROFILE%\\Saved Games\\Frontier Developments\\Elite Dangerous');
            const options = {
                title: 'Select Where to Store User Data',
                defaultPath: DefaultLocation,
                properties: ['openDirectory', 'createDirectory', 'promptToCreate', 'dontAddToRecent'],
                message: 'Select Where to Store User Data',
            };
            const filePath = await window.api.ShowOpenDialog(options);
            if (filePath) {
                this.config.PlayerJournal = filePath[0];
            }
        },
        /* Cleans html tags */
        sanitizeId(id) {
            return id.replace(/\s/g, '');
        },
        async InstallGameInstance(FolderPath){
            try {
                await window.api.terminateProgram('EliteDangerous64.exe');
            } catch {} 
        },


        /* Attempts to Detect the running Game Process and then sets the Paths */
        async runGameLocationAssistant() {
            EventBus.emit('RoastMe', { type: 'Info', message: 'Waiting for Game to Start...<br>Leave the game running at menus and return here.' });

            // 1. Check if the Game is already Running:
            const fullPath = await window.api.detectProgram('EliteDangerous64.exe');

            if (fullPath) {
                console.log('Process found at:', fullPath);
                EventBus.emit('RoastMe', { type: 'Success', message: `Process found!<br>Game will now Close<br>Don't Panic..` });

                await window.api.terminateProgram('EliteDangerous64.exe');
                const FolderPath = await window.api.getParentFolder(fullPath);
                //this.addNewGameInstance(FolderPath);
                this.selectedGamePath = FolderPath;    console.log(this.selectedGamePath);
                console.log('Selected Game Path:', this.selectedGamePath);

                this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].path = this.selectedGamePath;
                this.config.ActiveInstance = this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].instance;

                console.log('ActiveInstance:', this.config.ActiveInstance);
                this.InstallGameInstance(this.selectedGamePath);
                
            } else {
                console.log('Process not found.');
                EventBus.emit('RoastMe', { type: 'Info', message: 'Waiting for Game to Start...<br>Leave the game running at menus and return here.' });

                window.api.startMonitoring('EliteDangerous64.exe');
                
                // Event listener for program detection
                window.api.onProgramDetected(async (event, exePath) => {
                    console.log(`Executable Path: ${exePath}`);
                    EventBus.emit('RoastMe', { type: 'Success', message: `Process found!<br>Game will now Close<br>Don't Panic..` });

                    await window.api.terminateProgram('EliteDangerous64.exe');
                    const FolderPath = await window.api.getParentFolder(exePath);
                    
                    //this.addNewGameInstance(String(FolderPath));
                    this.selectedGamePath = FolderPath;    console.log(this.selectedGamePath);
                    console.log('Selected Game Path:', this.selectedGamePath);

                    this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].path = this.selectedGamePath;
                    this.config.ActiveInstance = this.config.GameInstances[this.selectedPublisher].games[this.selectedVersion].instance;

                    console.log('ActiveInstance:', this.config.ActiveInstance);
                    this.InstallGameInstance(this.selectedGamePath);
                });
            }
        },

        /** Button Click: Clean Install
         * Deletes the Settings File for a Clean Install
         */
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
                        EventBus.emit('RoastMe', { type: 'Success', message: 'EDHM Settings got wiped!<br>You should re-start the App now.' });
                    }
                }
            } catch (error) {
                EventBus.emit('ShowError', error);
            }
        }
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
