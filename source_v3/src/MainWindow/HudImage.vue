<!-- Ship's HUD & Main Navbar -->
<template>
  <div id="Container" class="image-container" ref="container" @mousemove="handleMouseMove" @mouseleave="handleMouseLeave" @click="handleClick">
    <img :src="imageSrc" alt="HUD Image" ref="image" class="hud-image" @load="setupCanvas" />
    <canvas ref="canvas"></canvas>
  </div>
</template>


<script>
import { ref, onMounted } from 'vue';
import EventBus from '../EventBus';

export default {
  name: 'HUD_Areas',
  components: {

  },
  data() {
    return {
      imageSrc: '',
      originalWidth: 0,
      originalHeight: 0,
      areas: [], // Hold the areas of interest],
      scaledAreas: [], // Hold the scaled areas
      currentArea: null, //Selected Area
      clickedArea: null // Track the clicked area
    }; 
  },  
  methods: {    
    async loadHUDSettings() {
      try {
        const defaultJsonPath = await window.api.getAssetPath('data/HUD_Default.json');
        const programSettings = await window.api.getSettings();

        if (!programSettings) { // More concise null check
          console.warn("Program settings are null. Using default HUD.");
          return; // Exit early if settings are null
        }

        const hudCover = programSettings.HUD_Cover;
        let jsonPath;

        if (hudCover.includes('%')) {
          jsonPath = await window.api.resolveEnvVariables(hudCover);
        } else {
          try {
            jsonPath = await window.api.getAssetPath(`data/${hudCover}.json`);
          } catch (assetPathError) {
            console.error(`Error getting asset path for ${hudCover}:`, assetPathError);
            // Consider displaying a user-friendly error message here
            return; // or throw the error if you want to stop execution
          }
        }
        //console.log("defaultJsonPath:", defaultJsonPath);
        //console.log("jsonPath:", jsonPath);

        let hudData;
        try {
          hudData = await window.api.getJsonFile(jsonPath);
        } catch (jsonFileError) {
          console.error('Failed to load specified HUD settings:', jsonFileError);
          console.log('Falling back to default HUD settings.');

          try {            
            hudData = await window.api.getJsonFile(defaultJsonPath);
          } catch (defaultJsonFileError) {
            console.error('Failed to load default HUD settings:', defaultJsonFileError);
            window.api.logEvent('Failed to load default HUD settings:', defaultJsonFileError, 'loadHUDSettings:82');
            //Handle the error appropriately, maybe show a message to the user
            return;
          }
        }

        this.imageSrc = await window.api.getAssetFileUrl(hudData.Image);
        this.areas = hudData.Areas;
      } catch (overallError) {
        console.error('Overall error loading HUD settings:', overallError);
      }
    },
    setupCanvas() {
      /* SET A CANVAS TO DRAW THE AREAS OF INTEREST */
      const image = this.$refs.image;
      const canvas = this.$refs.canvas;
      const container = this.$refs.container;

      if (image.complete) {
        this.originalWidth = image.naturalWidth;
        this.originalHeight = image.naturalHeight;
        canvas.width = container.clientWidth;
        canvas.height = container.clientHeight;
        this.updateAreas();
      } else {
        image.onload = () => {
          this.originalWidth = image.naturalWidth;
          this.originalHeight = image.naturalHeight;
          canvas.width = container.clientWidth;
          canvas.height = container.clientHeight;
          this.updateAreas();
        };
      }
    },
    updateAreas() {
      /* ADAPT THE AREAS LOCATIONS WHEN THE IMAGE IS RE-SIZED  */
      const image = this.$refs.image;
      const container = this.$refs.container;
      const canvas = this.$refs.canvas;

      // Calculate scale and offsets
      const imageAspectRatio = this.originalWidth / this.originalHeight;
      const containerAspectRatio = container.clientWidth / container.clientHeight;
      let offsetX = 0;
      let offsetY = 0;
      let scaleX, scaleY;

      if (imageAspectRatio > containerAspectRatio) {
        // Image is wider than container
        scaleX = container.clientWidth / this.originalWidth;
        scaleY = scaleX;
        offsetY = (container.clientHeight - this.originalHeight * scaleY) / 2;
      } else {
        // Image is taller than container
        scaleY = container.clientHeight / this.originalHeight;
        scaleX = scaleY;
        offsetX = (container.clientWidth - this.originalWidth * scaleX) / 2;
      }

      this.scaledAreas = this.areas.map(area => ({
        id: area.id,
        title: area.title,
        scaledX: area.x * scaleX + offsetX,
        scaledY: area.y * scaleY + offsetY,
        scaledWidth: area.width * scaleX,
        scaledHeight: area.height * scaleY
      }));

      this.clearCanvas();
    },
    handleMouseMove(event) {
      /* HIGTLIGHTS THE AREA WHEN THE MOUSE IS OVER  */
      const rect = this.$refs.canvas.getBoundingClientRect();
      const x = event.clientX - rect.left;
      const y = event.clientY - rect.top;

      this.currentArea = null;
      for (const area of this.scaledAreas) {
        if (x > area.scaledX && x < area.scaledX + area.scaledWidth && y > area.scaledY && y < area.scaledY + area.scaledHeight) {
          this.currentArea = area;
          this.drawRect(area);
          break;
        }
      }

      if (!this.currentArea) {
        this.clearCanvas();
      }
    },
    handleMouseLeave() {
      /* cleanup when mouse is away */
      this.clearCanvas();
      this.currentArea = null;
    },
    handleClick() {
      /* WHEN THE USER CLICKS AN AREA  */
      //console.log('Area clicked:', this.currentArea);
      if (this.currentArea) {
        this.clickedArea = this.currentArea;        
        this.highlightClickedArea(this.currentArea);

        if (this.currentArea.title != "XML Editor") {
          EventBus.emit('areaClicked', this.currentArea); //<- Event listen in 'PropertiesTab.vue'
          EventBus.emit('setActiveTab', 'properties');    //<- Event listen in 'MainNavBars.vue'
        } else {
          /* OPENING XML EDITOR */
          EventBus.emit('OnShowXmlEditor', this.currentArea); //<- Event listen in 'App.vue'
        }        
      }
    },
    drawRect(area) {
      const canvas = this.$refs.canvas;
      const ctx = canvas.getContext('2d');
      this.clearCanvas();

      // Draw border with reduced height
      const reducedHeight = area.scaledHeight - 5; // Adjust height reduction as needed
      ctx.strokeStyle = 'orange';
      ctx.lineWidth = 2;
      ctx.strokeRect(area.scaledX, area.scaledY, area.scaledWidth, reducedHeight);

      // Draw area name centered below the rectangle
      ctx.fillStyle = 'white';
      ctx.font = '14px Segoe UI';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'top';
      ctx.shadowColor = 'rgba(0, 0, 0, 0.5)';
      ctx.shadowOffsetX = 2;
      ctx.shadowOffsetY = 2;
      ctx.shadowBlur = 2;
      ctx.fillText(area.title, area.scaledX + area.scaledWidth / 2, area.scaledY + reducedHeight + 5);

      // Highlight clicked area if applicable
      if (this.clickedArea && this.clickedArea.id === area.id) {
        this.highlightClickedArea(area);
      }
    },
    highlightClickedArea(area) {
      const canvas = this.$refs.canvas;
      const ctx = canvas.getContext('2d');

      // Draw semi-transparent orange background with reduced height
      const reducedHeight = area.scaledHeight - 5; // Adjust height reduction as needed
      ctx.fillStyle = 'rgba(255, 165, 0, 0.3)';
      ctx.fillRect(area.scaledX, area.scaledY, area.scaledWidth, reducedHeight);

      // Draw bright blue border
      ctx.strokeStyle = 'orange';
      ctx.lineWidth = 1;
      ctx.strokeRect(area.scaledX, area.scaledY, area.scaledWidth, reducedHeight);

      // Draw area name centered below the rectangle
      ctx.fillStyle = 'white';
      ctx.font = '14px Segoe UI';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'top';
      ctx.shadowColor = 'rgba(0, 0, 0, 0.5)';
      ctx.shadowOffsetX = 2;
      ctx.shadowOffsetY = 2;
      ctx.shadowBlur = 2;
      ctx.fillText(area.title, area.scaledX + area.scaledWidth / 2, area.scaledY + reducedHeight + 5);
    },
    clearCanvas() {
      const canvas = this.$refs.canvas;
      const ctx = canvas.getContext('2d');
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      // Redraw the clicked area if it exists
      if (this.clickedArea) {
        this.highlightClickedArea(this.clickedArea);
      }
    },

    async OnInitialize(event) {
      console.log('Initializing HUD image...');
      await this.loadHUDSettings();
      this.setupCanvas();
    },
  },
  async mounted() {
    try {
      this.imageSrc = await window.api.getAssetFileUrl('images/HUD_Clean.png');         //console.log(this.imageSrc);
      window.addEventListener('resize', this.setupCanvas);
      EventBus.on('InitializeHUDimage', this.OnInitialize);
    } catch (error) {
      console.log(error);
    }    
  },
  beforeUnmount() { //beforeDestroy() {
    window.removeEventListener('resize', this.setupCanvas);
    EventBus.off('InitializeHUDimage', this.OnInitialize);
  },
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
.hud-image {
  width: 100%;
  height: 100%;
  object-fit: contain;  
}

.navbar-thin {
  padding-top: 0.25rem;
  padding-bottom: 0.25rem;
}

.image-container {
  position: relative;
  display: inline-block;
  width: 100%;
  height: 100%;
  /*border: 2px solid rgba(0, 0, 255, 0.5); /* Blue-ish border */
}

canvas {
  position: absolute;
  top: 0;
  left: 0;
  pointer-events: none;
  /*border: 2px solid rgb(9, 255, 0);
  /* Ensure the canvas does not block mouse events */
}

</style>