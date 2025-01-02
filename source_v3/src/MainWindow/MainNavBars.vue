<template>
  <div id="Container">
    <!-- Top Navbar -->
    <nav id="TopNavBar" class="navbar bg-dark text-light border-body fixed-top bg-body-tertiary" data-bs-theme="dark">
      <div class="container-fluid d-flex justify-content-between align-items-center">

        <!-- Main Menu -->
        <div class="nav-item">
          <div class="input-group mb-3">
            <select ref="mainMenuSelect" id="mainMenuSelect" class="form-select main-menu-style"
              @change="menuClicked($event.target.value)">
              <option default value="mnuDummy">Main Menu</option>
              <option value="mnuSettings">Settings</option>
              <option value="mnuOpenGame">Open Game Folder</option>
              <option value="" disabled>──────────</option>
              <option value="mnuShipyard">Shipyard</option>
              <option value="mnu3PModsManager">3PMods (Plugins)</option>
              <option value="" disabled>──────────</option>
              <option value="mnuInstallMod">Install EDHM</option>
              <option value="mnuUninstallMod">Un-install EDHM</option>
              <option value="" disabled>──────────</option>
              <option value="mnuCheckUpdates">Check for Updates</option>
              <option value="mnuGoToDiscord">Help</option>
              <option value="mnuAbout">About</option>
            </select>
          </div>
        </div>

        <!-- Navbar for Buttons on the right side -->
        <div class="nav-item d-flex align-items-center">
          <div class="input-group mb-3">

            <button id="cmdAddNewTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-title="Add New Theme" @mousedown="addNewTheme">
              <i class="bi bi-plus-circle"></i>
            </button>
            <button id="cmdExportTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-title="Export Theme" @mousedown="exportTheme">
              <i class="bi bi-save"></i>
            </button>
            <button id="cmdSaveTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-title="Save Theme" @mousedown="saveTheme">
              <i class="bi bi-floppy"></i>
            </button>
            <button id="cmdShowFavorites" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-title="Toggle Favorites" @mousedown="toggleFavorites">
              <i class="bi bi-star"></i>
            </button>


            <button id="cmdApplyTheme" class="btn btn-apply-theme" @click="applyTheme">Apply Theme</button>

            <select class="form-select" id="cboHistoryBox" @change="handleHistoryChange" v-model="selectedHistory" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-title="History Box">
              <option default value="mnuDummy">.</option>
              <option v-for="option in historyOptions" :key="option.value" :value="option.value" :data-tag="option.tag">{{ option.text }}</option>
            </select>


            <!--<button id="cmdApplyTheme" type="button" class="btn btn-danger dropdown-toggle" data-bs-toggle="dropdown" aria-expanded="false"  @click="ApplyTheme">
              Apply Theme</button>
            <ul class="dropdown-menu">
              <li><a class="dropdown-item" href="#">Action</a></li>
              <li><a class="dropdown-item" href="#">Another action</a></li>
              <li><a class="dropdown-item" href="#">Something else here</a></li>
              <li><hr class="dropdown-divider"></li>
              <li><a class="dropdown-item" href="#">Separated link</a></li>
            </ul>-->

          </div>

        </div>
      </div>
    </nav>


    <!-- Middle Div - Content -->
    <div class="middle-div">

      <div v-if="showSpinner" class="d-flex justify-content-center align-items-center position-fixed top-0 left-0 w-100 h-100 bg-dark bg-opacity-75 z-index-999">
        <div class="spinner-border text-light" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>

      <!-- The Ship's HUD image -->
      <div class="row no-gutters full-height m-0 h-100">
        <div class="col-8 h-100">
          <HUD_Areas />
        </div>

        <div class="col-4 border border-secondary d-flex flex-column h-100">
          <!-- This contains the Controls of the Tabs -->
          <div id="MainTabBarControls" class="content flex-grow-1 d-flex flex-column overflow-hidden">
            <ThemeTab v-show="activeTab === 'themes'" />
            <PropertiesTab v-show="activeTab === 'properties'" />
            <UserSettingsTab v-show="activeTab === 'settings'" />
            <GlobalSettingsTab v-show="activeTab === 'global-settings'" />
          </div>

          <!-- These are the TabBar buttons -->
          <nav id="MainTabBar" class="navbar navbar-expand-sm navbar-dark bg-dark custom-navbar">
            <ul class="navbar-nav mx-auto">
              <li class="nav-item">
                <a class="nav-link" href="#" :class="{ 'active-nav-item': activeTab === 'themes' }"
                  @click.prevent="setActiveTab('themes')">Themes</a>
              </li>
              <li class="nav-item">
                <a class="nav-link" href="#" :class="{ 'active-nav-item': activeTab === 'properties' }"
                  @click.prevent="setActiveTab('properties')">Properties</a>
              </li>
              <li class="nav-item">
                <a class="nav-link" href="#" :class="{ 'active-nav-item': activeTab === 'settings' }"
                  @click.prevent="setActiveTab('settings')">User Settings</a>
              </li>
              <li class="nav-item">
                <a class="nav-link" href="#" :class="{ 'active-nav-item': activeTab === 'global-settings' }"
                  @click.prevent="setActiveTab('global-settings')">Global Settings</a>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </div>

    <!-- Bottom Navbar -->
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark fixed-bottom navbar-thin" data-bs-theme="dark">
      <div class="container-fluid">
        <!-- Main Menu -->
        <div class="navbar-nav">
          <!-- Game Selection Dropdown -->
          <div class="nav-item">
            <select id="gameSelect" class="form-select game-dropdown-border main-menu-style" v-model="selectedGame"
              @change="selectGame">
              <option v-for="(game, index) in gameMenuItems" :key="index" :value="game">{{ game }}</option>
            </select>
          </div>
        </div>

        <!-- App Version Label -->
        <span class="navbar-text mx-3" id="lblVersion">App Version: {{ appVersion }}</span>
        <!-- Mod Version Label -->
        <span class="navbar-text mx-3" id="lblModVersion">EDHM Version: {{ modVersion }}</span>

        <!-- Search Box -->
        <form class="d-flex ms-auto">
          <input class="form-control me-2 main-menu-style" type="search" placeholder="Search" aria-label="Search">
          <button class="btn btn-outline-warning" type="submit">Search</button>
        </form>
      </div>
    </nav>
  </div>

  <div v-if="showSpinner" class="spinner-border text-primary" role="status">
      <span class="visually-hidden">Loading...</span>
    </div>
</template>

<script>
import { ref, onMounted, provide } from 'vue';

import eventBus from '../EventBus';
import HUD_Areas from './HudImage.vue';
import ThemeTab from './ThemeTab.vue';
import PropertiesTab from './PropertiesTab.vue';
import UserSettingsTab from './UserSettingsTab.vue';
import GlobalSettingsTab from './GlobalSettingsTab.vue';

import defaultTemplate from '../data/ED_Odissey_ThemeTemplate.json';
let themeTemplate = JSON.parse(JSON.stringify(defaultTemplate)); 
let programSettings = null;

export default {
  name: 'MainNavBars',
  props: {
    settings: {
      type: Object,
      required: true
    }
  },
  components: {
    HUD_Areas,
    ThemeTab,
    PropertiesTab,
    UserSettingsTab,
    GlobalSettingsTab
  },
  data() {
    return {
      activeTab: 'themes',
      showFavorites: false,
      showSpinner: true,
    };
  },
  setup(props) {
    //console.log(props);
   
    const appVersion = ref('');
    const modVersion = ref('');
    const selectedGame = ref(props.settings.ActiveInstance || 'Select a Game'); // Set initial value from settings
    const gameMenuItems = ref( // Populate game instances with the `instance` values from `Settings`
      props.settings.GameInstances.flatMap(instance => instance.games.map(game => game.instance && game.path != ''))
    );
    programSettings = props.settings;
    const historyOptions = ref([]);
    const selectedHistory = ref('dummy'); // Initialize with the dummy value

    onMounted(async () => {
      try {
        appVersion.value = await window.api.getAppVersion();
        modVersion.value = props.settings.Version_ODYSS;
        await History_LoadElements();

        // Loads the 'Current Settings' from the Ini files:
        const ActiveInstance = await window.api.getActiveInstance();            //console.log('ActiveInstance', ActiveInstance.path);
        if (ActiveInstance.path != '') {
          const themePath = window.api.joinPath(ActiveInstance.path, 'EDHM-ini');
          const ThemeINIs = await window.api.LoadThemeINIs(themePath);  //console.log('ThemeINIs:', ThemeINIs);

          themeTemplate = await window.api.applyIniValuesToTemplate(themeTemplate, ThemeINIs);   //console.log('ThemeTemplate: ', themeTemplate);  
          themeTemplate.credits.theme = "Current Settings"; // theme.file.credits.theme;
          themeTemplate.credits.author = "User"; // theme.file.credits.author;
          themeTemplate.credits.description = "Currently Applied Colors in Game"; // theme.file.credits.description;
          themeTemplate.credits.preview = ""; // theme.file.credits.preview;
          themeTemplate.path = themePath;
          themeTemplate.version = props.settings.Version_ODYSS; //<- Load version from EDHM

          eventBus.emit('ThemeLoaded', themeTemplate); //<- this event will be heard in 'PropertiesTab.vue'
          //eventBus.emit('RoastMe', { title: 'Theme Loaded', message: 'Current Settings'}); //<- this event will be heard in 'App.vue'

          // Provide the themeTemplate data to be accessible by all components 
          provide('themeTemplate', themeTemplate);
        }      
 
      } catch (error) {
        eventBus.emit('ShowError', error);
      }
    });


    /**
     * When a Game Instance is selected from the '#gameSelect' combo
     */
    const selectGame = (event) => {
      selectedGame.value = event.target.value;
      if (props.settings) {
        props.settings.ActiveInstance = selectedGame.value;
        window.api.saveSettings(
          JSON.stringify(props.settings, null, 4));
      }
      console.log(`Game selected: ${selectedGame.value}`);
    };
    const History_LoadElements = async () => {
      try {
        const programSettings = props.settings;
        const NumberOfSavesToRemember = programSettings.SavesToRemember;
        const gameInstance = await window.api.getActiveInstance();
        const themesFolder = gameInstance.themes_folder;       //console.log('themesFolder:', themesFolder);

        if (!themesFolder) {
          throw new Error('Themes folder is undefined');
        }

        const UserDocsPath = window.api.getParentFolder(themesFolder);
        const HistoryFolder = window.api.joinPath(UserDocsPath, 'History'); //console.log('HistoryFolder:', HistoryFolder);

        const files = await window.api.loadHistory(HistoryFolder, NumberOfSavesToRemember);
        //console.log(files);

        historyOptions.value = files.map(file => ({
          value: file.name,
          text: file.date,
          tag: file.path  // Add the file path as a tag
        }));

        //console.log(historyOptions.value);

      } catch (error) {
        //eventBus.emit('ShowError', error);
      }
    };
    const History_AddSettings = async (theme) => {
      try {
        const programSettings = this.settings;
        const ActiveInstance = programSettings.ActiveInstance;
        const gameInstances = programSettings.GameInstances;

        const activeInstanceSettings = gameInstances.find(instance => instance.games.some(game => game.instance === ActiveInstance));
        const themesFolder = activeInstanceSettings.games[0]?.themes_folder;

        console.log('themesFolder:', themesFolder);

        if (!themesFolder) {
          throw new Error('Themes folder is undefined');
        }

        const UserDocsPath = window.api.resolvePath(themesFolder, '..');
        const HistoryFolder = window.api.joinPath(UserDocsPath, 'History');

        await window.api.saveHistory(HistoryFolder, theme);

        await this.History_LoadElements();
      } catch (error) {
        eventBus.emit('ShowError', error);
      }
    };
    return {
      appVersion,
      modVersion,
      selectedGame,
      gameMenuItems,
      programSettings,
      selectGame,
      historyOptions,
      History_LoadElements,
      History_AddSettings
    };
  },
  methods: {
    setActiveTab(tab) {
      this.activeTab = tab;
    },
    async menuClicked(value) {
      if (value) {
        this.$refs.mainMenuSelect.value = 'mnuDummy'; // Reset the select to "Main Menu"
        console.log(`Menu ${value} clicked`);
        
        const ActiveInstance = await window.api.getActiveInstance(); 
        const GamePath = await window.api.joinPath(ActiveInstance.path, 'EDHM-ini'); 

        if (value === 'mnuOpenGame') {
          await window.api.openPathInExplorer(GamePath);
        }
        if (value === 'mnuSettings') {
          const InstallStatus = await window.api.InstallStatus();
          eventBus.emit('open-settings-editor', InstallStatus);          
        }

      }
    },

   
    async LoadTheme(theme) {
      /* Happens when a Theme in the list is Selected  */
      this.showSpinner = true;
      try {
        const template = JSON.parse(JSON.stringify(theme.file));
        themeTemplate = await window.api.LoadTheme(template.path);
        console.log('Loaded theme: ', themeTemplate);

        eventBus.emit('ThemeLoaded', themeTemplate); //<- this event will be heard in 'PropertiesTab.vue'

      } catch (error) {
        eventBus.emit('ShowError', error);
      } finally { this.showSpinner = false; }
    },
    async applyTheme() {
      this.showSpinner = true;
      try {
        const ActiveInstance = await window.api.getActiveInstance(); 
        console.log('1. ActiveInstance:', ActiveInstance);
        console.log('2. ThemeTemplate:', themeTemplate);
        
        const GamePath = await window.api.joinPath(ActiveInstance.path, 'EDHM-ini'); 
        const defaultInisPath = await window.api.getAssetPath('data/ODY');  
        console.log('3. Preparing all the Paths:', GamePath);

        const defaultINIs = await window.api.LoadThemeINIs(defaultInisPath);  
        console.log('4. Get Default Inis:', defaultINIs);
        
        const updatedInis = await window.api.ApplyTemplateValuesToIni(themeTemplate, defaultINIs);
        console.log('5. Applying Changes to the INIs...', updatedInis);
        
        console.log('6. Saving the INI files..');
        const _ret = await window.api.SaveThemeINIs(GamePath, updatedInis);
        if (_ret) {
          eventBus.emit('RoastMe', { type: 'Success', message: `Theme: '${themeTemplate.credits.theme}' Applied!'` });
        }
        setTimeout(() => {
            this.showSpinner = false;       
          }, 1500);
      } catch (error) {
        this.showSpinner = false; 
        eventBus.emit('ShowError', error);
      } 
      


  /*    setTimeout(() => {
          this.loading = false;
          eventBus.emit('ShowSpinner', { visible: false } ); //<- this event will be heard in 'MainNavBars.vue'          
      }, 3000); */

      /*
      const options = {
        type: 'none', //<- none, info, error, question, warning
        //icon: image,
        buttons: ['Cancel', 'Yes, please', 'No, thanks'],
        defaultId: 1,
        title: 'Question',
        message: 'Do you want to proceed?',
        detail: 'It does not really matter',
        cancelId: 0,
        checkboxLabel: 'Remember my answer', checkboxChecked: false,
      }; 
      const result = await window.api.ShowDialog(options); 

      console.log('User clicked:', result.response); // Index of the button clicked
      console.log('Checkbox checked:', result.checkboxChecked); // Boolean for checkbox state
*/
      
    },
    async addNewTheme(event) {
      console.log('Add New Theme button clicked');
      this.applyIconColor(event.target);

      const myPath = await window.api.resolveEnvVariables('%USERPROFILE%\\EDHM_UI\\Settings.json');
      console.log('myPath:', myPath);
      const myPath2 = await window.api.resolveEnvVariables('%USERPROFILE%');
      console.log('%USERPROFILE%:', myPath2);
      const myPath3 = await window.api.resolveEnvVariables('D:\\@Codigo\\EDHM_UI\\source_v3');
      console.log('myPath3:', myPath3); 
      const myPath4 = await window.api.resolveEnvVariables('%APPDATA%\\EDHM_UI');
      console.log('%APPDATA%:', myPath4);
      const myPath5 = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\EDHM_UI');
      console.log('%LOCALAPPDATA%:', myPath5);

     /* const Key = "x232|y232|z232|w232"; console.log('Key:',Key);  
      const IntValue = 16755200; console.log('IntValue', IntValue);
      const RGBAcolor = await window.api.intToRGBA(IntValue); console.log('RGBAcolor', RGBAcolor); //<- {r: 255, g: 170, b: 0, a: 255}
      const sRGBcolor = await window.api.GetGammaCorrected_RGBA(RGBAcolor, 2.4); console.log('sRGBcolor', sRGBcolor); //<- {r: 1, g: 0.402, b: 0, a: 1}
      const ShouldBe = { r: 1, g: 0.3763, b: 0, a: 1 }; console.log('ShouldBe', ShouldBe); //<- { r: 1, g: 0.3763, b: 0, a: 1 }
*/
      // Red x232 =1 ; Green y232 =0.3763 ; Blue z232 =0 ; Alpha w232 =1
      //eventBus.emit('RoastMe', { type: 'Success', message: 'First Line\r\nSecond Line\r\nThird Line' }); //<- this event will be heard in 'App.vue'

    },
    async exportTheme(event) {
      console.log('Export Theme button clicked');
      this.applyIconColor(event.target);

      eventBus.emit('RoastMe', { type: 'Error', message: '<p>This is normal text - <b>and this is bold text</b>.</p>' }); //<- this event will be heard in 'App.vue'

      if (themeTemplate != null && themeTemplate.credits.theme != "Current Settings") {
        console.log('Exporting theme: ', themeTemplate.credits.theme);
        
      } else {
        console.error('Current Settings can not be saved??');
      }
    },
    async saveTheme(event) {
      //console.log('Save Theme button clicked');
      this.applyIconColor(event.target);
      eventBus.emit('RoastMe', { type: 'Info', message: `Theme Saved!:\r\n ${themeTemplate.credits.theme}` }); //<- this event will be heard in 'App.vue'
      
      if (themeTemplate != null && themeTemplate.credits.theme != "Current Settings") {
        const jsonPath = window.api.joinPath(themeTemplate.path, 'Theme.json');
        window.api.writeJsonFile(jsonPath, themeTemplate, true);

      } else {
        eventBus.emit('RoastMe', { type: 'Error', message: 'Current Settings can not be saved' });
      }
      //
    },

    toggleFavorites(event) {
      console.log('Favorites toggled:', this.showFavorites);
      this.applyIconColor(event.target);
      eventBus.emit('RoastMe', { type: 'Warning', message: 'First Line\r\nSecond Line\r\nThird Line' });
    },
    applyIconColor(button) {
      const icon = button.querySelector('i');
      if (icon) {
        icon.classList.add('icon-shine');
        button.addEventListener('mouseup', () => {
          icon.classList.remove('icon-shine');
        }, { once: true });
      }
    },

    handleHistoryChange(event) {
      // Click an item on the History Box
      const selectedValue = event.target.value;
      const selectedOption = event.target.options[event.target.selectedIndex];
      const tag = selectedOption.getAttribute('data-tag');

      console.log('History option changed to:', selectedValue);
      console.log('Selected file path (tag):', tag);

      document.querySelector('#cboHistoryBox').value = 'mnuDummy';  // Reset the select 
    },
    async History_AddSettings(theme) {
      try {
        const programSettings = this.settings;
        const ActiveInstance = programSettings.ActiveInstance;
        const gameInstances = programSettings.GameInstances;

        const activeInstanceSettings = gameInstances.find(instance => instance.games.some(game => game.instance === ActiveInstance));
        const themesFolder = activeInstanceSettings?.themes_folder;
        const UserDocsPath = path.resolve(themesFolder, '..');
        const HistoryFolder = path.join(UserDocsPath, 'History');

        // Use IPC to save history data
        await window.api.saveHistory(HistoryFolder, theme);

        // Reload history options
        await this.History_LoadElements();
      } catch (error) {
        eventBus.emit('ShowError', error);
      }
    },
    showHideSpinner(status) {
      //console.log('showHideSpinner: ', status.visible);
      this.showSpinner = status.visible;
      //EXAMPLE: ->    eventBus.emit('ShowSpinner', { visible: true } );//<- this event will be heard in 'MainNavBars.vue'
    },
    modUpdated(data) {
      // happens when the mod gets updated
      this.programSettings = data;
      //console.log('programSettings: ', programSettings);
      this.modVersion = data.Version_ODYSS;      
    }

  },
  mounted() {
     /* EVENTS WE LISTEN TO HERE:  */
    eventBus.on('setActiveTab', this.setActiveTab);
    eventBus.on('ThemeClicked', this.LoadTheme);
    eventBus.on('ShowSpinner', this.showHideSpinner);
    eventBus.on('modUpdated', this.modUpdated);
  },
  beforeUnmount() {
    // Clean up the event listener
    eventBus.off('setActiveTab', this.setActiveTab);
    eventBus.off('ThemeClicked', this.LoadTheme);
    eventBus.off('ShowSpinner', this.showHideSpinner);
    eventBus.off('modUpdated', this.modUpdated);
  }
};
</script>


<style scoped>
body {
  background-color: #1F1F1F;
  color: #fff; /* Optional: Set text color to white */
}
.z-index-999 {
  z-index: 999; /* Ensure the spinner is on top of other elements */
}
#Container {
  background-color: #1F1F1F;
  color: #fff; /* Optional: Set text color to white */
}
#cboHistoryBox {
  width: 1px; /* Minimal width for the select element */
}

#cboHistoryBox option {
  white-space: nowrap; /* Prevent line breaks */
}

#TopNavBar {
  height: 62px;
  background-color: #1F1F1F;
}


.main-menu-style {
  border: 2px solid orange;
  /* Add an orange border */
}

.middle-div {
  border: 2px solid orange;
  position: fixed;
  top: 62px;
  /* Height of the top navbar */
  bottom: 56px;
  /* Height of the bottom navbar */
  left: 0;
  right: 0;
  background-color: #1F1F1F;
  /* Dark background */
  overflow-y: auto;
  /* Enable scrolling */
  scrollbar-width: thin;
  /* For Firefox */
  scrollbar-color: #343a40 #1e1e1e;
  /* For Firefox */
}

.btn-outline-secondary i { transition: color 0.3s ease-in-out; } 
.btn-outline-secondary:active i { color: Dodgerblue !important;}

.full-height {
  height: 100%;
}

.h-100 {
  height: 100%;
}

/* For WebKit browsers */
.middle-div::-webkit-scrollbar {
  width: 8px;
  /* Width of the scrollbar */
}

.middle-div::-webkit-scrollbar-thumb {
  background-color: #1F1F1F;
  /* Color of the scrollbar */
  border-radius: 10px;
  /* Rounded corners */
}

.middle-div::-webkit-scrollbar-track {
  background-color: #1F1F1F;
  /* Color of the track */
}

/* Styles for the Apply Theme Button */
.btn-apply-theme {
  background-color: darkorange;
  color: rgb(12, 12, 12);
  border: none;
}

.btn-apply-theme:hover {
  background-color: orangered;
  color: white;
}

/* Styles for MainTabBar nav */
.custom-navbar {
  border: 1px solid rgb(110, 73, 2);
  font-family: 'Segoe UI', sans-serif;
  font-size: 14px;
  color: #f8f9fa;
  /* Light text color for contrast */
}

.navbar-nav .nav-item .nav-link {
  color: #f8f9fa;
  transition: background-color 0.3s, color 0.3s;
  border-radius: 5px;
  /* Ensure rounded corners */
}

.navbar-nav .nav-item .nav-link:hover {
  background-color: orangered;
  color: white;
  border-radius: 5px;
  /* Ensure rounded corners on hover */
}

.navbar-nav .nav-item .active-nav-item {
  background-color: darkorange;
  color: #000000;
  /* Dark text color for contrast */
  border-radius: 5px;
}


/* ----- Styles for the Buttons on the top bar  --------- */
.btn-icon {
  color: #ffffff;
  /* Default icon color */
  border: none;
  background: none;
  padding: 5px;
  margin: 0 5px;
  font-size: 18px;
  cursor: pointer;
}

.btn-icon:hover {
  color: orange;
  /* Change icon color on hover */
}

.btn-icon:active {
  color: dodgerblue;
  /* Change icon color on click */
}

.lblInfo {
  position: absolute;
  background-color: #333;
  color: #fff;
  /* Clear text color */
  padding: 5px;
  border-radius: 3px;
  font-size: 12px;
  text-align: right;
  /* Right align text */
  white-space: nowrap;
  transform: translateX(-100%);
  /* Spread text to the left */
  z-index: 10;
  margin-right: 10px;
  /* Adjust as needed for tooltip position */
  animation: fadeIn 0.5s ease-in-out;
}

.lblInfo.fade-out {
  animation: fadeOut 0.5s ease-in-out;
}

.separator {
  width: 10px;
  height: 100%;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }

  to {
    opacity: 1;
  }
}

@keyframes fadeOut {
  from {
    opacity: 1;
  }

  to {
    opacity: 0;
  }
}
</style>
