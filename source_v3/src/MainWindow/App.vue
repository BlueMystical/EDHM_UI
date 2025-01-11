<template>
  <div id="app" class="bg-dark text-light" data-bs-theme="dark">
    
    <NavBarsBody :themesLoaded="themesLoaded" />
    <SettingsEditor @save="saveConfig" />
    <Notifications/>

    <SearchBox ref="searchBox" :searchResults="searchResults" @resultClicked="OnSearchBox_Click"/>

  </div>
</template>

<script>
import EventBus from '../EventBus.js';
import NavBarsBody from './NavBars.vue';
import SettingsEditor from './SettingsEditor.vue';
import SearchBox from './Components/SearchBox.vue';
import Notifications from './Components/Notifications.vue';

export default {
  name: 'App',
  components: {
    SearchBox,
    NavBarsBody,
    SettingsEditor,    
    Notifications
  },
  data() {
    return {
      loading: true,
      settings: null,
      InstallStatus: null,

      themesLoaded: [],
      themeTemplate: {},

      searchResults: [],
      searchQuery: '',
      isSearchBoxVisible: false
    };
  },
  methods: {

     /** This is the Start Point of the Program **/
     async Initialize() {
      try {
        console.log('Initializing App..');
        EventBus.emit('ShowSpinner', { visible: true });

        this.settings = await window.api.initializeSettings();    //console.log(this.settings);
        this.InstallStatus = await window.api.InstallStatus();    //console.log('this.InstallStatus',  this.InstallStatus);
        const ActiveInstance = await window.api.getActiveInstance();  //console.log('ActiveInstance', ActiveInstance);

        switch (this.InstallStatus) {
          case 'existingInstall':
            if (ActiveInstance && ActiveInstance.path != "") {
              // Normal Load, All seems Good
            } else {
              // Either the Active Instance or its path is not set:
              EventBus.emit('ShowSpinner', { visible: false });
              EventBus.emit('RoastMe', { type: 'Success', message: 'Welcome to the application!<br>You now need to tell EDHM where is your game located.' });
              EventBus.emit('open-settings-editor', this.InstallStatus); //<- Open the Settings Window
            }
            break;
          case 'freshInstall':
            // Welcome New User!
            EventBus.emit('ShowSpinner', { visible: false });
            EventBus.emit('RoastMe', { type: 'Success', message: 'Welcome to the application!<br>You now need to tell EDHM where is your game located.' });
            EventBus.emit('open-settings-editor', this.InstallStatus); //<- Open the Settings Window

            break;
          default:
            break;
        }        
        
        EventBus.emit('InitializeNavBars', JSON.parse(JSON.stringify(this.settings))); //<- Event Listened at NavBars.vue
        EventBus.emit('OnInitializeThemes', JSON.parse(JSON.stringify(this.settings)));//<- Event Listened at ThemeTab.vue
        EventBus.emit('InitializeHUDimage', null ); //<- Event Listened at HudImage.vue

      } catch (error) {
        console.error(error);
      } finally {
        setTimeout(() => {
          EventBus.emit('ShowSpinner', { visible: false });
          this.loading = false;

          this.$nextTick(() => {
            const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
            tooltipTriggerList.forEach(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

            const dropdownElementList = document.querySelectorAll('[data-bs-toggle="dropdown"]');
            dropdownElementList.forEach(dropdownToggleEl => new bootstrap.Dropdown(dropdownToggleEl));
          });
        }, 1000);
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

    /** Fires when the Settings had been changed, called from 'SettingsEditor'
     * @param newConfig the updated settings
     */
    async OnProgramSettings_Changed(newConfig) { 
      try {
        //console.log('newConfig:', newConfig);

        EventBus.emit('ShowSpinner', { visible: true });
        EventBus.emit('RoastMe', { type: 'Info', message: 'Installing EDHM files..' });

        const gameInstance = await window.api.getActiveInstanceEx();
        let gameVersion = gameInstance.key === "ED_Odissey" ? newConfig.Version_ODYSS : newConfig.Version_HORIZ ;
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
        this.settings = await window.api.saveSettings(jsonString);

        EventBus.emit('InitializeNavBars', JSON.parse(JSON.stringify(this.settings))); //<- Event Listened at NavBars.vue
        EventBus.emit('OnInitializeThemes', JSON.parse(JSON.stringify(this.settings)));//<- Event Listened at ThemeTab.vue

        EventBus.emit('modUpdated', newConfig);     //<- Event listen in MainNavBars.vue
        EventBus.emit('loadThemes', newConfig.FavToogle);  //<- this event will be heard in 'ThemeTab.vue'

        EventBus.emit('RoastMe', { type: 'Success', message: `EDHM ${gameVersion} Installed.` });
        EventBus.emit('RoastMe', { type: 'Info', message: 'You can Close this now.' });

        if (this.InstallStatus === 'freshInstall') {
          EventBus.emit('RoastMe', { type: 'Info', message: 'You can close this now.' });
        }
      } catch (error) {
        EventBus.emit('ShowError', error);
      } finally {
        EventBus.emit('ShowSpinner', { visible: false });
      }      
    },

    /** When the User pick a different Game on the Combo. * 
     * @param GameInstanceName Name of the New Active Instance
     */
    async OnGameInstance_Changed(GameInstanceName) {
      try {
        EventBus.emit('ShowSpinner', { visible: true });
        console.log('activeInstance', this.settings.ActiveInstance ); //<- 'ActiveInstance' Changed by Ref.

        const NewInstance = await window.api.getInstanceByName(GameInstanceName);
        console.log('NewInstance:', NewInstance);

        const EdhmExists = await window.api.CheckEDHMinstalled(NewInstance.path);
        if (!EdhmExists) {
          EventBus.emit('RoastMe', { type: 'Info', message: `Installing EDHM on '${GameInstanceName}'..` });
          const edhmInstalled = await window.api.installEDHMmod(NewInstance);

          if (edhmInstalled.game === 'ODYSS') {
            this.settings.Version_ODYSS = edhmInstalled.version;
          } else {
            this.settings.Version_HORIZ = edhmInstalled.version;
          }

          EventBus.emit('InitializeNavBars', JSON.parse(JSON.stringify(this.settings))); //<- Event Listened at NavBars.vue
          EventBus.emit('OnInitializeThemes', JSON.parse(JSON.stringify(this.settings)));//<- Event Listened at ThemeTab.vue

          EventBus.emit('modUpdated', this.settings);     //<- Event listen in MainNavBars.vue
          EventBus.emit('loadThemes', this.settings.FavToogle);  //<- this event will be heard in 'ThemeTab.vue'

          EventBus.emit('RoastMe', { type: 'Success', message: `EDHM ${edhmInstalled.version} Installed.` });
          EventBus.emit('RoastMe', { type: 'Info', message: 'You can Close this now.' });
        }
        
        const jsonString = JSON.stringify(this.settings, null, 4);
        await window.api.saveSettings(jsonString);

      } catch (error) {
        EventBus.emit('ShowError', error);
      } finally {
        EventBus.emit('ShowSpinner', { visible: false });
      } 
    },

    /** Shows the Results of the Search Box 
     * @param event Contains the data to be shown
     */
    OnSearchBox_Shown(event) {
      try {
        this.searchResults = event.data;
        this.$refs.searchBox.show();
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },
    /** When the User Clicks on a Search Result. 
     * @param result The Result clicked
     */
    OnSearchBox_Click(result) { 
      console.log('Result clicked:', result); 
      try {
        if (result.Parent === 'Themes') {
          EventBus.emit('setActiveTab', 'themes'); //<- Event listen in 'MainNavBars.vue'
          EventBus.emit('OnSelectTheme', result.Tag); //<- Event listened on 'ThemeTab.vue'
        } else {
          if (result.Parent === 'Global Settings') {
            
          } else { //<- It's a Normal HUD Settings
            // Emit an event with the clicked area 
            EventBus.emit('areaClicked', { id: result.Parent, title: result.Title } ); //<- Event listen in 'PropertiesTab.vue'           
            // Emit an event to set the active tab to 'properties' 
            EventBus.emit('setActiveTab', 'properties'); //<- Event listen in 'MainNavBars.vue'
            // Ensure Visibility of Selected Item
            EventBus.emit('OnSelectCategory', result.Category); //<- Event listen in 'PropertiesTab.vue'
          }
        }
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },

    /** When all Themes are Loaded. 
     * @param event Contains the data of the themes
     */
    OnThemesLoaded(event) {
      try {        
        this.themesLoaded = event;
        //console.log('OnThemesLoaded:', this.themesLoaded );

      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },   
    /** When the Theme Template is Loaded. 
     * @param event Contains the template of the theme
     */
    OnTemplateLoaded(event) {
      try {
        this.themeTemplate = event;
        //console.log('OnTemplateLoaded:', this.themeTemplate );
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },

   

  },
  async mounted() {

    this.Initialize();

    /* LISTENING EVENTS:   */
    EventBus.on('SettingsChanged', this.OnProgramSettings_Changed); 
    EventBus.on('GameInsanceChanged', this.OnGameInstance_Changed); 
    EventBus.on('SearchBox', this.OnSearchBox_Shown); 
    EventBus.on('OnThemesLoaded', this.OnThemesLoaded); 
    EventBus.on('ThemeLoaded', this.OnTemplateLoaded);
  },
  beforeUnmount() {
    // Clean up the event listener
    EventBus.off('SettingsChanged', this.OnProgramSettings_Changed); 
    EventBus.off('GameInsanceChanged', this.OnGameInstance_Changed); 
    EventBus.off('SearchBox', this.OnSearchBox_Shown); 
    EventBus.off('OnThemesLoaded', this.OnThemesLoaded); 
    EventBus.off('ThemeLoaded', this.OnTemplateLoaded);
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


