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
                                            <div id="userDataLocation" class="input-group mb-3">
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
                        <button type="button" class="btn btn-success" @click="runGameLocationAssistant">Game Location Assistant</button>
                        <button type="button" class="btn btn-primary" @click="save">Save Changes</button>
                    </div>
                    
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import eventBus from '../EventBus';

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
            }
        };
    },
    created() {
        eventBus.on('open-settings-editor', this.open);
    },
    methods: {
        async open() {
            this.visible = true;
            this.config = await window.api.getSettings();
        },
        close() {
            this.visible = false;
        },
        save() {
            this.$emit('save', this.config);
            this.close();
        },
        browseFile(instanceIndex, gameIndex) {
            const options = {
                title: 'Select the Game Executable',
                defaultPath: 'EliteDangerous64',
                filters: [
                    { name: 'Game Exe', extensions: ['exe'] }
                ],
                properties: ['openFile', 'multiSelections', 'showHiddenFiles', 'dontAddToRecent'],
                message: 'Select the Game Executable', // (optional)
            };
            window.api.ShowOpenDialog(options).then(filePath => {
                if (filePath) {
                    this.config.GameInstances[instanceIndex].games[gameIndex].path = filePath;
                }
            });
        },
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
        sanitizeId(id) {
            return id.replace(/\s/g, '');
        },
        runGameLocationAssistant() {
            console.log('Waiting for Game to Start...');
            eventBus.emit('RoastMe', { type: 'Info', message: 'Waiting for Game to Start...<br>Leave the game running at menus and return here.' });

            this.checkProcess().then(fullPath => {
                if (fullPath) {
                    console.log('Process found at:', fullPath);
                    eventBus.emit('RoastMe', { type: 'Success', message: `Process found at: '${fullPath}''` });

                    //TODO: Instalar el mod en la nueva ubicacion del juego
                } else {
                    console.log('Process not found.');
                }
            });
        },
        async checkProcess() {
            return new Promise((resolve, reject) => {
                const check = async () => {
                    const processPath = await window.api.checkProcess('EliteDangerous64.exe');
                    if (processPath) {
                        resolve(processPath);
                    } else {
                        setTimeout(() => {
                            requestAnimationFrame(check);
                        }, 3000); // Check every 3 seconds
                    }
                };
                check();
            });
        }
    },
    beforeDestroy() {
        eventBus.off('open-settings-editor', this.open);
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
