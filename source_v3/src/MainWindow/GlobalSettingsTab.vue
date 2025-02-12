<!-- Global Settings Tab -->
<template>
  <div class="data-table" ref="dataTable" v-if="groupedElements.length > 0">
    <div v-for="(group, groupIndex) in groupedElements" :key="groupIndex">
      <p class="category-name">{{ group.category }}</p>
      <table class="table table-bordered">
        <tbody>
          <tr v-for="(element, elementIndex) in group.elements" :key="elementIndex"
              @mouseover="showIcon(groupIndex, elementIndex)" @mouseleave="hideIcon(groupIndex, elementIndex)">
            <td class="fixed-width title-cell">
              {{ element.Title }}
              <span class="info-icon" v-show="element.iconVisible" @mouseover="showPopover(element, $event)" @mouseleave="hidePopover">
                <i class="bi bi-info-circle text-info"></i>
              </span>
            </td>

            <td class="fixed-width cell-content">

              <!-- Dynamic Preset Select Combo -->
              <template v-if="element.ValueType === 'Preset'">
                <select class="form-select select-combo" :id="'selectPreset-' + element.Key" v-model="element.Value"
                  @change="OnPresetValueChange(element, $event)">
                  <option v-for="preset in getPresetsForType(element.Type)" :key="preset.Name" :value="preset.Index">
                    {{ preset.Name }}
                  </option>
                </select>
              </template>

              <!-- Range Slider Control -->
              <template v-if="element.ValueType === 'Brightness'">
                <div class="range-container">
                  <input type="range" v-model="element.Value" :min="getMinValue(element.Type)"
                    :max="getMaxValue(element.Type)" step="0.01" class="form-range range-input"
                    @input="OnBrightnessValueChange(element, $event)" style="height: 10px;" />
                  <label class="slider-value-label">{{ element.Value }}</label>
                </div>
              </template>

              <!-- On/Off Swap control -->
              <template v-if="element.ValueType === 'ONOFF'">
                <div class="form-check form-switch">
                  <input class="form-check-input larger-switch" type="checkbox" :checked="element.Value === 1"
                    @change="OnToggleValueChange(element, $event)" />
                </div>
              </template>

              <!-- Custom Color Picker Control -->
              <template v-if="element.ValueType === 'Color'">
                <ColorDisplay :id="'colorPreset-' + element.Key" :color="element.Value" @OncolorChanged="OnColorValueChange(element, $event)"></ColorDisplay>
              </template>

            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import ColorDisplay from './Components/ColorDisplay.vue'; 
import Util from '../Helpers/Utils.js';
import eventBus from '../EventBus';

const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
const popoverList = [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl));


export default {
  name: 'GlobalSettingsTab',
  components: {
    ColorDisplay
  },
  data() {
    return {
      dataSource: null,
      groupedElements: [], // Initialize as an empty array
      presets: [],
      activePopover: null,
      popoverElement: null, // Store the popover element
      componentKey: 0, // Add a key for component re-rendering
    };
  },
  watch: {
    dataSource: {
      immediate: true,
      handler() {
        this.loadGroupData()
      }
    }
  },
  methods: {
    async OnInitialize() {
      this.dataSource = await window.api.LoadGlobalSettings();
      //console.log('Loaded DataSource:', this.dataSource);
      this.loadGroupData();
    },
    loadGroupData() {
      if (this.dataSource) {
        this.groupedElements = []; // Clear existing elements FIRST
        const grouped = {};

        this.dataSource.Elements.forEach(element => {
          if (!grouped[element.Category]) {
            grouped[element.Category] = [];
          }

          grouped[element.Category].push({
            ...element,
            iconVisible: false, // Add iconVisible property
          });
        });

        this.groupedElements = Object.keys(grouped).map(category => ({
          category,
          elements: grouped[category],
        }));

      }
    },
    showIcon(groupIndex, elementIndex) {
      // Create a *new* groupedElements array with the updated iconVisible:
      const newGroupedElements = this.groupedElements.map((group, gIndex) => {
        if (gIndex === groupIndex) {
          return {
            ...group,
            elements: group.elements.map((element, eIndex) => {
              if (eIndex === elementIndex) {
                return { ...element, iconVisible: true };
              }
              return element;
            }),
          };
        }
        return group;
      });

      this.groupedElements = newGroupedElements; // Update with the new array
    },
    hideIcon(groupIndex, elementIndex) {
      // Create a *new* groupedElements array with the updated iconVisible:
      const newGroupedElements = this.groupedElements.map((group, gIndex) => {
        if (gIndex === groupIndex) {
          return {
            ...group,
            elements: group.elements.map((element, eIndex) => {
              if (eIndex === elementIndex) {
                return { ...element, iconVisible: false };
              }
              return element;
            }),
          };
        }
        return group;
      });

      this.groupedElements = newGroupedElements; // Update with the new array
    },
    showPopover(element, event) {
      if (this.activePopover) {
        this.activePopover.dispose();
        this.activePopover = null;
      }

      this.$nextTick(() => {
        const popover = new bootstrap.Popover(event.target, {
          title: `${element.Title} (${element.Key})`,
          content: `<p>${element.Description}</p>`,
          html: true,
          trigger: 'manual', // Important: trigger is manual
          placement: 'right',
          container: 'body',
          template: `
          <div class="popover border border-warning" role="tooltip">
            <div class="popover-arrow"></div>
            <h4 class="popover-header"></h3>
            <div class="popover-body"></div>
          </div>
        `,
        });

        popover.show();
        this.activePopover = popover;

      /*  const popoverElement = document.querySelector('.popover');
        if (popoverElement) {
          const closeButton = popoverElement.querySelector('.btn-close');
          if (closeButton) {
            closeButton.addEventListener('click', () => {
              popover.dispose();
              this.activePopover = null;
            });
          }
        }*/

        event.target.addEventListener('hidden.bs.popover', () => {
          this.activePopover = null;
        });
      });
    },
    hidePopover() {
      if (this.activePopover) {
        this.activePopover.dispose();
        this.activePopover = null;
      }
    },

    /** Gets all Presets for the selected Type
     * @param type Type of Preset
     */
    getPresetsForType(type) {
      return this.presets.filter(preset => preset.Type === type);
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
    intToRGBAstring(value){
      return Util.intToRGBAstring(value);
    },

    /* METHODS TO UPDATE CHANGES IN THE CONTROLS  */
    OnPresetValueChange(item, event) {
      const value = parseFloat(event.target.value);
      console.log(item, value);
      //this.UpdateValueToTemplate(item);
    },
    OnBrightnessValueChange(item, event) {
      const value = parseFloat(event.target.value);
      console.log(item, value);
      //this.UpdateValueToTemplate(item);
    },
    OnToggleValueChange(item, event) {
      const value = event.target.checked ? 1 : 0; 
      console.log(item, value); //item.value = item.value === 1 ? 0 : 1;
      //this.UpdateValueToTemplate(item);
    },
    OnColorValueChange(item, event) {
      const hex = event.hex; //console.log(event.rgba);
      const intValue = event.int; // Util.hexToSignedInt(hex);
      //console.log(event);
      if (item.value !== intValue) { // This is the KEY FIX
        let modif = JSON.parse(JSON.stringify(item));
        modif.value = intValue; 
        //this.UpdateValueToTemplate(modif); // Call this only if value changed!
      }
    },

  },
  mounted() {
    eventBus.on('DoLoadGlobalSettings', this.OnInitialize);
  },
  beforeUnmount() {
    eventBus.off('DoLoadGlobalSettings', this.OnInitialize);
    this.$nextTick(() => {
      if (this.activePopover) {
        this.activePopover.dispose();
        this.activePopover = null;
      }
    })
  },
}
</script>

<style scoped>
.data-table {
  width: 100%;
  height: 100%;
  overflow-y: auto;
}

.table {
  width: 100%;
  margin-bottom: 1rem;
  color: #212529;
}


.fixed-width {
  width: 50%;
}

.category-name {
  color: darkorange;
  font-size: 18px;
  font-weight: bold;
}

.title-cell {
  cursor: pointer;  /* Indicate it's clickable */
  font-size: 14px;
  color: lightgrey;

  align-items: center; /* Vertically center content */
  padding: 0.5rem; /* Add some padding for better visual spacing */
}
.info-icon {
  margin-left: 0.5rem; /* Space between text and icon */
  cursor: pointer; /* Indicate icon is clickable */
}

.popover-image {
  max-width: 200px;
  height: auto;
}
/* Style the popover arrow */
.popover[data-popper-placement^="right"] > .popover-arrow::before,
.popover[data-popper-placement^="right"] > .popover-arrow::after {
  border-left-color: orange !important;
}

</style>