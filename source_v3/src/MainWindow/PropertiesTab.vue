<template>
  <div class="tab-content">
    <div class="scrollable-content">
      <ul class="list-group">
        <li v-for="(category, index) in properties" :key="index" class="list-group-item">
          <!-- Name of the Category -->
          <p class="category-name">{{ category.name }}</p>
          
          <table class="table table-bordered table-dark">
            <tbody>
              <tr v-for="(item, itemIndex) in category.items" :key="itemIndex">
                
                <td class="tooltip-container"> <!-- The 1st Column -->

                  <!-- Name of the Item and a Tooltip with information -->
                  {{ item.title || "No title available" }}
                  <div class="custom-tooltip">
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
                      @change="updatePresetValue(item, $event.target.value)">
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
                        @input="updateBrightnessValue(item, $event)" style="height: 10px;" />
                      <label class="slider-value-label">{{ item.value }}</label>
                    </div>
                  </template>

                  <!-- Color Picker Control -->
                  <template v-if="item.valueType === 'Color'">
                    <input type="color" :value="intToHexColor(item.value)"
                      @input="updateColorValue(item, $event.target.value)" class="form-control color-picker"
                      :style="{ backgroundColor: 'transparent' }" />
                  </template>

                  <!-- On/Off Swap control -->
                  <template v-if="item.valueType === 'ONOFF'">
                    <div class="form-check form-switch">
                      <input class="form-check-input larger-switch" type="checkbox" :checked="item.value === 1"
                        @change="toggleOnOffValue(item)" />
                    </div>
                  </template>

                  <!-- Add more controls based on item.valueType as needed -->
                </td>
              </tr>
            </tbody>
          </table>
        </li>
      </ul>
    </div>
  </div>
</template>


<script>
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import eventBus from '../EventBus';
import { inject, defineComponent, reactive, onMounted } from 'vue';
import { ref } from 'vue';


export default defineComponent({
  name: 'PropertiesTab',
  setup() {
    const themeTemplate = inject('themeTemplate');
    const tooltipStyles = reactive([]);
    const properties = reactive([]);
    const key = ref(0);
    const reference = ref(null);


    /**
     * Gets all Presets for the selected Type
     * @param type Type of Preset
     */
    const getPresetsForType = (type) => {
      return themeTemplate.Presets.filter(preset => preset.Type === type);
    };

    /**
     * Gets the Description of a Preset
     * @param type Type of Preset
     * @param index Value
     */
    const getPresetNameByIndex = (type, index) => {
      const preset = themeTemplate.Presets.find(preset => preset.Type === type && preset.Index === index);
      return preset ? preset.Name : null;
    };

    /**
     * Gets the Minimum Value for a Range-slider control
     * @param type Type of Range
     */
    const getMinValue = (type) => {
      switch (type) {
        case '2X':
        case '4X':
          return 0.0;
        default:
          return 0.0;
      }
    };

    /**
     * Gets the Maximum Value for a Range-slider control
     * @param type Type of Range
     */
    const getMaxValue = (type) => {
      switch (type) {
        case '2X':
          return 2.0;
        case '4X':
          return 4.0;
        default:
          return 1.0;
      }
    };

    /* METHODS TO UPDATE CHANGES IN THE CONTROLS  */
    const updatePresetValue = (item, value) => {
      item.value = value;
      saveToThemeTemplate(item);
    };
    const updateBrightnessValue = (item, event) => {
      item.value = parseFloat(event.target.value);
      saveToThemeTemplate(item);
    };
    const updateColorValue = (item, color) => {
      item.value = parseInt(color.slice(1), 16);
      saveToThemeTemplate(item);
    };
    const toggleOnOffValue = (item) => {
      // Toggle the boolean value
      item.value = item.value === 1 ? 0 : 1;
      saveToThemeTemplate(item);
    };
    const saveToThemeTemplate = (item) => {
      /* Here the changes in controls are stored back into the 'themeTemplate' object  */
      themeTemplate.ui_groups.forEach(group => {
        if (group && group.Elements) {
          const element = group.Elements.find(el => el.Key === item.key);
          if (element) {
            element.Value = item.value;
            console.log(`Updated! Key: '${item.key}', Value: '${item.value}''`);
          }
        }
      });
    };


    /**
     * Convert a Color from Number to a Hex HTML string.
     * @param number Integer Value representing a Color.
     */
    const intToHexColor = (number) => {
      const colorString = (number & 0x00FFFFFF).toString(16).padStart(6, '0');
      return `#${colorString}`;
    };

    /**
     * Gets the path for an Element Image
     * @param key The file name of the image matches the 'key' of the Element.
     */
    const getImagePath = (key) => {
      const imageKey = key.replace(/\|/g, '_');
      return new URL(`../images/Elements_ODY/${imageKey}.png`, import.meta.url).href;
    };

    const hideImage = (event) => {
      event.target.style.display = 'none';
    };

    onMounted(() => {
      //checkImagesExistence();
    });

    return {
      themeTemplate,
      properties,
      tooltipStyles,
      getImagePath, hideImage,
      getPresetsForType,
      getPresetNameByIndex,
      getMinValue,
      getMaxValue,
      updatePresetValue,
      updateBrightnessValue,
      updateColorValue,
      toggleOnOffValue,
      intToHexColor,
    };
  },
  data() {
    return {
      tooltipStyles: []
    };
  },
  mounted() {
    eventBus.on('areaClicked', this.loadProperties);
  },
  beforeUnmount() {
    eventBus.off('areaClicked', this.loadProperties);
  },
  methods: {
    loadProperties(area) {
      console.log('Loading properties for area:', area);
      this.updateProperties(this.getPropertiesForArea(area));
      this.key++;
    },
    updateProperties(newProperties) {
      this.properties.splice(0, this.properties.length, ...newProperties);
    },
    getPropertiesForArea(area) {
      /* Gets all the properties of the indicated Category */
      const themeTemplate = this.themeTemplate;
      const matchingGroups = themeTemplate.ui_groups.filter(group => group.Name === area.id);
      const categorizedProperties = [];

      matchingGroups.forEach(group => {
        group.Elements.forEach(element => {
          let category = categorizedProperties.find(cat => cat.name === element.Category);

          if (!category) {
            category = {
              name: element.Category,
              items: []
            };
            categorizedProperties.push(category);
          }

          // Returns an object with the required fields:
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

      return categorizedProperties;
    },
    updateBrightnessValue(item, event) {
      // Updates the Slider value on the little label over the slider
      item.value = event.target.value;
    },

  }
});
</script>

<style scoped>
.category-name {
  color: darkorange;
  font-size: 18px;
  font-weight: bold;
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
  font-family: 'Segoe UI';
  font-size: 12px;
  scrollbar-width: thin;
  /* For Firefox */
  scrollbar-color: #2c2c2c #121212;
  /* For Firefox */
}

.scrollable-content::-webkit-scrollbar {
  width: 8px;
  /* Adjust width to make it thin */
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
  /* Reduce padding */
  background-color: transparent;
  /* Make background transparent */
  border: none;
  /* Optional: remove border */
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
  width: 50%;
  /* Set width to half the table's width */
}

.dropdown-button {
  width: 200px;
  /* Set fixed width */
  padding: 0.25rem 0.5rem;
  /* Make it thinner */
  text-align: right;
  /* Align text to the right */
}

.dropdown-menu {
  width: 100%;
  /* Make the dropdown menu fill the column width */
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
.color-picker {
  width: 100%;
  /* Ensure the color picker fills the entire column */
  height: 36px;
  /* Match the height of the other inputs */
  padding: 0.25rem 0.5rem;
  background-color: transparent;
  /* Set background color to transparent */
}

.color-picker-container {
  position: relative;
}

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
  /* Dark background */
  color: #f8f9fa;
  /* Light text color */
  border: 1px solid #6c757d;
  /* Border color */
  border-radius: 5px;
  /* Rounded corners */
  height: 2rem;
  /* Make it thin */
  padding: 0.25rem 0.5rem;
  /* Padding */
  font-size: 0.875rem;
  /* Font size */
}

.select-combo:hover,
.select-combo:focus {
  background-color: #495057;
  /* Slightly lighter dark background on hover/focus */
  border-color: #adb5bd;
  /* Lighter border color on hover/focus */
}

/* ----------------------------------------------------------*/
/* Tooltip Styles */
.tooltip-container {
  position: relative;
  display: inline-block;
  width: 230px;
  height: 50px;
}

.custom-tooltip {
  visibility: hidden;
  width: 350px;
  background-color: #343a40;
  /* Dark background */
  color: #f8f9fa;
  /* Clear text color */
  text-align: left;
  border: 1px solid #ff8c00;
  /* Dark-orange border */
  border-radius: 5px;
  padding: 10px;
  position: absolute;
  z-index: 1;
  top: 100%;
  /* Position the tooltip below the text */
  left: 90%;
  transform: translateX(-50%);
  /* Center align the tooltip */
  opacity: 0;
  transition: opacity 0.3s;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
  /* Add a subtle shadow for better visibility */
}

.tooltip-container:hover .custom-tooltip {
  visibility: visible;
  opacity: 1;
}
</style>