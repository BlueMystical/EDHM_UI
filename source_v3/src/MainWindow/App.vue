<template>
  <div id="app" class="bg-dark text-light">
     <div v-if="loading" class="d-flex justify-content-center align-items-center bg-dark text-light" style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; z-index: 9999;">
      <div class="bg-dark text-light" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div> 
    <MainNavBars v-if="!loading" :settings="settings" />

    <!-- A Dialog Popup 
    <div aria-live="polite" aria-atomic="true" class="d-flex justify-content-center align-items-center w-100 h-100 position-fixed top-20 ">
      <div id="toastConfirmation" class="toast bg-dark text-light" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="toast-header bg-dark text-light">
          <i class="bi bi-question-lg" style="font-size: 16px; margin-left:4px;"></i>
          <strong class="me-auto text-light">{{ toastTitle }}</strong> 
          <button type="button" class="btn-close text-light" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
        <div class="toast-body ">
          {{ toastMessage }}
          <div class="mt-2 pt-2 border-top ">
            <div class="btn-group" role="group" aria-label="Basic example">
              <button type="button" class="btn btn-primary">OK</button>
              <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="toast">Close</button>
            </div>
          </div>
        </div>
      </div>
    </div>-->

      <!-- Modal -->
      <div class="modal fade" id="staticBackdrop" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
        <div class="modal-dialog">
          <div class="modal-content bg-dark text-light">
            <div class="modal-header">
              <h1 class="modal-title fs-5" id="staticBackdropLabel">Modal title</h1>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
              <div v-if="isHtmlContent" v-html="toastMessage"></div>
              <div v-else>{{ toastMessage }}</div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
              <button type="button" class="btn btn-primary">Understood</button>
            </div>
          </div>
        </div>
      </div>

    <!-- Container for Bottom Colored Toasts -->
    <div class="toast-container position-fixed bottom-0 end-0 p-3">

      <!-- Error Type Toast Notification -->
      <div id="liveToast-Error" class="toast align-items-center bg-danger border-0" role="alert" aria-live="assertive"
        aria-atomic="true">
        <div class="d-flex">
          <i class="bi bi-exclamation-octagon" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
          <div class="toast-body" v-html="toastMessage"></div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"
            aria-label="Close"></button>
        </div>
      </div>

      <!-- Success Type Toast Notification -->
      <div id="liveToast-Success" class="toast align-items-center bg-success border-0" role="alert"
        aria-live="assertive" aria-atomic="true">
        <div class="d-flex">
          <i class="bi bi-check-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
          <div class="toast-body" v-html="toastMessage"></div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"
            aria-label="Close"></button>
        </div>
      </div>

      <!-- Warning Type Toast Notification -->
      <div id="liveToast-Warning" class="toast align-items-center bg-warning border-0" role="alert"
        aria-live="assertive" aria-atomic="true">
        <div class="d-flex">
          <i class="bi bi-exclamation-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
          <div class="toast-body text-black" v-html="toastMessage"></div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"
            aria-label="Close"></button>
        </div>
      </div>

      <!-- Info Type Toast Notification -->
      <div id="liveToast-Info" class="toast align-items-center bg-info border-0" role="alert" aria-live="assertive"
        aria-atomic="true">
        <div class="d-flex">
          <i class="bi bi-info-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
          <div class="toast-body text-black" v-html="toastMessage"></div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"
            aria-label="Close"></button>
        </div>
      </div>

    </div>



  </div>
</template>

<script>
import MainNavBars from './MainNavBars.vue';
import eventBus from '../EventBus';

export default {
  name: 'App',
  components: {
    MainNavBars,
  },
  data() {
    return {
      loading: true,
      settings: null,
      InstallStatus: null,
      toastTitle: '',
      toastMessage: '',
    };
  },
  async mounted() {
    try {
      eventBus.emit('ShowSpinner', { visible: true } );//<- this event will be heard in 'MainNavBars.vue'
      this.settings = await window.api.initializeSettings();
      this.InstallStatus = await window.api.InstallStatus();

      // Handle InstallStatus (if needed)
      console.log('InstallStatus: ', this.InstallStatus);
      switch (this.InstallStatus) {
        case 'existingInstall':
          // Handle existing install logic
          break;
        case 'freshInstall':
          // Handle fresh install logic
          showToast( { type: 'Success', message: 'Welcome to the application!'});
          break;
        case 'upgradingUser':
          // Handle upgrading user logic
          showToast( { type: 'Success', message: 'Upgrade Complete!\r\nThe application has been upgraded successfully.'});
          break;
        default:
          break;
      }
    } catch (error) {
      console.error('Failed to initialize settings:', error);
    } finally {
      // Delay hiding the spinner and initialize Bootstrap components
      setTimeout(() => {
        eventBus.emit('ShowSpinner', { visible: false} ); //<- this event will be heard in 'MainNavBars.vue'
        this.loading = false;

        this.$nextTick(() => {
          const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
          const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

          const dropdownElementList = document.querySelectorAll('[data-bs-toggle="dropdown"]');
          const dropdownList = [...dropdownElementList].map(dropdownToggleEl => new bootstrap.Dropdown(dropdownToggleEl));
        });
      }, 1000);
    }

    // Listen for toast events from the event bus
    eventBus.on('RoastMe', this.showToast);
    eventBus.on('ShowDialog', this.dialogConfirm);
  },
  beforeUnmount() {
    // Clean up the event listener
    eventBus.off('RoastMe', this.showToast);
    eventBus.off('ShowDialog', this.dialogConfirm);
  },
  methods: { 
    showToast(data) { 
      this.toastTitle = data.title; 
      this.toastMessage = data.message;

      const toast = document.getElementById(`liveToast-${data.type}`); //<- 'Success','Error','Warning','Info'
      let toastBootstrap = bootstrap.Toast.getInstance(toast); 

      if (toastBootstrap) { 
        toastBootstrap.show(); 
      } else { 
        toastBootstrap = new bootstrap.Toast(toast, { autohide: false }); 
        toastBootstrap.show(); 
      } 
    },


  dialogConfirm(data) {
    this.toastTitle = data.title;
    this.toastMessage = data.message;

    // Set isHtmlContent to true if data.message contains HTML tags
    this.isHtmlContent = /<\/?[a-z][\s\S]*>/i.test(data.message);

    const modalElement = document.getElementById('staticBackdrop');
    let exampleModal = bootstrap.Modal.getInstance(modalElement);

    if (!exampleModal) {
      exampleModal = new bootstrap.Modal(modalElement, {
        backdrop: 'static',   // Ensure the modal doesn't close when clicked outside
        keyboard: true // Allow closing the modal with the escape key
      });
    }

    modalElement.addEventListener('shown.bs.modal', event => {
      // Update the modal's content
      const modalTitle = modalElement.querySelector('.modal-title');
      modalTitle.textContent = data.title;
    });

    modalElement.addEventListener('hidden.bs.modal', event => {
      exampleModal.dispose();
    });

    // Show the modal
    exampleModal.show();
  }





  },
  
};
</script>

<style scoped>
#app {
  background-color: #1F1F1F;
  color: #ffffff;
}

.visually-hidden {
  color: #ffffff;
}

.toast-body {
  white-space: pre-wrap;
}
</style>
