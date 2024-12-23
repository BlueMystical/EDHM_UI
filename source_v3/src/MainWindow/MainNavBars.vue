<template>
  <div id="Container">
    <!-- Top Navbar -->
    <nav id="TopNavBar" class="navbar bg-dark border-body fixed-top bg-body-tertiary" data-bs-theme="dark">
      <div class="container-fluid d-flex justify-content-between align-items-center">

        <!-- Main Menu -->
        <div class="nav-item">
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

        <!-- Navbar for Buttons on the right side -->
        <div class="nav-item d-flex align-items-center">
          <div class="input-group mb-3">

            <button id="cmdAddNewTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-title="Add New Theme" @mousedown="addNewTheme">
              <i class="bi bi-plus-circle"></i>
            </button>
            <button id="cmdExportTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-title="Export Theme" @mousedown="exportTheme">
              <i class="bi bi-save"></i>
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

        <!-- Search Box -->
        <form class="d-flex ms-auto">
          <input class="form-control me-2 main-menu-style" type="search" placeholder="Search" aria-label="Search">
          <button class="btn btn-outline-warning" type="submit">Search</button>
        </form>
      </div>
    </nav>
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

export default {
  name: 'MainNavBars',
  props: {
    settings: {
      type: Object,
      required: true
    }
  },
  setup(props) {
    //console.log(props);
    const appVersion = ref('');
    const selectedGame = ref(props.settings.ActiveInstance || 'Select a Game'); // Set initial value from settings
    const gameMenuItems = ref( // Populate game instances with the `instance` values from `Settings`
      props.settings.GameInstances.flatMap(instance => instance.games.map(game => game.instance))
    );
    const historyOptions = ref([]);
    const selectedHistory = ref('dummy'); // Initialize with the dummy value

    onMounted(async () => {
      try {
        appVersion.value = await window.api.getAppVersion();
        await History_LoadElements();

        // Loads the 'Current Settings' from the Ini files:
        const ActiveInstance = await window.api.getActiveInstance();            //console.log('ActiveInstance', ActiveInstance.path);
        const themePath = window.api.joinPath(ActiveInstance.path, 'EDHM-ini');
        const ThemeINIs = await window.api.LoadThemeINIs(themePath);  console.log('ThemeINIs:', ThemeINIs);

        themeTemplate = await window.api.applyIniValuesToTemplate(themeTemplate, ThemeINIs);               //console.log('ThemeTemplate: ', themeTemplate);   
        console.log(themeTemplate.ui_groups[7].Elements[5].Key, themeTemplate.ui_groups[7].Elements[5].Value);   
        
        eventBus.emit('ThemeLoaded', themeTemplate); //<- this event will be heard in 'PropertiesTab.vue'

        // Provide the themeTemplate data to be accessible by all components 
        provide('themeTemplate', themeTemplate);

      } catch (error) {
        console.error('Error retrieving Active Instance:', error);
        // Handle the error (e.g., show error message)
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
        console.error('Failed to load history elements:', error);
        window.api.logEvent(error);
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
        console.error('Failed to add theme to history:', error);
        window.api.logEvent(error.message, error.stack);
      }
    };
    return {
      appVersion,
      selectedGame,
      gameMenuItems,
      selectGame,
      historyOptions,
      History_LoadElements
    };
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
      tooltipVisible: false,
      tooltipText: ''
    };
  },
  methods: {
    setActiveTab(tab) {
      this.activeTab = tab;
    },
    menuClicked(value) {
      if (value) {
        console.log(`Menu ${value} clicked`);
        this.$refs.mainMenuSelect.value = 'mnuDummy'; // Reset the select to "Main Menu"
      }
    },

    addNewTheme(event) {
      console.log('Add New Theme button clicked');
      this.applyIconColor(event.target);
    },
    exportTheme(event) {
      console.log('Export Theme button clicked');
      this.applyIconColor(event.target);
    },
    toggleFavorites(event) {
      console.log('Favorites toggled:', this.showFavorites);
      this.applyIconColor(event.target);
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

    showTooltip(event) {
      this.tooltipText = event.target.getAttribute('data-description');
      this.tooltipVisible = true;
    },
    hideTooltip() {
      this.tooltipVisible = false;
    },
    applyTheme() {
      console.log('Apply Theme button clicked');
      // Add your theme application logic here
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
        console.error('Failed to add theme to history:', error);
      }
    },
    async LoadTheme(theme) {
      /* Happens when a Theme in the list is Selected  */
      //console.log('Theme Selected: ', theme);    console.log(theme.file.path);

      const ThemeINIs = await window.api.LoadThemeINIs(theme.file.path);          //console.log('ThemeINIs:', ThemeINIs);
      themeTemplate = await window.api.applyIniValuesToTemplate(themeTemplate, ThemeINIs);   //console.log('ThemeTemplate: ', themeTemplate);  
      themeTemplate.name = theme.name;
      themeTemplate.author = theme.file.credits.author;

      console.log('Theme Changed: ', themeTemplate);
      eventBus.emit('ThemeLoaded', themeTemplate); //<- this event will be heard in 'PropertiesTab.vue'

      console.log(themeTemplate.ui_groups[7].Elements[5].Key, themeTemplate.ui_groups[7].Elements[5].Value);   

      //console.log('z103: ', themeTemplate.ui_groups[1].Elements[0].Value   );
      //console.log('z103: ->', defaultTemplate.ui_groups[1].Elements[0].Value   );
    }
  },
  mounted() {
    // Listen for that happens when the User clicks on one of the 'MainTabBar'
    eventBus.on('setActiveTab', this.setActiveTab);
    eventBus.on('ThemeClicked', this.LoadTheme);
  },
  beforeUnmount() {
    // Clean up the event listener
    eventBus.off('setActiveTab', this.setActiveTab);
    eventBus.off('ThemeClicked', this.LoadTheme);
  }
};
</script>


<style scoped>
body {
  background-color: #1F1F1F;
  color: #fff; /* Optional: Set text color to white */
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
