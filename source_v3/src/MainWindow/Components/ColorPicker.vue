<template>
  <div ref="colorPickerContainer" class="color-picker-container"></div>
</template>

<script>
import Picker from './vanilla-picker.csp.mjs';
import './vanilla-picker.csp.css';
import Util from '../../Helpers/Utils.js';

export default {
  props: {
    elementData: null,
  },
  data() {
    return {
      pickerInstance: null,
      currentColor: ''
    };
  },
  methods: {
    initializePicker() {
      try {
        console.log('elementData:',this.elementData);
        this.currentColor = Util.intToHexColor(this.elementData.Value);
        console.log('Ini-Color: ' + this.elementData.Key, this.currentColor);

        const picker = new Picker({
          parent: this.$refs.colorPickerContainer,
          popup: 'middle',        //<- 'top' | 'bottom' | 'left' | 'right' | 'middle' | false
          editorFormat: 'hex',  //<- 'hex' | 'hsl' | 'rgb'
          alpha: true,
          editor: true,
          color: this.currentColor,     //'rgba(255,0,0, 1)', #FF0000FF  
          cancelButton: false,
          okButton: false,

          onChange: (color) => {
            console.log('ColorChanged: ', color.rgba);
            this.drawTransparentColor(color);
            this.$emit('color-changed', { rgba: color.rgba, hex: color.hex });            
          },

        });
        //picker.setColour(this.currentColor, false);
        //this.pickerInstance = picker;

      } catch (error) {
        console.log(error);
      }
    },
    drawTransparentColor(color) {
      try {
        const canvas = document.createElement('canvas');
        const container = this.$refs.colorPickerContainer;
        canvas.width = container.clientWidth;
        canvas.height = container.clientHeight;
        const ctx = canvas.getContext('2d');

        // Draw checkerboard pattern for transparency indication
        console.log('Drawing Transparency Grid..');
        const size = 4;
        for (let y = 0; y < canvas.height; y += size) {
          for (let x = 0; x < canvas.width; x += size) {
            ctx.fillStyle = (x / size + y / size) % 2 === 0 ? '#fff' : '#ccc';
            ctx.fillRect(x, y, size, size);
          }
        }

        // Draw the color layer with proper alpha:
        console.log('Drawing Color Layer..');
        ctx.fillStyle = color.hex;
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // 3. Draw text (top layer)
        console.log('Drawing Color Label..');
        const text = 'rgba(' + color.rgba + ')';
        if (text) { // Only draw text if provided
          ctx.font = '14px Arial'; // Customize font

          // Determine if the color is "dark" or "light"
          const isDarkColor = Util.isColorDark(color.hex); // Helper function (see below)
          const textColor = isDarkColor ? '#fff' : '#000'; // White for dark, black for light

          ctx.fillStyle = textColor; // Set text color based on background
          ctx.textAlign = 'center';
          ctx.textBaseline = 'middle';

          const textMetrics = ctx.measureText(text);
          const textWidth = textMetrics.width;
          const x = canvas.width / 2;
          const y = canvas.height / 2;


          /*  // Optional: Background rectangle (with dynamic color)
            const rectColor = isDarkColor ? 'rgba(0, 0, 0, 0.5)' : 'rgba(255, 255, 255, 0.5)';
            ctx.fillStyle = rectColor;
            ctx.fillRect(x - textWidth / 2 - 5, y - 10, textWidth + 10, 20); */

          ctx.fillStyle = textColor; // Reset to text color
          ctx.fillText(text, x, y);

        }

        container.style.backgroundImage = `url(${canvas.toDataURL()})`;
      } catch (error) {
        console.log(error);
      }
    },

  },
  async mounted() {
    this.initializePicker();
    /*this.$nextTick(() => {
      this.initializePicker();
    });*/
  },
  beforeUnmount() {
    if (this.pickerInstance) {
      this.pickerInstance.destroy();
    }
  }
};
</script>


<style scoped>
.color-picker-container {
  width: 100%;
  height: 100%;
  min-height: 34px;
  border: 1px solid #ced4da;
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  background-size: 100% 100%
}

.color-canvas {
  width: 100%;
  height: 100%;
  position: absolute;
  top: 0;
  left: 0;
  z-index: -1;
  /* Ensure it is behind the color picker */
}
</style>