<template>
  <div id="XmlEditorModal" class="modal fade" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1"
    aria-labelledby="staticBackdropLabel" aria-hidden="true">
    <div class="modal-dialog modal-xl">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">XML Editor [ {{ themeName }} ]</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>
        <div class="modal-body">
          <div class="row h-50">

            <!--Left Column-->
            <div class="col-4">

              <!-- 3x3 Grid of Sliders -->
              <div class="row">
                <div class="col-4">
                  <label for="redSlider1">Red: {{ sliderValues[0][0] }}</label>
                  <input type="range" id="redSlider1" class="form-range" v-model="sliderValues[0][0]" min="-2" max="2"
                    step="0.01" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="greenSlider1">Green: {{ sliderValues[0][1] }}</label>
                  <input type="range" id="greenSlider1" class="form-range" v-model="sliderValues[0][1]" min="-2" max="2"
                    step="0.01" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="blueSlider1">Blue: {{ sliderValues[0][2] }}</label>
                  <input type="range" id="blueSlider1" class="form-range" v-model="sliderValues[0][2]" min="-2" max="2"
                    step="0.01" @input="applyFilter">
                </div>
              </div>

              <div class="row">
                <div class="col-4">
                  <label for="redSlider2">Red: {{ sliderValues[1][0] }}</label>
                  <input type="range" id="redSlider2" class="form-range" v-model="sliderValues[1][0]" min="-2" max="2"
                    step="0.01" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="greenSlider2">Green: {{ sliderValues[1][1] }}</label>
                  <input type="range" id="greenSlider2" class="form-range" v-model="sliderValues[1][1]" min="-2" max="2"
                    step="0.01" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="blueSlider2">Blue: {{ sliderValues[1][2] }}</label>
                  <input type="range" id="blueSlider2" class="form-range" v-model="sliderValues[1][2]" min="-2" max="2"
                    step="0.01" @input="applyFilter">
                </div>
              </div>

              <div class="row">
                <div class="col-4">
                  <label for="redSlider3">Red: {{ sliderValues[2][0] }}</label>
                  <input type="range" id="redSlider3" class="form-range" v-model="sliderValues[2][0]" min="-2" max="2"
                    step="0.01" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="greenSlider3">Green: {{ sliderValues[2][1] }}</label>
                  <input type="range" id="greenSlider3" class="form-range" v-model="sliderValues[2][1]" min="-2" max="2"
                    step="0.01" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="blueSlider3">Blue: {{ sliderValues[2][2] }}</label>
                  <input type="range" id="blueSlider3" class="form-range" v-model="sliderValues[2][2]" min="-2" max="2"
                    step="0.01" @input="applyFilter">
                </div>
              </div>

              <hr>

              <!-- XML Input -->
              <div class="row h-50">
                <div class="mb-3">
                  <label for="colorMatrixInput">XML input</label>
                  <textarea id="colorMatrixInput" class="form-control" 
                      rows="4" @input="onColorMatrixPaste" 
                      spellcheck="false" :value="formattedColorMatrix" 
                      style="resize: none; text-align: center; "></textarea>
                </div>
                <hr>
                <div class="btn-group mb-3" role="group" aria-label="Basic example">
                  <button class="btn btn-outline-secondary" type="button" @click="GoToNo2_Click">Get XML Presets</button>
                  <button class="btn btn-outline-secondary" type="button" @click="ShowInfo_Click">What is a Color Matrix?</button>
                </div>                
              </div><!--/XmlInput-->

            </div><!--/LeftColumn-->

            <!-- Right Column -->
            <div class="col-8 position-relative" style="height: 450px;">

              <!-- Tab Headers -->
              <ul id="myTab" class="nav nav-tabs w-100 p-3 position-absolute top-0"  role="tablist">
                <li class="nav-item" role="presentation">
                  <button class="nav-link active" id="home-tab" data-bs-toggle="tab" data-bs-target="#image-tab-pane" type="button" role="tab" aria-controls="image-tab-pane" aria-selected="true">Panels</button>
                </li>
                <li class="nav-item" role="presentation">
                  <button class="nav-link" id="profile-tab" data-bs-toggle="tab" data-bs-target="#profile-tab-pane" type="button" role="tab" aria-controls="profile-tab-pane" aria-selected="false">Color Transformation</button>
                </li>
              </ul>              
              <div id="myTabContent" class="tab-content h-100 w-100" >
                <!-- TabPane for Image -->
                <div class="tab-pane fade show active " id="image-tab-pane" role="tabpanel" aria-labelledby="home-tab" tabindex="0">
                  <!-- Define the SVG filter -->
                  <svg width="0" height="0">
                    <filter id="colorMatrixFilter">
                      <feColorMatrix type="matrix" :values="getMatrixValues"/>
                    </filter>
                  </svg>
                  <!-- Apply the filter to this image -->
                  <img id="targetImage" ref="image" :src="originalImageSrc" 
                      class="position-absolute w-100 p-3 " 
                      :style="{ filter: 'url(#colorMatrixFilter)', top: '80px' }" />
                </div>

                <!-- Tab Pane for Profile -->
                <div class="tab-pane fade h-100 w-100" id="profile-tab-pane" role="tabpanel" aria-labelledby="profile-tab" tabindex="0">
                  <!-- 6x6 Grid for Individual Color Transformations-->
                  <div class="container text-center position-absolute w-100 p-3 " style="top:80px">
                    <div class="row">
                      <div class="col"></div>
                      <div class="col">Elite Default</div>
                      <div class="col"></div>
                      <div class="col">XML Transform</div>
                      <div class="col">RGB Value</div>
                      <div class="col">HEX Value</div>
                    </div>

                    <div class="row" style="height: 50px;">
                      <div class="col d-flex justify-content-center align-items-center">Orange Elements</div>
                      <div class="col border" :style="{ 'background-color': colorTranform[0].default }"></div>
                      <div class="col">→</div>
                      <div class="col border" :style="{ 'background-color': colorTranform[0].hex }"></div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[0].rgb }}</div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[0].hex }}</div>
                    </div>

                    <div class="row" style="height: 50px;">
                      <div class="col d-flex justify-content-center align-items-center">White Elements</div>
                      <div class="col border" :style="{ 'background-color': colorTranform[1].default }"></div>
                      <div class="col">→</div>
                      <div class="col border" :style="{ 'background-color': colorTranform[1].hex }"></div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[1].rgb }}</div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[1].hex }}</div>
                    </div>

                    <div class="row" style="height: 50px;">
                      <div class="col d-flex justify-content-center align-items-center">Red Elements</div>
                      <div class="col border" :style="{ 'background-color': colorTranform[2].default }"></div>
                      <div class="col">→</div>
                      <div class="col border" :style="{ 'background-color': colorTranform[2].hex }"></div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[2].rgb }}</div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[2].hex }}</div>
                    </div>

                    <div class="row" style="height: 50px;">
                      <div class="col d-flex justify-content-center align-items-center">Cyan Elements</div>
                      <div class="col border" :style="{ 'background-color': colorTranform[3].default }"></div>
                      <div class="col">→</div>
                      <div class="col border" :style="{ 'background-color': colorTranform[3].hex }"></div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[3].rgb }}</div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[3].hex }}</div>
                    </div>

                    <div class="row" style="height: 50px;">
                      <div class="col d-flex justify-content-center align-items-center">Custom Color</div>
                      <div class="col border" :style="{ 'background-color': colorTranform[4].default }">
                        <input type="color" v-model="colorTranform[4].default" @input="onColorChange"></div>
                      <div class="col">→</div>
                      <div class="col border" :style="{ 'background-color': colorTranform[4].hex }"></div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[4].rgb }}</div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[4].hex }}</div>
                    </div>

                    <div class="alert alert-info  m-2" role="alert">Individual colors are transformed using the XML filter.</div>
                  </div>
                  
                </div>
              </div><!--/TabContent-->
            
            </div><!--/RightColumn-->
          </div><!--/Row-->
        </div><!--/ModalBody-->

        <div class="modal-footer">
          <div class="alert alert-warning alert-dismissible fade show" role="alert">
            Preview image may not accurately represent the final product.
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
          </div>
          <div class="btn-group" role="group">
            <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancel</button>
            <button type="button" class="btn btn-primary" data-bs-dismiss="modal" @click="save">Save changes</button>
          </div>
        </div><!--/Footer-->

      </div>
    </div>
  </div>
</template>
  
<script>
import { ref } from 'vue';
import EventBus from '../../EventBus.js';

export default {
  name: 'XmlEditor',
  data() {
    return {
      sliderValues: [
        [1.0, 0.0, 0.0], // Red[0]:   Red[0][0], Green[0][1], Blue[0][2]
        [0.0, 1.0, 0.0], // Green[1]: Red[1][0], Green[1][1], Blue[1][2]
        [0.0, 0.0, 1.0], // Blue[2]:  Red[2][0], Green[2][1], Blue[2][2]
      ],
      colorMatrix: '', // Placeholder for copied color matrix
      originalImageSrc: '../../images/xml-base.jpg',
      themeName: '', //For Dialog's Title
      colorTranform: [ 
        { id: 'Orange', default: '#FFA500', rgb: '255;165;0',   hex: '#FFA500'},
        { id: 'White',  default: '#FFFFFF', rgb: '255;255;255', hex: '#FFFFFF'},
        { id: 'Red',    default: '#FF0000', rgb: '255;0;0',     hex: '#FF0000'},
        { id: 'Cyan',   default: '#00FFFF', rgb: '0;255;255',   hex: '#00FFFF'},
        { id: 'Custom', default: '#008000', rgb: '0;128;0',     hex: '#008000'},
      ],
    };
  },
  computed: {
    formattedColorMatrix() {
      /* Converts the RGB Matrix into XML Format */
      const redMatrix = `<MatrixRed>${this.sliderValues[0].join(', ')}</MatrixRed>`;
      const greenMatrix = `<MatrixGreen>${this.sliderValues[1].join(', ')}</MatrixGreen>`;
      const blueMatrix = `<MatrixBlue>${this.sliderValues[2].join(', ')}</MatrixBlue>`;

      return `${redMatrix}\n${greenMatrix}\n${blueMatrix}`; //<- This is directly applied into the textarea for XML input
    },
    getMatrixValues() {
      /* Converts the RGB Matrix into a 5x4 'feColorMatrix' -> https://developer.mozilla.org/en-US/docs/Web/SVG/Element/feColorMatrix   */
      //                                    R                               G                               B        A W
      let matrixString = this.sliderValues[0][0] + ' ' + this.sliderValues[0][1] + ' ' + this.sliderValues[0][2] + ' 0 0 '; //<- R
      matrixString += this.sliderValues[1][0] + ' ' + this.sliderValues[1][1] + ' ' + this.sliderValues[1][2] + ' 0 0 '; //<- G
      matrixString += this.sliderValues[2][0] + ' ' + this.sliderValues[2][1] + ' ' + this.sliderValues[2][2] + ' 0 0 '; //<- B
      matrixString += '0 0 0 1 0'; //<- Alpha

      return matrixString; //<- This is directly applied into the SVG filter
    }
  },
  methods: {
    async OnInitialize() {
      this.loadImage();

      //this.filters = this.getColorMatrixFilters();
      //this.filtersList = this.filters.getFilterNames();
      //console.log(this.filtersList);
      //const deuteranopiaFilter = this.filters('brightness'); 
      //console.log(deuteranopiaFilter(1));

      /*watch(this.colorMatrix, () => {
        parseColorMatrix(); 
      });*/
    },
    getLabel(colIndex) {
      const labels = ['Red', 'Green', 'Blue'];
      return labels[colIndex];
    },
    ShowModal(data) {
      this.sliderValues = data.matrix;
      this.themeName = data.name;
      const myModal = new bootstrap.Modal('#XmlEditorModal', { keyboard: false });
      this.parseColorMatrix();
      myModal.show();
    },
    save() {
      console.log(this.sliderValues);
      this.$emit('onCloseModal', this.sliderValues); //<- Evento escuchado en App.vue
      this.sliderValues = [
        [1.0, 0.0, 0.0],
        [0.0, 1.0, 0.0],
        [0.0, 0.0, 1.0],
      ];
      this.$refs.image.src = this.originalImageSrc;
    },
    async loadImage() {
      const img = await window.api.getAssetFileUrl('images/xml-base.jpg');
      this.originalImageSrc = img;
    },
    applyFilter() {
      // Reset the image source to the original
      this.$refs.image.src = this.originalImageSrc;
      this.TransformXMLColors();

      // Force Vue to reapply the filter
      this.$nextTick(() => {
        //:style="{ filter: 'url(#colorMatrixFilter)', top: '80px' }" 
       this.$refs.image.style.filter = 'url(#colorMatrixFilter)';
       this.applyColorMatrix();
      });
    },
    applyColorMatrix() {
      const img = this.$refs.image; // still showing originalImageSrc
      const width = img.naturalWidth;
      const height = img.naturalHeight;

      // create an off-screen canvas
      const canvas = document.createElement('canvas');
      canvas.width = width;
      canvas.height = height;
      const ctx = canvas.getContext('2d');

      // draw the original image as the starting point
      ctx.drawImage(img, 0, 0, width, height);

      // get pixel data
      const imageData = ctx.getImageData(0, 0, width, height);
      const data = imageData.data;

      // build 3x3 matrix from sliders
      const matrix = [
        [this.sliderValues[0][0], this.sliderValues[0][1], this.sliderValues[0][2]],
        [this.sliderValues[1][0], this.sliderValues[1][1], this.sliderValues[1][2]],
        [this.sliderValues[2][0], this.sliderValues[2][1], this.sliderValues[2][2]],
      ];

      // loop over pixels
      for (let i = 0; i < data.length; i += 4) {
        const r = data[i];
        const g = data[i + 1];
        const b = data[i + 2];

        data[i] = r * matrix[0][0] + g * matrix[0][1] + b * matrix[0][2];
        data[i + 1] = r * matrix[1][0] + g * matrix[1][1] + b * matrix[1][2];
        data[i + 2] = r * matrix[2][0] + g * matrix[2][1] + b * matrix[2][2];
        // alpha stays the same
      }

      ctx.putImageData(imageData, 0, 0);

      // now set the result somewhere — e.g., show in another <img>
      //this.filteredImageSrc = canvas.toDataURL();
      this.$refs.image.src  = canvas.toDataURL();
    },

    GoToNo2_Click() {
      window.api.openUrlInBrowser('https://forums.frontier.co.uk/threads/no2o-the-definitive-list-of-1-7-2-2-compatible-hud-colour-color-configs-please-add-yours.259311/');
    },
    ShowInfo_Click() {
      window.api.openUrlInBrowser('https://lisyarus.github.io/blog/posts/transforming-colors-with-matrices.html');
    },
    onColorMatrixPaste(event) {
      // Set the pasted color matrix
      this.colorMatrix = event.target.value;
      // Parse the color matrix and update slider values
      this.parseColorMatrix();
    },
    onColorChange(event) {
      const newColor = event.target.value;
      this.colorTranform[4].default = newColor;
      this.TransformXMLColors();
      //console.log(`Selected color: ${newColor}`);
    },
    parseColorMatrix() {
      const redMatrix = this.colorMatrix.match(/<MatrixRed>\s*(.*?)\s*<\/MatrixRed>/);
      const greenMatrix = this.colorMatrix.match(/<MatrixGreen>\s*(.*?)\s*<\/MatrixGreen>/);
      const blueMatrix = this.colorMatrix.match(/<MatrixBlue>\s*(.*?)\s*<\/MatrixBlue>/);

      if (redMatrix && greenMatrix && blueMatrix) {
        this.sliderValues[0] = redMatrix[1].split(',').map(Number);
        this.sliderValues[1] = greenMatrix[1].split(',').map(Number);
        this.sliderValues[2] = blueMatrix[1].split(',').map(Number);
      }
      this.TransformXMLColors();
      this.applyFilter();      
    },
    /*  THIS IS A MANUAL WAY TO APPLY THE XML MATRIX
        applyColorMatrix() {
          const canvas = this.$refs.canvas;
          const ctx = canvas.getContext('2d');
          const img = this.$refs.image;
    
          // Check if image is loaded
          if (!img.complete) {
            console.error('Image is not loaded yet!');
            return;
          }
    
          // Ensure canvas size matches image
          canvas.width = img.width;
          canvas.height = img.height;
    
          // Draw the image onto the canvas
          ctx.drawImage(img, 0, 0);
    
          // Get the image data
          const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
          const data = imageData.data;
    
          // Define the color matrix (5x5 matrix for RGBA)
          const matrix = [
            [this.sliderValues[0][0], this.sliderValues[0][1], this.sliderValues[0][2], 0, 0],
            [this.sliderValues[1][0], this.sliderValues[1][1], this.sliderValues[1][2], 0, 0],
            [this.sliderValues[2][0], this.sliderValues[2][1], this.sliderValues[2][2], 0, 0],
            [0, 0, 0, 1, 0],
            [0, 0, 0, 0, 1]
          ];
    
          for (let i = 0; i < data.length; i += 4) {
            const r = data[i];
            const g = data[i + 1];
            const b = data[i + 2];
            const a = data[i + 3];
    
            // Apply the color matrix
            const newR =
              matrix[0][0] * r +
              matrix[0][1] * g +
              matrix[0][2] * b +
              matrix[0][3] * a +
              matrix[0][4];
    
            const newG =
              matrix[1][0] * r +
              matrix[1][1] * g +
              matrix[1][2] * b +
              matrix[1][3] * a +
              matrix[1][4];
    
            const newB =
              matrix[2][0] * r +
              matrix[2][1] * g +
              matrix[2][2] * b +
              matrix[2][3] * a +
              matrix[2][4];
    
            // Keep alpha unchanged (assuming premultiplied alpha)
            const newA = a;
    
            // Clamp values to 0-255
            data[i] =     Math.max(0, Math.min(255, newR));
            data[i + 1] = Math.max(0, Math.min(255, newG));
            data[i + 2] = Math.max(0, Math.min(255, newB));
            // data[i + 3] = Math.max(0, Math.min(255, newA)); // Uncomment if you need to modify alpha
    
          }
    
          ctx.putImageData(imageData, 0, 0);
        },*/

    rgbToHex(r, g, b) {
      function componentToHex(c) {
        let hex = c.toString(16);
        return hex.length === 1 ? "0" + hex : hex;
      }
      return "#" + componentToHex(r) + componentToHex(g) + componentToHex(b);
    },
    hexToRgb(hex) {
      // Remove the '#' if it's there
      hex = hex.replace(/^#/, '');

      // Convert the string to an integer
      let bigint = parseInt(hex, 16);

      let r = (bigint >> 16) & 255;
      let g = (bigint >> 8) & 255;
      let b = bigint & 255;

      return { red: r, green: g, blue: b };
    },
    transformColorFromXML(xml, percentages) {
      let ret = { r: 255, g: 255, b: 255 }; // White color in RGB format

      try {
        // 1. Get Raw Values
        let rawR = (xml.red.red * percentages.red) + (xml.green.red * percentages.green) + (xml.blue.red * percentages.blue);
        let rawG = (xml.red.green * percentages.red) + (xml.green.green * percentages.green) + (xml.blue.green * percentages.blue);
        let rawB = (xml.red.blue * percentages.red) + (xml.green.blue * percentages.green) + (xml.blue.blue * percentages.blue);

        // 2. Normalize values
        let maxV = Math.max(rawR, rawG, rawB);

        let normR = Math.max(0.0, maxV > 1.0 ? (1 / maxV) * rawR : rawR);
        let normG = Math.max(0.0, maxV > 1.0 ? (1 / maxV) * rawG : rawG);
        let normB = Math.max(0.0, maxV > 1.0 ? (1 / maxV) * rawB : rawB);

        // 3. Convert to RGB values
        const rgbR = Math.round(normR * 255);
        const rgbG = Math.round(normG * 255);
        const rgbB = Math.round(normB * 255);
        const hex = this.rgbToHex(rgbR, rgbG, rgbB)

        ret = { r: rgbR, g: rgbG, b: rgbB, h: hex };
      } catch (ex) {
        console.error(`ERROR: ${ex.message}\n${ex.stack}`);
        EventBus.emit('ShowError', ex);
      }
      return ret;
    },
    TransformXMLColors() {
      try {
        const xmlValues = 
				{
					red:    { red: this.sliderValues[0][0], green: this.sliderValues[0][1], blue: this.sliderValues[0][2] },
					green:  { red: this.sliderValues[1][0], green: this.sliderValues[1][1], blue: this.sliderValues[1][2] },
					blue:   { red: this.sliderValues[2][0], green: this.sliderValues[2][1], blue: this.sliderValues[2][2] },
				};

        // Orange Transformation:
				let Percentages = { red: 1.0, green: 0.5, blue: 0.0 };
				const TransformColor_Orange = this.transformColorFromXML(xmlValues, Percentages);
        this.colorTranform[0].hex = TransformColor_Orange.h;
        this.colorTranform[0].rgb = TransformColor_Orange.r + ';' + TransformColor_Orange.g + ';' + TransformColor_Orange.b ;
        //console.log('TransformColor_Orange:', TransformColor_Orange);

        // White Transformation:
				Percentages = { red: 1.0, green: 1.0, blue: 1.0 };
				const TransformColor_White = this.transformColorFromXML(xmlValues, Percentages);
        this.colorTranform[1].hex = TransformColor_White.h;
        this.colorTranform[1].rgb = TransformColor_White.r + ';' + TransformColor_White.g + ';' + TransformColor_White.b ;
        //console.log('TransformColor_White:', TransformColor_White);

        // Red Transformation:
				Percentages = { red: 1.0, green: 0.0, blue: 0.0 };
				const TransformColor_Red = this.transformColorFromXML(xmlValues, Percentages);
        this.colorTranform[2].hex = TransformColor_Red.h;
        this.colorTranform[2].rgb = TransformColor_Red.r + ';' + TransformColor_Red.g + ';' + TransformColor_Red.b ;
        //console.log('TransformColor_Red:', TransformColor_Red);

        //Cyan Transformation:
				Percentages = { red: 0.0, green: 1.0, blue: 1.0 };
				const TransformColor_Cyan = this.transformColorFromXML(xmlValues, Percentages);
        this.colorTranform[3].hex = TransformColor_Cyan.h;
        this.colorTranform[3].rgb = TransformColor_Cyan.r + ';' + TransformColor_Cyan.g + ';' + TransformColor_Cyan.b ;
        //console.log('TransformColor_Cyan:', TransformColor_Cyan);

        // Custom Color Transformation:
				const CustomColor = this.hexToRgb(this.colorTranform[4].default);
        Percentages = { red: CustomColor.red / 255, green: CustomColor.green / 255, blue: CustomColor.blue / 255 };
				const TransformColor_Custom = this.transformColorFromXML(xmlValues, Percentages);
        this.colorTranform[4].hex = TransformColor_Custom.h;
        this.colorTranform[4].rgb = TransformColor_Custom.r + ';' + TransformColor_Custom.g + ';' + TransformColor_Custom.b ;
        //console.log('TransformColor_Custom:', TransformColor_Custom);

      } catch (ex) {
        console.error(`ERROR: ${ex.message}\n${ex.stack}`);
        EventBus.emit('ShowError', ex);
      }
    },



  },
  mounted() {
    this.OnInitialize();
  },
  beforeUnmount() { }
}
</script>
<style scoped></style>