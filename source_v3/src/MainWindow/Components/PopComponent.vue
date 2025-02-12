<template>
    <div v-if="isVisible" class="pop-component" :style="popoverStyle">
      <div class="pop-header">
        <h5 class="pop-title">{{ title }}</h5>
        <button type="button" class="btn-close btn-close-white" @click="close"></button>
      </div>
      <div class="pop-body">
        <p v-if="description">{{ description }}</p>
        <img v-if="image" :src="image" alt="Popover Image" class="pop-image">
        <slot></slot> </div>
    </div>
  </template>
  
  <script>
  export default {
    props: {
      title: {
        type: String,
        required: true,
      },
      description: String,
      image: String,
      x: Number, // X position
      y: Number, // Y position
      show: Boolean,
    },
    data() {
      return {
        isVisible: this.show,
      };
    },
    computed: {
      popoverStyle() {
        return {
          left: `${this.x}px`,
          top: `${this.y}px`,
        };
      },
    },
    watch: {
      show(newVal) {
        this.isVisible = newVal;
      },
    },
    methods: {
      close() {
        this.$emit('close');
      },
    },
  };
  </script>
  
  <style scoped>
  .pop-component {
    position: absolute;
    background-color: #343a40; /* Dark mode background */
    color: white; /* Dark mode text color */
    border: 1px solid #495057; /* Dark mode border */
    border-radius: 0.5rem;
    padding: 1rem;
    box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15); /* Optional shadow */
    z-index: 1050; /* Ensure it's above other elements */
  }
  
  .pop-header {
    display: flex;
    justify-content: space-between; /* Align title and close button */
    align-items: center;
    margin-bottom: 0.5rem;
  }
  
  .pop-title {
    margin: 0;
    font-size: 1.25rem;
  }
  
  .btn-close-white{
      filter: invert(1) grayscale(100%) brightness(200%);
  }
  
  .pop-body {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
  }
  
  .pop-image {
    max-width: 100%;
    height: auto;
    margin-top: 0.5rem;
  }
  </style>