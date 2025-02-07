<template>
  <div ref="colorPickerContainer" class="color-picker-container"></div>
</template>

<script>
import Picker from 'vanilla-picker';
//import 'vanilla-picker/dist/vanilla-picker.csp.css';

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
      const picker = new Picker({
        parent: this.$refs.colorPickerContainer, // Or document.body if you want it to be a popup:
        popup: 'left', // Or 'right' if you want it on the right
        color: this.initialColor,
        editorFormat: 'hex', //<- 'hex' | 'hsl' | 'rgb'
        onChange: (color) => {
          const hex = color.hex;
          const rgba = color.rgba;
          this.$emit('color-changed', { rgba, hex });
          this.$refs.colorPickerContainer.style.backgroundColor = hex;
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
  min-height: 38px;
  border: 1px solid #ced4da;
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
}
</style>