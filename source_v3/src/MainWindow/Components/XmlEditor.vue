<template>
  <div id="XmlEditorModal" class="modal fade" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1"
    aria-labelledby="staticBackdropLabel" aria-hidden="true">
    <div class="modal-dialog modal-xl">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">XML Editor</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>
        <div class="modal-body">
          <div class="row h-50">
            <div class="col-4">

              <!-- 3x3 Grid of Sliders -->
              <div class="row">
                <div class="col-4">
                  <label for="redSlider1">Red: {{ sliderValues[0][0] }}</label>
                  <input type="range" id="redSlider1" class="form-range" v-model="sliderValues[0][0]" min="-1" max="2"
                    step="0.1" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="greenSlider1">Green: {{ sliderValues[0][1] }}</label>
                  <input type="range" id="greenSlider1" class="form-range" v-model="sliderValues[0][1]" min="-1" max="2"
                    step="0.01" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="blueSlider1">Blue: {{ sliderValues[0][2] }}</label>
                  <input type="range" id="blueSlider1" class="form-range" v-model="sliderValues[0][2]" min="-1" max="2"
                    step="0.01" @input="applyFilter">
                </div>
              </div>

              <div class="row">
                <div class="col-4">
                  <label for="redSlider2">Red: {{ sliderValues[1][0] }}</label>
                  <input type="range" id="redSlider2" class="form-range" v-model="sliderValues[1][0]" min="-1" max="2"
                    step="0.01" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="greenSlider2">Green: {{ sliderValues[1][1] }}</label>
                  <input type="range" id="greenSlider2" class="form-range" v-model="sliderValues[1][1]" min="-1" max="2"
                    step="0.01" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="blueSlider2">Blue: {{ sliderValues[1][2] }}</label>
                  <input type="range" id="blueSlider2" class="form-range" v-model="sliderValues[1][2]" min="-1" max="2"
                    step="0.01" @input="applyFilter">
                </div>
              </div>

              <div class="row">
                <div class="col-4">
                  <label for="redSlider3">Red: {{ sliderValues[2][0] }}</label>
                  <input type="range" id="redSlider3" class="form-range" v-model="sliderValues[2][0]" min="-1" max="2"
                    step="0.01" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="greenSlider3">Green: {{ sliderValues[2][1] }}</label>
                  <input type="range" id="greenSlider3" class="form-range" v-model="sliderValues[2][1]" min="-1" max="2"
                    step="0.01" @input="applyFilter">
                </div>
                <div class="col-4">
                  <label for="blueSlider3">Blue: {{ sliderValues[2][2] }}</label>
                  <input type="range" id="blueSlider3" class="form-range" v-model="sliderValues[2][2]" min="-1" max="2"
                    step="0.01" @input="applyFilter">
                </div>
              </div>

              <!-- XML Input -->
              <div class="row h-50">
                <div class="mb-3">
                  <label for="colorMatrixInput">XML input</label>
                  <textarea id="colorMatrixInput" class="form-control" rows="4" @input="onColorMatrixPaste"
                    spellcheck="false" :value="formattedColorMatrix"></textarea>
                </div>

                <div class="mb-3">
                  <button class="btn btn-outline-secondary" type="button" id="button-addon1">Get XMLs from the No2O site</button>
                  <!--<div class="input-group mb-3">
                    <label class="input-group-text" for="cboFilters">Filters:</label>
                    <select id="cboFilters" class="form-select" aria-label="Default select example"
                      v-model="selectedFilter" @change="filterSelected">
                      <option value="" disabled>Select a filter</option>
                      <option v-for="filter in filtersList" :key="filter" :value="filter">{{ filter }}</option>
                    </select>
                    <input type="text" class="form-control" aria-label="0">
                    <button class="btn btn-outline-secondary" type="button" id="button-addon1">Button</button>
                  </div> -->
                </div>
              </div>
            </div><!--/XmlInput-->

            <!-- Right Side Column -->
            <div class="col-8 d-flex justify-content-center align-items-center ">

              <!-- Define the SVG filter -->
              <svg width="0" height="0">
                <filter id="colorMatrixFilter">
                  <feColorMatrix type="matrix" :values="getMatrixValues"/>
                </filter>
              </svg>
              <!-- Apply the filter to the image -->
              <img id="targetImage" ref="image" :src="originalImageSrc" class="border" :style="{ filter: 'url(#colorMatrixFilter)' }" alt="...">
            
            </div><!--/Right Column-->

          </div><!--/Row-->
        </div><!--/modal-body-->

        <div class="modal-footer">
          <div class="btn-group" role="group">
            <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Close</button>
            <button type="button" class="btn btn-primary" data-bs-dismiss="modal" @click="save">Save changes</button>
          </div>
        </div><!--/Footer-->

      </div>
    </div>
  </div>
</template>
  
<script>
export default {
  name: 'XmlEditor',
  props: {},
  data() {
    return {
      sliderValues: [
        [1.0, 0.0, 0.0], // Row[0]: Red, Green, Blue
        [0.0, 1.0, 0.0], // Row[1]: Red, Green, Blue
        [0.0, 0.0, 1.0], // Row[2]: Red, Green, Blue
      ],
      colorMatrix: '', // Placeholder for copied color matrix
      originalImageSrc: '../../images/xml-base.jpg',
      /* filters: '',
       filtersList: '',*/
    };
  },
  computed: {
    formattedColorMatrix() {
      const redMatrix = `<MatrixRed>${this.sliderValues[0].join(', ')}</MatrixRed>`;
      const greenMatrix = `<MatrixGreen>${this.sliderValues[1].join(', ')}</MatrixGreen>`;
      const blueMatrix = `<MatrixBlue>${this.sliderValues[2].join(', ')}</MatrixBlue>`;
      return `${redMatrix}\n${greenMatrix}\n${blueMatrix}`;
    },
    getMatrixValues() {      
      //                                   R                              G                         B             A M
      let matrixString = this.sliderValues[0][0] + ' ' + this.sliderValues[0][1] + ' ' + this.sliderValues[0][2] + ' 0 0 '; //<- R
      matrixString +=    this.sliderValues[1][0] + ' ' + this.sliderValues[1][1] + ' ' + this.sliderValues[1][2] + ' 0 0 '; //<- G
      matrixString +=    this.sliderValues[2][0] + ' ' + this.sliderValues[2][1] + ' ' + this.sliderValues[2][2] + ' 0 0 '; //<- B
      matrixString += '0 0 0 1 0'; //<- A

      console.log('Matrix values:', matrixString);
      return matrixString;
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
    ShowModal(matrix) {
      this.sliderValues = matrix;
      const myModal = new bootstrap.Modal('#XmlEditorModal', { keyboard: false });
      this.parseColorMatrix();
      myModal.show();
    },
    save() {
      console.log(this.sliderValues);
      this.$emit('onCloseModal', this.sliderValues);
    },

    filterSelected() {
      if (this.selectedFilter) {
        // Get the selected filter function
        const selectedFilterFunction = this.filters.getFilterByName(this.selectedFilter);

        // Apply the filter (example: if you have an image object)
        if (selectedFilterFunction) {
          // Assuming you have an image object and a function to apply filters
          //this.applyFilterToImage(this.image, selectedFilterFunction); 
          console.log(selectedFilterFunction);
        }
      }
    },

    /** Crea una Matrix de Identidad de Color.
     * @returns {number[][]} RGB color Matrix Elements
     */
    CreateIdentityMatrix() {
      let colorMatrixElements;
      try {
        /* R=Red, G=Green, B=Blue, A=Alpha, I=Intensity */
        //--------------------------------------------------------
        //| RR (0,0) | RG (0,1) | RB (0,2) | RA (0,3) | RI (0,4) |  <- 0,4 Debe ser Siempre 0
        //| GR (1,0) | GG (1,1) | GB (1,2) | GA (1,3) | GI (1,4) |  <- 1,4 Debe ser Siempre 0
        //| BR (2,0) | BG (2,1) | BB (2,2) | BA (2,3) | BI (2,4) |  <- 2,4 Debe ser Siempre 0
        //| AR (3,0) | AG (3,1) | AB (3,2) | AA (3,3) | AI (3,4) |  <- 3,4 Debe ser Siempre 0
        //| IR (4,0) | IG (4,1) | IB (4,2) | IA (4,3) | II (4,4) |  <- 4,4 Debe ser siempre 1
        //--------------------------------------------------------
        // 3,3 Determina la Transparencia Global de la Imagen

        colorMatrixElements = [
          [1, 0, 0, 0, 0],    // red scaling 
          [0, 1, 0, 0, 0],    // green scaling 
          [0, 0, 1, 0, 0],    // blue scaling 
          [0, 0, 0, 1, 0],    // alpha scaling 
          [0, 0, 0, 0, 1]     // intensity scaling
        ];
      } catch (error) {
        throw error;
      }
      return colorMatrixElements;
    },

    colorMatrixValues() {
      const colorMatrix = [
        [this.sliderValues[0][0], this.sliderValues[0][1], this.sliderValues[0][2], 0, 0],    // red scaling 
        [this.sliderValues[1][0], this.sliderValues[1][1], this.sliderValues[1][2], 0, 0],    // green scaling 
        [this.sliderValues[2][0], this.sliderValues[2][1], this.sliderValues[2][2], 0, 0],    // blue scaling 
        [0, 0, 0, 1, 0],    // alpha scaling 
        [0, 0, 0, 0, 1]     // intensity scaling
      ];

      return colorMatrix.join(' ');
    },
    onColorMatrixPaste(event) {
      // Set the pasted color matrix
      this.colorMatrix = event.target.value;
      // Parse the color matrix and update slider values
      this.parseColorMatrix();
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
      this.applyFilter();
    },
/*
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
    applyFilter() {
      // Reset the image source to the original
      this.$refs.image.src = this.originalImageSrc;
      
      // Force Vue to reapply the filter
      this.$nextTick(() => {
        this.$refs.image.style.filter = 'url(#colorMatrixFilter)';
      });
    },

    /*
    applyColorMatrix() {
      const canvas = this.$refs.canvas;
      const ctx = canvas.getContext('2d');
      const img = this.$refs.image;

      // Draw the image onto the canvas
      canvas.width = img.width;
      canvas.height = img.height;
      ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

      // Get the image data
      const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
      const data = imageData.data;

      // Define the color matrix (5x5 matrix for RGBA)
      const matrix = [
        [this.sliderValues[0][0], this.sliderValues[0][1], this.sliderValues[0][2], 0, 0],    // red scaling 
        [this.sliderValues[1][0], this.sliderValues[1][1], this.sliderValues[1][2], 0, 0],    // green scaling 
        [this.sliderValues[2][0], this.sliderValues[2][1], this.sliderValues[2][2], 0, 0],    // blue scaling 
        [0, 0, 0, 1, 0],    // alpha scaling 
        [0, 0, 0, 0, 1]     // intensity scaling
      ];
      //console.log('matrix', matrix);

      // Apply the color matrix transformation
      for (let i = 0; i < data.length; i += 4) {
        const red =   data[i];
        const green = data[i + 1];
        const blue =  data[i + 2];
        const alpha = data[i + 3];

        // Perform matrix multiplication for each color channel
        const newRed =    matrix[0][0] * red + matrix[0][1] * green + matrix[0][2] * blue + matrix[0][3] * alpha + matrix[0][4];
        const newGreen =  matrix[1][0] * red + matrix[1][1] * green + matrix[1][2] * blue + matrix[1][3] * alpha + matrix[1][4];
        const newBlue =   matrix[2][0] * red + matrix[2][1] * green + matrix[2][2] * blue + matrix[2][3] * alpha + matrix[2][4];
        const newAlpha =  matrix[3][0] * red + matrix[3][1] * green + matrix[3][2] * blue + matrix[3][3] * alpha + matrix[3][4];

        // Clamp values to 0-255
        data[i] =     Math.max(0, Math.min(255, newRed));
        data[i + 1] = Math.max(0, Math.min(255, newGreen));
        data[i + 2] = Math.max(0, Math.min(255, newBlue));
        data[i + 3] = Math.max(0, Math.min(255, newAlpha));
      }

      // Put the image data back onto the canvas
      ctx.putImageData(imageData, 0, 0);
    },

    */

    crop(value, min, max) {
      value = Math.max(value, min);
      value = Math.min(value, max);
      return value;
    },
    multiply(rgba, m) {
      var ret = [], i, row;
      for (i = 0; i < 4; i++) {
        row = 5 * i;
        ret[i] = (rgba[0] * m[row + 0]) + (rgba[1] * m[row + 1]) + (rgba[2] * m[row + 2]) + (rgba[3] * m[row + 3]) + m[row + 4];
      }
      return cleanRGBA(ret);
    },

    /**
       * @name transform
       * @param {array} rgba An RGBA array
       * @param {string} filter The name of the filter function
       * @param {number|array} value The value to pass to the filter function
       * @example
       * var result1 = colorMatrix.transform([255, 0, 0, 255], 'invert');
       * var result2 = colorMatrix.transform([255, 0, 0, 255], 'hueRotate', 180);
       */
    transform(rgba, filter, value) {
      return multiply(rgba, (api.getFilter(filter))(value));
    },

    /*
        applyColorMatrix() {
          const canvas = this.$refs.canvas;
          const ctx = canvas.getContext('2d');
          const img = this.$refs.image;
    
          // Draw the image onto the canvas
          canvas.width = img.width;
          canvas.height = img.height;
          ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
    
          // Get the image data
          const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
          const data = imageData.data;
    
          // Define the color matrix (5x5 matrix for RGBA)
          const matrix = [
            [this.sliderValues[0][0], this.sliderValues[0][1], this.sliderValues[0][2], 0, 0],    // red scaling 
            [this.sliderValues[1][0], this.sliderValues[1][1], this.sliderValues[1][2], 0, 0],    // green scaling 
            [this.sliderValues[2][0], this.sliderValues[2][1], this.sliderValues[2][2], 0, 0],    // blue scaling 
            [0, 0, 0, 1, 0],    // alpha scaling 
            [0, 0, 0, 0, 1]     // intensity scaling
          ];
    
          // Apply the color matrix transformation
          for (let i = 0; i < data.length; i += 4) {
            const red =   data[i];
            const green = data[i + 1];
            const blue =  data[i + 2];
            const alpha = data[i + 3];
    
            // Perform matrix multiplication for each color channel
            const newRed =    matrix[0][0] * red + matrix[0][1] * green + matrix[0][2] * blue + matrix[0][3] * alpha + matrix[0][4];
            const newGreen =  matrix[1][0] * red + matrix[1][1] * green + matrix[1][2] * blue + matrix[1][3] * alpha + matrix[1][4];
            const newBlue =   matrix[2][0] * red + matrix[2][1] * green + matrix[2][2] * blue + matrix[2][3] * alpha + matrix[2][4];
            const newAlpha =  matrix[3][0] * red + matrix[3][1] * green + matrix[3][2] * blue + matrix[3][3] * alpha + matrix[3][4];  
    
            // Clamp values to 0-255
            data[i] =     Math.max(0, Math.min(255, newRed));
            data[i + 1] = Math.max(0, Math.min(255, newGreen));
            data[i + 2] = Math.max(0, Math.min(255, newBlue));
            data[i + 3] = Math.max(0, Math.min(255, newAlpha));
    
          }
    
          // Put the image data back onto the canvas
          ctx.putImageData(imageData, 0, 0);
        },*/



    async loadImage() {
      this.originalImageSrc = await window.api.getAssetFileUrl('images/xml-base.jpg');
      const img = this.$refs.image;
      img.onload = () => {
        this.applyFilter();
      };
      if (img.complete) {
        this.applyFilter();
      }
    },

    getColorMatrixFilters() {
      const filters = {
        'normal': function () {
          return [
            1, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        /**
         * @name matrix
         * @param {array|string} values A list of 20 matrix values (a00 a01 a02 a03 a04 a10 a11 ... a34), separated by whitespace and/or a comma.
         */
        'matrix': function (v) {
          return v;
        },
        /**
         * @name saturate
         * @see http://www.w3.org/TR/SVG/filters.html#feColorMatrixElement
         * @see https://github.com/jcupitt/gegl-vips/blob/master/operations/common/svg-saturate.c
         * @param {number} value a single real number value (0 to 1)
         */
        'saturate': function (v) {
          v = v || 0;
          return [
            0.213 + 0.787 * v, 0.715 - 0.715 * v, 0.072 - 0.072 * v, 0, 0,
            0.213 - 0.213 * v, 0.715 + 0.285 * v, 0.072 - 0.072 * v, 0, 0,
            0.213 - 0.213 * v, 0.715 - 0.715 * v, 0.072 + 0.928 * v, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        /**
         * @name hueRotate
         * @see http://www.w3.org/TR/SVG/filters.html#feColorMatrixElement
         * @see https://github.com/jcupitt/gegl-vips/blob/master/operations/common/svg-huerotate.c
         * @see https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/values
         * @param {number} value a single real number value (in degrees)
         */
        'hueRotate': function (v) {
          v = v || 0;
          var a00 = (0.213) + (Math.cos(v) * 0.787) - (Math.sin(v) * 0.213);
          var a01 = (0.715) - (Math.cos(v) * 0.715) - (Math.sin(v) * 0.715);
          var a02 = (0.072) - (Math.cos(v) * 0.072) + (Math.sin(v) * 0.928);
          var a10 = (0.213) - (Math.cos(v) * 0.213) + (Math.sin(v) * 0.143);
          var a11 = (0.715) + (Math.cos(v) * 0.285) + (Math.sin(v) * 0.140);
          var a12 = (0.072) - (Math.cos(v) * 0.072) - (Math.sin(v) * 0.283);
          var a20 = (0.213) - (Math.cos(v) * 0.213) - (Math.sin(v) * 0.787);
          var a21 = (0.715) - (Math.cos(v) * 0.715) + (Math.sin(v) * 0.715);
          var a22 = (0.072) + (Math.cos(v) * 0.928) + (Math.sin(v) * 0.072);
          return [
            a00, a01, a02, 0, 0,
            a10, a11, a12, 0, 0,
            a20, a21, a22, 0, 0,
            0, 0, 0, 1, 0,
          ];
        },
        /**
         * @name luminanceToAlpha
         * @see http://www.w3.org/TR/SVG/filters.html#feColorMatrixElement
         * @see https://github.com/jcupitt/gegl-vips/blob/master/operations/common/svg-luminancetoalpha.c
         */
        'luminanceToAlpha': function () {
          return [
            0, 0, 0, 0, 0,
            0, 0, 0, 0, 0,
            0, 0, 0, 0, 0,
            0.2125, 0.7154, 0.0721, 0, 0
          ];
        },
        'invert': function () {
          return [-1, 0, 0, 0, 255,
            0, -1, 0, 0, 255,
            0, 0, -1, 0, 255,
            0, 0, 0, 1, 0
          ];
        },
        'grayscale': function () {
          return [
            0.299, 0.587, 0.114, 0, 0,
            0.299, 0.587, 0.114, 0, 0,
            0.299, 0.587, 0.114, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'sepia': function () {
          return [
            0.393, 0.769, 0.189, 0, 0,
            0.349, 0.686, 0.168, 0, 0,
            0.272, 0.534, 0.131, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'nightvision': function () {
          return [
            0.1, 0.4, 0, 0, 0,
            0.3, 1, 0.3, 0, 0,
            0, 0.4, 0.1, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'warm': function () {
          return [
            1.06, 0, 0, 0, 0,
            0, 1.01, 0, 0, 0,
            0, 0, 0.93, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'cool': function () {
          return [
            0.99, 0, 0, 0, 0,
            0, 0.93, 0, 0, 0,
            0, 0, 1.08, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'brightness': function (v) {
          // -100 is black, 0 is normal, 100 is white
          v = 255 * ((v || 0) / 100);
          return [
            1, 0, 0, 0, v,
            0, 1, 0, 0, v,
            0, 0, 1, 0, v,
            0, 0, 0, 1, 0
          ];
        },
        'exposure': function (v) {
          // .5 is half, 1 is normal, 2 is double, etc.
          v = Math.max(v, 0);
          return [
            v, 0, 0, 0, 0,
            0, v, 0, 0, 0,
            0, 0, v, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'contrast': function (v) {
          v = v !== undefined ? v : 1;
          var n = 0.5 * (1 - v);
          return [
            v, 0, 0, 0, n,
            0, v, 0, 0, n,
            0, 0, v, 0, n,
            0, 0, 0, 1, 0
          ];
        },
        'temperature': function (v) {
          v = v || 0;
          return [
            1 + v, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1 - v, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'tint': function (v) {
          v = v || 0;
          return [
            1 + v, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1 + v, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'threshold': function (v) {
          v = v || 0;
          var r_lum = 0.3086; // 0.212671
          var g_lum = 0.6094; // 0.715160
          var b_lum = 0.0820; // 0.072169
          var r = r_lum * 256;
          var g = g_lum * 256;
          var b = b_lum * 256;
          return [
            r, g, b, 0, -255 * v,
            r, g, b, 0, -255 * v,
            r, g, b, 0, -255 * v,
            0, 0, 0, 1, 0
          ];
        },
        'protanomaly': function () {
          return [
            0.817, 0.183, 0, 0, 0,
            0.333, 0.667, 0, 0, 0,
            0, 0.125, 0.875, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'deuteranomaly': function () {
          return [
            0.8, 0.2, 0, 0, 0,
            0.258, 0.742, 0, 0, 0,
            0, 0.142, 0.858, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'tritanomaly': function () {
          return [
            0.967, 0.033, 0, 0, 0,
            0, 0.733, 0.267, 0, 0,
            0, 0.183, 0.817, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'protanopia': function () {
          return [
            0.567, 0.433, 0, 0, 0,
            0.558, 0.442, 0, 0, 0,
            0, 0.242, 0.758, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'deuteranopia': function () {
          return [
            0.625, 0.375, 0, 0, 0,
            0.7, 0.3, 0, 0, 0,
            0, 0.3, 0.7, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'tritanopia': function () {
          return [
            0.95, 0.05, 0, 0, 0,
            0, 0.433, 0.567, 0, 0,
            0, 0.475, 0.525, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'achromatopsia': function () {
          return [
            0.299, 0.587, 0.114, 0, 0,
            0.299, 0.587, 0.114, 0, 0,
            0.299, 0.587, 0.114, 0, 0,
            0, 0, 0, 1, 0
          ];
        },
        'achromatomaly': function () {
          return [
            0.618, 0.320, 0.062, 0, 0,
            0.163, 0.775, 0.062, 0, 0,
            0.163, 0.320, 0.516, 0, 0,
            0, 0, 0, 1, 0
          ];
        }
      };
      return {
        getFilterByName: function (name) {
          if (filters.hasOwnProperty(name)) {
            return filters[name];
          } else {
            console.warn(`Filter "${name}" not found.`);
            return null;
          }
        },
        getFilterNames: function () {
          return Object.keys(filters);
        }
      };
    },


  },
  mounted() {
    this.OnInitialize();
  },
  beforeUnmount() { }
}
</script>
<style scoped></style>