<template>
  <div class="theme-tab">
    <div v-if="loading" class="spinner-container">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    <ul v-else>
      <li v-for="image in images" :key="image.id" class="image-container" @click="handleImageClick(image)"         :class="{ 'selected': image.id === selectedImageId }">
        <img :src="image.src" :alt="image.alt" class="img-thumbnail" aria-label="Image of {{ image.name }}" />
        <span class="image-label">{{ image.name }}</span>
      </li>
    </ul>
  </div>
</template>

<script>
import eventBus from '../EventBus';
export default {
  name: 'ThemeTab',
  data() {
    return {
      images: [],
      selectedImageId: null,
      loading: false // Set loading to false initially
    };
  },
  methods: {
    /**
     * WHEN THE USER CLICKS ON A THEME IN THE LIST
     * @param image 
     */
    handleImageClick(image) {
      this.selectedImageId = image.id; // Set the selected image ID
      //console.log('Associated file object:', image.file);
      eventBus.emit('ThemeClicked', image); //<- this event will be heard in 'MainNavBars.vue'
    },

    /**
     * LOADS THE LIST OF THEMES FROM THE USER'S THEMES FOLDER
     */
    async loadThemes(gameInstance) {
      try {
        //Gets the Themes Folder from the Active Instance:
        this.loading = true;
        const programSettings = await window.api.getSettings(); //console.log('programSettings: ', programSettings);
        const dataPath = await window.api.resolveEnvVariables(programSettings.UserDataFolder);
        const themesPath = await window.api.joinPath(dataPath, 'ODYSS', 'Themes');  
        const ThumbImage = await window.api.getAssetFileUrl('images/PREVIEW.png');  // console.log('ThumbImage:',ThumbImage);
        const GamePath = await window.api.joinPath(gameInstance.path, 'EDHM-ini'); //<- the Game Folder        

        //console.log('themesPath',themesPath);
        //Loads all Themes in the Directory:
        const files = await window.api.getThemes(themesPath);    //console.log('Theme File: ', files[0]);

        // Add the dummy item for 'Current Settings':
        this.images = [{
          id: 0,
          name: "Current Settings",
          src:  ThumbImage, //'images/PREVIEW.png', 
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
          files.map((file, index) => ({
            id: index + 1,
            name: file.credits.theme,
            src: `file:///${window.api.joinPath(file.path, file.thumbnail)}`,
            alt: file.credits.theme,
            file: file
          }))
        );

      } catch (error) {
        console.error('Failed to load files:', error);
      } finally {
        // Set loading to false with a delay after themes are loaded
        setTimeout(() => {
          this.loading = false;
          eventBus.emit('ShowSpinner', { visible: false } ); //<- this event will be heard in 'MainNavBars.vue'
          
        }, 2000);
      }
    }

  },
  async mounted() {
    const gameInstance = await window.api.getActiveInstance();
    await this.loadThemes(gameInstance);

    eventBus.on('loadThemes', this.loadThemes);

  },
  beforeUnmount() {
    eventBus.off('loadThemes', this.loadThemes);
  }
};
</script>

<style scoped>
.spinner-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  /* Full height to cover the entire tab */
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

.img-thumbnail {
  width: 100%;
  height: auto;
  background-color: transparent;
  border: 1px solid orange;
}

.image-container:hover .img-thumbnail {
  border-color: #00bfff;
}

.selected .img-thumbnail {
  border-color: #00bfff;
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