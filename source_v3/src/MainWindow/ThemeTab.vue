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
        @click="OnSelectTheme(image)" @contextmenu.prevent="onRightClick(image, $event)" :class="{ 'selected': image.id === selectedImageId }">
        <img :src="image.src" :alt="image.alt" class="img-thumbnail" aria-label="Image of {{ image.name }}" />
        <span class="image-label">{{ image.name }}</span>
      </li>
    </ul>
  </div> <!-- theme-container -->

  <!-- Context Menu -->
  <div ref="contextMenu" :style="{ top: contextMenuY + 'px', left: contextMenuX + 'px' }" class="collapse context-menu"
    id="contextMenu">
    <div class="card card-body">
      <ul class="list-group list-group-flush">
        <a href="#" class="list-group-item list-group-item-action" @click="onContextMenuAction('ApplyTheme')">Apply Theme</a>
        <a href="#" class="list-group-item list-group-item-action" :class="{ 'disabled': !isPreviewAvailable }" 
             @click="isPreviewAvailable ? onContextMenuAction('ThemePreview') : null">Theme Preview</a>
        <a href="#" class="list-group-item list-group-item-action" @click="onContextMenuAction('OpenFolder')">Open Theme Folder</a>
        <a href="#" class="list-group-item list-group-item-action" @click="onContextMenuAction(isFavorite ? 'UnFavorite' : 'Favorite')">
          {{ isFavorite ? 'Remove Favorite' : 'Add to Favorites' }} </a>
      </ul>
    </div>
  </div>

</template>


<script>
import EventBus from '../EventBus';

// Enable Collapse for the Context Menu
const collapseElementList = document.querySelectorAll('.collapse');
const collapseList = [...collapseElementList].map(collapseEl => new bootstrap.Collapse(collapseEl));

export default {
  name: 'ThemeTab',
  data() {
    return {
      images: [],               //<- List of Themes
      loading: false,           //<- Flag to show the Spinner
      selectedImageId: null,    //<- Index of the selected Theme
      selectedTheme: null,      //<- The Selected Theme
      showContextMenu: false,   //<- Flag to show the Context Menu
      programSettings: null,    //<- The Program Settings
      contextMenuX: 0,
      contextMenuY: 0,
    };
  },
  computed: {
    /** Check if the selected theme has a preview available */
    isPreviewAvailable() {
      return this.selectedTheme && this.selectedTheme.file && this.selectedTheme.file.credits && this.selectedTheme.file.credits.preview;
    },
    isFavorite() {
      return this.selectedTheme && this.selectedTheme.file && this.selectedTheme.file.isFavorite;
    }
  },
  methods: {

    async OnInitialize(event) {
      console.log('Initializing ThemeTab..');
      this.programSettings = event; // await window.api.getSettings();
      await this.loadThemes(this.programSettings.FavToogle);
    },

    /** LOADS THE LIST OF THEMES FROM THE USER'S THEMES FOLDER
     */
    async loadThemes(favoritesOnly) {
      try {
        //Gets the Themes Folder from the Active Instance:
        this.loading = true;       

        const gameInstance = await window.api.getActiveInstance();        
        const dataPath = await window.api.resolveEnvVariables(this.programSettings.UserDataFolder);
        const GameType = gameInstance.key === 'ED_Odissey' ? 'ODYSS' : 'HORIZ';
        const themesPath = await window.api.joinPath(dataPath, GameType, 'Themes');
        const ThumbImage = await window.api.getAssetFileUrl('images/PREVIEW.png');  // console.log('ThumbImage:',ThumbImage);
        const GamePath = await window.api.joinPath(gameInstance.path, 'EDHM-ini'); //<- the Game Folder        

        //console.log('themesPath',themesPath);
        //Loads all Themes in the Directory:
        const files = await window.api.getThemes(themesPath);    //console.log('Theme File: ', files[0]);

        // Add the dummy item for 'Current Settings':
        this.images = [{
          id: 0,
          name: "Current Settings",
          src: ThumbImage, //'images/PREVIEW.png', 
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
            .filter(file => !favoritesOnly || file.isFavorite === favoritesOnly)
            .map((file, index) => ({
              id: index + 1,
              name: file.credits.theme,
              src: `file:///${window.api.joinPath(file.path, file.thumbnail)}`,
              alt: file.credits.theme,
              file: file
            }))
        );

        EventBus.emit('OnThemesLoaded', this.images);  //<- this event will be heard in 'App.vue'

      } catch (error) {
        console.error('Failed to load files:', error);
        //EventBus.emit('ShowError', error);  //<- Not needed here
      } finally {
        // Set loading to false with a delay after themes are loaded
        setTimeout(() => {
          this.loading = false;
          EventBus.emit('ShowSpinner', { visible: false }); //<- this event will be heard in 'MainNavBars.vue'

        }, 2000);
      }
    },

    /** When Fired, Selects and Loads a given Theme   * 
     * @param theme 
     */
    OnSelectTheme(theme) {
      try {
        this.selectedImageId = theme.id;
        this.selectedTheme = theme;  console.log(this.selectedTheme);

        this.$nextTick(() => {
          const selectedElement = document.getElementById('image-' + theme.id);
          if (selectedElement) {
            selectedElement.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
          }
        });
        EventBus.emit('ThemeClicked', theme); //<- this event will be heard in 'MainNavBars.vue'

      } catch (error) {
        EventBus.emit('ShowError', error);
      }
    },

    /** When Fired, Shows the Context Menu for the Selected Theme and Selects it  * 
    * @param image The Selected Theme
    * @param event Mouse Event related
    */
    onRightClick(image, event) {

      this.OnSelectTheme(image);

      this.contextMenuX = event.clientX;
      this.contextMenuY = event.clientY - 20;
      this.showContextMenu = true;

      this.$nextTick(() => {
        const contextMenu = this.$refs.contextMenu;
        if (contextMenu) {
          contextMenu.classList.add('show');
        }
      });
    },

    /** When the User clicks on one of the Context menus * 
    * @param action Name of the clicked menu
    */
    async onContextMenuAction(action) {
      try {
        this.showContextMenu = false;
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
              if (this.selectedTheme.file.credits.preview) {
                window.api.openUrlInBrowser(this.selectedTheme.file.credits.preview);              
              }
              break;
            case 'OpenFolder':
              window.api.openPathInExplorer(this.selectedTheme.file.path);
              break;
            case 'Favorite':
              console.log(this.selectedTheme.file.path);
              const _ret = await window.api.FavoriteTheme(this.selectedTheme.file.path);
              this.selectedTheme.file.isFavorite = _ret;
              if (_ret) {
                EventBus.emit('RoastMe', { type: 'Success', message: 'Theme added to Favorites' });	
              } else {
                EventBus.emit('RoastMe', { type: 'Warning', message: 'Failure adding to Favorites' });
              }
              break;
            case 'UnFavorite':
              const _ret2 = await window.api.UnFavoriteTheme(this.selectedTheme.file.path);
              this.selectedTheme.file.isFavorite = _ret2;
              if (_ret) {
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
    hideContextMenu() {
      this.showContextMenu = false;
      const contextMenu = this.$refs.contextMenu;
      if (contextMenu) {
        contextMenu.classList.remove('show');
      }
    }


  },
  async mounted() {

    /** EVENT LISTENERS */
    document.addEventListener('click', this.hideContextMenu);
    EventBus.on('loadThemes', this.loadThemes);     //<- Event to Initiate, on demeand, the Load of all Themes
    EventBus.on('OnSelectTheme', this.OnSelectTheme); //<- Event to, on demand, Select a Theme
    EventBus.on('OnInitializeThemes', this.OnInitialize); //<- Event listened on App.vue to Initiate the Load of all Themes 
  },
  beforeUnmount() {
    EventBus.off('loadThemes', this.loadThemes);
    EventBus.off('OnSelectTheme', this.OnSelectTheme);
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