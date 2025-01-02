<template>
  <div id="app" class="bg-dark text-light" data-bs-theme="dark">
    
    <MainNavBars v-if="!loading" :settings="settings" />
    <SettingsEditor @save="saveConfig" />
    <Notifications/>

  </div>
</template>

<script>
import eventBus from '../EventBus';
import MainNavBars from './MainNavBars.vue';
import SettingsEditor from './SettingsEditor.vue';
import Notifications from './Components/Notifications.vue';
//import WaitSpinner from './Components/WaitSpinner.vue';

export default {
  name: 'App',
  components: {
    MainNavBars,
    SettingsEditor,
    Notifications
  },
  data() {
    return {
      loading: true,
      settings: null,
      InstallStatus: null,
    };
  },
  methods: {

    copyToClipboard(text) {
      navigator.clipboard.writeText(text)
        .then(() => {
          console.log('Copied to clipboard successfully!');
        })
        .catch((err) => {
          console.error('Failed to copy to clipboard: ', err);
        });
    },

    async installEDHMmod(gameInstance) {
      // Instalar el mod en la nueva ubicacion del juego
      try {
        eventBus.emit('ShowSpinner', { visible: true });
        eventBus.emit('RoastMe', { type: 'Info', message: 'Installing EDHM files..' });

        const ZipPath = await window.api.getAssetPath('data/ODY');
        const ZipFile = await window.api.findLatestFile(ZipPath, '.zip');
        console.log('ZipFile:', ZipFile);

        //TODO: Unzip Themes

        if (ZipFile && ZipFile.success) {
          const unzipPath = gameInstance.path;
          const versionMatch = ZipFile.file.match(/v\d+\.\d+/);     console.log('versionMatch', versionMatch[0]);
          const match = ZipFile.file.match(/_(Odyssey|Horizons)_/); console.log('match', match[1]);

          const _ret = await window.api.decompressFile(ZipFile.file, unzipPath);
          console.log('ZipFile:', _ret);
          if (_ret.success) {

            if (match[1] === 'Odyssey') {
              this.settings.Version_ODYSS = versionMatch[0];
            } else {
              this.settings.Version_HORIZ = versionMatch[0];
            }
            
          }
        } else {
          eventBus.emit('ShowError', new Error('404 - Zip File Not Found'));
        }
      } catch (error) {
        eventBus.emit('ShowError', error);
      } finally {
        eventBus.emit('ShowSpinner', { visible: false });
      }
    },

    /**
     * Fires when the Settings had been changed
     * @param newConfig the updated settings
     */
    async saveConfig(newConfig) { // Handle the updated config here 
      console.log('Config saved:', newConfig);

      const gameInstance = await window.api.getActiveInstanceEx();
      eventBus.emit('loadThemes', gameInstance);  //<- this event will be heard in 'ThemeTab.vue'

      eventBus.emit('RoastMe', { type: 'Success', message: 'Settings Applied!' });

      this.installEDHMmod(gameInstance);

      const jsonString = JSON.stringify(this.settings, null, 4);
      const updateSettings = await window.api.saveSettings(jsonString);

      eventBus.emit('RoastMe', { type: 'Success', message: `EDHM v${updateSettings.Version_ODYSS} Installed.` });
      eventBus.emit('RoastMe', { type: 'Info', message: 'You can Close this now.' });
      eventBus.emit('modUpdated', updateSettings); //<- Event listen in MainNavBars.vue

      //await window.api.saveSettings(jsonString);

      if (this.InstallStatus === 'freshInstall') {
        eventBus.emit('RoastMe', { type: 'Info', message: 'You can close this now.' });
      }
    },


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
          eventBus.emit('ShowSpinner', { visible: false });
          eventBus.emit('RoastMe', { type: 'Success', message: 'Welcome to the application!<br>You now need to tell EDHM where is your game located.' });
          eventBus.emit('open-settings-editor', this.InstallStatus); //<- Open the Settings Window

          break;
        case 'upgradingUser':
          eventBus.emit('RoastMe', { type: 'Success', message: 'Upgrade Complete!\r\nThe application has been upgraded successfully.' });
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

    eventBus.on('SettingsChanged', this.saveConfig); //No es necesario, al parecer va por Ref..
  },
  beforeUnmount() {
    // Clean up the event listener
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

</style>


