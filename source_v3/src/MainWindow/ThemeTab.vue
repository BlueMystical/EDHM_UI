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
      </li>
    </ul>

    <!-- Context Menu for Themes -->
    <ul v-if="showContextMenuFlag" :style="contextMenuStyle" class="dropdown-menu shadow show" ref="contextMenu">
      <li><a class="dropdown-item" href="#" @click="onContextMenu_Click('ApplyTheme')">Apply Theme</a></li>
      <li><a class="dropdown-item" href="#" :class="{ 'disabled': !isPreviewAvailable }"
          @click="isPreviewAvailable ? onContextMenu_Click('ThemePreview') : null">Theme Preview</a></li>
          <li><a class="dropdown-item" href="#" @click="onContextMenu_Click('OpenFolder')">Open Theme Folder</a></li>
      <li><hr class="dropdown-divider"></li>
      <li><a class="dropdown-item" href="#" @click="onContextMenu_Click(isFavorite ? 'UnFavorite' : 'Favorite')">
          {{ isFavorite ? 'Remove Favorite' : 'Add to Favorites' }} </a></li>
          
      <li><hr class="dropdown-divider"></li>
      <li><h6 class="dropdown-header">{{ selectedTheme.name }}</h6></li>
      <p><small class="px-3 disabled">Author: <span v-html="selectedTheme.file.credits.author"></span></small></p>     
    </ul>

  </div> <!-- theme-container -->

  <!-- No quitar esto o se jode el tamaño de los temas, no sé por qué?? -->
  <div id="contextMenu" ref="contextMenu" class="collapse context-menu"></div>

  

</template>


<script>
import EventBus from '../EventBus';

// Enable Collapse for the Context Menu
//const collapseElementList = document.querySelectorAll('.collapse');
//const collapseList = [...collapseElementList].map(collapseEl => new bootstrap.Collapse(collapseEl));

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
      showContextMenuFlag: false //<- Flag to show the Context Menu
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

  },
  methods: {

    async OnInitialize(event) {
      console.log('Initializing ThemeTab..');
      this.programSettings = event; // await window.api.getSettings();
      await this.loadThemes();
      this.FilterThemes(this.programSettings.FavToogle);

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
        const ThumbImage = await window.api.getAssetFileUrl('images/PREVIEW.png');  // console.log('ThumbImage:',ThumbImage);
        const GamePath = await window.api.joinPath(gameInstance.path, 'EDHM-ini');  //<- the Game Folder        

        //Loads all Themes in the Directory:
        const files = await window.api.getThemes(themesPath);    //console.log('Theme File: ', files[4]);

        // Add the dummy item for 'Current Settings':
        this.themes = [{
          id: 0,
          name: "Current Settings",
          src: ThumbImage, //'images/PREVIEW.png', 
          preview: '',
          alt: "Current Settings",
          file: {
            path: GamePath,
            thumbnail: ThumbImage,
            credits: {
              author: "Blue Mystic",
              description: "** THESE ARE THE CURRENTLY APPLIED COLORS **",
              preview: "",
              theme: "Current Settings"
            }
          }
        }].concat( // Adding the rest of loaded Themes:          
          files
            //.filter(file => !favoritesOnly || file.isFavorite === favoritesOnly)
            .map((file, index) => ({
              id: index + 1,
              name: file.credits.theme,
              src: `file:///${window.api.joinPath(file.path, file.thumbnail)}`,
              preview: file.preview,
              alt: file.credits.theme,
              file: file
            }))
        );

        //console.log('Themes Loaded: ', this.themes);
        EventBus.emit('OnThemesLoaded', this.themes);  //<- this event will be heard in 'App.vue'

      } catch (error) {
        console.error('Failed to load files:', error);
        //EventBus.emit('ShowError', error);  //<- Not needed here
      } finally {
        // Set loading to false with a delay after themes are loaded
        this.loading = false;
        EventBus.emit('ShowSpinner', { visible: false }); //<- this event will be heard in 'MainNavBars.vue'
        /*  setTimeout(() => {
            this.loading = false;
            EventBus.emit('ShowSpinner', { visible: false }); //<- this event will be heard in 'MainNavBars.vue'
          }, 2000); */
      }
    },

    FilterThemes(favoritesOnly) {
      if (this.themes && this.themes.length > 0) {
        this.images = this.themes.filter(theme => !favoritesOnly || theme.file.isFavorite === favoritesOnly);
      }
      console.log('Filtering Favorites: ', this.images.length);
    },

    /** When Fired, Selects and Loads a given Theme   * 
     * @param theme We only need the id (index in the list) -> { id: 0 }
     */
    OnSelectTheme(theme) {
      try {
        if (theme && !isEmpty(theme)) {
          if (this.themes && !isEmpty(this.themes)) {
            const searchIndex = theme.id;                                             //console.log('searchIndex: ', searchIndex);
            const selectedItem = this.themes.find(item => item.id === searchIndex);   //console.log('selectedItem: ', selectedItem);

            if (selectedItem) {
              this.selectedImageId = searchIndex;
              this.selectedTheme = selectedItem;                                      //console.log('selectedTheme: ', this.selectedTheme);

              this.$nextTick(() => {
                const selectedElement = document.getElementById('image-' + selectedItem.id);
                if (selectedElement) {
                  selectedElement.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                }
              });
              EventBus.emit('ThemeClicked', selectedItem); //<- this event will be heard in 'MainNavBars.vue'
            }
          }
          else {
            console.log('Waiting for themes to be loaded..');
            this.quequeSelect = theme;
          }
        }
      } catch (error) {
        EventBus.emit('ShowError', error);
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
            this.FilterThemes(true);
          }
          EventBus.emit('RoastMe', { type: 'Success', message: 'Theme added to Favorites' });
        } else {
          EventBus.emit('RoastMe', { type: 'Warning', message: 'Failure adding to Favorites' });
        }

      } catch (error) {
        EventBus.emit('ShowError', error);
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
      // Position the context menu
      this.contextMenuStyle = {
        top: `${y - 60}px`,
        left: `${x}px`,
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

    // #endregion



  },
  async mounted() {

    /** EVENT LISTENERS */
    EventBus.on('loadThemes', this.loadThemes);           //<- Event to Initiate, on demeand, the Load of all Themes
    EventBus.on('FilterThemes', this.FilterThemes);       //<- Event to Filter the favorite themes
    EventBus.on('OnSelectTheme', this.OnSelectTheme);     //<- Event to, on demand, Select a Theme
    EventBus.on('OnFavoriteTheme', this.DoFavoriteTheme); //<- Event to, on demand, Favorite a Theme
    EventBus.on('OnInitializeThemes', this.OnInitialize); //<- Event listened on App.vue to Initiate the Load of all Themes 
  },
  beforeUnmount() {
    EventBus.off('loadThemes', this.loadThemes);
    EventBus.off('FilterThemes', this.FilterThemes);
    EventBus.off('OnSelectTheme', this.OnSelectTheme);
    EventBus.off('OnFavoriteTheme', this.DoFavoriteTheme);
    EventBus.off('OnInitializeThemes', this.OnInitialize);
  }
};
</script>

<style scoped>
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

.theme-tab {
  overflow-y: auto;
  max-height: 100%;
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
</style>