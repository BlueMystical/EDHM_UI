<template>
  <!-- Modal -->
  <div id="ModalDialog" class="modal fade" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1"
    aria-labelledby="staticBackdropLabel" aria-hidden="true">
    <div class="modal-dialog modal-xl">
      <div class="modal-content">

        <div class="modal-header">
          <h1 class="modal-title fs-5" id="staticBackdropLabel">Modal title</h1>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>
        <div class="modal-body" style="height: 400px;">


          <!-- Define the SVG filter -->
          <svg width="0" height="0">
            <filter id="colorMatrixFilter">
              <feColorMatrix type="matrix" :values="getMatrixValues"/>
            </filter>
          </svg>
          <!-- Apply the filter to the image -->
          <img id="targetImage" ref="image" :src="originalImageSrc" class="border" :style="{ filter: 'url(#colorMatrixFilter)' }" alt="...">



        </div><!--/body-->

        <div class="modal-footer">
          <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancel</button>
          <button type="button" class="btn btn-primary" data-bs-dismiss="modal" @click="save">Save changes</button>
        </div><!--/Footer-->

      </div><!--/Content-->
    </div><!--/Modal-->
  </div><!--/Backdrop-->
</template>


<script>
export default {
  name: 'ModalDialog',
  props: {},
  data() {
    return {
      colorMatrix: [
        [1.0, 0.0, 0.0], // Row[0]: Red, Green, Blue
        [0.0, 1.0, 0.0], // Row[1]: Red, Green, Blue
        [0.0, 0.0, -1.0], // Row[2]: Red, Green, Blue
      ],
      originalImageSrc: '../../images/xml-base.jpg',
    }
  },
  computed: {
    getMatrixValues() {      
      //                                   R                              G                         B             A M
      let matrixString = this.colorMatrix[0][0] + ' ' + this.colorMatrix[0][1] + ' ' + this.colorMatrix[0][2] + ' 0 0 ';
      matrixString +=    this.colorMatrix[1][0] + ' ' + this.colorMatrix[1][1] + ' ' + this.colorMatrix[1][2] + ' 0 0 ';
      matrixString +=    this.colorMatrix[2][0] + ' ' + this.colorMatrix[2][1] + ' ' + this.colorMatrix[2][2] + ' 0 0 ';
      matrixString += '0 0 0 1 0';

      console.log('Matrix values:', matrixString);
      return matrixString;
    }
  },
  methods: {
    ShowModal(matrix) {
      this.colorMatrix = matrix; // User can pass a different Matrix every time
      const myModal = new bootstrap.Modal('#ModalDialog', { keyboard: false });
      myModal.show();
      //console.log('Matrix:', this.colorMatrix);
    },
    save() {
      this.$emit('onCloseModal', this.colorMatrix);
    },
    applyFilter() {
      // Reset the image source to the original
      this.$refs.image.src = this.originalImageSrc;
      
      // Force Vue to reapply the filter
      this.$nextTick(() => {
        this.$refs.image.style.filter = 'url(#colorMatrixFilter)';
      });
    },
    
  },
  async mounted() {
    //this.getMatrixValues();
    this.originalImageSrc = await window.api.getAssetFileUrl('images/xml-base.jpg');
    this.applyFilter(); 
  },
  beforeUnmount() { }
};
</script>
