<template>
  <div v-for="group in uiGroups" :key="group.Name">
    <h2>{{ group.Title }}</h2>  <div v-for="(categoryElements, category) in categorizedElements(group.Elements)" :key="category">
      <h3>{{ category }}</h3> <ul class="list-group two-columns">
        <li class="list-group-item clickable-column" v-for="element in categoryElements" :key="element.Key" @click="handleClick(element)">
          {{ element.Title }}
        </li>
        <li class="list-group-item controls-column">
          <component :is="getComponent(element.ValueType)" :key="element.Key" v-bind="getProps(element)"></component>
        </li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
/* ... (same CSS as before) ... */
</style>

<script>
import ColorPicker from './Components/ColorPicker.vue';
import Util from '../Helpers/Utils.js';
import eventBus from '../EventBus';

export default {
  name: 'UserSettingsTab',
  components: {
    ColorPicker,
  },
  data() {
    return {
      uiGroups: [], // Initialize as empty array
    };
  },
  methods: {
    loadData(newData) {
      this.uiGroups = newData; // Replace with new data
    },
    categorizedElements(elements) {
      const categorized = {};
      elements.forEach(element => {
        if (!categorized[element.Category]) {
          categorized[element.Category] = [];
        }
        categorized[element.Category].push(element);
      });
      return categorized;
    },
    handleClick(element) {
      console.log('Clicked on:', element.Title);
    },
    getComponent(valueType) {
      return this.components[valueType] || MyInput; // Default to MyInput
    },
    getProps(element) {
      if (element.ValueType === 'Color') {
        return { color: element.Value }; // Pass 'color' prop to MyColorPicker
      } else if(element.ValueType === 'Select'){
        return { options: element.Options, selected: element.Value }
      } else {
        return { value: element.Value }; // Default: pass 'value' prop
      }
    }
  }
};
</script>

<style scoped>
.two-columns {
  display: grid;
  grid-template-columns: 1fr 1fr; /* Two equal columns */
  gap: 0;
}

.clickable-column {
  cursor: pointer; /* Indicate it's clickable */
}

.controls-column{
    display: contents; /* Needed to render the dynamic components without a wrapper */
}

.controls-column > * { /* style all direct children */
    grid-column: 2; /* Place controls in the second column */
}

.tab-content {
    flex-grow: 1;
    display: flex;
    flex-direction: column;
    overflow-y: hidden;
  }
  
  .scrollable-content {
    flex-grow: 1;
    overflow-y: auto;
  }
  
  .table {
    margin: 10px 0;
    color: #f8f9fa;
    width: 100%;
  }

/* Optional: Responsive design */
@media (max-width: 768px) {
  .two-columns {
    grid-template-columns: 1fr;
  }
  .controls-column > * {
    grid-column: 1; /* Stack controls vertically */
  }
}
</style>

  