<template>
  <div class="tab-content">
    <div class="scrollable-content">
      <ul class="list-group" id="listProperties99">

        <li v-for="(category, index) in properties" :key="index" :id="'li-' + category.name" class="list-group-item">
          <!-- Name of the Category -->
          <p class="category-name">{{ category.name }}</p>
          
          <table class="table table-bordered table-dark">
            <tbody>
              <tr v-for="(item, itemIndex) in category.items" :key="itemIndex">

                <td class="tooltip-container" id="tooltip-{{ item.key }}"> <!-- The 1st Column -->
                  <!-- Name of the Item and a Tooltip with information -->
                  {{ item.title || "No title available" }}
                  <div class="custom-tooltip" :class="{ 'tooltip-top': isTooltipTop, 'tooltip-bottom': !isTooltipTop }">
                    <strong>Key:</strong> {{ item.key }}<br />
                    {{ item.description }}<br />
                    <img :src="getImagePath(item.key)" alt="Image of {{ item.key }}" style="width:270px;height:180px;"
                      @error="hideImage($event)" />
                  </div>
                </td>

                <td class="dropdown-column"> <!-- on the 2nd Column -->

                  <!-- Dynamic Preset Select Combo -->
                  <template v-if="item.valueType === 'Preset'">
                    <select class="form-select select-combo" :id="'selectPreset-' + item.key" v-model="item.value"
                      @change="OnPresetValueChange(item, $event.target.value)">
                      <option v-for="preset in getPresetsForType(item.type)" :key="preset.Name" :value="preset.Index">
                        {{ preset.Name }}
                      </option>
                    </select>
                  </template>

                  <!-- Range Slider Control -->
                  <template v-if="item.valueType === 'Brightness'">
                    <div class="range-container">
                      <input type="range" v-model="item.value" :min="getMinValue(item.type)"
                        :max="getMaxValue(item.type)" step="0.01" class="form-range range-input"
                        @input="OnBrightnessValueChange(item, $event)" style="height: 10px;" />
                      <label class="slider-value-label">{{ item.value }}</label>
                    </div>
                  </template>

                  <!-- Custom Color Picker Control -->
                  <template v-if="item.valueType === 'Color'">
                    <ColorPicker :color="item.value" @OncolorChanged="OnColorValueChange(item, $event)" />
                  </template>

                  <!-- Bootstrap 5.0 Color Picker Control
                  <template v-if="item.valueType === 'Color'">                    
                    <input type="color" :value="intToHexColorEx(item)"
                      @input="OnColorValueChange(item, $event.target.value)" 
                      class="form-control color-picker" :style="{ backgroundColor: 'transparent' }" />                     
                  </template> -->

                  <!-- On/Off Swap control -->
                  <template v-if="item.valueType === 'ONOFF'">
                    <div class="form-check form-switch">
                      <input class="form-check-input larger-switch" type="checkbox" :checked="item.value === 1"
                        @change="OnToggleValueChange(item, $event)" />
                    </div>
                  </template>

                  <!-- Add more controls based on item.valueType as needed -->
                </td>

              </tr>
            </tbody>
          </table>
        </li>

      </ul> <!-- list-group -->
    </div>
  </div>
</template>


<script>
import { inject, defineComponent, reactive, ref } from 'vue';
//import ColorPicker from './Components/ColorPicker.vue';
import ColorPicker from './Components/ColorDisplay.vue';
import Util from '../Helpers/Utils.js';
import eventBus from '../EventBus';

let themeTemplate = inject('themeTemplate');

const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]')
const popoverList = [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl))

export default defineComponent({
  name: 'PropertiesTab',
  components: {
    ColorPicker,
  },
  data() {
    return {
      themeTemplate,
      tooltipStyles: [],
      selectedColor: '#ffffff',
      componentKey: 0, // Add a key for component re-rendering
    };
  },
  setup() {
    // Reactive state
    const properties = []; //reactive([]);
    const tooltipStyles = reactive([]);
    const key = ref(0);
    const reference = ref(null);

    return {
      properties,            
    };
  },
  methods: {

    loadProperties(area) {
      //console.log('Loading properties for area:', area);
      this.properties = reactive([]);
      this.componentKey = 0; // Reset the component key
      this.$nextTick(() => {
        this.componentKey++; // Increment key to force re-render
      });
      this.updateProperties(this.getPropertiesForArea(area));
      this.key++;
    },
    updateProperties(newProperties) {
      this.properties.splice(0, this.properties.length, ...newProperties);
    },

    getPropertiesForArea(area) {
      /* Gets all  Categories and Properties of the Area */
      //console.log('Loaded Template:', themeTemplate);

      const categorizedProperties = [];
      if (themeTemplate != null && themeTemplate.ui_groups != null) {

        // 1. Look for the UI-Group of the Selected Area:
        const matchingGroups = themeTemplate.ui_groups.filter(group => group.Name === area.id);
        if (matchingGroups != null) {
          matchingGroups.forEach(group => {
            // 2. Loop all Elements in each Group:
            group.Elements.forEach(element => {
              // 3. Get the Category of each Element
              let category = categorizedProperties.find(cat => cat.name === element.Category);
              if (!category) {
                category = {
                  name: element.Category,
                  items: []
                };
                categorizedProperties.push(category);
              }

              // 4. Returns an object with the required fields:
              category.items.push({
                category: element.Category,
                title: element.Title,

                file: element.File,
                section: element.Section,
                key: element.Key,

                value: element.Value,
                valueType: element.ValueType,
                type: element.Type,
                description: element.Description,

                options: element.Options || []
              });

            });
          });
        }
      } else {
        //console.error('No Theme Loaded!');
      }

      return categorizedProperties;
    },

    /** Gets all Presets for the selected Type
     * @param type Type of Preset
     */
    getPresetsForType(type) {
      return themeTemplate.Presets.filter(preset => preset.Type === type);
    },

    /** Gets the Description of a Preset
     * @param type Type of Preset
     * @param index Value
     */
    getPresetNameByIndex(type, index) {
      const preset = themeTemplate.Presets.find(preset => preset.Type === type && preset.Index === index);
      return preset ? preset.Name : null;
    },

    /**     * Gets the Minimum Value for a Range-slider control
     * @param type Type of Range
     */
    getMinValue(type) {
      switch (type) {
        case '2X': return -1.0;
        case '4X':
          return 0.0;
        default:
          return 0.0;
      }
    },

    /**     * Gets the Maximum Value for a Range-slider control
     * @param type Type of Range
     */
    getMaxValue(type) {
      switch (type) {
        case '2X':
          return 2.0;
        case '4X':
          return 4.0;
        default:
          return 1.0;
      }
    },

    /** Gets the path for an Element Image
     * @param key The file name of the image matches the 'key' of the Element.     */
    getImagePath(key) {
      const imageKey = key.replace(/\|/g, '_');
      return new URL(`../images/Elements_ODY/${imageKey}.png`, import.meta.url).href;
    },
    hideImage(event) {
      event.target.style.display = 'none';
    },

    /* METHODS TO UPDATE CHANGES IN THE CONTROLS  */
    OnPresetValueChange(item, value) {
      item.value = value;
      this.UpdateValueToTemplate(item);
    },
    OnBrightnessValueChange(item, event) {
      item.value = parseFloat(event.target.value);
      this.UpdateValueToTemplate(item);
    },
    OnColorValueChange(item, colorData) {
      const hex = colorData.hex; //console.log(colorData.rgba);
      const intValue = Util.hexToSignedInt(hex);
      //console.log('HEX:' + hex + ', INT: ' + intValue);
      if (item.value !== intValue) { // This is the KEY FIX
        let modif = JSON.parse(JSON.stringify(item));
        modif.value = intValue; 
        this.UpdateValueToTemplate(modif); // Call this only if value changed!
      }
    },
    OnToggleValueChange(item, event) {
      // Toggle the boolean value
      const value = event.target.checked ? 1 : 0; 
      item.value = value;
      this.UpdateValueToTemplate(item);
    },
    UpdateValueToTemplate(item) {
      /* Here the changes in controls are stored back into the 'themeTemplate' object  */
      themeTemplate.ui_groups.forEach(group => {
        if (group && group.Elements) {
          const element = group.Elements.find(el => el.Key === item.key);
          if (element) {
            element.Value = parseFloat(item.value.toFixed(1)); // item.value;
            //console.log(`Updated! Key: '${item.key}', Value: '${item.value}''`);
          }
        }
      });
      this.$emit('onThemeChanged', JSON.parse(JSON.stringify(themeTemplate))); //<- Listened on NavBars.vue
    },

    OnThemeLoaded(item) {
      //console.log('On_ThemeLoaded', item)
      themeTemplate = item;
    },
    /** When a Category is Selected, it scrolls to the selected Category     * 
     * @param category Name of the Category
     */
    OnSelectCategory(category) {
      //console.log('Category Selected:', category);
      try {
        this.$nextTick(() => {
          const selectedElement = document.getElementById('li-' + category);
          if (selectedElement) {
            selectedElement.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
          }
        });
      } catch (error) {
        eventBus.emit('ShowError', error);        
      }
    },

  },
  mounted() {
    eventBus.on('areaClicked', this.loadProperties);
    eventBus.on('ThemeLoaded', this.OnThemeLoaded);
    eventBus.on('OnSelectCategory', this.OnSelectCategory);
  },
  beforeUnmount() {
    eventBus.off('areaClicked', this.loadProperties);
    eventBus.off('ThemeLoaded', this.OnThemeLoaded);
    eventBus.off('OnSelectCategory', this.OnSelectCategory);
  },
});
</script>

<style scoped>

.category-name {
  color: darkorange;
  font-size: 18px;
  font-weight: bold;
}

.tab-content {
  overflow-y: auto;
  max-height: 100%;
  width: 100%;
  background-color: #212529;
}


.scrollable-content {
  flex-grow: 1;
  overflow-y: auto;
  font-family: 'Segoe UI';
  font-size: 12px;
  scrollbar-width: thin;
  scrollbar-color: #2c2c2c #121212;
}
.scrollable-content::-webkit-scrollbar {
  width: 8px;
}
.scrollable-content::-webkit-scrollbar-thumb {
  background-color: #2c2c2c;
  border-radius: 4px;
}
.scrollable-content::-webkit-scrollbar-track {
  background-color: #121212;
}

.list-group-item {
  padding: 2px 2px;
  background-color: transparent;
  border: none;
  font-family: 'Segoe UI';
  font-size: 14px;
  color: orange;
}

.table {
  margin: 4px 0;
  color: #f8f9fa;
  width: 100%;
}

.dropdown-column {
  width: 50%;  /* Set width to half the table's width */
}
.dropdown-button {
  width: 200px;
  padding: 0.25rem 0.5rem;
  text-align: right;
}
.dropdown-menu {
  width: 100%;  /* Make the dropdown menu fill the column width */
}

/* ----------------------------------------------------------*/
/* Slider controls on the Properties Tab */
.range-container {
  position: relative;
  width: 100%;
  height: 20px;
}

.slider-value-label {
  display: block;
  margin-top: 2px;
  font-size: 12px;
  color: #f8f9fa;
  text-align: left;
}

/* ----------------------------------------------------------*/


/* ----------------------------------------------------------*/

.form-check-input.larger-switch {
  transform: scale(2.0);
  position: relative;
  left: 20px;
  top: 4px;
}

.category-group {
  margin-bottom: 10px;
  /* Reduce margin between categories */
}


/* ----------------------------------------------------------*/
/* Styles for the Select Combos */
.select-combo {
  background-color: #343a40;
  color: #f8f9fa;
  border: 1px solid #6c757d;
  border-radius: 5px;
  height: 2rem;
  padding: 0.25rem 0.5rem;
  font-size: 0.875rem;
}

.select-combo:hover,
.select-combo:focus {
  background-color: #495057;  /* Slightly lighter dark background on hover/focus */
  border-color: #adb5bd;  /* Lighter border color on hover/focus */
}

/* ----------------------------------------------------------*/
/* Tooltip Styles */
.tooltip-container {
  position: relative;
  display: inline-block;
  width: 230px;
  height: 50px;
}
.tooltip-container:hover .custom-tooltip {
  visibility: visible;
  opacity: 1;
}
.custom-tooltip {
  visibility: hidden;
  width: 350px;
  background-color: #343a40;
  color: #f8f9fa;
  text-align: left;
  border: 1px solid #ff8c00;
  border-radius: 5px;
  padding: 10px;
  position: absolute;
  z-index: 1;
  top: 100%;
  left: 90%;
  transform: translateX(-50%);
  opacity: 0;
  transition: opacity 0.3s;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);  /* Add a subtle shadow for better visibility */
}

</style>