<template>
    <div class="modal fade" id="staticBackdrop" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content bg-dark">
          <div class="modal-header">
            <h5 class="modal-title" id="staticBackdropLabel">{{ modalTitle }}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body" v-html="sanitizedMessage"></div>
          <div class="modal-footer input-group" role="group">
            <button :id="cancelButtonId" type="button" class="btn btn-outline-primary" data-bs-dismiss="modal">{{ cancelButtonLabel }}</button>
            <button :id="confirmButtonId" type="button" class="btn btn-primary">{{ confirmButtonLabel }}</button>
          </div>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  import DOMPurify from 'dompurify';
  import EventBus from '../../EventBus';
  
  export default {
    name: 'ModalDialog',
    props: {
      cancelButtonId: {
        type: String,
        default: 'cancelButton',
      },
      confirmButtonId: {
        type: String,
        default: 'confirmButton',
      },
      cancelButtonLabel: {
        type: String,
        default: 'Cancel',
      },
      confirmButtonLabel: {
        type: String,
        default: 'Confirm',
      },
    },
    data() {
      return {
        modalTitle: '',
        modalMessage: '',
      };
    },
    methods: {
      dialogConfirm(data) {
        return new Promise((resolve, reject) => {
          if (!data || !data.title || !data.message) {
            reject(new Error('Missing required data properties (title, message)'));
            return;
          }
  
          this.modalTitle = data.title;
          this.modalMessage = data.message;
          this.sanitizedMessage = DOMPurify.sanitize(data.message);
  
          const modalElement = document.getElementById('staticBackdrop');
          let exampleModal = bootstrap.Modal.getInstance(modalElement);
  
          exampleModal = new bootstrap.Modal(modalElement, {
            backdrop: 'static',
            keyboard: true,
          });
  
          const handleConfirm = () => {
            try {
              exampleModal.hide();
              EventBus.emit('modal-confirmed'); // Emit event
              resolve('Confirmed');
            } catch (error) {
              // Handle error
            } finally {
              resolve('Confirmed');
            }
          };
  
          const handleCancel = () => {
            try {
              exampleModal.hide();
              EventBus.emit('modal-cancelled'); // Emit event
              resolve('Cancelled');
            } catch (error) {
              // Handle error
            } finally {
              resolve('Cancelled');
            }
          };
  
          modalElement.addEventListener('shown.bs.modal', () => {
            const confirmButton = modalElement.querySelector('#' + this.confirmButtonId);
            const cancelButton = modalElement.querySelector('#' + this.cancelButtonId);
  
            confirmButton.removeEventListener('click', handleConfirm);
            cancelButton.removeEventListener('click', handleCancel);
  
            confirmButton.addEventListener('click', handleConfirm);
            cancelButton.addEventListener('click', handleCancel);
          });
  
          modalElement.addEventListener('hidden.bs.modal', () => {
            confirmButton.removeEventListener('click', handleConfirm);
            cancelButton.removeEventListener('click', handleCancel);
  
            if (exampleModal) {
              exampleModal.dispose();
              exampleModal = null;
            }
          });
  
          exampleModal.show();
        });
      },
    },
  };
  </script>
  
  <style scoped></style>
  