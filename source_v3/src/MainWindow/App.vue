<template>
  <div id="app">
    <div v-if="loading" class="d-flex justify-content-center align-items-center" style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0, 0, 0, 0.5); z-index: 9999;">
      <div class="spinner-border text-dark" role="status"> 
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    <MainNavBars v-if="!loading" :settings="settings" /> 
  </div>
</template>

<script>
import MainNavBars from './MainNavBars.vue';

export default {
  name: 'App',
  components: {
    MainNavBars
  },
  data() {
    return {
      loading: true, 
      settings: null,
      InstallStatus: null 
    };
  },
  async mounted() {
    try {
      this.settings = await window.api.initializeSettings(); 
      this.InstallStatus = await window.api.InstallStatus();

      //const iniSettings = await window.api.loadIniFile();

      // Handle InstallStatus (if needed)
      console.log('InstallStatus: ', this.InstallStatus);
      switch (this.InstallStatus) {
        case 'existingInstall':
          // Handle existing install logic
          break;
        case 'freshInstall':
          // Handle fresh install logic
          break;
        case 'upgradingUser':
          // Handle upgrading user logic
          break;
        default:
          break;
      }

    } catch (error) {
      console.error('Failed to initialize settings:', error);
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
      }, 2000); 
    }
  }
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

.spinner-border {
  border-color: #333333;
  /* Dark border color for spinner */
  border-top-color: #ffffff;
  /* Lighter color for the top border */
  /*background-color: #121212; /* Dark background color for spinner */
}

/* Example of setting text color for better contrast */
.visually-hidden {
  color: #ffffff;
}
</style>
