<template>
    <div id="app" class="bg-dark text-light" data-bs-theme="dark">

        <!-- Toolbar -->
        <div class="floating-toolbar">
            <div class="btn-group" role="group" aria-label="Basic outlined example">
                <button type="button" class="btn btn-outline-secondary" @mousedown="loadTheme_click">
                    <i class="bi bi-filetype-json"></i> Load
                </button>
                <button type="button" class="btn btn-outline-secondary" @mousedown="saveTheme_Click">
                    <i class="bi bi-floppy"></i> Save
                </button>
                <button type="button" class="btn btn-outline-secondary" @mousedown="exportTheme_Click">
                    <i class="bi bi-arrow-bar-up"></i> Export
                </button>
                <button type="button" class="btn btn-outline-secondary" @mousedown="importTheme_click">
                    <i class="bi bi-arrow-bar-down"></i> Import
                </button>
                <button type="button" class="btn btn-outline-secondary" data-bs-toggle="offcanvas"
                    data-bs-target="#offcanvasTop" aria-controls="offcanvasTop">
                    <i class="bi bi-info-circle"></i> Info
                </button> 
                <button type="button" class="btn btn-outline-secondary" @mousedown="openImages_click">
                    <i class="bi bi-arrow-card-image"></i> Images
                </button>
                <button type="button" class="btn btn-outline-secondary" data-bs-toggle="offcanvas"
                    data-bs-target="#offcanvasExample" aria-controls="offcanvasExample">
                    <i class="bi bi-sliders2"></i> Theme Props
                </button> 
            </div>
        </div>

        <div id="liveAlertPlaceholder"></div>

        <div class="layout">

            <!-- Imagen Central -->
            <div class="panel panel-left" ref="panelLeft">
                <img :src="imagePath" ref="imageElement" class="image-container" draggable="false" />
                <canvas ref="canvasElement" class="overlay-canvas"></canvas>
            </div>

            <!-- Offcanvas Panel Izquierdo -->
            <div class="offcanvas offcanvas-start" tabindex="-1" id="offcanvasExample"
                aria-labelledby="offcanvasExampleLabel">
                <div class="offcanvas-header">
                    <h5 class="offcanvas-title" id="offcanvasExampleLabel">Offcanvas</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="offcanvas" aria-label="Close"></button>
                </div>
                <div class="offcanvas-body">
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
                                    <!-- Slider para numéricos -->
                                    <div v-if="isNumeric(value)" class="range-container" :id="'element-' + key">
                                        <input type="range" class="form-range range-input"
                                            v-model.number="hudData.Colors[key]" min="0" max="20" step="0.1"
                                            style="height: 8px;" @input="OnBrightnessValueChange(key, value, $event)" />
                                        <label class="slider-value-label">{{ hudData.Colors[key] }}</label>
                                    </div>

                                    <!--  Textbox para 'Font' -->
                                    <input v-else-if="key === 'Font'" type="text" class="form-control form-control-sm"
                                        v-model="hudData.Colors[key]" @input="OnTextValueChange(key, value, $event)" />

                                    <!-- 🎨 Color picker -->
                                    <ColorDisplay v-else-if="isColor(value)" :id="'element-' + key"
                                        :color="StringToColor(value)" :recentColors="recentColors"
                                        @OncolorChanged="OnColorValueChange(key, value, $event)"
                                        @OnRecentColorsChange="OnRecentColorsChange($event)" />

                                    <!-- Fallback -->
                                    <input v-else type="text" class="form-control form-control-sm"
                                        v-model="hudData.Colors[key]" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>

            <!-- Offcanvas Panel Derecho -->
            <div class="offcanvas offcanvas-end" tabindex="-1" id="areaEditor" aria-labelledby="areaEditorLabel">
                <div class="offcanvas-header">
                    <h5 class="offcanvas-title" id="areaEditorLabel">{{ selectedArea?.title || 'Área seleccionada' }}
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="offcanvas" aria-label="Close"></button>
                </div>
                <div class="offcanvas-body">
                    <div v-if="clickedArea">
                        <div class="mb-3">
                            <label class="form-label">ID</label>
                            <input type="text" class="form-control" v-model="clickedArea.data.id" disabled>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Título</label>
                            <input type="text" class="form-control" v-model="clickedArea.data.title"
                                @input="OnAreaControl_Change">
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Posición (X, Y)</label>
                            <div class="d-flex gap-2">
                                <input type="number" class="form-control" v-model.number="clickedArea.data.x"
                                    @input="OnAreaControl_Change">
                                <input type="number" class="form-control" v-model.number="clickedArea.data.y"
                                    @input="OnAreaControl_Change">
                            </div>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Tamaño (W, H)</label>
                            <div class="d-flex gap-2">
                                <input type="number" class="form-control" v-model.number="clickedArea.data.width"
                                    @input="OnAreaControl_Change">
                                <input type="number" class="form-control" v-model.number="clickedArea.data.height"
                                    @input="OnAreaControl_Change">
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Offcanvas Panel Inferior -->
            <div class="offcanvas offcanvas-bottom" tabindex="-1" id="offcanvasTop" aria-labelledby="offcanvasTopLabel">
                <div class="offcanvas-header">
                    <h5 class="offcanvas-title" id="offcanvasTopLabel">This is an Editor for the HUD of EDHM-UI.</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="offcanvas" aria-label="Close"></button>
                </div>
                <div class="offcanvas-body">
                    <ul>
                        <li>Select a Background Image from an ingame screenshot.</li>
                        <li>Recomended size for image is 1080p, make it PNG format.</li>
                        <li>Move and re-size the areas to better fit your ship's HUD</li>
                        <li>Double Click an Area for details</li>
                        <li>Can Export and Import Themes</li>
                        <li>HUD Themes are stored here: <a href="#" @click.prevent="openHudFolder">%USERPROFILE%\EDHM_UI\HUD</a></li>                     
                    </ul>
                </div>
            </div>

        </div>
        
    </div>
</template>

<script>
import { ref } from 'vue';
import Util from '../Helpers/Util';
import ColorDisplay from '../components/ColorPicker.vue';

export default {
    name: 'App',
    components: {
        ColorDisplay
    },
    data() {
        return {
            loading: true,        //<- Flag to Show/hide the Loading Spinner
            hudData: {
                Name: '',
                Author: '',
                Image: '',
                Colors: {}
            },

            recentColors: [],     //<- Array of colors (Hex) for the Recent Colors boxes, up to 8

            imagePath: '',       //<- path to the background image
            themePath: '',       //<- path to the theme file

            currentArea: null,
            clickedArea: null,
            hoveredArea: null,
            originalWidth: 0,
            originalHeight: 0
        };
    },
    watch: {
        clickedArea: {
            handler() {
                const canvas = this.$refs.canvasElement;
                if (!canvas) return;

                this.syncCanvasPosition();
            },
            deep: true
        }
    },
    methods: {

        /** This is the Start Point of the Program **/
        async Initialize() {
            try {
                const hudCover = await window.api.readSetting('HUD_Cover');
                const DATA_DIRECTORY = await window.api.GetProgramDataDirectory();

                this.themePath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', `${hudCover}.json`);
                this.hudData = await window.api.getJsonFile(this.themePath);
                this.imagePath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', this.hudData.Image);

                const canvas = this.$refs.canvasElement;
                const panel = this.$refs.panelLeft;
                if (!canvas || !panel) return;

                canvas.width = panel.clientWidth;
                canvas.height = panel.clientHeight;

                panel.addEventListener("scroll", this.syncCanvasPosition);
                this.EstablecerAreas();
                this.initializeCanvasEvents();

            } catch (error) {
                console.error(error);
            } finally {
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
            this.EstablecerAreas?.();
            this.syncCanvasPosition();
        },
        OnColorValueChange(key, value, event) {
            //console.log('OnColorValueChange:', key, value, event);
            const val = Util.intToRGBAstring(event.int);
            this.setColor(key, val);
            //console.log('Output:', this.hudData.Colors[key]);
            this.EstablecerAreas?.();
            this.syncCanvasPosition();
        },
        OnRecentColorsChange(colors) {
            this.recentColors = [...colors]; // Update the array reactively
            console.log('Recent colors updated in parent:', colors);
        },

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
        async openHudFolder() {
            const path = "%USERPROFILE%\\EDHM_UI\\HUD";
            try {
                await window.api.openPathInExplorer(path);
            } catch (err) {
                console.error("Error abriendo la carpeta:", err);
            }
        },
        async openImages_click() {
            const DATA_DIRECTORY = await window.api.GetProgramDataDirectory();
            const HUD_DIRECTORY = await window.api.joinPath(DATA_DIRECTORY, 'images');
            window.api.openPathInExplorer(HUD_DIRECTORY);
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
                this.EstablecerAreas();
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
                console.log('Saving HUD Theme to:', window.api.getBaseNameNoExt(filePath));

                const BaseName = window.api.getBaseNameNoExt(filePath);
                this.themePath = filePath;
                this.hudData.Name = BaseName; // Update the Name property to match the file name
                this.hudData.Image = BaseName + '.png'; // Ensure the image name matches the theme name

                const newImagePath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', this.hudData.Image);
                await window.api.copyFile(this.imagePath, newImagePath);
                this.imagePath = await window.api.joinPath(DATA_DIRECTORY, 'HUD', this.hudData.Image);

                // Actualizar this.hudData.Areas desde this.rects
                this.hudData.Areas = this.rects.map(rect => ({
                    ...rect.data,
                    x: rect.x,
                    y: rect.y,
                    width: rect.width,
                    height: rect.height
                }));

                await window.api.writeJsonFile(this.themePath, JSON.parse(JSON.stringify(this.hudData)), true);
                console.log('HUD Theme saved successfully:', this.hudData);

                const msg_options = {
                    type: 'question', //<- none, info, error, question, warning
                    title: 'Set Active HUD Theme?',
                    message: 'Do you want to set this theme as the active one?',
                    buttons: ['Cancel', "Yes, Activate it!", 'No, thanks.'],
                    defaultId: 1,
                    detail: '',
                    cancelId: 0
                };
                const result = await window.api.ShowMessageBox(msg_options);
                if (result && result.response === 1) {
                    await window.api.writeSetting('HUD_Cover', this.hudData.Name);
                }
            }
        },

        async exportTheme_Click() {
            const DATA_DIRECTORY = await window.api.GetProgramDataDirectory();
            const TEMP_DIRECTORY = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\Temp\\EDHM_UI'); 
            const HOME_DIRECTORY = await window.api.resolveEnvVariables('$HOME');
            const jsonPath = await window.api.joinPath(HOME_DIRECTORY, `${this.hudData.Name}`);
            //console.log('HOME_DIRECTORY', HOME_DIRECTORY)
            const options = {
                title: 'Export the HUD Theme as..',
                message: 'Export the HUD Theme as..',
                defaultPath: jsonPath,
                properties: ['showHiddenFiles', 'createDirectory', 'showOverwriteConfirmation ', 'dontAddToRecent'],                
                filters: [
                    { name: 'ZIP Files', extensions: ['zip'] },
                    { name: 'All Files', extensions: ['*'] }
                ],
            };
            const filePath = await window.api.ShowSaveDialog(options);
            if (filePath) {
                const ThemeName = this.hudData.Name;
                const tempFolder = await window.api.ensureDirectoryExists(
                    window.api.joinPath(TEMP_DIRECTORY, ThemeName)
                );
                const DestFolder = window.api.getParentFolder(filePath);

                await window.api.copyFile(
                    window.api.joinPath(DATA_DIRECTORY, 'HUD', `${this.hudData.Name}.json`),
                    window.api.joinPath(tempFolder, `${this.hudData.Name}.json`)
                );
                await window.api.copyFile(
                    window.api.joinPath(DATA_DIRECTORY, 'HUD', `${this.hudData.Name}.png`),
                    window.api.joinPath(tempFolder, `${this.hudData.Name}.png`)
                );

                await window.api.compressFolder(
                    tempFolder, 
                    filePath
                );
                this.showAlert('ZIP exported.', 'info');
            }
        },
        async importTheme_click() {
            const DATA_DIRECTORY = await window.api.GetProgramDataDirectory();
            const TEMP_DIRECTORY = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\Temp\\EDHM_UI'); 
            const HOME_DIRECTORY = await window.api.resolveEnvVariables('$HOME');
            const jsonPath = await window.api.joinPath(HOME_DIRECTORY, `${this.hudData.Name}`);

            const options = {
                title: 'Select a HUD Theme',
                defaultPath: HOME_DIRECTORY,
                properties: ['openFile', 'showHiddenFiles', 'createDirectory', 'dontAddToRecent'],
                message: 'Select a HUD Theme',
                filters: [
                    { name: 'Zip Files', extensions: ['zip'] },
                    { name: 'All Files', extensions: ['*'] }
                ],
            };
            const filePath = await window.api.ShowOpenDialog(options);
            if (filePath) {                
                await window.api.clearFolderContents(TEMP_DIRECTORY);
                await window.api.decompressFile(
                    filePath[0], 
                    TEMP_DIRECTORY
                );
                const sub_folders = await window.api.listFolders(TEMP_DIRECTORY);                
                const themeName = await window.api.getBaseNameNoExt(sub_folders[0]);
                console.log('themeName', themeName);
                const Destination = window.api.joinPath(DATA_DIRECTORY, 'HUD');
                await window.api.copyFolderContents(sub_folders[0], Destination);
                this.showAlert('ZIP imported.', 'info');
            }
        },

        OnAreaControl_Change() {
            if (this.clickedArea && this.clickedArea.data) {
                console.log("Area control changed:", this.clickedArea.data);
                // Actualizar las propiedades del área seleccionada
                this.setArea(this.clickedArea.id, {
                    x: this.clickedArea.data.x,
                    y: this.clickedArea.data.y,
                    width: this.clickedArea.data.width,
                    height: this.clickedArea.data.height,
                    title: this.clickedArea.data.title
                });
                this.EstablecerAreas?.();
                this.syncCanvasPosition();
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

        // #region Renderizado de Areas

        EstablecerAreas() {
            this.$nextTick(() => {
                setTimeout(() => {
                    const canvas = this.$refs.canvasElement;
                    const panel = this.$refs.panelLeft;
                    if (!canvas || !panel || !this.hudData?.Areas) {
                        console.error("Canvas o áreas no disponibles");
                        return;
                    }

                    this.rects = [];
                    canvas.width = panel.clientWidth;
                    canvas.height = panel.clientHeight;

                    this.hudData.Areas.forEach(area => {
                        this.rects.push({
                            x: area.x,
                            y: area.y,
                            width: area.width,
                            height: area.height,
                            data: area
                        });
                    });

                    this.drawAllRectangles();
                    this.syncCanvasPosition();
                }, 100);
            });
        },

        dibujarArea({ x, y, width, height, data }) {
            if (!this.rects) this.rects = [];
            this.rects.push({ x, y, width, height, data });
            this.drawAllRectangles();
        },

        drawAllRectangles() {
            const canvas = this.$refs.canvasElement;
            if (!canvas || !this.rects || !this.hudData?.Colors) return;

            const ctx = canvas.getContext("2d");
            ctx.clearRect(0, 0, canvas.width, canvas.height);

            const {
                BorderColor = "red",
                BorderWidth = 2,
                HighlightColor = "rgba(255, 165, 0, 0.3)",
                HighlightBorder = "orange",
                FontColor = "white",
                Font = "14px Segoe UI",
                HighlightFontColor = "white",
                ShadowColor = "rgba(0,0,0,0.5)",
                HighlightShadowColor = "rgba(0,0,0,0.5)",
                ShadowBlur = 2,
                ShadowOffset = 2
            } = this.hudData.Colors;

            ctx.font = Font;

            this.rects.forEach(rect => {
                const isActive =
                    this.clickedArea?.data?.id &&
                    rect.data?.id &&
                    rect.data.id === this.clickedArea.data.id;

                const isHovered =
                    !isActive &&
                    this.hoveredArea?.data?.id &&
                    rect.data?.id &&
                    rect.data.id === this.hoveredArea.data.id;

                if (isActive) {
                    ctx.lineWidth = BorderWidth;
                    ctx.strokeStyle = HighlightBorder;
                    ctx.fillStyle = HighlightColor;
                    ctx.shadowColor = HighlightShadowColor;
                    ctx.shadowBlur = ShadowBlur;
                    ctx.shadowOffsetX = ShadowOffset;
                    ctx.shadowOffsetY = ShadowOffset;
                    ctx.fillRect(rect.x, rect.y, rect.width, rect.height);
                } else if (isHovered) {
                    ctx.lineWidth = BorderWidth;
                    ctx.strokeStyle = BorderColor;
                    ctx.fillStyle = "transparent";
                    ctx.shadowColor = ShadowColor;
                    ctx.shadowBlur = ShadowBlur;
                    ctx.shadowOffsetX = ShadowOffset;
                    ctx.shadowOffsetY = ShadowOffset;
                } else {
                    ctx.lineWidth = 0.5;
                    ctx.strokeStyle = "#ccc";
                    ctx.fillStyle = "transparent";
                    ctx.shadowColor = "transparent";
                    ctx.shadowBlur = 0;
                }

                ctx.strokeRect(rect.x, rect.y, rect.width, rect.height);

                // Mostrar título solo si está activo o hovered
                if ((isActive || isHovered) && rect.data?.title) {
                    ctx.shadowColor = "transparent";
                    ctx.shadowBlur = 0;
                    ctx.fillStyle = isActive ? HighlightFontColor : FontColor;
                    ctx.textAlign = "center";
                    ctx.textBaseline = "top";

                    const textX = rect.x + rect.width / 2;
                    const textY = rect.y + rect.height + 4; // debajo del borde inferior

                    ctx.fillText(rect.data.title, textX, textY);
                }

                ctx.shadowColor = "transparent";
                ctx.shadowBlur = 0;

                if (isActive) {
                    ctx.fillStyle = "cyan";
                    const anchors = [
                        [rect.x, rect.y],
                        [rect.x + rect.width, rect.y],
                        [rect.x, rect.y + rect.height],
                        [rect.x + rect.width, rect.y + rect.height]
                    ];
                    const anchorSize = 6;
                    anchors.forEach(([ax, ay]) => {
                        ctx.beginPath();
                        ctx.arc(ax, ay, anchorSize / 2, 0, Math.PI * 2);
                        ctx.fill();
                    });
                }
            });
        },

        initializeCanvasEvents() {
            if (this.canvasEventsInitialized) return;
            this.canvasEventsInitialized = true;

            const canvas = this.$refs.canvasElement;
            const anchorSize = 6;

            let isDragging = false;
            let isResizing = false;
            let offsetX, offsetY;
            let selectedRect = null;
            let selectedAnchor = null;
            let wasDragged = false;

            canvas.addEventListener("mousedown", (event) => {
                const mouseX = event.offsetX;
                const mouseY = event.offsetY;
                wasDragged = false;

                selectedRect = null;
                selectedAnchor = null;
                isResizing = false;

                for (const rect of this.rects) {
                    const anchors = [
                        { name: "top-left", x: rect.x, y: rect.y },
                        { name: "top-right", x: rect.x + rect.width, y: rect.y },
                        { name: "bottom-left", x: rect.x, y: rect.y + rect.height },
                        { name: "bottom-right", x: rect.x + rect.width, y: rect.y + rect.height }
                    ];

                    for (const anchor of anchors) {
                        if (
                            mouseX >= anchor.x - anchorSize &&
                            mouseX <= anchor.x + anchorSize &&
                            mouseY >= anchor.y - anchorSize &&
                            mouseY <= anchor.y + anchorSize
                        ) {
                            selectedRect = rect;
                            selectedAnchor = anchor.name;
                            isResizing = true;
                            return;
                        }
                    }
                }

                for (const rect of this.rects) {
                    if (
                        mouseX >= rect.x && mouseX <= rect.x + rect.width &&
                        mouseY >= rect.y && mouseY <= rect.y + rect.height
                    ) {
                        selectedRect = rect;
                        this.clickedArea = rect; // aseguro referencia idéntica
                        break;
                    }
                }

                if (selectedRect) {
                    isDragging = true;
                    offsetX = mouseX - selectedRect.x;
                    offsetY = mouseY - selectedRect.y;
                } else {
                    this.clickedArea = null; // click vacío deselecciona
                }

                this.drawAllRectangles();
            });

            canvas.addEventListener("mousemove", (event) => {
                const mouseX = event.offsetX;
                const mouseY = event.offsetY;
                let cursorSet = false;
                let hoveredNow = null;

                if (this.rects) {
                    for (const rect of this.rects) {
                        const anchors = [
                            { name: "top-left", x: rect.x, y: rect.y, cursor: "nwse-resize" },
                            { name: "top-right", x: rect.x + rect.width, y: rect.y, cursor: "nesw-resize" },
                            { name: "bottom-left", x: rect.x, y: rect.y + rect.height, cursor: "nesw-resize" },
                            { name: "bottom-right", x: rect.x + rect.width, y: rect.y + rect.height, cursor: "nwse-resize" }
                        ];

                        // Detección de área bajo el cursor
                        /*if (
                            mouseX >= rect.x &&
                            mouseX <= rect.x + rect.width &&
                            mouseY >= rect.y &&
                            mouseY <= rect.y + rect.height
                        ) {
                            hoveredNow = rect;
                            break;
                        }*/
                        hoveredNow = this.rects.find(rect =>
                            mouseX >= rect.x && mouseX <= rect.x + rect.width &&
                            mouseY >= rect.y && mouseY <= rect.y + rect.height
                        );


                        // Detección de anclas
                        for (const anchor of anchors) {
                            if (
                                mouseX >= anchor.x - anchorSize &&
                                mouseX <= anchor.x + anchorSize &&
                                mouseY >= anchor.y - anchorSize &&
                                mouseY <= anchor.y + anchorSize
                            ) {
                                canvas.style.cursor = anchor.cursor;
                                cursorSet = true;
                                break;
                            }
                        }
                        if (cursorSet) break;
                    }
                }
                this.hoveredArea = hoveredNow;

                if (!cursorSet) canvas.style.cursor = "default";

                if (isDragging && selectedRect) {
                    wasDragged = true;
                    selectedRect.x = mouseX - offsetX;
                    selectedRect.y = mouseY - offsetY;

                    if (selectedRect.data) {
                        selectedRect.data.x = selectedRect.x;
                        selectedRect.data.y = selectedRect.y;
                    }

                    this.drawAllRectangles();
                    return;
                }

                if (isResizing && selectedRect) {
                    wasDragged = true;
                    const minSize = 10;

                    switch (selectedAnchor) {
                        case "top-left":
                            selectedRect.width += selectedRect.x - mouseX;
                            selectedRect.height += selectedRect.y - mouseY;
                            selectedRect.x = mouseX;
                            selectedRect.y = mouseY;
                            break;
                        case "top-right":
                            selectedRect.width = mouseX - selectedRect.x;
                            selectedRect.height += selectedRect.y - mouseY;
                            selectedRect.y = mouseY;
                            break;
                        case "bottom-left":
                            selectedRect.width += selectedRect.x - mouseX;
                            selectedRect.x = mouseX;
                            selectedRect.height = mouseY - selectedRect.y;
                            break;
                        case "bottom-right":
                            selectedRect.width = mouseX - selectedRect.x;
                            selectedRect.height = mouseY - selectedRect.y;
                            break;
                    }

                    selectedRect.width = Math.max(minSize, selectedRect.width);
                    selectedRect.height = Math.max(minSize, selectedRect.height);

                    if (selectedRect.data) {
                        selectedRect.data.x = selectedRect.x;
                        selectedRect.data.y = selectedRect.y;
                        selectedRect.data.width = selectedRect.width;
                        selectedRect.data.height = selectedRect.height;
                    }

                    this.drawAllRectangles();
                    return;
                }

                // Redibujar si no se está arrastrando ni redimensionando
                this.drawAllRectangles();
            });

            canvas.addEventListener("mouseup", () => {
                if (!wasDragged && selectedRect?.data && !isResizing) {
                    console.log("Clicked on area:", selectedRect.data);
                }

                isDragging = false;
                isResizing = false;
                selectedRect = null;
                selectedAnchor = null;
            });

            canvas.addEventListener("mouseleave", () => {
                isDragging = false;
                isResizing = false;
                selectedRect = null;
                selectedAnchor = null;
            });

            canvas.addEventListener("dblclick", (event) => {
                const mouseX = event.offsetX;
                const mouseY = event.offsetY;

                for (const rect of this.rects) {
                    if (
                        mouseX >= rect.x && mouseX <= rect.x + rect.width &&
                        mouseY >= rect.y && mouseY <= rect.y + rect.height
                    ) {
                        this.clickedArea = rect;

                        const offcanvasEl = document.getElementById("areaEditor");
                        if (offcanvasEl) {
                            const bsOffcanvas = bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl);
                            bsOffcanvas.show();
                        }

                        this.drawAllRectangles();
                        break;
                    }
                }
            });
        },

        syncCanvasPosition() {
            const canvas = this.$refs.canvasElement;
            const panel = this.$refs.panelLeft;
            if (canvas && panel) {
                canvas.style.transform = `translate(${-panel.scrollLeft}px, ${-panel.scrollTop}px)`;
            }
        },

        onWindowResize() {
            this.EstablecerAreas?.();
            this.syncCanvasPosition();
        },

        // #endregion

        showAlert(pMessage, pType = 'sucess') {
            const alertPlaceholder = document.getElementById('liveAlertPlaceholder')
            const appendAlert = (message, type) => {
                const wrapper = document.createElement('div')
                wrapper.innerHTML = [
                    `<div class="alert alert-${type} alert-dismissible" role="alert">`,
                    `   <div>${message}</div>`,
                    '   <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
                    '</div>'
                ].join('')

                alertPlaceholder.append(wrapper)
            }
            appendAlert(pMessage, pType);
        }

    },
    mounted() {
        this.Initialize();
        this.$nextTick(() => {
            this.$refs.panelLeft?.addEventListener("scroll", this.syncCanvasPosition);
        });
        window.addEventListener("resize", this.onWindowResize);
    },
    beforeUnmount() {
        this.$refs.panelLeft?.removeEventListener("scroll", this.syncCanvasPosition);
        window.removeEventListener("resize", this.onWindowResize);
    }
};
</script>

<style scoped>
.layout {
    position: relative;
    height: 100vh;
    width: 100vw;
    overflow: hidden;
}

.floating-toolbar {
    position: fixed;
    bottom: 20px;
    right: 20px;
    z-index: 1050;
    /* Asegura que esté por encima de otros elementos */
    background: rgba(5, 5, 71, 0.6);
    padding: 4px;
    border-radius: 8px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
}

.panel {
    overflow: auto;
    /* Activa scroll independiente en cada panel */
    height: 100%;
}

.panel-left {
    position: relative;
    overflow: auto;
}

.image-container {
    max-width: none;
    max-height: none;
    pointer-events: none;
}


.overlay-canvas {
    position: absolute;
    top: 0;
    left: 0;
    pointer-events: auto;
    z-index: 0;
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
