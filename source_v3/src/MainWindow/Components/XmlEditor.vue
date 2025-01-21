<template>
    <div id="XmlEditorModal" class="modal fade" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
      <div class="modal-dialog modal-xl">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">XML Editor</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <div class="row h-50">
              <div class="col">
  
                <!-- 3x3 Grid of Sliders -->
                <div class="row">
                  <div class="col-4">
                    <label for="redSlider1">Red: {{ sliderValues[0][0] }}</label>
                    <input type="range" id="redSlider1" class="form-range" v-model="sliderValues[0][0]" min="-1" max="2" step="0.1" @input="applyColorMatrix">
                  </div>
                  <div class="col-4">
                    <label for="greenSlider1">Green: {{ sliderValues[0][1] }}</label>
                    <input type="range" id="greenSlider1" class="form-range" v-model="sliderValues[0][1]" min="-1" max="2" step="0.01" @input="applyColorMatrix">
                  </div>
                  <div class="col-4">
                    <label for="blueSlider1">Blue: {{ sliderValues[0][2] }}</label>
                    <input type="range" id="blueSlider1" class="form-range" v-model="sliderValues[0][2]" min="-1" max="2" step="0.01" @input="applyColorMatrix">
                  </div>
                </div>
  
                <div class="row">
                  <div class="col-4">
                    <label for="redSlider2">Red: {{ sliderValues[1][0] }}</label>
                    <input type="range" id="redSlider2" class="form-range" v-model="sliderValues[1][0]" min="-1" max="2" step="0.01" @input="applyColorMatrix">
                  </div>
                  <div class="col-4">
                    <label for="greenSlider2">Green: {{ sliderValues[1][1] }}</label>
                    <input type="range" id="greenSlider2" class="form-range" v-model="sliderValues[1][1]" min="-1" max="2" step="0.01" @input="applyColorMatrix">
                  </div>
                  <div class="col-4">
                    <label for="blueSlider2">Blue: {{ sliderValues[1][2] }}</label>
                    <input type="range" id="blueSlider2" class="form-range" v-model="sliderValues[1][2]" min="-1" max="2" step="0.01" @input="applyColorMatrix">
                  </div>
                </div>
  
                <div class="row">
                  <div class="col-4">
                    <label for="redSlider3">Red: {{ sliderValues[2][0] }}</label>
                    <input type="range" id="redSlider3" class="form-range" v-model="sliderValues[2][0]" min="-1" max="2" step="0.01" @input="applyColorMatrix">
                  </div>
                  <div class="col-4">
                    <label for="greenSlider3">Green: {{ sliderValues[2][1] }}</label>
                    <input type="range" id="greenSlider3" class="form-range" v-model="sliderValues[2][1]" min="-1" max="2" step="0.01" @input="applyColorMatrix">
                  </div>
                  <div class="col-4">
                    <label for="blueSlider3">Blue: {{ sliderValues[2][2] }}</label>
                    <input type="range" id="blueSlider3" class="form-range" v-model="sliderValues[2][2]" min="-1" max="2" step="0.01" @input="applyColorMatrix">
                  </div>
                </div>
  
                <!-- Label and Multi-line Text Input -->
                <div class="row h-50">
                  <label for="colorMatrixInput">XML input</label>
                  <textarea id="colorMatrixInput" class="form-control" rows="6" @input="onColorMatrixPaste" spellcheck="false" :value="formattedColorMatrix"></textarea>
                </div>
              </div>
              <div class="col d-flex justify-content-center align-items-center">
                <!-- Hidden Image and Canvas -->
                <img ref="image" src="../../images/xml-base.png" class="d-none border" alt="...">
                <canvas ref="canvas" class="img-fluid border"></canvas>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <div class="btn-group" role="group">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
              <button type="button" class="btn btn-primary" data-bs-dismiss="modal" @click="save">Save changes</button>
            </div>                    
          </div>
        </div>
      </div>
      <!-- SVG Filter Definition -->
      <svg xmlns="http://www.w3.org/2000/svg" style="display: none;">
        <filter id="matrixFilter">
          <feColorMatrix in="SourceGraphic" type="matrix" :values="colorMatrixValues()"></feColorMatrix>
        </filter>
      </svg>
    </div>
  </template>
  


<script>
export default {
    name: 'XmlEditor',
    props: {},
    data() {
        return {
            sliderValues: [
                [1.0, 0.0, 0.0], // Row 1: Red, Green, Blue
                [0.0, 1.0, 0.0], // Row 2: Red, Green, Blue
                [0.0, 0.0, 1.0], // Row 3: Red, Green, Blue
            ],
            colorMatrix: '' // Placeholder for copied color matrix
        };
    },
    computed: {
        formattedColorMatrix() {
            const redMatrix = `<MatrixRed>${this.sliderValues[0].join(', ')}</MatrixRed>`;
            const greenMatrix = `<MatrixGreen>${this.sliderValues[1].join(', ')}</MatrixGreen>`;
            const blueMatrix = `<MatrixBlue>${this.sliderValues[2].join(', ')}</MatrixBlue>`;
            return `${redMatrix}\n${greenMatrix}\n${blueMatrix}`;
        }
    },
    methods: {
        async OnInitialize() {
            this.loadImage();
        },
        getLabel(colIndex) {
            const labels = ['Red', 'Green', 'Blue'];
            return labels[colIndex];
        },
        ShowModal() {
            const myModal = new bootstrap.Modal('#XmlEditorModal', { keyboard: false });
            myModal.show();
        },
        save() {
            console.log(this.sliderValues);
            this.$emit('onCloseModal');
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
            this.applyColorMatrix();
        },
        colorMatrixValues() {
            const matrix = [
                ...this.sliderValues[0], 0,
                ...this.sliderValues[1], 0,
                ...this.sliderValues[2], 0,
                0, 0, 0, 1, 0
            ];
            return matrix.join(' ');
        },
        onColorMatrixPaste(event) {
            // Set the pasted color matrix
            this.colorMatrix = event.target.value;
            // Parse the color matrix and update slider values
            this.parseColorMatrix();
        },
  
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
    
                // Define the color matrix (4x5 matrix for RGBA)
                const matrix = [
                    ...this.sliderValues[0], 0, 0,
                    ...this.sliderValues[1], 0, 0,
                    ...this.sliderValues[2], 0, 0,
                    0, 0, 0, 1, 0
                ];
    
                // Apply the color matrix transformation
                for (let i = 0; i < data.length; i += 4) {
                    const r = data[i];
                    const g = data[i + 1];
                    const b = data[i + 2];
                    const a = data[i + 3];
    
                    data[i] = matrix[0] * r + matrix[1] * g + matrix[2] * b + matrix[3] * a + matrix[4];
                    data[i + 1] = matrix[5] * r + matrix[6] * g + matrix[7] * b + matrix[8] * a + matrix[9];
                    data[i + 2] = matrix[10] * r + matrix[11] * g + matrix[12] * b + matrix[13] * a + matrix[14];
                    data[i + 3] = matrix[15] * r + matrix[16] * g + matrix[17] * b + matrix[18] * a + matrix[19];
                }
    
                // Put the image data back onto the canvas
                ctx.putImageData(imageData, 0, 0);
            },



/*
        applyColorMatrix() {
            const canvas = this.$refs.canvas;
            const ctx = canvas.getContext('2d');
            const img = this.$refs.image;

            if (!img.complete) {
                console.log("Image not fully loaded");
                return;
            }

            // Set canvas dimensions to match the image
            canvas.width = img.naturalWidth;
            canvas.height = img.naturalHeight;

            // Clear canvas and draw the image
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

            // Apply the filter and redraw the image
            ctx.filter = `url(#matrixFilter)`;
            ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
        },
*/



        loadImage() {
            const img = this.$refs.image;
            img.onload = () => {
                this.applyColorMatrix();
            };
            if (img.complete) {
                this.applyColorMatrix();
            }
        }


    },
    mounted() {
        this.OnInitialize();
    },
    beforeUnmount() { }
}
</script>
<style scoped></style>