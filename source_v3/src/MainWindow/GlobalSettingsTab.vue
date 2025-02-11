<!-- Global Settings Tab -->
<template>
  <div class="data-table" ref="dataTable" v-if="groupedElements.length > 0">
    <div v-for="(group, index) in groupedElements" :key="index">
      <p class="category-name">{{ group.category }}</p>
      <table class="table table-bordered">
        <tbody>
          <tr v-for="(element, idx) in group.elements" :key="idx">
            <td class="fixed-width title-cell" @click="showPopover(element, $event)">{{ element.Title }}</td>
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
               <!-- {{ element.Value + '->' + intToRGBAstring(element.Value) }} -->
                <ColorPicker :id="'colorPreset-' + element.Key" :elementData="element" @color-changed="OnColorValueChange(element, $event)" />
              </template>

            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import { reactive, ref } from 'vue';
import ColorPicker from './Components/ColorPicker.vue';
import Util from '../Helpers/Utils.js';
import eventBus from '../EventBus';

const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]')
const popoverList = [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl))

export default {
  name: 'GlobalSettingsTab',
  components: {
    ColorPicker,
  },
  data() {
    return {
      dataSource: null,
      groupedElements: [],
      presets: [],
      activePopover: null,
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
          grouped[element.Category].push(element);
        });

        this.groupedElements = Object.keys(grouped).map(category => ({
          category,
          elements: grouped[category],
        }));

        this.presets = this.dataSource.Presets;

        this.componentKey = 0; // Reset the component key
        this.$nextTick(() => {
          this.componentKey++; // Increment key to force re-render
        });
      }
    },
    onRowClick(element) {
      console.log('Row clicked', element);
      // You can add your popup logic here
    },
    getInputComponent(element) {
      if (element.ValueType === 'Preset') {
        return 'SelectInput'
      }
      // Add other input types as needed
      return 'DefaultInput'
    },

    showPopover(element, event) {
      // 1. Hide any existing popover
      if (this.activePopover) {
        this.activePopover.dispose();
        this.activePopover = null;
      }

      // 2. Create and show the new popover
      const popover = new bootstrap.Popover(event.target, {
        title: element.Title + ' (' + element.Key + ')' ,
        content: `<p>${element.Description}</p>`,
        html: true,
        trigger: 'manual',
        placement: 'right',
        container: 'body',
        template: `
          <div class="popover border border-warning" role="tooltip">
            <div class="popover-arrow bg-warning"></div>
            <h3 class="popover-header"></h3>            
            <div class="popover-body"></div>
          </div>
        `
      });

      popover.show();
      this.activePopover = popover;

      // 3. Attach event listener for the close button (after popover is fully in DOM)
      const shownListener = () => {
        const popoverElement = document.querySelector('.popover'); // Select the popover element
        if (popoverElement) {
          const closeButton = popoverElement.querySelector('.btn-close');
          if (closeButton) {
            closeButton.addEventListener('click', () => {
              popover.dispose();
              this.activePopover = null;
            });
          }
        }
        event.target.removeEventListener('shown.bs.popover', shownListener); // Remove the listener after it's executed
      };
      event.target.addEventListener('shown.bs.popover', shownListener);

      // 4. Robust click outside check (improved)
      const outsideClickListener = (clickEvent) => {
        if (this.activePopover &&
          !event.target.contains(clickEvent.target) &&
          (clickEvent.target.closest('.popover') === null)) { // Simplified check
          this.activePopover.dispose();
          this.activePopover = null;
          document.removeEventListener('click', outsideClickListener);
        }
      };

      document.addEventListener('click', outsideClickListener);

      // 5. Hidden event listener (important for cleanup)
      event.target.addEventListener('hidden.bs.popover', () => {
        document.removeEventListener('click', outsideClickListener);
        this.activePopover = null;
      });
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
      const intValue = Util.hexToSignedInt(hex);
      //console.log('HEX:' + hex + ', INT: ' + intValue);
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
  cursor: pointer;
  /* Indicate it's clickable */
  font-size: 14px;
}

.popover-image {
  max-width: 200px;
  /* Adjust as needed */
  height: auto;
}
</style>