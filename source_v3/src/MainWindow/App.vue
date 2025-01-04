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


    /** Fires when the Settings had been changed, called from 'SettingsEditor'
     * @param newConfig the updated settings
     */
    async OnProgramSettings_Changed(newConfig) { 
      try {
        //console.log('newConfig:', newConfig);

        eventBus.emit('ShowSpinner', { visible: true });
        eventBus.emit('RoastMe', { type: 'Info', message: 'Installing EDHM files..' });

        const gameInstance = await window.api.getActiveInstanceEx();
        const gameVersion = gameInstance.key === "ED_Odissey" ? newConfig.Version_ODYSS : newConfig.Version_HORIZ ;
        const EdhmExists = await window.api.CheckEDHMinstalled(gameInstance.path);
        if (!EdhmExists) {
          const edhmInstalled = await window.api.installEDHMmod(gameInstance);
          console.log('edhmInstalled:', edhmInstalled);
          gameVersion = edhmInstalled.version;

          if (edhmInstalled.game === 'ODYSS') {
            newConfig.Version_ODYSS = edhmInstalled.version;
          } else {
            newConfig.Version_HORIZ = edhmInstalled.version;
          }        
          this.settings = newConfig;
        }        

        const jsonString = JSON.stringify(newConfig, null, 4);
        await window.api.saveSettings(jsonString);

        eventBus.emit('modUpdated', newConfig);     //<- Event listen in MainNavBars.vue
        eventBus.emit('loadThemes', gameInstance);  //<- this event will be heard in 'ThemeTab.vue'

        eventBus.emit('RoastMe', { type: 'Success', message: `EDHM ${gameVersion} Installed.` });
        eventBus.emit('RoastMe', { type: 'Info', message: 'You can Close this now.' });

        if (this.InstallStatus === 'freshInstall') {
          eventBus.emit('RoastMe', { type: 'Info', message: 'You can close this now.' });
        }
      } catch (error) {
        eventBus.emit('ShowError', error);
      } finally {
        eventBus.emit('ShowSpinner', { visible: false });
      }      
    },

    async OnGameInstance_Changed(GameInstanceName) {
      try {
        eventBus.emit('ShowSpinner', { visible: true });
        console.log('activeInstance', this.settings.ActiveInstance ); //<- 'ActiveInstance' Changed by Ref.

        const NewInstance = await window.api.getInstanceByName(GameInstanceName);
        console.log('NewInstance:', NewInstance);

        const EdhmExists = await window.api.CheckEDHMinstalled(NewInstance.path);
        if (!EdhmExists) {
          eventBus.emit('RoastMe', { type: 'Info', message: `Installing EDHM on '${GameInstanceName}'..` });
          const edhmInstalled = await window.api.installEDHMmod(NewInstance);

          if (edhmInstalled.game === 'ODYSS') {
            this.settings.Version_ODYSS = edhmInstalled.version;
          } else {
            this.settings.Version_HORIZ = edhmInstalled.version;
          }

          eventBus.emit('modUpdated', this.settings);     //<- Event listen in MainNavBars.vue
          eventBus.emit('loadThemes', NewInstance);  //<- this event will be heard in 'ThemeTab.vue'

          eventBus.emit('RoastMe', { type: 'Success', message: `EDHM ${edhmInstalled.version} Installed.` });
          eventBus.emit('RoastMe', { type: 'Info', message: 'You can Close this now.' });
        }
        
        const jsonString = JSON.stringify(this.settings, null, 4);
        await window.api.saveSettings(jsonString);

      } catch (error) {
        eventBus.emit('ShowError', error);
      } finally {
        eventBus.emit('ShowSpinner', { visible: false });
      } 
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

    /* LISTENING EVENTS:   */
    eventBus.on('SettingsChanged', this.OnProgramSettings_Changed); 
    eventBus.on('GameInsanceChanged', this.OnGameInstance_Changed); 
  },
  beforeUnmount() {
    // Clean up the event listener
    eventBus.off('SettingsChanged', this.OnProgramSettings_Changed); 
    eventBus.off('GameInsanceChanged', this.OnGameInstance_Changed); 
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


