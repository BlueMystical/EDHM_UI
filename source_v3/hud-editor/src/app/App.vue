<template>
    <div id="app" class="bg-dark text-light" data-bs-theme="dark">
        <div class="layout">

            <!-- Panel Izquierdo -->
<div class="panel panel-left" ref="panelLeft">
    <img :src="imagePath" ref="imageElement" class="image-container" draggable="false" />
    <canvas ref="canvasElement" class="overlay-canvas"></canvas>
</div>
            <!-- <div class="panel panel-left">
                <div id="Container" class="image-container" ref="container" 
                    @mousemove="OnArea_MouseMove"
                    @mouseleave="OnArea_MouseLeave" 
                    @click="OnArea_Click($event)">
                    <img :src="imagePath" alt="HUD Image" ref="image" @load="setupCanvas"
                        style="max-width: none; max-height: none;" />
                    <canvas ref="canvas"></canvas>
                </div>
            </div>-->


            <!-- Panel Derecho -->
            <div class="panel panel-right p-3 text-light" v-if="hudData" style="font-size: 0.9rem;">

                <!-- Tool Bar -->
                <div class="btn-group" role="group" aria-label="Basic outlined example">
                    <button type="button" class="btn btn-outline-secondary" @mousedown="loadTheme_click">
                        <i class="bi bi-filetype-json"></i> Load</button>
                    <button type="button" class="btn btn-outline-secondary" @mousedown="saveTheme_Click">
                        <i class="bi bi-floppy"></i> Save</button>
                    <button type="button" class="btn btn-outline-secondary" @mousedown="exportTheme_Click">
                        <i class="bi bi-arrow-bar-up"></i> Export</button>
                    <button type="button" class="btn btn-outline-secondary" @mousedown="importTheme_click">
                        <i class="bi bi-arrow-bar-down"></i> Import</button>
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

            currentArea: null, 
            clickedArea: null, 
            originalWidth: 0,
            originalHeight: 0

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
  
                // this.$nextTick(() => this.setupCanvas());

                this.EstablecerAreas();


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

        async exportTheme_Click() {
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
        async importTheme_click() {
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
        
        // #region Eventos del Area

        EstablecerAreas() {
            this.$nextTick(() => {
                setTimeout(() => {
                    const canvas = this.$refs.canvasElement;
                    if (!canvas) {
                        console.error("Error: Elemento no encontrado");
                        return;
                    }

                    canvas.width = this.$refs.panelLeft.clientWidth;
                    canvas.height = this.$refs.panelLeft.clientHeight;

                    // Llamamos varias veces a `dibujarArea()` con distintos parámetros
                    this.dibujarArea({ x: 0, y: 10, width: 200, height: 300 });
                    this.dibujarArea({ x: 0, y: 301, width: 200, height: 300 });
                    this.dibujarArea({ x: 0, y: 620, width: 200, height: 300 });
                }, 100);
            });
        },

        dibujarArea({ x, y, width, height }) {
            const canvas = this.$refs.canvasElement;
            if (!canvas) {
                console.error("Error: El canvas no está definido");
                return;
            }

            const ctx = canvas.getContext("2d");

            // Almacenar los rectángulos en una lista global
            if (!this.rects) this.rects = [];
            this.rects.push({ x, y, width, height });

            function drawRectangles() {
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                ctx.strokeStyle = "red";
                ctx.lineWidth = 2;
                this.rects.forEach(rect => {
                    ctx.strokeRect(rect.x, rect.y, rect.width, rect.height);
                });
            }
            drawRectangles.call(this);

            // Agregar eventos para mover los rectángulos
            let isDragging = false;
            let offsetX, offsetY;
            let selectedRect = null;

            canvas.addEventListener("mousedown", (event) => {
                const mouseX = event.offsetX;
                const mouseY = event.offsetY;

                selectedRect = this.rects.find(rect =>
                    mouseX >= rect.x && mouseX <= rect.x + rect.width &&
                    mouseY >= rect.y && mouseY <= rect.y + rect.height
                );

                if (selectedRect) {
                    isDragging = true;
                    offsetX = mouseX - selectedRect.x;
                    offsetY = mouseY - selectedRect.y;
                }
            });

            canvas.addEventListener("mousemove", (event) => {
                if (!isDragging || !selectedRect) return;
                selectedRect.x = event.offsetX - offsetX;
                selectedRect.y = event.offsetY - offsetY;
                drawRectangles.call(this);
            });

            canvas.addEventListener("mouseup", () => {
                if (isDragging && selectedRect) {
                    console.log(`Rectángulo fijado en: X=${selectedRect.x}, Y=${selectedRect.y}`);
                    isDragging = false;
                    selectedRect = null;
                }
            });

            canvas.addEventListener("mouseleave", () => {
                isDragging = false;
                selectedRect = null;
            });
        },


        DrawRectangle() {
    this.$nextTick(() => {
        setTimeout(() => {
            const img = this.$refs.imageElement;
            const canvas = this.$refs.canvasElement;
            const panelLeft = this.$refs.panelLeft; // Capturamos el contenedor

            if (!img || !canvas || !panelLeft) {
                console.error("Error: Elemento no encontrado", { img, canvas, panelLeft });
                return;
            }

            const ctx = canvas.getContext("2d");

            // Ajustamos el tamaño del canvas al del panel visible
            canvas.width = panelLeft.clientWidth;
            canvas.height = panelLeft.clientHeight;

            // Calculamos escala real entre imagen y panel
            const scaleX = canvas.width / img.naturalWidth;
            const scaleY = canvas.height / img.naturalHeight;

            // Variables del rectángulo ajustadas según escala
            let rectX = 100 * scaleX, rectY = 100 * scaleY;
            const rectWidth = 200 * scaleX, rectHeight = 300 * scaleY;
            let isDragging = false;
            let offsetX, offsetY;

            function drawRect() {
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                ctx.strokeStyle = "red";
                ctx.lineWidth = 2;
                ctx.strokeRect(rectX, rectY, rectWidth, rectHeight);
            }
            drawRect();

            canvas.addEventListener("mousedown", (event) => {
                const mouseX = event.offsetX;
                const mouseY = event.offsetY;

                if (mouseX >= rectX && mouseX <= rectX + rectWidth &&
                    mouseY >= rectY && mouseY <= rectY + rectHeight) {
                    
                    isDragging = true;
                    offsetX = mouseX - rectX;
                    offsetY = mouseY - rectY;
                }
            });

            canvas.addEventListener("mousemove", (event) => {
                if (!isDragging) return;
                rectX = event.offsetX - offsetX;
                rectY = event.offsetY - offsetY;
                drawRect();
            });

            canvas.addEventListener("mouseup", () => {
                if (isDragging) {
                    // Convertir coordenadas al sistema basado en la imagen original
                    const finalX = Math.round(rectX / scaleX);
                    const finalY = Math.round(rectY / scaleY);
                    console.log(`Rectángulo fijado en: X=${finalX}, Y=${finalY}`);
                    isDragging = false;
                }
            });

        }, 100);
    });
}
,

        /** Inicialización del Canvas **/
        setupCanvas() {
            const image = this.$refs.image;
            const canvas = this.$refs.canvas;
            const container = this.$refs.container;

            if (!canvas) {
                console.error("Canvas no está disponible aún.");
                return;
            }

            if (image.complete) {
                this.originalWidth = image.naturalWidth;
                this.originalHeight = image.naturalHeight;
                canvas.width = image.naturalWidth;
                canvas.height = image.naturalHeight;
                this.updateAreas();
            } else {
                image.onload = () => {
                    this.originalWidth = image.naturalWidth;
                    this.originalHeight = image.naturalHeight;
                    canvas.width = image.naturalWidth;
                    canvas.height = image.naturalHeight;
                    this.updateAreas();
                };
            }
        },

        /** Procesa las áreas desde el JSON **/
        updateAreas() {
            if (!this.hudData || !this.hudData.Areas) {
                console.error('hudData.Areas es undefined o null');
                return;
            }

            this.clearCanvas();
        },

        /** Detección de áreas con el mouse **/
OnArea_MouseMove(event) {
    const container = this.$refs.container;
    const image = this.$refs.image;
    const rect = container.getBoundingClientRect();

    console.log(`Scroll del panel-left: (${this.$refs.container.scrollLeft}, ${this.$refs.container.scrollTop})`);

    // Ajustamos la posición del mouse considerando el desplazamiento dentro de panel-left
    const x = (event.clientX - rect.left) + container.scrollLeft;
    const y = (event.clientY - rect.top) + container.scrollTop;

    // Convertimos las coordenadas al sistema de referencia de la imagen original
    const adjustedX = x * (image.naturalWidth / image.width);
    const adjustedY = y * (image.naturalHeight / image.height);

    this.currentArea = null;

    for (const area of this.hudData.Areas) {
        if (adjustedX > area.x && adjustedX < area.x + area.width &&
            adjustedY > area.y && adjustedY < area.y + area.height) {
            this.currentArea = area;
            this.drawRect(area);
            break;
        }
    }

    if (!this.currentArea) {
        this.clearCanvas();
    }
},

        /** Limpieza cuando el mouse sale del área **/
        OnArea_MouseLeave() {
            this.clearCanvas();
            this.currentArea = null;
        },

        /** Manejo del clic en un área **/
        OnArea_Click(event, setActiveTab = true) {
            if (this.currentArea) {
                this.clickedArea = this.currentArea;
                this.highlightClickedArea(this.currentArea);

                if (this.currentArea.title !== "XML Editor") {
                    // EventBus.emit('areaClicked', this.currentArea);
                    if (setActiveTab === true) {
                        //EventBus.emit('setActiveTab', 'properties');
                    }
                } else {
                    //EventBus.emit('OnShowXmlEditor', this.currentArea);
                }
            }
        },

        /** Dibuja un rectángulo sobre el área detectada **/
        drawRect(area) {
            const canvas = this.$refs.canvas;
            const ctx = canvas.getContext('2d');
            if (!ctx) {
                console.error("Contexto del canvas no disponible.");
                return;
            }

            this.clearCanvas();

            const reducedHeight = area.height - 5;

            ctx.strokeStyle = this.hudData.Colors.BorderColor;
            ctx.lineWidth = this.hudData.Colors.BorderWidth;
            ctx.strokeRect(area.x, area.y, area.width, reducedHeight);

            ctx.fillStyle = this.hudData.Colors.FontColor;
            ctx.font = this.hudData.Colors.Font;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'top';
            ctx.shadowColor = this.hudData.Colors.ShadowColor;
            ctx.shadowOffsetX = this.hudData.Colors.ShadowOffset;
            ctx.shadowOffsetY = this.hudData.Colors.ShadowOffset;
            ctx.shadowBlur = this.hudData.Colors.ShadowBlur;
            ctx.fillText(area.title, area.x + area.width / 2, area.y + reducedHeight + 5);

            if (this.clickedArea && this.clickedArea.id === area.id) {
                this.highlightClickedArea(area);
            }
        },

        /** Destaca el área seleccionada **/
        highlightClickedArea(area) {
            const canvas = this.$refs.canvas;
            const ctx = canvas.getContext('2d');

            const reducedHeight = area.height - 5;
            ctx.fillStyle = this.hudData.Colors.HighlightColor;
            ctx.fillRect(area.x, area.y, area.width, reducedHeight);

            ctx.strokeStyle = this.hudData.Colors.HighlightBorder;
            ctx.lineWidth = 1;
            ctx.strokeRect(area.x, area.y, area.width, reducedHeight);

            ctx.fillStyle = this.hudData.Colors.HighlightFontColor;
            ctx.font = this.hudData.Colors.Font;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'top';
            ctx.shadowColor = this.hudData.Colors.HighlightShadowColor;
            ctx.shadowOffsetX = this.hudData.Colors.ShadowOffset;
            ctx.shadowOffsetY = this.hudData.Colors.ShadowOffset;
            ctx.shadowBlur = this.hudData.Colors.ShadowBlur;
            ctx.fillText(area.title, area.x + area.width / 2, area.y + reducedHeight + 5);
        },

        /** Limpia el canvas **/
        clearCanvas() {
            const canvas = this.$refs.canvas;
            const ctx = canvas.getContext('2d');
            ctx.clearRect(0, 0, canvas.width, canvas.height);

            if (this.clickedArea) {
                this.highlightClickedArea(this.clickedArea);
            }
        }

        
        // #endregion

    },
    async mounted() {
       /* this.$nextTick(() => {
        setTimeout(() => {
            const canvas = this.$refs.canvasElement;
            if (!canvas) {
                console.error("Error: El canvas no está definido en $refs");
                return;
            }

            console.log("Canvas correctamente referenciado:", canvas);

            canvas.addEventListener("mousedown", (event) => {
                console.log("Canvas_mousedown:", event);
            });
        }, 100);
    });*/


        await this.Initialize();

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
    overflow: hidden; /* Evita scroll en `app` y `layout` */
}

.panel {
    overflow: auto; /* Activa scroll independiente en cada panel */
    height: 100%;
}

.panel-left {
    width: 72%;
    border-right: 1px solid #444;
    position: relative;
    display: flex; 
    justify-content: center; 
    align-items: start; 
    overflow: scroll;  /* Activar scroll horizontal y vertical */
}
.panel-right {
    width: 28%;
    border-left: 1px solid #444;
    position: relative;
}

.image-container {
    max-width: none; /* Para evitar que Vue ajuste el tamaño automáticamente */
    max-height: none;
    pointer-events: none;
}

.overlay-canvas {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    pointer-events: auto;
}
/*
.image-container {
    display: inline-block; 
    max-width: 100%; 
    max-height: none;
}

canvas {
    position: absolute; 
    top: 0;
    left: 0;
    width: 100%;
    height: auto;
    pointer-events: none; 
}
*/

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
