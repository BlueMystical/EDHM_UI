<template>
  <div ref="colorPickerContainer" class="color-picker-container"></div>
</template>

<script>
import Picker from 'vanilla-picker';
import 'vanilla-picker/dist/vanilla-picker.csp.css';

export default {
  props: {
    initialColor: {
      type: String,
      default: '#000000'
    }
  },
  data() {
    return {
      pickerInstance: null,
    };
  },
  mounted() {
    this.$nextTick(() => {
      this.initializePicker();
    });
  },
  watch: {
    initialColor(newColor) {
      if (this.pickerInstance) {
        this.pickerInstance.setColor(newColor);
      }
    }
  },
  methods: {
    initializePicker() {
      console.log('Picker Ini-Color: ', this.initialColor);
      const picker = new Picker({
        parent: this.$refs.colorPickerContainer,
        popup: 'left', //<- 'top' | 'bottom' | 'left' | 'right' | false
        editorFormat: 'hex', //<- 'hex' | 'hsl' | 'rgb'
        alpha: true,
        editor: true,
        color: this.initialColor,     //'rgba(255,0,0, 1)',   
        onChange: (color) => {
          console.log('Picker Color: ', color.rgba);
          const hex = color.hex;
          const rgba = color.rgba;
          this.$emit('color-changed', { rgba, hex });
          this.drawTransparentColor(rgba);
        },
        onOpen: (color, instance) => {
          // Use querySelectorAll to get all picker elements and apply transform to each
          const pickerDropdowns = document.querySelectorAll('.picker_wrapper');
          pickerDropdowns.forEach(pickerDropdown => {
            pickerDropdown.style.transform = `translateX(${50}px)`; // Apply transform
            pickerDropdown.style.zIndex = '1050'; // Ensure it's on top
          });
        },
        onClose: () => {
          // Use querySelectorAll to reset transform for all picker elements
          const pickerDropdowns = document.querySelectorAll('.picker_wrapper');
          pickerDropdowns.forEach(pickerDropdown => {
            pickerDropdown.style.transform = ''; // Reset transform
          });
        }
      });

      this.pickerInstance = picker;
    },
    drawTransparentColor(rgba) {
      const canvas = document.createElement('canvas');
      const container = this.$refs.colorPickerContainer;
      canvas.width = container.clientWidth;
      canvas.height = container.clientHeight;
      const ctx = canvas.getContext('2d');

      // Draw checkerboard pattern for transparency indication
      const size = 4;
      for (let y = 0; y < canvas.height; y += size) {
        for (let x = 0; x < canvas.width; x += size) {
          ctx.fillStyle = (x / size + y / size) % 2 === 0 ? '#fff' : '#ccc';
          ctx.fillRect(x, y, size, size);
        }
      }

      // Draw the color layer with proper alpha
      ctx.fillStyle = `rgba(${rgba[0]}, ${rgba[1]}, ${rgba[2]}, ${rgba[3]})`;
      ctx.fillRect(0, 0, canvas.width, canvas.height);

      container.style.backgroundImage = `url(${canvas.toDataURL()})`;
    },

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
  z-index: -1;  /* Ensure it is behind the color picker */
}
</style>