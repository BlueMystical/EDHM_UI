<template>
  <div id="app" class="bg-dark text-light" data-bs-theme="dark">
    <div v-if="loading" class="d-flex justify-content-center align-items-center bg-dark text-light"
      style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; z-index: 9999;">
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
    <ModalDialog ref="modalDialog" />

    <SettingsEditor @save="saveConfig" />


    <!-- Container for Bottom Colored Toasts -->
    <div class="toast-container position-fixed bottom-0 end-0 p-3">

      <!-- Error Message Toast Notification -->
      <div id="liveToast-ErrMsg" class="toast align-items-center bg-danger border-0" role="alert" aria-live="assertive" aria-atomic="true" style="width: 650px;">
        <div class="d-flex align-items-center" style="height: 100%;">
          <i class="bi bi-bug" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
          <div class="toast-body" >
            <h5>{{ toasts.ErrMsg.title }}</h5>
            <p v-html="toasts.ErrMsg.message"></p>
            <pre class="bg-danger" v-html="toasts.ErrMsg.stack" style="width: 550px;"></pre>
            <div class="btn-group" role="group" aria-label="Basic outlined example">
              <button type="button" class="btn btn-outline-light" @click="copyToClipboard(toasts.ErrMsg.message + '\n\n' + toasts.ErrMsg.stack)">Copy Error</button>
              <button type="button" class="btn btn-outline-light" data-bs-dismiss="toast" aria-label="Close">Close</button>
            </div>            
          </div>
        </div>
      </div>

      <!-- Error Type Toast Notification -->
      <div id="liveToast-Error" class="toast align-items-center bg-danger border-0" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex align-items-center" style="height: 100%;">
          <i class="bi bi-exclamation-octagon" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
          <div class="toast-body" v-html="toasts.Error.message"></div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
      </div>

      <!-- Success Type Toast Notification -->
      <div id="liveToast-Success" class="toast align-items-center bg-success border-0" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex align-items-center" style="height: 100%;">
          <i class="bi bi-check-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
          <div class="toast-body" v-html="toasts.Success.message"></div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
      </div>

      <!-- Warning Type Toast Notification -->
      <div id="liveToast-Warning" class="toast align-items-center bg-warning border-0" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex align-items-center" style="height: 100%;">
          <i class="bi bi-exclamation-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
          <div class="toast-body text-black" v-html="toasts.Warning.message"></div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
      </div>

      <!-- Info Type Toast Notification -->
      <div id="liveToast-Info" class="toast align-items-center bg-info border-0" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex align-items-center" style="height: 100%;">
          <i class="bi bi-info-circle" style="font-size: 64px; margin-left:10px; margin-right: 4px;"></i>
          <div class="toast-body text-black" v-html="toasts.Info.message"></div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
      </div>

    </div> <!-- Container for Bottom Colored Toasts -->

  </div>
</template>

<script>
import MainNavBars from './MainNavBars.vue';
import ModalDialog from './Components/ModalDialog.vue';
import eventBus from '../EventBus';
import SettingsEditor from './SettingsEditor.vue';

export default {
  name: 'App',
  components: {
    MainNavBars,
    ModalDialog,
    SettingsEditor
  },
  data() {
    return {
      loading: true,
      settings: null,
      InstallStatus: null,
      modalTitle: '',
      modalMessage: '',
      toasts: {
        Error: { title: '', message: '' },
        Success: { title: '', message: '' },
        Warning: { title: '', message: '' },
        Info: { title: '', message: '' },
        ErrMsg: { title: '', message: '', stack: '' },
      },
    };
  },
  methods: {
    /**
     * Displays a Colored Toast Notification at Bottom Right corner of the Window
     * @param data Configuration Object: { title: '', message: ''}
     */
    showToast(data) {
      const { type, title, message, stack } = data;
      const toastType = type.charAt(0).toUpperCase() + type.slice(1);

      if (this.toasts[toastType]) {
        this.toasts[toastType].title = title;
        this.toasts[toastType].message = message;
        if (stack) this.toasts[toastType].stack = stack;

        const toast = document.getElementById(`liveToast-${toastType}`);
        let toastBootstrap = bootstrap.Toast.getInstance(toast);

        if (toastBootstrap) {
          toastBootstrap.show();
        } else {
          toastBootstrap = new bootstrap.Toast(toast, { autohide: false });
          toastBootstrap.show();
        }

        if (toastType === 'ErrMsg') {
          window.api.logError(error.message, error.stack);
        }
      } else {
        console.error(`Toast type "${type}" not recognized.`);
      }
    },
    showError(error) {
      const toastType = 'ErrMsg';

      if (this.toasts[toastType]) {

        this.toasts[toastType].title = 'Unexpected Error';
        this.toasts[toastType].message = error.message;
        this.toasts[toastType].stack = error.stack;

        const toast = document.getElementById(`liveToast-${toastType}`);
        let toastBootstrap = bootstrap.Toast.getInstance(toast);

        if (toastBootstrap) {
          toastBootstrap.show();
        } else {
          toastBootstrap = new bootstrap.Toast(toast, { autohide: false });
          toastBootstrap.show();
        }

        window.api.logError(error.message, error.stack);
        console.error(error.message, error.stack);

      } else {
        console.error(`Toast type "${type}" not recognized.`);
      }
    },

    copyToClipboard(text) {
      navigator.clipboard.writeText(text)
        .then(() => {
          console.log('Copied to clipboard successfully!');
        })
        .catch((err) => {
          console.error('Failed to copy to clipboard: ', err);
        });
    },
    showModal() { 
      this.$refs.modalDialog.dialogConfirm(
        { 
          title: 'Your Title', 
          message: 'Your Message', 
        }) .then((result) => { 
          console.log('Modal result:', result); 
        }); 
      },

      saveConfig(newConfig) { // Handle the updated config here 
      console.log('Config saved:', newConfig); 
    }

  },
  async mounted() {
    try {
      eventBus.emit('ShowSpinner', { visible: true });
      this.settings = await window.api.initializeSettings();
      this.InstallStatus = await window.api.InstallStatus();

      switch (this.InstallStatus) {
        case 'existingInstall':
          break;
        case 'freshInstall':
          this.showToast({ type: 'Success', title: 'Success', message: 'Welcome to the application!' });
          break;
        case 'upgradingUser':
          this.showToast({ type: 'Success', title: 'Success', message: 'Upgrade Complete!\r\nThe application has been upgraded successfully.' });
          break;
        default:
          break;
      }
    } catch (error) {
      console.error(error);
    } finally {
      setTimeout(() => {
        eventBus.emit('ShowSpinner', { visible: false });
        this.loading = false;

        this.$nextTick(() => {
          const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
          tooltipTriggerList.forEach(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

          const dropdownElementList = document.querySelectorAll('[data-bs-toggle="dropdown"]');
          dropdownElementList.forEach(dropdownToggleEl => new bootstrap.Dropdown(dropdownToggleEl));
        });
      }, 1000);
    }

     /* EVENTS WE LISTEN TO HERE:  */
    eventBus.on('RoastMe', this.showToast);
    eventBus.on('ShowError', this.showError);

    eventBus.on('modal-confirmed', () => { console.log('Modal confirmed'); }); 
    eventBus.on('modal-cancelled', () => { console.log('Modal cancelled'); });

    // Listen for events with callback support 
    //eventBus.on('ShowDialog', (data, callback) => { this.dialogConfirm(data).then(callback); });
  },
  beforeUnmount() {
    // Clean up the event listener
    eventBus.off('RoastMe', this.showToast);
    eventBus.off('ShowError', this.showError);

    eventBus.off('modal-confirmed'); eventBus.off('modal-cancelled');
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
  display: flex;
  flex-direction: column;
  align-items: start;
}

.toast-body h5 {
  margin: 0;
}

.toast-body pre {
  background-color: #2c2c2c;
  color: #ffffff;
  padding: 10px;
  border-radius: 5px;
  overflow-x: auto;
}

</style>


