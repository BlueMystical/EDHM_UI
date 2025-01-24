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
      myVar: '',
    }
  },
  computed: {

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
    
  },
  async mounted() {
  },
  beforeUnmount() { }
};
</script>
