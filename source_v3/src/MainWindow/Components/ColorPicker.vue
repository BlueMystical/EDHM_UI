<template>
    <div class="color-picker">
      <input type="color" v-model="selectedColor" @input="updateFromInput">
      <div class="sliders">
        <div class="slider-row">
          <label for="red-slider">Red:</label>
          <input type="range" id="red-slider" min="0" max="255" v-model="red" @input="updateFromSliders">
        </div>
        <div class="slider-row">
          <label for="green-slider">Green:</label>
          <input type="range" id="green-slider" min="0" max="255" v-model="green" @input="updateFromSliders">
        </div>
        <div class="slider-row">
          <label for="blue-slider">Blue:</label>
          <input type="range" id="blue-slider" min="0" max="255" v-model="blue" @input="updateFromSliders">
        </div>
      </div>
      <div class="preview" :style="{ backgroundColor: selectedColor }"></div>
    </div>
  </template>
  
  <script>
  export default {
    data() {
      return {
        selectedColor: '#ffffff',
        red: 255,
        green: 255,
        blue: 255,
      };
    },
    methods: {
      updateFromInput() {
        const hex = this.selectedColor.substring(1); // Remove '#'
        this.red = parseInt(hex.substring(0, 2), 16);
        this.green = parseInt(hex.substring(2, 4), 16);
        this.blue = parseInt(hex.substring(4, 6), 16);
      },
      updateFromSliders() {
        this.selectedColor = `#<span class="math-inline">\{this\.red\.toString\(16\)\.padStart\(2, '0'\)\}</span>{this.green.toString(16).padStart(2, '0')}${this.blue.toString(16).padStart(2, '0')}`;
      },
    },
  };
  </script>
  
  <style scoped>
  /* Add your CSS styles for the component here */
  .color-picker {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 20px;
  }
  
  .slider-row {
    display: flex;
    align-items: center;
    margin-bottom: 10px;
  }
  
  .preview {
    width: 100px;
    height: 50px;
    margin-top: 10px;
    border: 1px solid #ccc;
  }
  </style>