<template>
    <div id="app" class="bg-dark text-light" data-bs-theme="dark">
        <div class="layout">
            <!-- Panel Izquierdo -->
            <div class="panel panel-left">
                <img :src="imagePath" alt="HUD" style="max-width: none; max-height: none;" />
            </div>

            <!-- Panel Derecho -->
            <div class="panel panel-right p-3 text-light" v-if="hudData" style="font-size: 0.9rem;">

                <!-- Tool Bar -->
                <div class="btn-group" role="group" aria-label="Basic outlined example">
                    <button type="button" class="btn btn-outline-secondary" @mousedown="loadTheme_click"><i
                            class="bi bi-filetype-json"></i> Load</button>
                    <button type="button" class="btn btn-outline-secondary" @mousedown="saveTheme_Click"><i
                            class="bi bi-floppy"></i> Save</button>
                </div>
                <br><br>

                <!-- Theme Identificators -->
                <h6 class="border-bottom pb-2 mb-3">🧭 HUD Properties</h6>
                <table class="table table-bordered table-hover align-middle table-dark table-sm">
                    <tbody>
                        <tr>
                            <td>Name</td>
                            <td><input type="text" class="form-control form-control-sm" v-model="hudData.Name"
                                    @input="OnIdentificadorChange('Name', $event.target.value)"></td>
                        </tr>
                        <tr>
                            <td>Author</td>
                            <td><input type="text" class="form-control form-control-sm" v-model="hudData.Author"
                                    @input="OnIdentificadorChange('Author', $event.target.value)"></td>
                        </tr>
                        <tr>
                            <td>Image</td>
                            <td>
                                <div id="customImagePath" class="input-group mb-3" style="height: 22px;">
                                    <input type="text" class="form-control form-control-sm"
                                        placeholder="Pick a Location" aria-label="Pick a Location"
                                        aria-describedby="button-addon4" v-model="hudData.Image">
                                    <button class="btn btn-outline-secondary" type="button" id="button-addon4"
                                        @click="browseCustomImage">Browse</button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>

                <!-- Theme Properties -->
                <h6 class="mt-4 mb-2">🎨 Colors</h6>
                <table class="table table-bordered table-hover align-middle table-dark table-sm">
                    <tbody>
                        <tr v-for="(value, key) in hudData?.Colors" :key="key">
                            <!-- Propiedad -->
                            <td>{{ key }}</td>

                            <!-- Control editable -->
                            <td>
                                <!-- 🎚️ Slider para numéricos -->
                                <div v-if="isNumeric(value)" class="range-container" :id="'element-' + key">
                                    <input type="range" class="form-range range-input"
                                        v-model.number="hudData.Colors[key]" min="0" max="10" step="0.1"
                                        style="height: 8px;" @input="OnBrightnessValueChange(key, value, $event)" />
                                    <label class="slider-value-label">{{ hudData.Colors[key] }}</label>
                                </div>

                                <!-- 🔠 Textbox para 'Font' -->
                                <input v-else-if="key === 'Font'" type="text" class="form-control form-control-sm"
                                    v-model="hudData.Colors[key]" @input="OnTextValueChange(key, value, $event)" />

                                <!-- 🎨 Color picker -->
                                <ColorDisplay v-else-if="isColor(value)" :id="'element-' + key"
                                    :color="StringToColor(value)" :recentColors="recentColors"
                                    @OncolorChanged="OnColorValueChange(key, value, $event)"
                                    @OnRecentColorsChange="OnRecentColorsChange($event)" />

                                <!-- 📎 Fallback -->
                                <input v-else type="text" class="form-control form-control-sm"
                                    v-model="hudData.Colors[key]" />
                            </td>
                        </tr>
                    </tbody>
                </table>

            </div>

        </div>
    </div>
</template>

<script>
import { ref } from 'vue';
import Util from '../Helpers/Util';
import ColorDisplay from '../components/ColorPicker.vue';

//console.log('App.vue loaded');

export default {
    name: 'App',
    components: {
        ColorDisplay
    },
    data() {
        return {
            loading: true,        //<- Flag to Show/hide the Loading Spinner
            hudData: null,       //<- The Program Settings
            recentColors: [],     //<- Array of colors (Hex) for the Recent Colors boxes, up to 8
            
            imagePath: '',       //<- path to the background image
            themePath: '',       //<- path to the theme file
        };
    },
    methods: {

        /** This is the Start Point of the Program **/
        async Initialize() {
            try {
                const hudCover = await window.api.readSetting('HUD_Cover');// 'HUD_Type8';
                const DATA_DIRECTORY = await window.api.GetProgramDataDirectory();

                this.themePath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', `${hudCover}.json`);
                this.hudData = await window.api.getJsonFile(this.themePath); 
                this.imagePath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', this.hudData.Image);

                //console.log('hudData:', this.hudData);
            } catch (error) {
                console.error(error);
            } finally {
                //EventBus.emit('ShowSpinner', { visible: false });
                this.loading = false;
            }
        },

        // #region Conversion de Datos
        
        isNumeric(value) {
            return !isNaN(parseFloat(value)) && isFinite(value);
        },
        isColor(val) {
            const s = new Option().style;
            s.color = val;
            return s.color !== '';
        },
        StringToColor(value) {
            const _ret = Util.colorStringToInt(value);
            //console.log(`Input: ${value}, Output: ${_ret}`);
            return _ret;
        },
        ColorToString(value) {
            return Util.intToRGBAstring(value);
        },
        
        // #endregion

        // #region Eventos de Controles

        OnIdentificadorChange(key, value) {
            //console.log(`Propiedad "${key}" cambió a:`, value);
            this.hudData[key] = value;
            //console.log(key, this.hudData[key]);
        },
        OnTextValueChange(key, value, event) {
            //console.log('OnBrightnessValueChange:', key, value, event);         
            this.setColor(key, value);
            //console.log('Output:', this.hudData.Colors[key]);
        },        
        OnBrightnessValueChange(key, value, event) {
            //console.log('OnBrightnessValueChange:', key, value, event);
            const val = parseFloat(event.target.value);            
            this.setColor(key, val);
        },
        OnColorValueChange(key, value, event) {
            //console.log('OnColorValueChange:', key, value, event);
            const val = Util.intToRGBAstring(event.int);            
            this.setColor(key, val);
            //console.log('Output:', this.hudData.Colors[key]);
        },
        OnRecentColorsChange(colors) {
            this.recentColors = [...colors]; // Update the array reactively
            console.log('Recent colors updated in parent:', colors);
        },

        /* Browse for the location of a Custom Icon for the App */
        async browseCustomImage() {
            const hudCover = 'HUD_Type8';
            const DATA_DIRECTORY = await window.api.GetProgramDataDirectory(); //console.log('DATA_DIRECTORY:', DATA_DIRECTORY);
            const jsonPath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', `${hudCover}.png`);//console.log('DefaultLocation:', DefaultLocation);
            const options = {
                title: 'Select an Image, Recomended Size 1080p',
                defaultPath: jsonPath,
                properties: ['openFile', 'showHiddenFiles', 'createDirectory', 'dontAddToRecent'],
                message: 'Select a Background Image',
                filters: [
                    { name: 'Images', extensions: ['jpg', 'jpeg', 'png', 'gif'] },
                    { name: 'All Files', extensions: ['*'] }
                ],
            };
            const filePath = await window.api.ShowOpenDialog(options);
            if (filePath) {
                this.imagePath = filePath[0];
                this.hudData.Image = window.api.getBaseName(filePath[0]);
            }
        },
        async loadTheme_click() {
            const hudCover = 'HUD_Type8';
            const DATA_DIRECTORY = await window.api.GetProgramDataDirectory(); 
            const jsonPath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', `${hudCover}.json`);
            const options = {
                title: 'Select a HUD Theme',
                defaultPath: jsonPath,
                properties: ['openFile', 'showHiddenFiles', 'createDirectory', 'dontAddToRecent'],
                message: 'Select a HUD Theme',
                filters: [
                    { name: 'JSON Data', extensions: ['json'] },
                    { name: 'All Files', extensions: ['*'] }
                ],
            };
            const filePath = await window.api.ShowOpenDialog(options);
            if (filePath) {
                this.themePath = filePath[0];
                this.hudData = await window.api.getJsonFile(this.themePath);
                this.imagePath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', this.hudData.Image);
            }
        },
        async saveTheme_Click() {
            const DATA_DIRECTORY = await window.api.GetProgramDataDirectory(); 
            const jsonPath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', `${this.hudData.Name}.json`);
            const options = {
                title: 'Save the HUD Theme as..',
                defaultPath: jsonPath,
                properties: ['showHiddenFiles', 'createDirectory', 'showOverwriteConfirmation ', 'dontAddToRecent'],
                message: 'Save the HUD Theme as..',
                filters: [
                    { name: 'JSON Data', extensions: ['json'] },
                    { name: 'All Files', extensions: ['*'] }
                ],
            };
            const filePath = await window.api.ShowSaveDialog(options);
            if (filePath) {
                console.log('Saving HUD Theme to:',window.api.getBaseNameNoExt(filePath));

                const BaseName = window.api.getBaseNameNoExt(filePath);
                this.themePath = filePath;
                this.hudData.Name = BaseName; // Update the Name property to match the file name
                this.hudData.Image = BaseName + '.png'; // Ensure the image name matches the theme name

                const newImagePath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', this.hudData.Image);
                await window.api.copyFile(this.imagePath, newImagePath);
                this.imagePath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', this.hudData.Image);

                await window.api.writeJsonFile(this.themePath, JSON.parse(JSON.stringify(this.hudData)), true);
                console.log('HUD Theme saved successfully:', this.hudData);
                

                //this.hudData = await window.api.getJsonFile(this.themePath);
                //this.imagePath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', this.hudData.Image);
            }
        },
        
        // #endregion

        // #region Grabacion de Datos
        
        getColor(propName) {
            return this.hudData.Colors?.[propName];
        },
        setColor(propName, value) {
            if (!this.hudData.Colors) this.hudData.Colors = {};
            this.hudData.Colors[propName] = value;
        },

        getArea(areaId) {
            return this.hudData.Areas?.find(area => area.id === areaId);
        },
        setArea(areaId, newProps) {
            if (!this.hudData.Areas) this.hudData.Areas = [];

            const index = this.hudData.Areas.findIndex(area => area.id === areaId);
            if (index !== -1) {
                this.hudData.Areas[index] = { ...this.hudData.Areas[index], ...newProps };
            } else {
                this.hudData.Areas.push({ id: areaId, ...newProps });
            }
        },

        // #endregion
        

    },
    async mounted() {
        this.Initialize();
    },
    beforeUnmount() {
    }
};
</script>

<style scoped>
.layout {
    display: flex;
    height: 100vh;
    width: 100vw;
    overflow: hidden;
}

.panel {
    overflow: auto;
    height: 100%;
}

.panel-left {
    width: 72%;
    border-right: 1px solid #444;
}

.panel-right {
    width: 28%;
    border-left: 1px solid #444;
}

.table {
    width: 100%;
    margin-bottom: 1rem;
    color: #212529;
}

.range-container {
    position: relative;
    width: 100%;
    height: 38px;
}

.slider-value-label {
    display: block;
    margin-top: 2px;
    font-size: 12px;
    color: #f8f9fa;
    text-align: left;
}
</style>
