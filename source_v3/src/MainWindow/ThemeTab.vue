<template>

  <div class="theme-tab"> <!-- theme-container -->

    <!-- Loading Spinner -->
    <div v-if="loading" class="spinner-container">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <!-- List of Themes -->
    <ul v-else>
      <li v-for="image in images" :key="image.id" :id="'image-' + image.id" class="image-container"
        :class="{ 'selected': image.id === selectedImageId }" @click="OnSelectTheme(image)"
        @contextmenu="onRightClick($event, image)">
        <img :src="image.src" :alt="image.alt" class="img-thumbnail" aria-label="Image of {{ image.name }}" />
        <span class="image-label">{{ image.name }}</span>
        <div v-if="isFavoriteEx(image)" class="badge-triangle"></div>
      </li>
    </ul>

    <!-- Context Menu for Themes -->
    <ul v-if="showContextMenuFlag" :style="contextMenuStyle" class="dropdown-menu border border-primary shadow show" ref="contextMenu">
      <li><a class="dropdown-item" href="#" @click="onContextMenu_Click('ApplyTheme')">Apply Theme</a></li>
      <li><a class="dropdown-item" href="#" :class="{ 'disabled': !isPreviewAvailable }"
          @click="isPreviewAvailable ? onContextMenu_Click('ThemePreview') : null">Theme Preview</a></li>
      <li><a class="dropdown-item" href="#" @click="onContextMenu_Click('OpenFolder')">Open Theme Folder</a></li>
      <li><a class="dropdown-item" href="#" @click="onContextMenu_Click('DeleteTheme')">Delete Theme</a></li>
      <li><hr class="dropdown-divider"></li>
      <li><a class="dropdown-item" href="#" @click="onContextMenu_Click(isFavorite ? 'UnFavorite' : 'Favorite')"
          :class="{ disabled: isCurrentSettings }">{{ isFavorite ? 'Remove Favorite' : 'Add to Favorites' }}
        </a></li>
      <li><hr class="dropdown-divider"></li>

      <div class="header-text px-3"><h6>{{ selectedTheme.name }}</h6>
        <p><small class="px-3 text-info">By <span v-html="selectedTheme.file.credits.author"></span></small></p>
      </div>
      <p><small class="px-3 description-text" v-html="selectedTheme.file.credits.description" @click="descriptionLinkClick"></small></p>
    </ul>

  </div> <!-- theme-container -->

  <!-- No quitar esto o se jode el tamaño de los temas, no sé por qué?? -->
  <div id="contextMenu" ref="contextMenu" class="collapse context-menu"></div>

</template>


<script>
import EventBus from '../EventBus';

// Enable Dropdown for the Context Menu
const dropdownElementList = document.querySelectorAll('.dropdown-toggle');
const dropdownList = [...dropdownElementList].map(dropdownToggleEl => new bootstrap.Dropdown(dropdownToggleEl));

/** To Check is something is Empty
 * @param obj Object to check
 */
const isEmpty = obj => Object.keys(obj).length === 0;

export default {
  name: 'ThemeTab',
  data() {
    return {
      themes: [],               //<- Full List of Themes
      images: [],               //<- Filtered List of Themes
      loading: false,           //<- Flag to show the Spinner
      selectedTheme: null,      //<- The Selected Theme
      selectedImageId: null,    //<- Index of the selected Theme      

      programSettings: null,    //<- The Program Settings
      quequeFavorite: null,     //<- Index of a theme to be Favorited waiting in a Queque
      quequeSelect: null,       //<- Index of a theme to be selected waiting in a Queque

      contextMenuStyle: {},
      showContextMenuFlag: false, //<- Flag to show the Context Menu
      favToogle: false,
    };
  },
  computed: {
    /** Check if the selected theme has a preview available */
    isPreviewAvailable() {
      return this.selectedTheme && this.selectedTheme.preview;
    },
    isFavorite() {
      return this.selectedTheme && this.selectedTheme.file && this.selectedTheme.file.isFavorite;
    },
    isCurrentSettings(){
      return this.selectedTheme && this.selectedTheme.name === "Current Settings";
    }
  },
  methods: {

    async OnInitialize(event) {
      console.log('Initializing ThemeTab..');
      this.programSettings = event; // await window.api.getSettings();
      await this.loadThemes();      

      //If there is a theme waiting to be selected or Favorited:
      if (this.quequeSelect) {
        this.OnSelectTheme(this.quequeSelect);
        this.quequeSelect = null;
      }
      if (this.quequeFavorite) {
        await this.DoFavoriteTheme(this.quequeFavorite);
        this.quequeFavorite = null;
      }
    },

    /** LOADS THE LIST OF THEMES FROM THE USER'S THEMES FOLDER
     */
    async loadThemes() {
      try {
        //Gets the Themes Folder from the Active Instance:
        this.loading = true;

        const gameInstance = await window.api.getActiveInstance();
        const dataPath = await window.api.resolveEnvVariables(this.programSettings.UserDataFolder);
        const GameType = gameInstance.key === 'ED_Odissey' ? 'ODYSS' : 'HORIZ';
        const themesPath = await window.api.joinPath(dataPath, GameType, 'Themes'); //console.log('themesPath',themesPath);
        const CurrSetImage = await window.api.getAssetFileUrl('images/PREVIEW.png');  // console.log('ThumbImage:',ThumbImage);
        const ThumbImage = await window.api.getAssetFileUrl('images/PREVIEW.jpg');
        const GamePath = await window.api.joinPath(gameInstance.path, 'EDHM-ini');  //<- the Game Folder     

        this.themes = [];
        this.images = [];

        //Loads all Themes in the Directory:
        const files = await window.api.getThemes(themesPath);    //console.log('Theme File: ', files[4]);        

        // Add the dummy item for 'Current Settings':
        this.themes = [{
          id: 0,
          name: "Current Settings",
          src: CurrSetImage, 
          preview: '',
          alt: "Current Settings",
          file: {
            path: GamePath,
            thumbnail: CurrSetImage,
            credits: {
              author: "You",
              description: "** THESE ARE THE CURRENTLY APPLIED COLORS **",
              preview: "",
              theme: "Current Settings",              
            },
            isFavorite: true
          }
        }].concat(  //<- Adding the rest of loaded Themes:     
          await Promise.all(files.map(async (file, index) => {
            let thumbnailSrc = ThumbImage; // Default Preview Image
            const previewPath = await window.api.joinPath(file.path, 'PREVIEW.jpg');
            const previewExists = await window.api.fileExists(previewPath);

            if (previewExists) {
              thumbnailSrc = `file:///${previewPath}`;
            }

            return {
              id: index + 1,
              name: file.credits.theme,
              src: thumbnailSrc, //`file:///${window.api.joinPath(file.path, file.thumbnail)}`,
              preview: file.preview,
              alt: file.credits.theme,
              file: file,
            };
          }))
        );      

        this.FilterThemes(this.programSettings.FavToogle);
        EventBus.emit('OnThemesLoaded', this.themes);  //<- this event will be heard in 'App.vue'

      } catch (error) {
        console.error('Failed to load files:', error);
      } finally {
        this.loading = false;
        EventBus.emit('ShowSpinner', { visible: false }); //<- this event will be heard in 'MainNavBars.vue'
      }
    },

    isFavoriteEx(image) {
      if (image && image.file) {
        return image && image.file.isFavorite;
      }
      else {
        return false;
      }
    },
    FilterThemes(favoritesOnly) {
      this.favToogle = favoritesOnly;
      if (this.themes && this.themes.length > 0) {
        this.images = this.themes.filter(theme => !favoritesOnly || theme.file.isFavorite === favoritesOnly);
      }
      //console.log('Filtering Favorites: ', this.images.length);
    },

    /** When Fired, Selects and Loads a given Theme   * 
     * @param theme We only need the id (index in the list) -> { id: 0 }
     */
    OnSelectTheme(theme) {
      try {
        //console.log(this.selectedTheme);
        if (theme && !isEmpty(theme)) {
          // This check allowes the user to select its own theme without reloading old values:
          //if (this.selectedTheme === null || this.selectedTheme.id != theme.id) {
            if (this.themes && !isEmpty(this.themes)) {
              const searchIndex = theme.id;                                             //console.log('searchIndex: ', searchIndex);
              const selectedItem = this.themes.find(item => item.id === searchIndex); 
              console.log('Selected Theme: ', selectedItem.name);

              if (selectedItem) {
                this.selectedImageId = searchIndex;
                this.selectedTheme = selectedItem;                                      //console.log('selectedTheme: ', this.selectedTheme);

                this.$nextTick(() => {
                  const selectedElement = document.getElementById('image-' + selectedItem.id);
                  if (selectedElement) {
                    selectedElement.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                  }
                });
                EventBus.emit('ThemeClicked', JSON.parse(JSON.stringify(selectedItem))); //<- this event will be heard in 'MainNavBars.vue'
              }
            }
            else {
              console.log('Waiting for themes to be loaded..');
              this.quequeSelect = theme;
            }
          //}
        }
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },

    /** When Fired, Applies a given Theme   * 
     * @param theme_name Name of the Theme to be applied     */
    FindAndApplyTheme(theme_name) {
      console.log('FindAndApplyTheme: ', theme_name);
      if (this.themes && this.themes.length > 0) {        
        const index = this.themes.findIndex(obj => obj.file.name === theme_name);
        if (index !== -1) {
          console.log('Theme Found: ', this.themes[index].name);
          this.OnSelectTheme({ id: index });
          EventBus.emit('OnApplyTheme', null); //<- this event will be heard in 'NavBars.vue'  
        } else {
            console.log(`Object with name ${tName} not found.`);
        }
      }
    },

    /** Makes Favorite a theme. * 
     * @param pSelectTheme if 'null' favorite the Selected theme, else favorites the given theme (a newly added)
     */
    async DoFavoriteTheme(pSelectTheme) {
      try {
        // Si Estamos favoriteando un nuevo tema:
        if (pSelectTheme && !isEmpty(pSelectTheme)) {
          // - Hay que ver si la lista está cargada, o esperar a que lo esté
          if (this.themes && !isEmpty(this.themes)) {
            // - Buscar el tema x nombre en la lista            
            let foundTheme = this.themes.find(theme => theme.name === pSelectTheme.ThemeName);    //console.log('foundTheme', foundTheme);  
            // - Seleccionarlo
            this.OnSelectTheme({ id: foundTheme.id });
          } else {
            //No hay temas cargados, esperar a que lo esten e intentar de nuevo
            this.quequeFavorite = pSelectTheme;
            return;
          }
        }

        const _ret = await window.api.FavoriteTheme(this.selectedTheme.file.path);
        if (_ret) {
          this.selectedTheme.file.isFavorite = true;                                      //console.log('id: ',this.selectedTheme.id);
          let favTheme = this.themes.find(image => image.id === this.selectedTheme.id);   //console.log('favTheme: ', favTheme);
          if (favTheme) {
            favTheme.file.isFavorite = true;
            this.FilterThemes(this.favToogle);
          }
          EventBus.emit('RoastMe', { type: 'Success', message: 'Theme added to Favorites' });
        } else {
          EventBus.emit('RoastMe', { type: 'Warning', message: 'Failure adding to Favorites' });
        }

      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },

    /** Updates and reloads a theme
     * @param theme data of the theme  */
    DoReloadTheme(theme){
      const tName = theme.name;
      if (this.themes && this.themes.length > 0) {
        const index = this.themes.findIndex(obj => obj.file.name === tName);
        if (index !== -1) {
          this.themes[index] = theme;
          //console.log('Reloaded: ', theme);
          this.FilterThemes(this.favToogle);
        } else {
            console.log(`Object with name ${tName} not found.`);
        }
      }
    },

    /** When a Theme is Applied and Current Settings got updated
     * @param theme data of the applied theme     */
    CurretSettingsUpdated(theme) {
      if (this.themes && this.themes.length > 0) {
        //console.log(theme);
        this.themes[0].name = 'Current Settings: ' + theme.credits.theme;
        this.themes[0].file.credits = theme.credits;
      }
    },

    // #region Context Menus

    /** When the User clicks on one of the Context menus * 
    * @param action Name of the clicked menu
    */
    async onContextMenu_Click(action) {
      try {
        const contextMenu = this.$refs.contextMenu;
        if (contextMenu) {
          contextMenu.classList.remove('show');
        }
        console.log('Context Menu:', action);
        // Handle the context menu action
        if (this.selectedTheme) {
          switch (action) {
            case 'ApplyTheme':
              EventBus.emit('OnApplyTheme', null); //<- this event will be heard in 'MainNavBars.vue'
              break;

            case 'ThemePreview':
              //console.log(this.selectedTheme);
              if (this.selectedTheme.preview) {
                window.api.openUrlInBrowser(this.selectedTheme.preview);
              }
              break;

            case 'OpenFolder':
              window.api.openPathInExplorer(this.selectedTheme.file.path);
              break;

            case 'DeleteTheme':
              const options = {
                type: 'question', //<- none, info, error, question, warning
                buttons: ['Cancel', "Yes, I am sure", 'No! take me back!'],
                defaultId: 1,
                title: 'Delete Theme?',
                message: 'Do you want to Delete this theme?',
                detail: `Theme: '${this.selectedTheme.file.credits.theme}'`,
                cancelId: 0
              };
              const result = await window.api.ShowMessageBox(options);
              if (result && result.response === 1) {
                const _ret = await window.api.DeleteTheme(this.selectedTheme.file.path);
                if (_ret) {
                  EventBus.emit('RoastMe', { type: 'Success', message: 'Theme Deleted!' });
                  this.loadThemes();
                }
              };
            break;

            case 'Favorite':
              this.DoFavoriteTheme(null);
              break;

            case 'UnFavorite':
              const _ret2 = await window.api.UnFavoriteTheme(this.selectedTheme.file.path);
              if (_ret2) {
                this.selectedTheme.file.isFavorite = false;
                let favTheme = this.images.find(image => image.id === this.selectedTheme.id);   //console.log('favTheme: ', favTheme);
                if (favTheme) {
                  favTheme.file.isFavorite = false;
                }
                EventBus.emit('RoastMe', { type: 'Success', message: 'Theme removed from Favorites' });
              } else {
                EventBus.emit('RoastMe', { type: 'Warning', message: 'Failure removing from Favorites' });
              }
              break;

            default:
              break;
          }
        }
      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },

    /** When Fired, Shows the Context Menu for the Selected Theme and Selects it  * 
    * @param image The Selected Theme
    * @param event Mouse Event related
    */
    onRightClick(event, image) {
      event.preventDefault(); // Prevent the default context menu
      this.OnSelectTheme(image);
      this.showContextMenu(event.clientX, event.clientY); // Show the custom context menu
    },
    showContextMenu(x, y) {
      const contextMenu = this.$refs.contextMenu;
      const container = document.querySelector('.theme-tab');
      const containerRect = container.getBoundingClientRect();
      const contextMenuHeight = contextMenu.offsetHeight;
      const contextMenuWidth = contextMenu.offsetWidth;

      // Calculate centered position
      const centeredY = 100; // containerRect.top + (containerRect.height - contextMenuHeight) / 2;
      const centeredX = containerRect.left + (containerRect.width - contextMenuWidth) / 2;

      // Position the context menu in the center of the container
      this.contextMenuStyle = {
        top: `${centeredY}px`,
        left: `${centeredX}px`,
        display: 'block'
      };
      this.showContextMenuFlag = true;

      // Close context menu when clicking outside
      document.addEventListener('click', this.hideContextMenu);
    },
    hideContextMenu() {
      this.showContextMenuFlag = false;
      document.removeEventListener('click', this.hideContextMenu); // Remove the event listener
    },
    descriptionLinkClick(event) {
      if (event.target.tagName === 'A') {        
        event.preventDefault(); // Prevent default link behavior        
        const link = event.target.getAttribute('href'); // Get the href attribute
        window.api.openUrlInBrowser(link);
        console.log('Link clicked:', link);
      }
    },

    // #endregion

  },
  async mounted() {
    /** EVENT LISTENERS */
    EventBus.on('loadThemes', this.loadThemes);           //<- Event to Initiate, on demeand, the Load of all Themes
    EventBus.on('FilterThemes', this.FilterThemes);       //<- Event to Filter the favorite themes
    EventBus.on('OnSelectTheme', this.OnSelectTheme);     //<- Command to Select a Theme
    EventBus.on('OnFavoriteTheme', this.DoFavoriteTheme); //<- Command to Favorite a Theme
    EventBus.on('OnInitializeThemes', this.OnInitialize); //<- Event listened on App.vue to Initiate the Load of all Themes 
    EventBus.on('DoReloadTheme', this.DoReloadTheme);     //<- Command to Reload an specific theme
    EventBus.on('FindAndApplyTheme', this.FindAndApplyTheme); //<- Command to Apply an specific theme
    EventBus.on('CurretSettingsUpdated', this.CurretSettingsUpdated);
  },
  beforeUnmount() {
    EventBus.off('loadThemes', this.loadThemes);
    EventBus.off('FilterThemes', this.FilterThemes);
    EventBus.off('OnSelectTheme', this.OnSelectTheme);
    EventBus.off('OnFavoriteTheme', this.DoFavoriteTheme);
    EventBus.off('OnInitializeThemes', this.OnInitialize);
    EventBus.off('DoReloadTheme', this.DoReloadTheme); 
    EventBus.off('FindAndApplyTheme', this.FindAndApplyTheme); 
    EventBus.off('CurretSettingsUpdated', this.CurretSettingsUpdated);
  }
};
</script>

<style scoped>

.theme-tab {
  overflow-y: auto;
  max-height: 100%;
  width: 100%;
}

.theme-tab::-webkit-scrollbar {
  width: 8px;
}
.theme-tab::-webkit-scrollbar-track {
  background: #333;
}
.theme-tab::-webkit-scrollbar-thumb {
  background-color: #555;
  border-radius: 10px;
}
.theme-tab::-webkit-scrollbar-thumb:hover {
  background-color: #777;
}

ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.disabled {
  pointer-events: none;
  opacity: 0.6;
}

.image-container {
  position: relative;
  background-color: transparent;
  color: #f8f9fa;
  padding: 1px;
  margin-bottom: 1px;
  border: 1px solid transparent;
  display: flex;
  align-items: center;
  justify-content: center;
}

.image-container:hover .img-thumbnail {
  border-color: #00bfff;
}

.img-thumbnail {
  width: 100%;
  height: auto;
  background-color: transparent;
  border: 1px solid orange;
}

.selected .img-thumbnail {
  border: 3px solid #00bfff;
  box-shadow: 0 0 10px #00bfff;
}

.image-label {
  position: absolute;
  bottom: 5px;
  left: 5px;
  background-color: rgba(0, 0, 0, 0.3);
  padding: 1px;
  border-radius: 3px;
}

.badge-triangle {
  width: 0;
  height: 0;
  border-left: 40px solid transparent;
  border-top: 40px solid #ffc107;
  position: absolute;
  top: 0;
  right: 0;
}
.badge-triangle::before {
  content: '★';
  position: absolute;
  top: -50px;
  right: 0px;
  font-size: 26px;
  color: #110ec7;
}

.spinner-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
}
.context-menu {
  position: absolute;
  z-index: 1000;
}

.dropdown-menu {
  width: 240px;
  max-width: 250px; /* Set your desired fixed width */
  white-space: normal; /* Allow text to wrap */
  display: block; /* Ensures the block display */
  box-sizing: border-box; /* Includes padding and border in width and height */
}
.header-text {
  overflow-wrap: break-word; /* Ensure long words break onto the next line */
  word-wrap: break-word; /* Old IE support */
  white-space: normal; /* Allow text to wrap */
  display: block; /* Ensures the block display */
  box-sizing: border-box; /* Includes padding and border in width and height */
  color: orange;
  margin-bottom: 2px; /* Reduce spacing below header */
}
.description-text {
  overflow-wrap: break-word; /* Ensure long words break onto the next line */
  word-wrap: break-word; /* Old IE support */
  white-space: normal; /* Allow text to wrap */
  display: block; /* Ensures the block display */
  color: inherit; /* Inherit color from parent to avoid looking disabled */
  cursor: default; /* Ensures it doesn't look clickable */
}
.text-info {
  margin-top: 0; /* Reduce spacing above author line */
  margin-bottom: 5px; /* Optional: adjust spacing below author line if needed */
}
</style>