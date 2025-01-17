<template>
    <div v-if="visible" class="modal fade show" style="display: block;" data-bs-theme="dark">
        <div class="modal-dialog modal-lg">
            <div class="modal-content bg-dark text-light">
                <div class="modal-header">
                    <h5 class="modal-title">Edit Game Instances</h5>
                    <button type="button" class="btn-close btn-close-white" aria-label="Close" @click="close"></button>
                </div>
                <div class="modal-body">
                    <label for="gameInstancesAccordion" class="form-label">Game Instances:</label>
                    <div class="accordion" id="gameInstancesAccordion">
                        <div v-for="(instance, instanceIndex) in config.GameInstances" :key="instanceIndex" class="accordion-item">
                            <h2 class="accordion-header" :id="'heading-' + sanitizeId(instance.instance)">
                                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse"
                                    :data-bs-target="'#collapse-' + sanitizeId(instance.instance)" aria-expanded="false" :aria-controls="'collapse-' + sanitizeId(instance.instance)">
                                    {{ instance.instance }}
                                </button>
                            </h2>
                            <div :id="'collapse-' + sanitizeId(instance.instance)" class="accordion-collapse collapse"
                                :aria-labelledby="'heading-' + sanitizeId(instance.instance)" data-bs-parent="#gameInstancesAccordion">
                                <div class="accordion-body">
                                    <div class="accordion" :id="'accordion-' + sanitizeId(instance.instance) + '-Games'">
                                        <div v-for="(game, gameIndex) in instance.games" :key="gameIndex" class="accordion-item">
                                            <h2 class="accordion-header" :id="'heading-' + sanitizeId(game.instance)">
                                                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse"
                                                    :data-bs-target="'#collapse-' + sanitizeId(game.instance)" aria-expanded="false" :aria-controls="'collapse-' + sanitizeId(game.instance)">
                                                    {{ game.name }}
                                                </button>
                                            </h2>
                                            <div :id="'collapse-' + sanitizeId(game.instance)" class="accordion-collapse collapse"
                                                :aria-labelledby="'heading-' + sanitizeId(game.instance)" :data-bs-parent="'#accordion-' + sanitizeId(instance.instance) + '-Games'">
                                                <div class="accordion-body">
                                                    <label :for="'Path-' + sanitizeId(game.instance)" class="form-label">Full path to the
                                                        Game's Executable:</label>
                                                    <div class="input-group mb-3">
                                                        <input type="text" class="form-control form-control-sm" :placeholder="'Pick a location for ' + game.name"
                                                               :aria-label="'Pick a location for ' + game.name" :id="'Path-' + sanitizeId(game.instance)" v-model="game.path">
                                                        <button class="btn btn-outline-secondary" type="button" @click="browseFile(instanceIndex, gameIndex)">Browse</button>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <hr>

                    <div class="accordion" id="accordionExample">
                        <div class="accordion-item">
                            <h2 class="accordion-header" id="headingOtherSettings">
                                <button class="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#collapseOtherSettings" aria-expanded="true" aria-controls="collapseOtherSettings">
                                    Other Settings
                                </button>
                            </h2>
                            <div id="collapseOtherSettings" class="accordion-collapse collapse" aria-labelledby="headingOtherSettings" data-bs-parent="#accordionExample">
                                <div class="accordion-body">
                                    <div class="row">
                                        <div class="col">
                                            <div class="form-check form-switch">
                                                <input class="form-check-input" type="checkbox" role="switch" id="flexSwitchCheckDefault" v-model="config.GreetMe">
                                                <label class="form-check-label" for="flexSwitchCheckDefault">Greet me on Startup</label>
                                            </div>
                                        </div>
                                        <div class="col">
                                            <label for="playerJournal">Player's Journal Location:</label>
                                            <div id="playerJournal" class="input-group mb-3">
                                                <input type="text" class="form-control form-control-sm" placeholder="Pick a Location" 
                                                    aria-label="Pick a Location" aria-describedby="button-addon2" v-model="config.PlayerJournal">
                                                <button class="btn btn-outline-secondary" type="button" id="button-addon2" @click="browseJournalFolder">Browse</button>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col">
                                            <div class="form-check form-switch">
                                                <input class="form-check-input" type="checkbox" role="switch" id="flexSwitchCheckTray" checked v-model="config.HideToTray">
                                                <label class="form-check-label" for="flexSwitchCheckTray">Hide to Tray on close</label>
                                            </div>
                                        </div>
                                        <div class="col">
                                            <label for="userDataLocation">Themes & User's Data:</label>
                                            <div id="userDataLocation" class="input-group mb-3" disabled>
                                                <input type="text" class="form-control form-control-sm" placeholder="Pick a Location"
                                                    aria-label="Pick a Location" aria-describedby="button-addon2" v-model="config.UserDataFolder">
                                                <button class="btn btn-outline-secondary" type="button" id="button-addon2" @click="browseUserDataFolder">Browse</button>
                                            </div>
                                        </div>

                                    </div>
                                    <div class="row">
                                        <div class="col">
                                        </div>
                                        <div class="col">
                                            <label for="quantity">Saves To Remember:</label>
                                            <input type="number" class="form-control" id="quantity" min="1" max="50" v-model="config.SavesToRemember">
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
                        <button type="button" class="btn btn-success" @click="runGameLocationAssistant">Game Locator Assistant</button>
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
            config: {
                GameInstances: [],
                GreetMe: false,
                HideToTray: false,
                PlayerJournal: '',
                UserDataFolder: '',
            },
            ActiveInstance: {}
        };
    },
    created() {
        // Emit the 'open-settings-editor' Event to open this Window
        EventBus.on('open-settings-editor', this.open);
    },
    methods: {
        /* Pops up this Component */
        async open(InstallStatus) {
            this.visible = true;
            if (InstallStatus === 'existingInstall') {
                this.config = await window.api.getSettings();
            } else {
                this.config = await window.api.getDefaultSettings();
                setTimeout(() => {
                    EventBus.emit('RoastMe', { type: 'Info', message: 'You can do it manually in the Game Instances..<br> or just click the Green Button.' });    
                }, 2000); 
            }    
            //console.log(this.config);        
        },
        close() {
            this.visible = false;
        },
        /* Button Click: Save the Settings */
        async save() {
            
            const jsonString = JSON.stringify(this.config, null, 4);
            await window.api.saveSettings(jsonString);
            EventBus.emit('SettingsChanged', JSON.parse(jsonString)); //<- this event will be heard in 'App.vue'  
            this.close();
        },
        async InstallGameInstance(FolderPath){
            await window.api.terminateProgram('EliteDangerous64.exe');
            this.addNewGameInstance(FolderPath);
        },
        /* Manually Browse for the Game Executable */
        browseFile(instanceIndex, gameIndex) {            
            const options = {
                title: 'Select the Game Executable',
                defaultPath: 'EliteDangerous64',
                filters: [
                    { name: 'Game Exe', extensions: ['exe'] }
                ],
                properties: ['openFile', 'multiSelections', 'showHiddenFiles', 'dontAddToRecent'],
                message: 'Select the Game Executable', 
            };
            window.api.ShowOpenDialog(options).then(filePath => {
                if (filePath) {
                    const FolderPath = window.api.getParentFolder(filePath[0]);
                    this.config.GameInstances[instanceIndex].games[gameIndex].path = FolderPath;
                    InstallGameInstance(FolderPath);
                }
            });
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
            window.api.ShowOpenDialog(options).then(filePath => {
                if (filePath) {
                    this.config.UserDataFolder = filePath;
                    
                }
            });
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
            window.api.ShowOpenDialog(options).then(filePath => {
                if (filePath) {
                    this.config.PlayerJournal = filePath;
                }
            });
        },
        /* Cleans html tags */
        sanitizeId(id) {
            return id.replace(/\s/g, '');
        },
        /* Adds a new Game Instance to the Settings */
        async addNewGameInstance(instancePath) {
            const _ret = await window.api.addNewInstance(instancePath, JSON.parse(JSON.stringify(this.config))); 
            this.config = JSON.parse(JSON.stringify(_ret));
            EventBus.emit('RoastMe', { type: 'Info', message: 'Now Save the Changes' });
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
                this.addNewGameInstance(FolderPath);
                
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
                    this.addNewGameInstance(String(FolderPath));
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
                    buttons: ['Cancel', "Yes, I'm Sure", 'No, take me back!'],
                    defaultId: 1,
                    title: 'Question',
                    message: 'Do you want to proceed?',
                    detail: "This will wipe the settings of the program to a 'clean State'",
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
