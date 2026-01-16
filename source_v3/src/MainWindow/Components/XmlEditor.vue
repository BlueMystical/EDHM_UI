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

              <!-- 3x3 Grid of Sliders  -->
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
                  <textarea id="colorMatrixInput" class="form-control" rows="4" @input="onColorMatrixPaste"
                    spellcheck="false" :value="formattedColorMatrix"
                    style="resize: none; text-align: center; "></textarea>
                </div>
                <hr>
                <div class="btn-group mb-3" role="group" aria-label="Basic example">
                  <button class="btn btn-outline-secondary" type="button" @click="GoToNo2_Click">Get XML
                    Presets</button>
                  <button class="btn btn-outline-secondary" type="button" @click="ShowInfo_Click">What is a Color
                    Matrix?</button>
                </div>
              </div><!--/XmlInput-->

            </div><!--/LeftColumn-->

          <!-- Right Column -->
            <div class="col-8 position-relative" style="height: 450px;">

              <!-- Tab Headers -->
              <ul id="myTab" class="nav nav-tabs w-100 p-3 position-absolute top-0" role="tablist">
                <li class="nav-item" role="presentation">
                  <button class="nav-link active" id="home-tab" data-bs-toggle="tab" data-bs-target="#image-tab-pane"
                    type="button" role="tab" aria-controls="image-tab-pane" aria-selected="true">Panels</button>
                </li>
                <li class="nav-item" role="presentation">
                  <button class="nav-link" id="profile-tab" data-bs-toggle="tab" data-bs-target="#profile-tab-pane"
                    type="button" role="tab" aria-controls="profile-tab-pane" aria-selected="false">Color Transformation</button>
                </li>
              </ul>

              <!-- Tab Content -->
              <div id="myTabContent" class="tab-content h-100 w-100">

                <!-- TabPane for Image -->
                <div class="tab-pane fade show active h-100" id="image-tab-pane" role="tabpanel" aria-labelledby="home-tab">
                  <div ref="canvasWrapper" class="d-flex justify-content-center align-items-center h-100 border rounded" >
                    <!-- Hidden original canvas -->
                    <canvas ref="canvasOriginal" class="d-none"></canvas>
                    <!-- Visible filtered canvas -->
                    <canvas ref="canvasFiltered" class="d-block mx-auto" style="top: 100px;"></canvas>
                  </div>
                </div>

                <!-- Tab Pane for Profile -->
                <div class="tab-pane fade h-100 w-100" id="profile-tab-pane" role="tabpanel"
                  aria-labelledby="profile-tab" tabindex="1">
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
                        <input type="color" v-model="colorTranform[4].default" @input="onColorChange">
                      </div>
                      <div class="col">→</div>
                      <div class="col border" :style="{ 'background-color': colorTranform[4].hex }"></div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[4].rgb }}</div>
                      <div class="col d-flex justify-content-center align-items-center">{{ colorTranform[4].hex }}</div>
                    </div>

                    <div class="alert alert-info  m-2" role="alert">Individual colors are transformed using the XML
                      filter.</div>
                  </div>

                </div><!-- /TabPane Profile -->
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
        [1.0, 0.0, 0.0], // Red[0][0], Green[0][1], Blue[0][2]
        [0.0, 1.0, 0.0], // Red[1][0], Green[1][1], Blue[1][2]
        [0.0, 0.0, 1.0], // Red[2][0], Green[2][1], Blue[2][2]
      ],
      colorMatrix: '', // Placeholder for copied color matrix
      originalImageSrc: '../../images/xml-base_02.png',
      canvasFiltered: ref(null),
      canvasOriginal: ref(null),
      imgObj: null,
      gamma: ref(1.0),
      saturation: ref(1.0),
      themeName: '', //For Dialog's Title
      colorTranform: [
        { id: 'Orange', default: '#FFA500', rgb: '255;165;0', hex: '#FFA500' },
        { id: 'White', default: '#FFFFFF', rgb: '255;255;255', hex: '#FFFFFF' },
        { id: 'Red', default: '#FF0000', rgb: '255;0;0', hex: '#FF0000' },
        { id: 'Cyan', default: '#00FFFF', rgb: '0;255;255', hex: '#00FFFF' },
        { id: 'Custom', default: '#008000', rgb: '0;128;0', hex: '#008000' },
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


    // ------------------------------------------------------------------------------------------
    //-- APLICA EL FILTRO XML USANDO WEBGL ------------------------------------------------------

    // Cargar imagen
    async loadImage() {
      const url = await window.api.getAssetFileUrl('images/xml-base_02.png');
      await new Promise((resolve, reject) => {
        this.imgObj = new Image();
        this.imgObj.onload = resolve;
        this.imgObj.onerror = () => reject(new Error(`No se pudo cargar la imagen: ${url}`));
        this.imgObj.src = url;

        // Acceder a los canvas desde $refs
        const canvasOriginal = this.$refs.canvasOriginal;
        const canvasFiltered = this.$refs.canvasFiltered;

        canvasOriginal.width = this.imgObj.width || 1;
        canvasOriginal.height = this.imgObj.height || 1;

        canvasFiltered.width = this.imgObj.width || 1;
        canvasFiltered.height = this.imgObj.height || 1;

        console.log('XML Image loaded.');

        const ctxOriginal = canvasOriginal.getContext('2d', { willReadFrequently: true });
        ctxOriginal.drawImage(this.imgObj, 0, 0);
      });
    },

    async applyFilter() {
      // Obtiene la imagen base
      const imagenSrc = await window.api.getAssetFileUrl('images/xml-base_02.png');
      console.log('Applying filter with matrix:', this.matrix);

      //this.$refs.image.src = this.originalImageSrc;
      this.TransformXMLColors();

      // Force Vue to reapply the filter
      this.$nextTick(() => {
        //:style="{ filter: 'url(#colorMatrixFilter)', top: '80px' }" 
        //this.$refs.image.style.filter = 'url(#colorMatrixFilter)';
        //this.applyColorMatrix();
      });

      // Construye la matriz 4x4 + offset desde tu 3x3
      const mat = this.buildWebGLColorMatrixFrom3x3(
        this.matrix,
        1.0,
        { r: 0, g: 0, b: 0, a: 0 }
      );

      // Aplica el filtro con gamma y saturación
      await this.applyColorMatrixWebGL({
        imageSrc: imagenSrc,
        canvas: this.$refs.canvasFiltered,   // tu ref al canvas destino
        colorMatrix: mat,
        gammaValue: this.gamma,        // valor dinámico del slider
        saturationValue: this.saturation // valor dinámico del slider
      });
    },
    // Construye la matriz 4x4 + offset para WebGL a partir de una matriz 3x3
    buildWebGLColorMatrixFrom3x3(m3, alphaScale = 1.0, offsets = { r: 0, g: 0, b: 0, a: 0 }) {
      //console.log('Building WebGL color matrix from 3x3:', m3, 'Alpha Scale:', alphaScale, 'Offsets:', offsets);

      const m00 = m3[0][0], m01 = m3[0][1], m02 = m3[0][2];
      const m10 = m3[1][0], m11 = m3[1][1], m12 = m3[1][2];
      const m20 = m3[2][0], m21 = m3[2][1], m22 = m3[2][2];

      const matrix4x4 = [
        m00, m01, m02, 0,
        m10, m11, m12, 0,
        m20, m21, m22, 0,
        0, 0, 0, alphaScale
      ];
      const offset = [offsets.r, offsets.g, offsets.b, offsets.a];
      return { matrix4x4, offset };
    },
    // Aplica la matriz de color usando WebGL con gamma y saturación
    async applyColorMatrixWebGL({ imageSrc, canvas, colorMatrix, gammaValue = 1.0, saturationValue = 1.0 }) {

      const gl = canvas.getContext('webgl', { premultipliedAlpha: false, alpha: true });
      if (!gl) throw new Error('WebGL no disponible');

      const vertSrc = this.vertexSourceWebGL1();
      const fragSrc = this.fragmentSourceWebGL1();

      const vs = compileShader(gl, gl.VERTEX_SHADER, vertSrc);
      const fs = compileShader(gl, gl.FRAGMENT_SHADER, fragSrc);
      const program = linkProgram(gl, vs, fs);
      gl.useProgram(program);

      // Fullscreen quad - posición
      const posBuf = gl.createBuffer();
      gl.bindBuffer(gl.ARRAY_BUFFER, posBuf);
      gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
        -1, -1, 1, -1, -1, 1,
        -1, 1, 1, -1, 1, 1
      ]), gl.STATIC_DRAW);
      const aPos = gl.getAttribLocation(program, 'a_position');
      gl.enableVertexAttribArray(aPos);
      gl.vertexAttribPointer(aPos, 2, gl.FLOAT, false, 0, 0);

      // Fullscreen quad - texcoords
      const texBuf = gl.createBuffer();
      gl.bindBuffer(gl.ARRAY_BUFFER, texBuf);
      gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
        0, 0, 1, 0, 0, 1,
        0, 1, 1, 0, 1, 1
      ]), gl.STATIC_DRAW);
      const aTex = gl.getAttribLocation(program, 'a_texCoord');
      gl.enableVertexAttribArray(aTex);
      gl.vertexAttribPointer(aTex, 2, gl.FLOAT, false, 0, 0);

      // Imagen como textura
      const img = await loadImage(imageSrc);
      canvas.width = img.width;
      canvas.height = img.height;
      gl.viewport(0, 0, canvas.width, canvas.height);

      const tex = gl.createTexture();
      gl.bindTexture(gl.TEXTURE_2D, tex);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
      gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, false);
      gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
      gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, img);

      // Uniforms
      gl.uniform1i(gl.getUniformLocation(program, 'u_image'), 0);
      gl.uniformMatrix4fv(gl.getUniformLocation(program, 'u_colorMatrix'), false, new Float32Array(colorMatrix.matrix4x4));
      gl.uniform4fv(gl.getUniformLocation(program, 'u_colorOffset'), new Float32Array(colorMatrix.offset));
      gl.uniform1f(gl.getUniformLocation(program, 'u_gamma'), gammaValue);
      gl.uniform1f(gl.getUniformLocation(program, 'u_saturation'), saturationValue);

      // Draw
      gl.drawArrays(gl.TRIANGLES, 0, 10);

      return canvas.toDataURL('image/png');

      // Helpers
      function compileShader(gl, type, src) {
        const sh = gl.createShader(type);
        gl.shaderSource(sh, src);
        gl.compileShader(sh);
        if (!gl.getShaderParameter(sh, gl.COMPILE_STATUS)) {
          const log = gl.getShaderInfoLog(sh);
          gl.deleteShader(sh);
          throw new Error('Shader compile error: ' + log);
        }
        return sh;
      }
      function linkProgram(gl, vs, fs) {
        const p = gl.createProgram();
        gl.attachShader(p, vs);
        gl.attachShader(p, fs);
        gl.linkProgram(p);
        if (!gl.getProgramParameter(p, gl.LINK_STATUS)) {
          const log = gl.getProgramInfoLog(p);
          gl.deleteProgram(p);
          throw new Error('Program link error: ' + log);
        }
        return p;
      }
      function loadImage(src) {
        return new Promise((resolve, reject) => {
          const im = new Image();
          im.crossOrigin = 'anonymous';
          im.onload = () => resolve(im);
          im.onerror = reject;
          im.src = src;
        });
      }
    },
    // ===== Shaders WebGL1 =====
    vertexSourceWebGL1() {
      return `
attribute vec2 a_position;
attribute vec2 a_texCoord;
varying vec2 v_texCoord;
void main() {
  gl_Position = vec4(a_position, 0.0, 1.0);
  v_texCoord = a_texCoord;
}
`;
    },
    fragmentSourceWebGL1() {
      return `
precision mediump float;
uniform sampler2D u_image;
uniform mat4 u_colorMatrix;
uniform vec4 u_colorOffset;
uniform float u_gamma;
uniform float u_saturation;
varying vec2 v_texCoord;

float srgbToLinear(float c) {
  return (c <= 0.04045) ? (c / 12.92) : pow((c + 0.055) / 1.055, 2.4);
}
vec3 srgbToLinearVec3(vec3 c) {
  return vec3(srgbToLinear(c.r), srgbToLinear(c.g), srgbToLinear(c.b));
}
float linearToSrgb(float c) {
  return (c <= 0.0031308) ? (c * 12.92) : (1.055 * pow(c, 1.0 / 2.4) - 0.055);
}
vec3 linearToSrgbVec3(vec3 c) {
  return vec3(linearToSrgb(c.r), linearToSrgb(c.g), linearToSrgb(c.b));
}

void main() {
  vec4 texColor = texture2D(u_image, v_texCoord);

  vec3 linearRGB = srgbToLinearVec3(texColor.rgb);
  vec4 srcColor = vec4(linearRGB, texColor.a);
  vec4 outLinear = u_colorMatrix * srcColor + u_colorOffset;

  // Saturación
  float luma = dot(outLinear.rgb, vec3(0.2126, 0.7152, 0.0722));
  outLinear.rgb = mix(vec3(luma), outLinear.rgb, u_saturation);

  // Gamma
  outLinear.rgb = pow(outLinear.rgb, vec3(u_gamma));

  outLinear = clamp(outLinear, 0.0, 1.0);

  vec3 outSRGB = linearToSrgbVec3(outLinear.rgb);
  gl_FragColor = vec4(outSRGB, outLinear.a);
}`;
    },
    // ------------------------------------------------------------------------------------

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

    /* -------- Color Transformation Section ----------- */

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

        const hex = this.rgbToHex(rgbR, rgbG, rgbB);
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
          red: { red: this.sliderValues[0][0], green: this.sliderValues[0][1], blue: this.sliderValues[0][2] },
          green: { red: this.sliderValues[1][0], green: this.sliderValues[1][1], blue: this.sliderValues[1][2] },
          blue: { red: this.sliderValues[2][0], green: this.sliderValues[2][1], blue: this.sliderValues[2][2] },
        };
        this.matrix = [
          [xmlValues.red.red, xmlValues.red.green, xmlValues.red.blue],
          [xmlValues.green.red, xmlValues.green.green, xmlValues.green.blue],
          [xmlValues.blue.red, xmlValues.blue.green, xmlValues.blue.blue],
        ];

        // Orange Transformation:
        let Percentages = { red: 1.0, green: 0.5, blue: 0.0 };
        const TransformColor_Orange = this.transformColorFromXML(xmlValues, Percentages);
        this.colorTranform[0].hex = TransformColor_Orange.h;
        this.colorTranform[0].rgb = TransformColor_Orange.r + ';' + TransformColor_Orange.g + ';' + TransformColor_Orange.b;
        //console.log('TransformColor_Orange:', TransformColor_Orange);

        // White Transformation:
        Percentages = { red: 1.0, green: 1.0, blue: 1.0 };
        const TransformColor_White = this.transformColorFromXML(xmlValues, Percentages);
        this.colorTranform[1].hex = TransformColor_White.h;
        this.colorTranform[1].rgb = TransformColor_White.r + ';' + TransformColor_White.g + ';' + TransformColor_White.b;
        //console.log('TransformColor_White:', TransformColor_White);

        // Red Transformation:
        Percentages = { red: 1.0, green: 0.0, blue: 0.0 };
        const TransformColor_Red = this.transformColorFromXML(xmlValues, Percentages);
        this.colorTranform[2].hex = TransformColor_Red.h;
        this.colorTranform[2].rgb = TransformColor_Red.r + ';' + TransformColor_Red.g + ';' + TransformColor_Red.b;
        //console.log('TransformColor_Red:', TransformColor_Red);

        //Cyan Transformation:
        Percentages = { red: 0.0, green: 1.0, blue: 1.0 };
        const TransformColor_Cyan = this.transformColorFromXML(xmlValues, Percentages);
        this.colorTranform[3].hex = TransformColor_Cyan.h;
        this.colorTranform[3].rgb = TransformColor_Cyan.r + ';' + TransformColor_Cyan.g + ';' + TransformColor_Cyan.b;
        //console.log('TransformColor_Cyan:', TransformColor_Cyan);

        // Custom Color Transformation:
        const CustomColor = this.hexToRgb(this.colorTranform[4].default);
        Percentages = { red: CustomColor.red / 255, green: CustomColor.green / 255, blue: CustomColor.blue / 255 };
        const TransformColor_Custom = this.transformColorFromXML(xmlValues, Percentages);
        this.colorTranform[4].hex = TransformColor_Custom.h;
        this.colorTranform[4].rgb = TransformColor_Custom.r + ';' + TransformColor_Custom.g + ';' + TransformColor_Custom.b;
        //console.log('TransformColor_Custom:', TransformColor_Custom);

      } catch (ex) {
        console.error(`ERROR: ${ex.message}\n${ex.stack}`);
        EventBus.emit('ShowError', ex);
      }
    },
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


  },
  mounted() {
    this.OnInitialize();
  },
  beforeUnmount() { }
}
</script>
<style scoped>
#image-tab-pane {
  overflow: hidden; /* keep canvas contained */
}
</style>