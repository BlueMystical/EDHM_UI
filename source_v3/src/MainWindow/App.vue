<template>
  <div id="app" class="bg-dark text-light" data-bs-theme="dark">
   
    <NavBarsBody :themesLoaded="themesLoaded" />
    <Notifications/>

    <SettingsEditor @save="saveConfig" />
    <TPMods @OnSave="OnSaveTPMods"/>
    <ShipyardUI @OnSave="OnSaveShipyardUI" :themesData="themesLoaded"/>

    <SearchBox ref="searchBox" :searchResults="searchResults" @resultClicked="OnSearchBox_Click"/>
    
    <!-- Theme Image Editor Modal -->
    <ThemeImageEditor v-if="showThemeImageEditorModal" :themeEditorData="themeEditorData" @save="handleImageEditorSave" @close="closeThemeImageEditor" />
    <ThemeEditor v-if="showThemeEditor" :themeEditorData="themeEditorData" @submit="handleThemeEditorSubmit" @close="closeThemeEditor" />
    
    <XmlEditor ref="xmlEditor" @onCloseModal="onXmlEditorClosed"/>   

  </div>
</template>

<script>
import EventBus from '../EventBus.js';
import NavBarsBody from './NavBars.vue';
import SettingsEditor from './SettingsEditor.vue';
import SearchBox from './Components/SearchBox.vue';
import Notifications from './Components/Notifications.vue';
import ThemeImageEditor from './Components/ThemeImageEditor.vue';
import ThemeEditor from './Components/ThemeEditor.vue';
import XmlEditor from './Components/XmlEditor.vue';
import Util from '../Helpers/Utils.js';
import TPMods from '../TPMods/TPModsManagerPop.vue';
import ShipyardUI from './ShipyardUI.vue';

console.log('App.vue loaded');


export default {
  name: 'App',
  components: {
    SearchBox,
    NavBarsBody,
    SettingsEditor,
    Notifications,
    ThemeImageEditor,
    ThemeEditor,
    XmlEditor,
    TPMods,
    ShipyardUI
  },
  data() {
    return {
      loading: true,        //<- Flag to Show/hide the Loading Spinner
      settings: null,       //<- The Program Settings
      InstallStatus: null,

      themesLoaded: [],     //<- List of all Loaded Themes
      themeTemplate: {},    //<- Current Selected Theme

      // #region Search Box

      searchResults: [],
      searchQuery: '',
      isSearchBoxVisible: false, //<- Flag to Show/Hide the SearchBox Results

      // #endregion      

      // #region Theme Editors
      showThemeImageEditorModal: false, //<- Flag to Show/Hide the Image Editor
      previewImage: null,
      thumbnailImage: null,
      showThemeEditor: false, //<- Flag to Show/Hide the Theme Editor
      themeEditorData: {
        theme: '',
        author: '',
        description: '',
        preview: null,
        thumb: null
      },
      editingTheme: null, //<- If Null, its a New Theme, else its Editing an existing theme
      // #endregion

      logData: null, // Make sure logData is defined here
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
        let VirginPlayer = false; //<- Flag to know if is the first time running the app

        //- Apply the Font Size taken from the Settings: Escucha el evento desde el main/preload
        window.api.onFontSizeSetting((event, size) => {
          const fontSize = this.normalizeFontSize(size); //          console.log('onFontSizeSetting: ', size, fontSize);

          // Aplicar la variable global
          setTimeout(() => {
            document.documentElement.style.setProperty('--user-font-size', fontSize);
          }, 0);
          try {
             window.api.writeSetting('FontSize', fontSize);
          } catch (err) {
            console.warn('No se pudo actualizar FontSize en settings:', err);
          }
        });

        this.$nextTick(() => {
          //- ENABLE THE TOOLTIPS POPUP:
          const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
          tooltipTriggerList.forEach(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

          const dropdownElementList = document.querySelectorAll('[data-bs-toggle="dropdown"]');
          dropdownElementList.forEach(dropdownToggleEl => new bootstrap.Dropdown(dropdownToggleEl));
        });

        if (this.InstallStatus === 'existingInstall') { 
          // Normal Load, All seems Good
          if (!Util.isNotNullOrEmpty(ActiveInstance.path)) {
            VirginPlayer = false; //<- We are not a New Player, we have an Active Instance
            // Either the Active Instance or its path is not set:
            EventBus.emit('RoastMe', { type: 'Success', message: 'Welcome to the application!<br>You now need to tell EDHM where is your game located.', delay: 10000 });
            EventBus.emit('RoastMe', { type: 'Info', message: '- Leave the Game running at the menus<br>- Click the `Green button`, read and do as it says<br>- the game will auto-close, the paths will set, save the settings.<br>- go check the game, should be pretty green.', autoHide: false, width:'460px' });
            EventBus.emit('open-settings-editor', this.InstallStatus); //<- Open the Settings Window
            return;
          }
        } else {
          // Welcome New User!  
          VirginPlayer = true; //<- We are a New Player
          EventBus.emit('RoastMe', { type: 'Success', message: 'Welcome to the application!<br>You now need to tell EDHM where is your game located.', delay: 10000 });
          EventBus.emit('RoastMe', { type: 'Info', message: '- Leave the Game running at the menus<br>- Click the `Green button`, read and do as it says<br>- the game will auto-close, the paths will set, save the settings.<br>- go check the game, should be pretty green.', autoHide: false, width:'460px' });
          EventBus.emit('open-settings-editor', this.InstallStatus); //<- Open the Settings Window
          return;
        }

        //- Check if we are first running after an update:
        const isUpdate = await window.api.readSetting('FirstRun', true);
        if (isUpdate) {
          console.log('First Run after Update: Running HotFix..');
          try {
            await window.api.DoHotFix(); //<- Hotfix runs before Mod Installing
            await this.OnGameInstance_Changed({ GameInstanceName: this.settings.ActiveInstance, InstallMod: true }); //<- Update the Game Instance (Mod Installing)
            await window.api.writeSetting('FirstRun', false); //console.log('First Run Flag Cleared.');
          } catch (error) {
            EventBus.emit('ShowError', error);
          }
        }

        //- Initialize the Components:
        EventBus.emit('OnInitializeThemes', JSON.parse(JSON.stringify(this.settings))); //<- Event Listened at ThemeTab.vue
        EventBus.emit('InitializeNavBars',  JSON.parse(JSON.stringify(this.settings))); //<- Event Listened at NavBars.vue        
        EventBus.emit('InitializeHUDimage', null);    //<- Event Listened at HudImage.vue
        EventBus.emit('DoLoadGlobalSettings', null);  //<- Event Listened at GlobalSettingsTab.vue
        EventBus.emit('DoLoadUserSettings', null);    //<- Event Listened at UserSettingsTab.vue
        EventBus.emit('ShipyardUI-Initialize', null); //<- Event Listened at ShipyardUI.vue

        this.StartShipyard();
        this.CheckForUpdates();

      } catch (error) {
        console.error(error);
      } finally {
        EventBus.emit('ShowSpinner', { visible: false });
        this.loading = false;       
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

    normalizeFontSize(raw) {
      // Si ya es un valor CSS válido (px, rem, em, %), lo devolvemos tal cual
      if (/^\d+(\.\d+)?(px|rem|em|%)$/i.test(raw)) {
        return raw;
      }

      // Mapeo para valores antiguos
      const legacyMap = {
        small: '12px',
        medium: '14px',
        large: '16px',
        'x-large': '18px'
      };

      return legacyMap[raw] || legacyMap['medium']; // fallback seguro
    },

    // #region Settings Changed
    
    /** Fires when the Settings have been changed, called from 'SettingsEditor'
     * @param newConfig the updated settings     */
    async OnProgramSettings_Changed(newConfig) {
      try {
        console.log('newConfig:', newConfig);

        const jsonString = JSON.stringify(newConfig, null, 4);
        this.settings = await window.api.saveSettings(jsonString);
        this.OnGameInstance_Changed({ GameInstanceName: newConfig.ActiveInstance, InstallMod:true }); //<- Update the Game Instance

      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },

    /** Silently Updates changes in the Settings   * 
     * @param newConfig  the updated settings     */
    OnProgramSettings_Updated(newConfig) {
      this.settings = newConfig;
    },

    /** When the User pick a different Game on the Combo. * 
     * @param GameInstanceName Name of the New Active Instance     */
    async OnGameInstance_Changed(e) {
      try {
        EventBus.emit('ShowSpinner', { visible: true });
        console.log('activeInstance:', this.settings.ActiveInstance); //<- 'ActiveInstance' Changed by Ref.
        console.log('InstallMod:', e);

        const NewInstance = await window.api.getInstanceByName(e.GameInstanceName);
        console.log('NewInstance:', NewInstance);

        if (e.InstallMod) {
          EventBus.emit('RoastMe', { type: 'Info', message: `Installing EDHM on '${e.GameInstanceName}'..` });
          const edhmInstalled = await window.api.installEDHMmod(NewInstance);

          if (edhmInstalled.game === 'ODYSS') {
            this.settings.Version_ODYSS = edhmInstalled.version;
          } else {
            this.settings.Version_HORIZ = edhmInstalled.version;
          }
          const Backup = await window.api.RestoreCurrentSettings();
          EventBus.emit('RoastMe', { type: 'Info', message: Backup });
          EventBus.emit('RoastMe', { type: 'Success', message: `EDHM ${edhmInstalled.version} Installed.` });
        }        

        EventBus.emit('InitializeNavBars', JSON.parse(JSON.stringify(this.settings))); //<- Event Listened at NavBars.vue
        EventBus.emit('OnInitializeThemes', JSON.parse(JSON.stringify(this.settings)));//<- Event Listened at ThemeTab.vue
        EventBus.emit('InitializeHUDimage', null);    //<- Event Listened at HudImage.vue
        EventBus.emit('DoLoadGlobalSettings', null);  //<- Event Listened at GlobalSettingsTab.vue
        EventBus.emit('DoLoadUserSettings', null);    //<- Event Listened at UserSettingsTab.vue

        EventBus.emit('modUpdated', this.settings);     //<- Event listen in MainNavBars.vue
        EventBus.emit('loadThemes', this.settings.FavToogle);  //<- this event will be heard in 'ThemeTab.vue'

        const jsonString = JSON.stringify(this.settings, null, 4);
        await window.api.saveSettings(jsonString);

        this.StartShipyard();

      } catch (error) {
        EventBus.emit('ShowError', error);
      } finally {
        EventBus.emit('ShowSpinner', { visible: false });
      }
    },
    
    // #endregion

    // #region Themes changed

    /** When all Themes are Loaded. 
     * @param event Contains the data of the themes     */
     OnThemesLoaded(event) {
      try {
        this.themesLoaded = event;
        // The data is now automatically passed to ShipyardUI via the props binding
        //console.log('OnThemesLoaded:', this.themesLoaded );

      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },

    /** When a Theme Template is Loaded. 
     * @param event Contains the template of the theme     */
    OnTemplateLoaded(event) {
      try {
        this.themeTemplate = JSON.parse(JSON.stringify(event));
        console.log('Theme Loaded: ', this.themeTemplate.credits.theme); console.log('Theme:', this.themeTemplate);
       
        EventBus.emit('InitializeProperties', JSON.parse(JSON.stringify(this.themeTemplate))); //<- Event Listened at PropertiesTabEx.vue
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },

    /** When a theme is Applied
     * @param event the template of the theme     */
    OnThemeApplied(event) {
      try {
        this.themeTemplate = JSON.parse(JSON.stringify(event));
        console.log('Theme Applied: ', this.themeTemplate.credits.theme); 
       
        EventBus.emit('InitializeProperties', JSON.parse(JSON.stringify(this.themeTemplate))); //<- Event Listened at PropertiesTabEx.vue
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },

    /** This will take your currently applied settings and Save them into the Selected Theme * 
    * @param e Selected Theme Data   */
    async DoUpdateTheme(e) {
      //console.log(e);
      if (e && e.theme != null) { //<-- The Selected Theme
        const NewThemeData = JSON.parse(JSON.stringify(e.theme));
        const CurrentSettings = JSON.parse(JSON.stringify(e.source));

        const _ret = await window.api.UpdateTheme(NewThemeData, CurrentSettings);
        console.log('UpdatedTheme:', _ret);

        if (_ret) {
          EventBus.emit('loadThemes', null); //<- Event Listened on ThemeTab.vue
          EventBus.emit('RoastMe', { type: 'Success', message: `Theme: '${NewThemeData.credits.theme}' Saved.` });
        }
      }
    },

    async ApplyLastTheme(instance) {
      // This will apply the last used theme:
      try {
        //console.log('ApplyLastTheme - ActiveInstance:', instance);      
        const DATA_DIRECTORY = await window.api.GetInstanceDataDirectory(instance.key); //<- Get the Data Directory: %USERPROFILE%\EDHM_UI\ODYSS
        const HistoryFolder = window.api.joinPath(DATA_DIRECTORY, 'History');       //console.log('ApplyLastTheme - HistoryFolder:', HistoryFolder);
        const files = await window.api.loadHistory(HistoryFolder, 4); //console.log('ApplyLastTheme - Files:', files);      

        if (files && files.length > 0) {
          const lastThemeFile = files[0]; // Get the most recent file
          //console.log('ApplyLastTheme - Last Theme File:', lastThemeFile);
          if (lastThemeFile) {
            const themeData = await window.api.getJsonFile(lastThemeFile.path); // Load the Theme Data
            themeData.credits = {
              theme: themeData.name,
              author: themeData.author
            };

            console.log('ApplyLastTheme - Theme Data:', themeData);
            EventBus.emit('ApplyGivenTheme', themeData); //<- Event Listen in 'NavBars.vue'
          }
        }
      } catch (error) {
        console.error('Error @ApplyLastTheme:', error);
        EventBus.emit('ShowError', error);
      }
    },

    // #endregion

    // #region Shipyard

    /** This will Import a Shipyard V2 File.
     * Converted from the old Shipyard V2 format to the new V2 format.
     * Moves the file to the EDHM_UI Data Directory and deletes the old file.     */
    async ImportShipyardV2(instance) {
      try {        
        const DATA_DIRECTORY = await window.api.GetProgramDataDirectory(); //<- Get the Data Directory: %USERPROFILE%\EDHM_UI
        const v2Location = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\edhm_ui');
        const v2ShipyardFile = window.api.joinPath(v2Location, 'Data', 'shipyard.json');
        const v3ShipyardFile = window.api.joinPath(DATA_DIRECTORY, 'Shipyard_v3.json');
        const v2FileExists = await window.api.fileExists(v2ShipyardFile);

        if (v2FileExists) {
          // Read the old Shipyard V2 file
          const shipyardV2Data = await window.api.getJsonFile(v2ShipyardFile);
          console.log('Shipyard V2 Data:', shipyardV2Data);

          // Convert the data to the new format
          const convertedData = shipyardV2Data.ships.map(item => ({
            ship_id: item.Ship.ship_id,
            kind_short: item.Ship.ed_short,
            kind_full: item.Ship.ship_full_name,
            custom_name: item.ship_name,
            plate: item.ship_plate || '',
            theme: item.theme || 'Current Settings',
            image: item.Ship.ed_short + '.jpg',
          }));

          var Shipyard = {
              enabled: shipyardV2Data.enabled,
              player_name: shipyardV2Data.player_name,
              ships: convertedData
          };

          // Save the converted data to the new file
          const _ret = await window.api.writeJsonFile(v3ShipyardFile, Shipyard);
          if (_ret) {
            console.log('Converted Shipyard V2 to V3:', Shipyard);
            console.log('Converted Shipyard V2 to V3 and saved to:', v3ShipyardFile);

            // Delete the old Shipyard V2 file
            await window.api.deleteFileByAbsolutePath(v2ShipyardFile);
            console.log('Deleted old Shipyard V2 file:', v2ShipyardFile);

            EventBus.emit('RoastMe', { type: 'Success', message: 'Shipyard V2 Imported Successfully!' });
          }          
        } else {
          console.log('No Shipyard V2 file found at:', v2ShipyardFile);          
        }

      } catch (error) {
        console.log('Error @ImportShipyardV2():', error);
        EventBus.emit('ShowError', error);        
      }
    },
    async OnSaveShipyardUI(e) {

    },

    async StartShipyard() {
      try {        
        if (window.api) {
          
          const shipyardEnabled = await window.shipyardAPI.start();
          console.log('Starting Shipyard...', shipyardEnabled);

          //window.shipyardAPI.onLogEntry(entry => {
          //  console.log('@App.vue: Entrada de log:', entry);
          //});
          window.shipyardAPI.onLastLineProcessed(info => {
            console.log(`Última línea procesada (${info.totalLines} líneas):`, info.lastLine);
          });

        }
      } catch (error) {
        console.error('Error starting Shipyard:', error);
        EventBus.emit('ShowError', error);        
      }
    },
    
    // #endregion

    // #region SearchBox
    
    /** Shows the Results of the Search Box 
     * @param event Contains the data to be shown     */
    OnSearchBox_Shown(event) {
      try {
        this.searchResults = event.data;
        this.$refs.searchBox.show();
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },
    /** When the User Clicks on a Search Result. 
     * @param result The Result clicked     */
    OnSearchBox_Click(result) {
      //console.log('Result clicked:', result);
      try {
        if (result.Parent === 'Themes') {
          EventBus.emit('setActiveTab', 'themes'); //<- Event listen in 'MainNavBars.vue'
          EventBus.emit('OnSelectTheme', result.Tag); //<- Event listened on 'ThemeTab.vue'
        } else {
          if (result.Parent === 'Global Settings') {
            EventBus.emit('setActiveTab', 'global-settings'); //<- Event listen in 'MainNavBars.vue'
            EventBus.emit('FindKeyInGlobalSettings', result.Tag.Key); //<- Event listen in 'GlobalSettings.vue'

          } else { //<- It's a Normal HUD Settings
            // Emit an event with the clicked area 
            EventBus.emit('areaClicked', { id: result.Parent, title: result.Title }); //<- Event listen in 'PropertiesTab.vue'           
            // Emit an event to set the active tab to 'properties' 
            EventBus.emit('setActiveTab', 'properties'); //<- Event listen in 'MainNavBars.vue'
            // Ensure Visibility of Selected Item
            EventBus.emit('OnSelectCategory', result.Key); //<- Event listen in 'PropertiesTab.vue'
          }
        }
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },

    // #endregion
    
    // #region Theme Editor

    async OnCreateTheme(e) {
      try {
        //console.log(e);
        if (e && e.theme != null) {
          //******  We are Editing a Theme     **************//
          const _preview = window.api.joinPath(e.theme.path, e.theme.credits.theme + '.jpg');
          const _thumbim = window.api.joinPath(e.theme.path, 'PREVIEW.jpg');
          this.previewImage = await window.api.GetImageB64(_preview);
          this.thumbnailImage = await window.api.GetImageB64(_thumbim);
          this.editingTheme = e.theme; //<- If Null, its a New Theme, else its Editing an existing theme        
          this.themeEditorData = {
            theme: e.theme.credits.theme,
            author: e.theme.credits.author,
            description: e.theme.credits.description,
            preview: this.previewImage,
            thumb: this.thumbnailImage
          };
        } else {
          //******** We are Creating a new Theme:   **********//
          this.previewImage = null;
          this.thumbnailImage = null;
          this.editingTheme = null;
          this.themeEditorData = {
            theme: 'New Theme',
            author: 'Unknown',
            description: 'This is a new EDHM Theme',
            preview: this.previewImage,
            thumb: this.thumbnailImage
          };
        }
      } catch (error) {
        EventBus.emit('ShowError', error);
      }

      // STEP 0:  SHOW THE IMAGE EDITOR:
      this.showThemeImageEditorModal = true;
    },
    handleImageEditorSave(images) {
      // When the Image Editor is closed (by Saving)
      // STEP 1:  GET THE IMAGES FOR PREVIEW & THUMBNAIL
      if (this.editingTheme) {
        /******  We are Editing a Theme     **************/
        this.themeEditorData.preview = images.previewImage;
        this.previewImage = images.previewImage;

        this.themeEditorData.thumb = images.thumbnailImage;
        this.thumbnailImage = images.thumbnailImage;

      } else {
        /******** We are Creating a new Theme:   **********/
        this.previewImage = images.previewImage;
        this.thumbnailImage = images.thumbnailImage;

        this.themeEditorData = {
          theme: 'New Theme',
          author: 'Unknown',
          description: 'This is a new EDHM Theme',
          preview: this.previewImage,
          thumb: this.thumbnailImage
        };
      }
      this.closeThemeImageEditor();

      // 1.1 SHOW THE THEME EDITOR:
      this.showThemeEditor = true;
    },
    async handleThemeEditorSubmit(data) {
      /**** WHEN THE THEME EDITOR IS CLOSED (by Saving) ***** */
      try {
        if (data instanceof SubmitEvent) { return; } // Ignore the SubmitEvent  
        //console.log('Data:', data); 
        this.closeThemeEditor();

        //STEP 2:  GET THE NEW THEME'S META-DATA      
        const NewThemeData = JSON.parse(JSON.stringify(data)); //<- credits: { theme: 'ThemeName', author: '', description: '', preview: 'Base64image', thumb: 'Base64image' }
        console.log('Modified Data:', NewThemeData);

        //STEP 3: WRITE THE NEW THEME
        const _ret = await window.api.CreateNewTheme(NewThemeData);
        console.log('Saving Theme:', _ret);
        if (_ret) {
          // Reloading Themes:
          EventBus.emit('OnInitializeThemes', JSON.parse(JSON.stringify(this.settings)));  //<- this event will be heard in 'ThemeTab.vue'
          EventBus.emit('RoastMe', { type: 'Success', message: `Theme: '${NewThemeData.credits.theme}' Saved.` });

          //if (this.settings.FavToogle) {
            const options = {
              type: 'question', //<- none, info, error, question, warning
              buttons: ['Cancel', "Yes, It's a Favorite", 'No, thanks.'],
              defaultId: 1,
              title: 'Favorite?',
              message: 'Do you want to Favorite this new theme?',
              detail: `Theme: '${NewThemeData.credits.theme}'`,
              cancelId: 0
            };
            window.api.ShowMessageBox(options).then(result => {
              if (result && result.response === 1) {
                EventBus.emit('OnFavoriteTheme', { ThemeName: NewThemeData.credits.theme }); //<- Event Listened on ThemeTab.vue
              }
            });
          //}
        }
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },
    closeThemeEditor() {
      this.showThemeEditor = false;
    },
    closeThemeImageEditor() {
      this.showThemeImageEditorModal = false;
    },

    // #endregion

    // #region XML Editor

    getValue(xml_profile, targetKey) {
      // Returns the Value of a Key from the xml_profile
      for (let i = 0; i < xml_profile.length; i++) {
        if (xml_profile[i].key === targetKey) {
          return xml_profile[i].value;
        }
      }
      return null; // If the key is not found
    },
    setValue(xml_profile, targetKey, newValue) {
      // Sets the Value of a Key from the xml_profile
      for (let i = 0; i < xml_profile.length; i++) {
        if (xml_profile[i].key === targetKey) {
          xml_profile[i].value = newValue;
          return true; // If the key is found and the value is set
        }
      }
      return false; // If the key is not found
    },

    async OnShowXmlEditor(e) {
      /* Shows a Modal Dialog to edit the selected theme XML */
      try {
        if (this.themeTemplate) {
          const xml = JSON.parse(JSON.stringify(this.themeTemplate.xml_profile));
          const sliderValues = [
            [this.getValue(xml,'x150'), this.getValue(xml,'y150'), this.getValue(xml,'z150') ],
            [this.getValue(xml,'x151'), this.getValue(xml,'y151'), this.getValue(xml,'z151') ],
            [this.getValue(xml,'x152'), this.getValue(xml,'y152'), this.getValue(xml,'z152') ]
          ];
          this.$refs.xmlEditor.ShowModal({ 
            matrix: sliderValues, 
            name: this.themeTemplate.credits.theme 
          });
        }
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },
    async onXmlEditorClosed(e) {
      try {
        //console.log('XML Editor Closed: ', e);
        this.setValue(this.themeTemplate.xml_profile, 'x150', e[0][0] );
        this.setValue(this.themeTemplate.xml_profile, 'y150', e[0][1] );
        this.setValue(this.themeTemplate.xml_profile, 'z150', e[0][2] );

        this.setValue(this.themeTemplate.xml_profile, 'x151', e[1][0] );
        this.setValue(this.themeTemplate.xml_profile, 'y151', e[1][1] );
        this.setValue(this.themeTemplate.xml_profile, 'y152', e[1][2] );

        this.setValue(this.themeTemplate.xml_profile, 'x152', e[2][0] );
        this.setValue(this.themeTemplate.xml_profile, 'y152', e[2][1] );
        this.setValue(this.themeTemplate.xml_profile, 'z152', e[2][2] );

        console.log('Saving Theme:', this.themeTemplate);
        if (this.themeTemplate.name === "Current Settings") {
          const ActiveInstance = await window.api.getActiveInstance();
          this.themeTemplate.path = await window.api.joinPath(ActiveInstance.path, 'EDHM-ini');
        }

        const _ret = await window.api.SaveTheme(JSON.parse(JSON.stringify(this.themeTemplate)));
        console.log('Theme Saved:', _ret);
        // Emit the XML Changed Event
        EventBus.emit('OnXmlChanged', { xml: JSON.parse(JSON.stringify(this.themeTemplate.xml_profile)) }); //<- Event Listen in 'NavBars.vue'

        if (_ret) {
          EventBus.emit('RoastMe', { type: 'Success', message: `Theme: '${this.themeTemplate.credits.theme}' Saved.<br>Remember to Apply the changes.` });
        }
      } catch (error) {
        console.error('Error onXmlEditorClosed:', error);
        EventBus.emit('ShowError', error);
      }
    },

    // #endregion

    // #region Updates

    async CheckForUpdates() {
      if (this.settings.CheckForUpdates === undefined) {
          // CheckForUpdates Property, if is not there, we simply add it and save the change.
          this.settings.CheckForUpdates = true;
          await window.api.saveSettings(JSON.stringify(this.settings, null, 4));
        }
        if (this.settings.CheckForUpdates) {
          // Waits 8 seconds and Look for Updates:
          setTimeout(() => {
            this.LookForUpdates();
            this.CheckForMaintenanceNotice();
          }, 8000);
        }
    },
    async LookForUpdates() {
      window.api.getLatestReleaseVersion('BlueMystical', 'EDHM_UI').then(latestRelease => {   //<- For PROD Release
      //window.api.getLatestPreReleaseVersion('BlueMystical', 'EDHM_UI').then(latestRelease => {   //<- For Beta Testing
        if (latestRelease) {
          console.log(latestRelease);
          this.AnalyseUpdate(latestRelease);
        } else {
          console.log('No pre-release version found.');
        }
      }).catch(error => {
        console.error('Error fetching pre-release version:', error);
        EventBus.emit('ShowError', error);
      });
    },    
    async CheckForMaintenanceNotice() {
      try {
        const TempDir = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\Temp\\EDHM_UI\\status.json');
        const data = await window.api.downloadAsset(
          'https://raw.githubusercontent.com/BlueMystical/EDHM_UI/refs/heads/main/status.json',
          TempDir
        );
        console.log('Maintenance Notice:', data);

        // Si no hay mensaje, salimos
        if (!data.info || data.info.trim() === '') return;

        // Si el JSON trae versión, la comparamos
        if (data.versión) {
          const localVersion = await window.api.getAppVersion();
          const isLocalNewer = Util.compareVersions(localVersion, data.versión);
          if (isLocalNewer) {
            console.log(`Notificación descartada: versión local (${localVersion}) > versión aviso (${data.versión})`);
            return; // No mostramos nada
          }
        }

        // Si llegamos aquí, mostramos la notificación
        EventBus.emit('RoastMe', {
          type:     data.status,
          title:    data.title,
          message:  data.info,          
          accent:   data.accent,
          background: data.background,
          autoHide: false,
          width:    '440px',
        });

      } catch (err) {
        console.error('Error cargando notificación de mantenimiento:', err);
      }
    },
    /*async CheckForMaintenanceNotice() {
      // Checks for any Maintenance Notice from the Server
      try {
        const TempDir = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\Temp\\EDHM_UI\\status.json');
        const data = await window.api.downloadAsset(
          'https://raw.githubusercontent.com/BlueMystical/EDHM_UI/refs/heads/main/status.json',
          TempDir
        );
        console.log('Maintenance Notice:', data);
        if (data.info && data.info.trim() !== '') {
          EventBus.emit('RoastMe', {
            type: data.status,
            message: data.info,
            autoHide: false,
            width:'480px'
          });
        }
      } catch (err) {
        console.error('Error cargando notificación de mantenimiento:', err);
      }
    },*/
    async AnalyseUpdate(latesRelease) {
      try {
        const localVersion = await window.api.getAppVersion();
        const serverVersion = latesRelease.version;
        const isUpdate = Util.compareVersions(serverVersion, localVersion); console.log('isUpdate:', isUpdate);

        //console.log(latesRelease);

        if (isUpdate) {
          //Separate the Changelogs from the Install instructions:
          var patchNotes = latesRelease.notes.split("----")[0];
          console.log(patchNotes);

          const options = {
            type: 'question', //<- none, info, error, question, warning
            buttons: ['Cancel', "Yes, Download it", 'No, maybe later.'],
            defaultId: 1,
            title: 'Update Available: ' + serverVersion,
            message: 'Do you want to Download the Update?',
            detail: patchNotes,
            cancelId: 0
          };
          let fileSavePath = await window.api.resolveEnvVariables('%LOCALAPPDATA%\\Temp\\EDHM_UI\\edhm-ui-v3-windows-x64.exe');
          const platform = await window.api.getPlatform();
          var download_url = '';  

          window.api.ShowMessageBox(options).then(async result => {
            if (result && result.response === 1) {

              // 1. Check if the Game is already Running:
              const fullPath = await window.api.detectProgram('EliteDangerous64.exe');
              if (fullPath) {
                console.log('Process found at:', fullPath);
                EventBus.emit('RoastMe', {
                  type: 'Accent', accent: 'error', background: 'warning',
                  title: 'Can NOT update!', message: 'The game is Running<br>You need to close it to allow the update to be installed<br>Try again after closing the game.',
                  width:'460px', autoHide: false
                });
                return;
              } else {
                if (platform === 'win32') {
                  console.log('Running on Windows');
                  //download_url = 'https://github.com/BlueMystical/EDHM_UI/releases/download/' + serverVersion + '/edhm-ui-v3-windows-x64.exe';
                  download_url = 'https://github.com/BlueMystical/EDHM_UI/releases/latest/download/edhm-ui-v3-windows-x64.exe';

                } else if (platform === 'linux') {
                  console.log('Running on Linux');
                  //download_url = 'https://github.com/BlueMystical/EDHM_UI/releases/download/' + serverVersion + '/edhm-ui-v3-linux-x64.zip';
                  download_url = 'https://github.com/BlueMystical/EDHM_UI/releases/latest/download/edhm-ui-v3-linux-x64.zip';
                  fileSavePath = '/tmp/EDHM_UI/edhm-ui-v3-linux-x64.zip';

                } else {
                  console.log(`Running on an unknown platform: ${platform}`);
                  return;
                }

                //- Send the Command to start the Download
                EventBus.emit('StartDownload', {  //<- Event Listen in 'NavBars.vue' -> DownloadAndInstallUpdate()
                  url: download_url,
                  save_to: fileSavePath,
                  platform: platform
                });

              }
              if (result && result.response === 2) {
                EventBus.emit('RoastMe', { type: 'Info', message: 'There is an Update Available: ' + serverVersion, autoHide: false });
              }
            }
          });
        }
        else {
          EventBus.emit('RoastMe', { type: 'Success', message: 'The Application is Updated!' });
        }

      } catch (error) {
        EventBus.emit('ShowError', new Error(error.message + error.stack));
      }
    },    

    // #endregion

    async OnSaveTPMods(e) {

    },

  },
  async mounted() {

    this.Initialize();

    /* LISTENING EVENTS:   */
    EventBus.on('SettingsChanged', this.OnProgramSettings_Changed);
    EventBus.on('OnUpdateSettings', this.OnProgramSettings_Updated);
    EventBus.on('GameInsanceChanged', this.OnGameInstance_Changed);

    EventBus.on('OnThemesLoaded', this.OnThemesLoaded);
    EventBus.on('OnThemeApplied', this.OnThemeApplied);
    EventBus.on('ThemeLoaded', this.OnTemplateLoaded);
    EventBus.on('SearchBox', this.OnSearchBox_Shown);

    EventBus.on('OnCreateTheme', this.OnCreateTheme);
    EventBus.on('OnEditTheme', this.OnCreateTheme);
    EventBus.on('DoUpdateTheme', this.DoUpdateTheme);    

    EventBus.on('OnShowXmlEditor', this.OnShowXmlEditor);
    EventBus.on('LookForUpdates', this.LookForUpdates);

    EventBus.on('StartShipyard', this.StartShipyard);

    window.api.events.onEvent('RoastMe', (payload) => {
      EventBus.emit('RoastMe', payload)
    });
    window.api.events.onEvent('ShowError', (payload) => {
      EventBus.emit('ShowError', payload)
    });
    window.api.events.onEvent('SettingsChanged', (payload) => {
      EventBus.emit('SettingsChanged', payload)
    });
  },
  beforeUnmount() {
    // Clean up the event listener
    EventBus.off('SettingsChanged', this.OnProgramSettings_Changed);
    EventBus.off('OnUpdateSettings', this.OnProgramSettings_Updated);
    EventBus.off('GameInstanceChanged', this.OnGameInstance_Changed);

    EventBus.off('SearchBox', this.OnSearchBox_Shown);

    EventBus.off('OnThemesLoaded', this.OnThemesLoaded);
    EventBus.off('OnThemeApplied', this.OnThemeApplied);
    EventBus.off('ThemeLoaded', this.OnTemplateLoaded);

    EventBus.off('OnCreateTheme', this.OnCreateTheme);
    EventBus.off('OnEditTheme', this.OnCreateTheme);
    EventBus.off('DoUpdateTheme', this.DoUpdateTheme);

    EventBus.off('OnShowXmlEditor', this.OnShowXmlEditor);
    EventBus.off('LookForUpdates', this.LookForUpdates);

    EventBus.off('StartShipyard', this.StartShipyard);
  },
};
</script>

<style scoped>
#app {
  background-color: #1F1F1F;
  color: #ffffff;
}
body {
  font-size: var(--user-font-size, 14px);
}
.visually-hidden {
  color: #ffffff;
}
</style>