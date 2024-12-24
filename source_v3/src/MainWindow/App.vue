<template>
  <div id="app" class="bg-secondary-subtle">
    <div v-if="loading" class="d-flex justify-content-center align-items-center" style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0, 0, 0, 0.5); z-index: 9999;">
      <div class="spinner-border text-dark bg-primary-subtle" role="status"> 
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    <MainNavBars v-if="!loading" :settings="settings" /> 

    <!-- A Notification Toast -->
    <div class="toast-container position-fixed bottom-0 end-0 p-3">
        <div id="liveToast" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="toast-header">
          <strong class="me-auto text-light">{{ toastTitle }}</strong> 
          <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
        <div class="toast-body">
          {{ toastMessage }}
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
          this.showToast('Welcome!', 'Welcome to the application!');
          break;
        case 'upgradingUser':
          // Handle upgrading user logic
          this.showToast('Upgrade Complete!', 'The application has been upgraded successfully.');
          break;
        default:
          break;
      }
    } catch (error) {
      console.error('Failed to initialize settings:', error);
      this.showToast('Error!', 'Failed to initialize settings.');
    } finally {
      // Delay hiding the spinner and initialize Bootstrap components
      setTimeout(() => {
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
  },
  beforeUnmount() {
    // Clean up the event listener
    eventBus.off('RoastMe', this.showToast);
  },
  methods: {
    showToast(data) {
      this.toastTitle = data.title;
      this.toastMessage = data.message;

      const toast = document.getElementById('liveToast');
      const toastBootstrap = bootstrap.Toast.getInstance(toast);

      if (toastBootstrap) {
        toastBootstrap.show();
      } else {
        const toast = document.getElementById('liveToast');
        const toastBootstrap = new bootstrap.Toast(toast);
        toastBootstrap.show();
      }
    },
  },
};
</script>

<style scoped>
    body {
      background-color: #1F1F1F;
      color: #fff; /* Optional: Set text color to white */
    }

#app {
  background-color: #1F1F1F;
  /* Dark background color */
  color: #ffffff;
  /* Light text color */
}
/*
.spinner-border {
  border-color: #333333;
  border-top-color: #ffffff;
  background-color: #121212;
}*/

/* Example of setting text color for better contrast */
.visually-hidden {
  color: #ffffff;
}

.toast {
  background-color: #333; 
  color: orange;
  border: 2px solid #ffffff; 
}
.toast-header {
  background-color: #333; 
  color: #fff; 
}

</style>
