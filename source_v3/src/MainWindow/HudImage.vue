<!-- Ship's HUD & Main Navbar -->
<template>
  <div id="Container" class="image-container" ref="container" @mousemove="OnArea_MouseMove" @mouseleave="OnArea_MouseLeave" @click="OnArea_Click($event)">
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
      hudData: null, // Hold the HUD data
      areas: [], // Hold the areas of interest],
      scaledAreas: [], // Hold the scaled areas
      currentArea: null, //Selected Area
      clickedArea: null, // Track the clicked area
    }; 
  },  
  methods: {    
    async loadHUDSettings() {
      try {
        const [defaultJsonPath, programSettings, DATA_DIRECTORY] = await Promise.all([
          window.api.getAssetPath('data/HUD/HUD_Default.json'),
          window.api.getSettings(),
          window.api.GetProgramDataDirectory()
        ]);

        if (!programSettings) {
          console.warn("Program settings are null. Using default HUD.");
          return;
        }

        //- Here we load the Data for the Selected HUD Cover
        const hudCover = programSettings.HUD_Cover;
        const jsonPath = window.api.joinPath(DATA_DIRECTORY, 'HUD', `${hudCover}.json`);
        this.hudData = await window.api.getJsonFile(jsonPath).catch(async () => {
          console.warn('Failed to load specified HUD settings, falling back to default.');
          return window.api.getJsonFile(defaultJsonPath);
        });
        if (!this.hudData) {
          console.error('Failed to load default HUD settings.');
          return;
        }

        //- Set the image source and areas
        this.imageSrc = window.api.joinPath(DATA_DIRECTORY, 'HUD', this.hudData.Image);
        this.areas = this.hudData.Areas;

      } catch (error) {
        console.error('Error loading HUD settings:', error);
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

    OnArea_MouseMove(event) {
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
    OnArea_MouseLeave() {
      /* cleanup when mouse is away */
      this.clearCanvas();
      this.currentArea = null;
    },
    OnArea_Click(event, setActiveTab = true) {
      /* WHEN THE USER CLICKS AN AREA  */
      //console.log('Area clicked:', this.currentArea, setActiveTab);
      if (this.currentArea) {
        this.clickedArea = this.currentArea;        
        this.highlightClickedArea(this.currentArea);

        if (this.currentArea.title != "XML Editor") {
          EventBus.emit('areaClicked', this.currentArea); //<- Event listen in 'PropertiesTab.vue'
          if (setActiveTab === true) {
            EventBus.emit('setActiveTab', 'properties');    //<- Event listen in 'MainNavBars.vue'
          }
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

      // Colors can be in any of this formats: hex, rgba, or color names
      ctx.strokeStyle = this.hudData.Colors.BorderColor;  //<- Border Color:  'orange'
      ctx.lineWidth = this.hudData.Colors.BorderWidth;  //<- Border Width: 2
      ctx.strokeRect(area.scaledX, area.scaledY, area.scaledWidth, reducedHeight);

      // Draw area name centered below the rectangle
      ctx.fillStyle = this.hudData.Colors.FontColor; //'white';
      ctx.font = this.hudData.Colors.Font; //'14px Segoe UI';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'top';
      ctx.shadowColor = this.hudData.Colors.ShadowColor; //'rgba(0, 0, 0, 0.5)';
      ctx.shadowOffsetX = this.hudData.Colors.ShadowOffset;//2;
      ctx.shadowOffsetY = this.hudData.Colors.ShadowOffset;//2; 
      ctx.shadowBlur = this.hudData.Colors.ShadowBlur;//2;
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
      ctx.fillStyle = this.hudData.Colors.HighlightColor; // 'rgba(255, 165, 0, 0.3)';
      ctx.fillRect(area.scaledX, area.scaledY, area.scaledWidth, reducedHeight);

      // Draw bright blue border
      ctx.strokeStyle = this.hudData.Colors.HighlightBorder; // 'orange';
      ctx.lineWidth = 1;
      ctx.strokeRect(area.scaledX, area.scaledY, area.scaledWidth, reducedHeight);

      // Draw area name centered below the rectangle
      ctx.fillStyle = this.hudData.Colors.HighlightFontColor; //'white';
      ctx.font = this.hudData.Colors.Font; //'14px Segoe UI';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'top';
      ctx.shadowColor = this.hudData.Colors.HighlightShadowColor; //'rgba(0, 0, 0, 0.5)';
      ctx.shadowOffsetX = this.hudData.Colors.ShadowOffset;//2;
      ctx.shadowOffsetY = this.hudData.Colors.ShadowOffset;//2;
      ctx.shadowBlur = this.hudData.Colors.ShadowBlur;//2;
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

      this.DoLoadArea('Panel_UP');
    },

    DoLoadArea(area){
      if (this.areas && this.areas.length > 0) {
       // console.log('Areas:', this.areas);
        const index = this.areas.findIndex(obj => obj.id === area);
        if (index !== -1) {
          this.currentArea = this.areas[index];
          this.OnArea_Click(null, false);
        }
      }
    },
  },
  async mounted() {
    try {
      this.imageSrc = await window.api.getAssetFileUrl('images/HUD_Clean.png');         //console.log(this.imageSrc);
      window.addEventListener('resize', this.setupCanvas);

      EventBus.on('InitializeHUDimage', this.OnInitialize);
      EventBus.on('LoadArea', this.DoLoadArea);
    } catch (error) {
      console.log(error);
    }    
  },
  beforeUnmount() { //beforeDestroy() {
    window.removeEventListener('resize', this.setupCanvas);

    EventBus.off('InitializeHUDimage', this.OnInitialize);
    EventBus.off('LoadArea', this.DoLoadArea);
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