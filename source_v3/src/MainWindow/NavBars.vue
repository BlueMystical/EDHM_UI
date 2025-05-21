<template>
  <div id="Container">

    <!-- Top Navbar -->
    <nav id="TopNavBar" class="navbar bg-dark text-light border-body fixed-top bg-body-tertiary" data-bs-theme="dark">
      <div class="container-fluid d-flex justify-content-between align-items-center">

        <!-- Main Menu -->
        <div class="nav-item">
          <div class="input-group mb-3 ">
            <select ref="mainMenuSelect" id="mainMenuSelect" class="form-select main-menu-style"
              @change="MainMenu_Click($event.target.value)">
              <option default value="mnuDummy">Main Menu</option>
              <option value="mnuSettings">Settings</option>
              <option value="mnuOpenGame">Open Game Folder</option>
              <option value="mnuOpenData">Open Data Folder</option>
              <option value="" disabled>──────────</option>
              <option value="mnuShipyard">Shipyard</option>
              <option value="mnu3PModsManager">3PMods (Plugins)</option>
              <option value="" disabled>──────────</option>
              <option value="mnuInstallMod">Install EDHM</option>
              <option value="mnuUninstallMod">Un-install EDHM</option>
              <option disabled value="mnuDisableMod">Enable/Disable EDHM</option>
              <option value="" disabled>──────────</option>
              <option value="mnuGoToDiscord">Help? Join our Discord</option>
              <option value="mnuReadManual">Read the Manual</option>
              <option value="" disabled>──────────</option>
              <option value="mnuCheckUpdates">Check for Updates</option>              
              <option value="mnuExit">Exit App</option>
            </select>
          </div>
        </div>

        <!-- Navbar for Buttons on the right side -->
        <div class="nav-item d-flex align-items-center">
          <span id="lblStatus" class="navbar-text mx-3 text-nowrap ml-auto" style="padding-top: -4px;">{{ statusText
            }}</span>

          <div class="input-group mb-3">

            <button id="cmdAddNewTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip"
              data-bs-placement="bottom" data-bs-title="Add New Theme" @mousedown="addNewTheme_Click">
              <i class="bi bi-plus-circle"></i>
            </button>
            <button id="cmdEditTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip"
              data-bs-placement="bottom" data-bs-title="Edit Theme" @mousedown="editTheme_Click">
              <i class="bi bi-pencil-square"></i>
            </button>
            <button id="cmdExportTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip"
              data-bs-placement="bottom" data-bs-title="Export Theme" @mousedown="exportTheme_Click">
              <i class="bi bi-arrow-bar-up"></i>
            </button>
            <button id="cmdImportTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip"
              data-bs-placement="bottom" data-bs-title="ImportTheme" @mousedown="importTheme_Click">
              <i class="bi bi-arrow-bar-down"></i>
            </button>
            <button id="cmdSaveTheme" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip"
              data-bs-placement="bottom" data-bs-title="Save Theme" @mousedown="saveTheme_Click">
              <i class="bi bi-floppy"></i>
            </button>
            <button id="cmdReloadThemes" class="btn btn-outline-secondary" type="button" data-bs-toggle="tooltip"
              data-bs-placement="bottom" data-bs-title="Reload Themes" @mousedown="reloadThemes_Click">
              <i class="bi bi-arrow-clockwise"></i>
            </button>

            <button id="cmdShowFavorites" :class="['btn btn-outline-secondary', { 'text-orange': showFavorites }]"
              type="button" data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-title="Toggle Favorites"
              @mousedown="toggleFavorites_click">
              <i class="bi bi-star"></i>
            </button>

            <button id="cmdApplyTheme" class="btn btn-apply-theme" @click="applyTheme">Apply Theme</button>

            <!-- History Box -->
            <select class="form-select" id="cboHistoryBox" @change="OnHistoryBox_Click" v-model="selectedHistory"
              data-bs-toggle="tooltip" data-bs-placement="bottom" data-bs-title="History Box">
              <option default value="mnuDummy">.</option>
              <option v-for="option in historyOptions" :key="option.value" :value="option.value" :data-tag="option.tag">
                {{ option.text }}</option>
            </select>

          </div>

        </div>
      </div>
    </nav><!-- Top Navbar -->

    <!-- Middle Div - Content -->
    <div class="middle-div">

      <div v-if="showSpinner"
        class="d-flex justify-content-center align-items-center position-fixed top-0 left-0 w-100 h-100 bg-dark bg-opacity-75 z-index-999">
        <div class="spinner-border text-light" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>

      <div class="row no-gutters full-height m-0 h-100">
        <!-- The Ship's HUD image -->
        <div class="col-8 h-100">
          <HUD_Areas />
        </div>

        <!-- This contains the Controls of the Tabs -->
        <div class="col-4 border border-secondary d-flex flex-column h-100">
          <!-- Nav tabs -->
          <ul class="nav nav-tabs " id="myTabHeaders" role="tablist">
            <li class="nav-item" role="presentation">
              <button class="nav-link active" id="themes-tab" data-bs-toggle="tab" data-bs-target="#themes-pane"
                type="button" role="tab" aria-controls="themes-pane" aria-selected="true">Themes</button>
            </li>
            <li class="nav-item" role="presentation">
              <button class="nav-link" id="properties-tab" data-bs-toggle="tab" data-bs-target="#properties-pane"
                type="button" role="tab" aria-controls="properties-pane" aria-selected="false">Properties</button>
            </li>
            <li class="nav-item" role="presentation">
              <button class="nav-link" id="global-settings-tab" data-bs-toggle="tab"
                data-bs-target="#global-settings-pane" type="button" role="tab" aria-controls="global-settings-pane"
                aria-selected="false">Global Settings</button>
            </li>
            <li class="nav-item" role="presentation">
              <button class="nav-link" id="user-settings-tab" data-bs-toggle="tab" data-bs-target="#user-settings-pane"
                type="button" role="tab" aria-controls="user-settings-pane" aria-selected="false">User Settings</button>
              <!--disabled-->
            </li>
          </ul>
          <!-- Tab panes -->
          <div class="tab-content" id="myTabContent">
            <div class="tab-pane fade show active" id="themes-pane" role="tabpanel" aria-labelledby="themes-tab"
              tabindex="0">
              <ThemeTab v-show="activeTab === 'themes'" class="tab-content" />
            </div>

            <div class="tab-pane fade" id="properties-pane" role="tabpanel" aria-labelledby="properties-tab"
              tabindex="0">
              <PropertiesTabEx class="tab-content" @OnProperties_Changed="OnThemeValuesChanged" />
            </div>

            <div class="tab-pane fade" id="user-settings-pane" role="tabpanel" aria-labelledby="user-settings-tab"
              tabindex="0">
              <UserSettingsTab class="tab-content" />
            </div>

            <div class="tab-pane fade" id="global-settings-pane" role="tabpanel" aria-labelledby="global-settings-tab"
              tabindex="0">
              <GlobalSettingsTab class="tab-content" />
            </div>
          </div>
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
              @change="OnGameInstanceChange">
              <option v-for="(game, index) in gameMenuItems" :key="index" :value="game">{{ game }}</option>
            </select>
          </div>
        </div>

        <!-- App Version Label -->
        <span class="navbar-text mx-3" id="lblVersion">App Version: {{ appVersion }}</span>
        <!-- Mod Version Label -->
        <span class="navbar-text mx-3" id="lblModVersion">EDHM Version: {{ modVersion }}</span>

        <!-- Progress bar-->
        <span v-show="showProgressBar" class="progress" role="progressbar" aria-label="Warning example"
          :aria-valuenow="progressValue" aria-valuemin="0" aria-valuemax="100" style="width: 540px;">
          <div class="progress-bar progress-bar-striped progress-bar-animated text-bg-warning"
            :style="{ width: progressValue + '%' }">{{ progressText }}</div>
        </span>

        <!-- Search Form -->
        <form class="d-flex ms-auto" @submit.prevent="OnSearchBox_Click">
          <input class="form-control me-2 main-menu-style" type="search" v-model="searchQuery" placeholder="Search"
            aria-label="Search">
          <button class="btn btn-outline-warning" type="submit">Search</button>
        </form>

      </div>
    </nav> <!-- Bottom Navbar -->


    <div v-if="showSpinner" class="spinner-border text-primary" role="status">
      <span class="visually-hidden">Loading...</span>
    </div>

  </div> <!-- Container -->


</template>
<script>
import { ref } from 'vue';
import EventBus from '../EventBus';
import ThemeTab from './ThemeTab.vue';
import Util from '../Helpers/Utils.js';
import HUD_Areas from './HudImage.vue';
import PropertiesTabEx from './PropertiesTabEx.vue';
import UserSettingsTab from './UserSettingsTab.vue';
import GlobalSettingsTab from './GlobalSettingsTab.vue';
import defaultTemplateString from '../data/ODYSS/ThemeTemplate.json';

let defaultTemplate = JSON.parse(JSON.stringify(defaultTemplateString));

//Enable Tooltips:
const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

// Enable Dropdown for the Context Menu
const dropdownElementList = document.querySelectorAll('.dropdown-toggle');
const dropdownList = [...dropdownElementList].map(dropdownToggleEl => new bootstrap.Dropdown(dropdownToggleEl));

/** To Check is something is Empty
 * @param obj Object to check
 */
const isEmpty = obj => Object.keys(obj).length === 0;

export default {
  name: 'NavBarsBody',
  props: {
    themesLoaded: {
      type: Array,
      required: false
    }
  },
  data() {
    return {
      activeTab: '',
      statusText: '',
      showFavorites: false,
      showSpinner: true,

      programSettings: {},
      themeTemplate: {},
      ActiveInstance: {},
      currentSettingsPath: '',    //<- Full pat to the Current Settings JSON stored in the Game Dir.

      historyOptions: [],
      selectedHistory: '',

      searchResults: [],
      globalSettings: [],

      appVersion: '',
      modVersion: '',
      selectedGame: '',
      gameMenuItems: [],

      showProgressBar: false,
      progressValue: 0,
      progressText: '',
      downloadSpeed: 0,
      averageSpeed: 0,
      startTime: 0,
      totalDownloadedBytes: 0,
      progressListener: null,

      DATA_DIRECTORY: '',
    };
  },
  setup(props) {

  },
  components: {
    ThemeTab,
    HUD_Areas,
    PropertiesTabEx,
    UserSettingsTab,
    GlobalSettingsTab
  },
  methods: {

    async OnInitialize(settings) {
      try {
        console.log('Initializing NavBars..');

        this.programSettings = settings;
        this.appVersion = await window.api.getAppVersion();
        this.modVersion = settings.Version_ODYSS;
        this.ActiveInstance = await window.api.getActiveInstance();
        this.selectedGame = this.ActiveInstance.instance;
        this.showFavorites = settings.FavToogle;
        this.activeTab = ref('themes');

        this.DATA_DIRECTORY = await window.api.GetInstanceDataDirectory(this.ActiveInstance.key); //<- Returns the path to the EDHM data directory.
        console.log('DATA_DIRECTORY:', this.DATA_DIRECTORY);

        // Populate game instances with the `instance` values from `Settings`
        this.gameMenuItems = ref(
          settings.GameInstances.flatMap(instance =>
            instance.games
              .filter(game => game.path) // only include games with non-empty 'path'
              .map(game => game.instance)
          )
        );

        this.themeTemplate = await this.LoadCurrentSettings();
        EventBus.emit('OnSelectTheme', { id: 0 });   //<- Event Listened at 'ThemeTab.vue'    
        await this.History_LoadElements();

      } catch (error) {
        EventBus.emit('ShowError', error);
      } finally {
        this.showSpinner = false;
      }
    },

    /** Sets the Active Tab
     * @param tab Tab's Name: 'themes, properties, global-settings, user-settings'     */
    setActiveTab(tab) {
      this.activeTab = ref(tab); //console.log(tab);
      var tabTriggerEl = document.querySelector('#' + tab + '-tab');
      var tab = new bootstrap.Tab(tabTriggerEl);
      tab.show();
    },

    /**  when a Theme in the list is Selected
     * @param theme Data of selected Theme
     */
    async LoadTheme(theme) {
      this.showSpinner = true;
      try {
        if (theme && theme.file) {
          const template = JSON.parse(JSON.stringify(theme.file));
          console.log('Loading Theme..', template.credits.theme);

          this.themeTemplate = await window.api.LoadTheme(template.path);
          this.themeTemplate.credits = theme.file.credits;

          //console.log('LoadedTheme..', this.themeTemplate.credits);

          if (template.credits.theme === 'Current Settings') {
            this.currentSettingsPath = template.path;
          }
          //console.log('Loaded Theme:', this.themeTemplate);
          EventBus.emit('ThemeLoaded', JSON.parse(JSON.stringify(this.themeTemplate))); //<- this event will be heard on 'App.vue'
          this.statusText = 'Theme: ' + theme.name;
        }
      } catch (error) {
        EventBus.emit('ShowError', new Error(error.message + error.stack));
      } finally { this.showSpinner = false; }
    },
    async applyTheme() {
      this.showSpinner = true;
      try {
        console.log('0. Applying Theme:', this.themeTemplate.credits.theme);

        this.ActiveInstance = await window.api.getActiveInstance();
        console.log('1. ActiveInstance:', this.ActiveInstance.instance);

        const GamePath = await window.api.joinPath(this.ActiveInstance.path, 'EDHM-ini');
        const GameType = this.ActiveInstance.key === 'ED_Odissey' ? 'ODYSS' : 'HORIZ';
        const defaultInisPath = await window.api.getAssetPath(`data/${GameType}`);
        console.log('2. Preparing all the Paths:', GamePath);

        let template = JSON.parse(JSON.stringify(this.themeTemplate));
        template.path = GamePath;
        console.log('3. Theme Template:', template);

        // Reusable function to apply Global/User settings:
       // Reusable function to apply Global/User settings:
        async function applySettings(settings, template, counterName) {
          try {
            let counter = 0;
            if (settings) {
              settings.Elements.forEach(gblSets => {
                let found = false;
                if (template.ui_groups) {
                  for (let i = 0; i < template.ui_groups.length - 1; i++) {
                    const uiGrp = template.ui_groups[i];
                    const itemIndex = uiGrp.Elements.findIndex(item => item.Key === gblSets.Key);
                    if (itemIndex >= 0) {
                      const oldV = uiGrp.Elements[itemIndex].Value;
                      uiGrp.Elements[itemIndex].Value = gblSets.Value;
                      found = true;
                      counter++;
                      break; // Break out of the inner loop once updated
                    }
                  }
                  if (!found) {
                    // Item not found, add it to the second last ui_group:
                    const lastGroup = template.ui_groups[template.ui_groups.length - 2];
                    lastGroup.Elements.push(gblSets); // Add the whole item from settings
                    counter++;
                  }
                }
              });
            }
            console.log(counter + ' ' + counterName + ' added!');
          } catch (error) {
            console.log('ERROR @SettingsHelper.applyTheme().applySettings():', error);
          }
        }

        // 4. Apply Global Settings
        const globalSettings = await window.api.LoadGlobalSettings();
        console.log('4. Global Settings:', globalSettings);
        await applySettings(globalSettings, template, 'Global Settings');

        // 5. Apply User Settings
        const userSettings = await window.api.LoadUserSettings();
        if (userSettings) {
          console.log('5. Get User Settings:', userSettings);
          await applySettings(userSettings, template, 'User Settings');
        }       

        const defaultINIs = await window.api.LoadThemeINIs(defaultInisPath);
        console.log('6. Get Default Inis:', defaultINIs);

        const _curSettsSAved = await window.api.SaveTheme(template);
        console.log('Current Settings Saved?: ', _curSettsSAved);

        const updatedInis = await window.api.ApplyTemplateValuesToIni(template, defaultINIs);
        console.log('7. Applying Changes to the INIs...', updatedInis);

        console.log('8. Saving the INI files..');
        const _ret = await window.api.SaveThemeINIs(GamePath, updatedInis);
        if (_ret) {
          EventBus.emit('RoastMe', { type: 'Success', message: `<b>Theme: '${template.credits.theme}' Applied!'</b><small>Press <b>F11</b> in game to refresh the colors.</small>` });
        }
        setTimeout(() => {
          this.showSpinner = false;
        }, 1500);
      } catch (error) {
        this.showSpinner = false;
        console.log(error.message); // Check if the error message is defined 
        console.log(error.stack); // Check the stack trace
        EventBus.emit('ShowError', error);
      }
    },
    async LoadCurrentSettings() {
      try {
        if (this.ActiveInstance.path != '') {
          const themePath = await window.api.joinPath(this.ActiveInstance.path, 'EDHM-ini');
          let _ret = await window.api.GetCurrentSettings(themePath);
          _ret.version = this.programSettings.Version_ODYSS; //<- Load version from EDHM  
          //console.log('Current Settings:', _ret);
          return _ret;
        }
      } catch (error) {
        EventBus.emit('ShowError', new Error(error.message + error.stack));
      }
    },

    /** Main Menu Click Events 
     * @param value id of the clicked menu
     */
    async MainMenu_Click(value) {
      if (value) {
        this.$refs.mainMenuSelect.value = 'mnuDummy'; // Reset the select to "Main Menu"
        console.log(`Menu ${value} clicked`);

        const ActiveInstance = await window.api.getActiveInstance(); //console.log('ActiveInstance', ActiveInstance);
        const GamePath = ActiveInstance.path;

        if (value === 'mnuOpenGame') {
          await window.api.openPathInExplorer(GamePath);
        }
        if (value === 'mnuOpenData') {
          const dataPath = await window.api.resolveEnvVariables('%USERPROFILE%\\EDHM_UI');
          await window.api.openPathInExplorer(dataPath);
        }
        if (value === 'mnuSettings') {
          const InstallStatus = await window.api.InstallStatus();
          EventBus.emit('open-settings-editor', InstallStatus);
        }
        if (value === 'mnuShipyard') {
          EventBus.emit('open-ShipyardUI', JSON.parse(JSON.stringify(ActiveInstance)));
        }
        if (value === 'mnu3PModsManager') {
          EventBus.emit('open-3PMods', JSON.parse(JSON.stringify(ActiveInstance)));
        }
        if (value === 'mnuInstallMod') {
          EventBus.emit('GameInsanceChanged', ActiveInstance.instance); //<- this event will be heard in 'App.vue'
        }
        if (value === 'mnuUninstallMod') {
          const _ret = await window.api.UninstallEDHMmod(JSON.parse(JSON.stringify(ActiveInstance)));
          if (_ret) {
            EventBus.emit('RoastMe', { type: 'Success', message: 'EDHM Un-Installed!' });
          }
        }
        if (value === 'mnuDisableMod') {
          const _ret = await window.api.DisableEDHMmod(JSON.parse(JSON.stringify(ActiveInstance)));
          if (_ret) {
            EventBus.emit('RoastMe', { type: 'Success', message: 'EDHM Disabled!' });
          }
        }
        if (value === 'mnuGoToDiscord') {
          await window.api.openUrlInBrowser('https://discord.gg/ZaRt6bCXvj');
        }
        if (value === 'mnuReadManual') {
          await window.api.openUrlInBrowser('https://bluemystical.github.io/edhm-api/');          
        }
        if (value === 'mnuCheckUpdates') {
          EventBus.emit('LookForUpdates', null); //<- this event will be heard in 'App.vue'
        }
        if (value === 'mnuExit') {
          window.api.quitProgram();        
        }
      }
    },
    async addNewTheme_Click(event) {
      const options = {
        type: 'question', //<- none, info, error, question, warning
        buttons: ['Cancel', 'Yes, I am sure', 'No, take me back'],
        defaultId: 1,
        title: 'Question',
        message: 'Create New Theme?',
        detail: 'This will take your currently applied settings to build a new theme',
        cancelId: 0
      };
      const result = await window.api.ShowMessageBox(options); //console.log(result);
      if (result && result.response === 1) {
        EventBus.emit('OnCreateTheme', { theme: null }); //<- Event Listened on App.vue
      }
    },
    async editTheme_Click(event) {
      if (this.themeTemplate && !isEmpty(this.themeTemplate)) {
        //console.log(themeTemplate);
        const tName = this.themeTemplate.credits.theme; //console.log(tName);
        if (tName != "Current Settings") {
          console.log('Editing theme: ', tName);
          EventBus.emit('OnEditTheme', { theme: JSON.parse(JSON.stringify(this.themeTemplate)) }); //<- Event Listened on App.vue
        }
        else {
          this.statusText = 'Current Settings can not be Edited!';
          console.log('Current Settings can not be Edited!');
        }
      }
    },
    async exportTheme_Click(event) {
      if (this.themeTemplate && !isEmpty(this.themeTemplate)) {
        //console.log(themeTemplate);
        const tName = this.themeTemplate.credits.theme; //console.log(tName);
        if (tName != "Current Settings") {
          console.log('Exporting theme: ', tName);

          const _ret = await window.api.ExportTheme(JSON.parse(JSON.stringify(this.themeTemplate))); console.log(_ret);
          if (_ret) {
            EventBus.emit('RoastMe', { type: 'Success', message: `Theme: '${tName}' exported successfully!` });
          }
        }
        else {
          this.statusText = 'Current Settings can not be Edited!';
          console.log('Current Settings can not be Edited!');
        }
      }
    },
    async importTheme_Click(event) {
      try {
        const options = {
          title: 'Import Theme from a ZIP file:', 
          defaultPath: '', //Absolute directory path, absolute file path, or file name to use by default. 
          filters: [
            { name: 'ZIP Files', extensions: ['zip'] },
            { name: 'All Files', extensions: ['*'] }
          ],
          properties: ['openFile', 'showHiddenFiles', 'createDirectory', 'promptToCreate', 'dontAddToRecent'],
        }; 
        const filePath = await window.api.ShowOpenDialog(options);
        if (filePath) {
          console.log('Selected file:', filePath[0]);
          const _ret = await window.api.ImportTheme(filePath[0]); //<- Load the JSON file from the History folder
          if (_ret) {
            console.log('Imported Theme:', _ret);
            EventBus.emit('loadThemes', _ret);           //<- Listen in ThemeTab.vue
            EventBus.emit('DoLoadGlobalSettings', _ret); //<- Listen in GlobalSettings.vue
            EventBus.emit('DoLoadUserSettings', _ret);   //<- Listen in UserSettings.vue     
            EventBus.emit('RoastMe', { type: 'Success', message: 'Theme Imported.' });
          }
          else {
            EventBus.emit('RoastMe', { type: 'Error', message: 'Theme Import Failed!' });
          }          
        }
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },
    async saveTheme_Click(event) {
      if (this.themeTemplate && !isEmpty(this.themeTemplate)) {
        //console.log(this.themeTemplate);
        const tName = this.themeTemplate.credits.theme; //console.log(tName);
        if (tName != "Current Settings") {
          console.log('Updating theme: ', tName);
          const options = {
            type: 'question', //<- none, info, error, question, warning
            buttons: ['Cancel', 'Yes, I am sure', 'No, take me back'],
            defaultId: 1,
            title: 'Question',
            message: 'Do you want to proceed?',
            detail: 'This will take your currently applied settings and Save them into the Selected Theme: ' + tName,
            cancelId: 0
          };
          const result = await window.api.ShowMessageBox(options); //console.log(result);
          if (result && result.response === 1) {
            const curSettings = await this.LoadCurrentSettings();
            EventBus.emit('DoUpdateTheme', {
              theme: JSON.parse(JSON.stringify(this.themeTemplate)),
              source: curSettings
            }); //<- Event Listened on App.vue
          }
        }
        else {
          this.statusText = 'Current Settings can not be Edited!';
          console.log('Current Settings can not be Edited!');
        }
      }
    },
    reloadThemes_Click(e) {
      EventBus.emit('loadThemes', e);           //<- Listen in ThemeTab.vue
      EventBus.emit('DoLoadGlobalSettings', e); //<- Listen in GlobalSettings.vue
      EventBus.emit('DoLoadUserSettings', e);   //<- Listen in UserSettings.vue
    },

    /** Toggles the Favorites list */
    async toggleFavorites_click(event) {
      this.showFavorites = !this.showFavorites;
      this.programSettings.FavToogle = this.showFavorites;

      await window.api.saveSettings(JSON.stringify(this.programSettings, null, 4));
      EventBus.emit('OnUpdateSettings', JSON.parse(JSON.stringify(this.programSettings))); //<- Event listened at 'App.vue'
      EventBus.emit('FilterThemes', this.showFavorites); //<- Event listened at 'ThemeTab.vue'

      //console.log('Favorites toggled:', this.showFavorites);
      return this.showFavorites;
    },

    async OnHistoryBox_Click(event) {
      // Click an item on the History Box
      const selectedValue = event.target.value;
      const selectedOption = event.target.options[event.target.selectedIndex];
      const tag = selectedOption.getAttribute('data-tag');

      //console.log('History option changed to:', selectedValue);
      //console.log('TODO: Selected file path (tag):', tag);

      const jsonData = await window.api.getJsonFile(tag); //<- Load the JSON file from the History folder
      if (jsonData) {
        console.log('Loaded JSON data:', jsonData);
        this.themeTemplate.ui_groups = jsonData.ui_groups; //<- Load the JSON data into the themeTemplate
        EventBus.emit('OnSelectTheme', { id: 0 }); //<- Event Listened on 'ThemeTab.vue'
        EventBus.emit('ThemeLoaded', JSON.parse(JSON.stringify(this.themeTemplate))); //<- this event will be heard on 'App.vue'
      } else {
        console.error('Failed to load JSON data from History folder');
      }

      document.querySelector('#cboHistoryBox').value = 'mnuDummy';  // Reset the select 
    },
    async History_LoadElements() {
      try {
        const NumberOfSavesToRemember = await window.api.readSetting('SavesToRemember', 10);  //console.log('NumberOfSavesToRemember',NumberOfSavesToRemember);    
        const HistoryFolder = window.api.joinPath(this.DATA_DIRECTORY, 'History'); //console.log('HistoryFolder:', HistoryFolder);
        const files = await window.api.loadHistory(HistoryFolder, NumberOfSavesToRemember); //console.log(files);        

        this.historyOptions = ref(
          files.map(file => ({
            value: file.name,
            text: file.date,
            tag: file.path  // Add the file path as a tag
          }))
        );
        //console.log('History:', this.historyOptions);

      } catch (error) {
        console.error(error);
        EventBus.emit('ShowError', error);
      }
    },
    async History_AddSettings(theme) {
      try {
        //TODO: Save the current settings to the History folder

        const ActiveInstance = this.programSettings.ActiveInstance;
        const gameInstances = this.programSettings.GameInstances;

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
        EventBus.emit('ShowError', error);
      }
    },

    /** Performs a Data Search based on the SearchBox Input 
     * @param query Input Query (what we are looking for)      */
    async DoSearchData(query) {

      //console.log('Search Query:', query);
      let searchQuery = query.trim().toLowerCase();
      searchQuery = searchQuery.includes('cutie') ? 'rico' : searchQuery;
      //console.log(searchQuery);

      // We gather data from this 2 datasets: this.themesLoaded and themeTemplate
      //console.log('themesLoaded:', this.themesLoaded);
      //console.log('themeTemplate:', this.themeTemplate );

      try {
        //1. Looking on the HUD settings:
        const allElements = this.themeTemplate.ui_groups.reduce((acc, group) => {
          if (group.Elements) {
            const elementsWithParent = group.Elements.map(element => ({
              ...element,
              Parent: group.Name
            }));
            return acc.concat(elementsWithParent);
          }
          return acc;
        }, []);

        // 2. Looking on the Themes Loaded:
        const filteredThemes = this.themesLoaded.filter(theme =>
          theme.file.credits &&
          (theme.file.credits.theme && typeof theme.file.credits.theme.toLowerCase === 'function' && theme.file.credits.theme.toLowerCase().includes(searchQuery)) ||
          (theme.file.credits.author && typeof theme.file.credits.author.toLowerCase === 'function' && theme.file.credits.author.toLowerCase().includes(searchQuery))
        ).map(theme => ({
          Parent: 'Themes',
          Category: "Theme by [" + theme.file.credits.author + ']',
          Title: theme.name,
          Description: theme.file.credits.description,
          Tag: theme
        }));

        // 3. Looking on the Global Settings:
        var FilteredGlobals = [];
        if (this.globalSettings && !isEmpty(this.globalSettings)) {
          this.globalSettings.forEach(group => {
            group.elements.forEach(element => {
              if (
                element.Category.toLowerCase().includes(searchQuery) ||
                element.Title.toLowerCase().includes(searchQuery) ||
                element.Description.toLowerCase().includes(searchQuery)
              ) {
                FilteredGlobals.push({ // <-- Mapping here
                  Parent: 'Global Settings',
                  Category: element.Category,
                  Title: element.Title,
                  Description: element.Description,
                  Tag: element
                });
              }
            });
          });
        }

        //4. Here we Apply the Filter:
        if (query) {

          this.searchResults = allElements.filter(element =>
            element &&
            element.Title && typeof element.Title.toLowerCase === 'function' &&
            element.Category && typeof element.Category.toLowerCase === 'function' &&
            element.Description && typeof element.Description.toLowerCase === 'function' &&
            (element.Title.toLowerCase().includes(searchQuery) ||
              element.Category.toLowerCase().includes(searchQuery) ||
              element.Description.toLowerCase().includes(searchQuery))
          ).concat(filteredThemes).concat(FilteredGlobals);


        } else {
          // If no filter, return them ALL !
          this.searchResults = allElements.filter(element =>
            element &&
            element.Title &&
            element.Category &&
            element.Description
          ).concat(filteredThemes);
        }
        //console.log('searchResults:', this.searchResults);
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
      // After this, control pass to 'OnSearchBox_Click'
    },

    /** Submit Event for the 'Search Form'
    * After Procesing the Query, the results are sent to the 'App.vue' to be shown.    */
    async OnSearchBox_Click() {
      //console.log('Search button click');
      await this.DoSearchData(this.searchQuery);
      EventBus.emit('SearchBox', { data: this.searchResults });//<- this event will be heard in 'App.vue'
    },

    /** When a Game Instance is selected from the '#gameSelect' combo     */
    OnGameInstanceChange(event) {
      const gameInstanceName = event.target.value;
      this.selectedGame = gameInstanceName;
      if (this.programSettings) {
        this.programSettings.ActiveInstance = gameInstanceName.toString();
        EventBus.emit('GameInsanceChanged', gameInstanceName); //<- this event will be heard in 'App.vue'
      }
      //console.log(`Game selected: ${selectedGame.value}`);
    },

    showHideSpinner(status) {
      //console.log('showHideSpinner: ', status.visible);
      this.showSpinner = status.visible;
      //EXAMPLE: ->    EventBus.emit('ShowSpinner', { visible: true } );//<- this event will be heard in 'MainNavBars.vue'
      //  -> showHideSpinner({ visible: true });
    },

    OnModUpdated(data) {
      // happens when the mod gets updated
      this.programSettings = data;
      //console.log('programSettings: ', programSettings);
      this.modVersion = data.Version_ODYSS;
    },
    OnXmlChanged(data) {
      console.log('XML Changed:', data);
      this.themeTemplate.xml_profile = data.xml;
    },

    async OnThemeValuesChanged(ui_group) {
      // Evento recibido del componente PropertiesTab.vue al cambiar los valores de un tema     
      if (this.themeTemplate && !isEmpty(this.themeTemplate)) {
        /* ALL CHANGES ARE STORED IN THE 'Current Settings' FILE AT THE GAME DIRECTORY.  */
        console.log('Values Updated for:', ui_group.Name);
        const GamePath = await window.api.joinPath(this.ActiveInstance.path, 'EDHM-ini');
        const areaIndex = this.themeTemplate.ui_groups.findIndex(item => item.Name === ui_group.Name);
        if (areaIndex >= 0) {
          this.themeTemplate.ui_groups[areaIndex] = ui_group;
          const theme = JSON.parse(JSON.stringify(this.themeTemplate));
          theme.path = GamePath;
          const saved = await window.api.SaveTheme(theme);
          console.log('Current Settings Saved?: ', saved);
        }
      }
    },

    /** Downloads an Install an Update for the App.
     * @param Options { url: 'to download from', save_to: 'path to store the file', platform: 'win32|linux|darwin' }     */
    async DownloadAndInstallUpdate(Options) {
      try {
        console.log('Downloading file:', Options);
        this.showHideSpinner({ visible: true });
        this.modVersion = 'Downloading...';
        this.showSpinner = true;

        let scriptPath = null;
        let filePath = Options.save_to;
        if (!filePath) return;

        const destDir = await window.api.getParentFolder(filePath);
        await window.api.ensureDirectoryExists(destDir);
        await window.api.deleteFilesByType(destDir, '.sh'); //<- Remove any previous installer script 
        await window.api.deleteFilesByType(destDir, '.zip');

        //- Setup Progress Control Variables:
        this.showProgressBar = true;  //<- Shows/Hides the Progressbar
        this.progressValue = 0;       //<- Progress Value in Percentage %
        this.downloadSpeed = 0;       //<- Download Speend in bytes/s
        this.averageSpeed = 0;        //<- Average Download Speend in KB/s
        this.totalDownloadedBytes = 0; //<- Bytes Downloaded
        this.startTime = Date.now();

        //- Setup a Progress Listener
        this.progressListener = (event, data) => {

          //- This will fillup the Progress Bar:          
          this.downloadSpeed = data.speed;
          this.progressValue = data.progress;
          this.totalDownloadedBytes += data.speed;

          //- Calculating the Average Spped:
          const elapsedTime = (Date.now() - this.startTime) / 1000;
          if (elapsedTime > 0) {
            const averageSpeedBytesPerSecond = this.totalDownloadedBytes / elapsedTime;
            this.averageSpeed = (averageSpeedBytesPerSecond / 1024).toFixed(1); //<- KB/s or   / (1024 * 1024) <- MB/s
          }

          this.progressText = `${data.progress.toFixed(1)}%, ${this.averageSpeed} KB/s`;
        };

        //- Start the Progress Listener:
        window.api.onDownloadProgress(this.progressListener);

        //- Start the Download and wait till it finishes..
        await window.api.downloadFile(Options.url, filePath);

        //- When the Download Finishes: 
        console.log('Download complete!');
        this.modVersion = 'Installing...';

        //- Some Cleanup:
        window.api.removeDownloadProgressListener(this.progressListener);

        //- Get the installer script from the Public dir:
        if (Options.platform === 'linux') {
          scriptPath = await window.api.getAssetPath('public/linux_installer.sh');
          filePath = window.api.joinPath(destDir, 'linux_installer.sh');
        }
        if (Options.platform === 'win32') {
          scriptPath = await window.api.getAssetPath('public/windows_installer.bat');
          filePath = window.api.joinPath(destDir, 'windows_installer.bat');
        }
        if (!scriptPath) {
          throw new Error('Failed to Copy the Installer Script.');
        }

        //- Remember we are running an Update, next time App runs it will do update stuff:
        await window.api.writeSetting('FirstRun', true);

        //- Now we Copy and Run the Installer thru the Script:
        await window.api.copyFile(scriptPath, filePath);
        const _ret = await window.api.runProgram(filePath); console.log(_ret);

        //- The Installer Script should terminate the running instance of the App, but..
        setTimeout(() => {
          this.StillHere(Options.platform, destDir); //<- Just in case the Installer Script did not started.          
        }, 10000); //<- 10 seconds to wait for the Installer Script to start.

      } catch (error) {
        console.error('Download failed:', error);
        this.showProgressBar = false;
        this.showSpinner = false;
        this.modVersion = 'ERROR!';
        window.api.removeDownloadProgressListener(this.progressListener);
        EventBus.emit('ShowError', new Error(error.message + error.stack));
      }
    },
    StillHere(platform, filePath) {
      //- This is a Failsafe method to ensure the Installer Script is started.
      let ScriptName = platform === 'linux' ? 'linux_installer.sh' : 'windows_installer.bat';
      const options = {
        type: 'question',
        buttons: ['Cancel', "Yes, Take me there", 'No, thanks.'],
        cancelId: 0,
        defaultId: 1,
        title: 'Still Here?',
        message: 'It seems the Installer Script did not started.\nYou may need to run it manually.\nLook for the file: ' + ScriptName,
        detail: 'Do you want to open the folder where the file is located?'
      };
      window.api.ShowMessageBox(options).then(result => {
        if (result && result.response === 1) {
          window.api.openPathInExplorer(filePath);
          setTimeout(() => {
            window.api.quitProgram(); //<- Close the App    
          }, 3000);
        }
      });
    },

    OnGlobalSettingsLoaded(data) {
      this.globalSettings = data;
      //console.log('OnGlobalSettingsLoaded', data);
    },


  },
  mounted() {
    /* EVENTS WE LISTEN TO HERE:  */
    EventBus.on('InitializeNavBars', this.OnInitialize);
    EventBus.on('setActiveTab', this.setActiveTab);
    EventBus.on('ThemeClicked', this.LoadTheme);
    EventBus.on('ShowSpinner', this.showHideSpinner);
    EventBus.on('modUpdated', this.OnModUpdated);
    EventBus.on('OnApplyTheme', this.applyTheme);
    EventBus.on('StartDownload', this.DownloadAndInstallUpdate);
    EventBus.on('OnGlobalSettingsLoaded', this.OnGlobalSettingsLoaded);
    EventBus.on('OnXmlChanged', this.OnXmlChanged);

    if (typeof this.progressListener === 'function') {
      window.api.removeDownloadProgressListener(this.progressListener);
    }
  },
  beforeUnmount() {
    // Clean up the event listener
    EventBus.off('InitializeNavBars', this.OnInitialize);
    EventBus.off('setActiveTab', this.setActiveTab);
    EventBus.off('ThemeClicked', this.LoadTheme);
    EventBus.off('ShowSpinner', this.showHideSpinner);
    EventBus.off('modUpdated', this.OnModUpdated);
    EventBus.off('OnApplyTheme', this.applyTheme);
    EventBus.off('StartDownload', this.DownloadAndInstallUpdate);
    EventBus.off('OnGlobalSettingsLoaded', this.OnGlobalSettingsLoaded);

    if (typeof this.progressListener === 'function') {
      window.api.removeDownloadProgressListener(this.progressListener);
    }
  }
}
</script>
<style scoped>
body {
  background-color: #1F1F1F;
  color: #fff;
  /* Optional: Set text color to white */
}

.z-index-999 {
  z-index: 999;
  /* Ensure the spinner is on top of other elements */
}

#Container {
  background-color: #1F1F1F;
  color: #fff;
}

#cboHistoryBox {
  width: 1px;
  /* Minimal width for the select element */
}

#cboHistoryBox option {
  white-space: nowrap;
  /* Prevent line breaks */
}

#TopNavBar {
  height: 62px;
  background-color: #1F1F1F;
}

#myTabContent {
  height: 90%;
  display: flex;
  flex-direction: column;
}

.tab-pane {
  flex: 1;
  overflow: auto;
  /* Enable scrolling if content exceeds the container size */
}

.nav-link.active {
  color: #ffc107;
  /* Replace with your desired color */
}

.tab-content {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  height: 100%;
  width: 100%;
}

.text-orange {
  /* for the Toggle Favorites Button */
  color: orange;
  background-color: #0c0c4d;
  border-color: #ffc107;
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

.btn-outline-secondary i {
  transition: color 0.3s ease-in-out;
}

.btn-outline-secondary:active i {
  color: Dodgerblue !important;
}

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
/*
.custom-navbar {
  border: 1px solid rgb(110, 73, 2);
  font-family: 'Segoe UI', sans-serif;
  font-size: 14px;
  color: #f8f9fa;
}
.navbar-nav .nav-item .nav-link {
  color: #f8f9fa;
  transition: background-color 0.3s, color 0.3s;
  border-radius: 5px;
}
.navbar-nav .nav-item .nav-link:hover {
  background-color: orangered;
  color: white;
  border-radius: 5px;
}
.navbar-nav .nav-item .active-nav-item {
  background-color: darkorange;
  color: #000000;
  border-radius: 5px;
}
*/

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