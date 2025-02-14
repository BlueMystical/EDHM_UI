<!-- Global Settings Tab -->
<template>
  <div class="data-table table-responsive" ref="dataTable" v-if="groupedElements.length > 0">
    <div v-for="(group, groupIndex) in groupedElements" :key="groupIndex">
      <p class="category-name">{{ group.category }}</p>
      <table class="table table-bordered table-hover align-middle">
        <tbody>

          <!-- Table Row -->
          <tr v-for="(element, elementIndex) in group.elements" :key="elementIndex" :id="'row-' + element.Key"
            @mouseover="showIcon(groupIndex, elementIndex)" @mouseleave="hideIcon(groupIndex, elementIndex)"
            @click="selectRow(element.Key)" :class="rowClass(element)"            
            @contextmenu="onRightClick($event, element)"> <!-- @contextmenu.prevent="showContextMenu(element, $event)"> -->

            <!-- Left Column -->
            <td class="fixed-width title-cell" @contextmenu.prevent="showContextMenu(element, $event)">
              {{ element.Title }}
              <span class="info-icon" v-show="element.iconVisible" @mouseover="showPopover(element, $event)"
                @mouseleave="hidePopover">
                <i class="bi bi-info-circle text-info"></i>
              </span>
            </td>

            <!-- Right Column -->
            <td class="fixed-width cell-content">

              <!-- Dynamic Preset Select Combo -->
              <template v-if="element.ValueType === 'Preset'">
                <select class="form-select select-combo" :id="'element-' + element.Key" v-model="element.Value"
                  @change="OnPresetValueChange(element, $event)">
                  <option v-for="preset in getPresetsForType(element.Type)" :key="preset.Name" :value="preset.Index">
                    {{ preset.Name }}
                  </option>
                </select>
              </template>

              <!-- Range Slider Control -->
              <template v-if="element.ValueType === 'Brightness'">
                <div class="range-container" :id="'element-' + element.Key">
                  <input type="range" class="form-range range-input" v-model="element.Value"
                    :min="getMinValue(element.Type)" :max="getMaxValue(element.Type)" step="0.01"
                    @input="OnBrightnessValueChange(element, $event)" style="height: 10px;" />
                  <label class="slider-value-label">{{ element.Value }}</label>
                </div>
              </template>

              <!-- On/Off Swap control -->
              <template v-if="element.ValueType === 'ONOFF'">
                <div class="form-check form-switch" :id="'element-' + element.Key">
                  <input class="form-check-input larger-switch" type="checkbox" :checked="element.Value === 1"
                    @change="OnToggleValueChange(element, $event)" />
                </div>
              </template>

              <!-- Custom Color Picker Control -->
              <template v-if="element.ValueType === 'Color'">
                <ColorDisplay :id="'element-' + element.Key" :color="element.Value"
                  @OncolorChanged="OnColorValueChange(element, $event)"></ColorDisplay>
              </template>

            </td>
          </tr>
        </tbody>
      </table>

      <!-- Context Menu for Elements -->
      <ul v-if="showContextMenuFlag" :style="contextMenuStyle" class="dropdown-menu show" ref="contextMenu">
        <li><a class="dropdown-item" href="#" @click="onContextMenu_Click('AddUserSettings')">Add to User Settings..</a> </li>        
      </ul>

    </div>

    <div id="contextMenu" ref="contextMenu" class="collapse context-menu"></div>
  </div>
</template>

<script>
import ColorDisplay from './Components/ColorDisplay.vue';
import Util from '../Helpers/Utils.js';
import eventBus from '../EventBus';

// Activate all Popovers:
const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
const popoverList = [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl));

export default {
  name: 'GlobalSettingsTab',
  components: {
    ColorDisplay
  },
  data() {
    return {
      dataSource: null,     //<- Raw Datasource directly from the File
      groupedElements: [],  //<- Processed Datasource from 'loadGroupData()'
      presets: [],          //<- Presets for the Combo Selects, from Raw Datasource
      activePopover: null,
      selectedKey: null,     //<- key of the selected row
      selectedRow: null,

      contextMenuStyle: {},
      showContextMenuFlag: false, //<- Flag to show the Context Menu
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
  computed: {
    rowClass: function () {
      return function (element) {
        return { 'table-active': element.Key === this.selectedKey };
      }
    }
  },
  methods: {
    async OnInitialize() {
      this.dataSource = await window.api.LoadGlobalSettings();
      this.presets = this.dataSource.Presets;
    },

    // #region Load Data

    loadGroupData() {
      //console.log(this.dataSource);
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

        eventBus.emit('OnGlobalSettingsLoaded', JSON.parse(JSON.stringify(this.groupedElements))); //<- Event listen in 'NavBars.vue'
      }
    },
    /** Gets all Presets for the selected Type
     * @param type Type of Preset  */
    getPresetsForType(type) {
      return this.presets.filter(preset => preset.Type === type);
    },

    // #endregion

    // #region Info Icon / Popover

    showIcon(groupIndex, elementIndex) {
      // Shows a little Info Icon when the mouse is over a Table's Row.
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
      // Hide the Icon when the mouse leaves the Row
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
      // Show a Bootstrap Popover with info about the current item
      if (this.activePopover) {
        this.activePopover.dispose();
        this.activePopover = null;
      }

      this.$nextTick(async () => {
        const imagePath = await this.getImagePath(element.Key);
        const content = `
        <p>${element.Description}</p>
        <img src="${imagePath}" alt="No Image" class="popover-image" />
        <p>Key: ${element.Key}</p>`;

        const popover = new bootstrap.Popover(event.target, {
          title: element.Title,
          content: content,
          html: true,
          trigger: 'manual',
          placement: 'right',
          container: 'body',
          template: `
          <div class="popover border border-warning custom-popover" role="tooltip">  
              <div class="popover-arrow"></div>
              <h4 class="popover-header"></h4>
              <div class="popover-body"></div>
          </div>`,
        });

        popover.show();
        this.activePopover = popover;

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

    // #endregion

    // #region Utility Methods

    /** Gets the path for an Element Image
    * @param key The file name of the image matches the 'key' of the Element.     */
    async getImagePath(key) {
      const imageKey = await window.api.GetElementsImage(key);
      return new URL(imageKey, import.meta.url).href;
    },
    hideImage(event) {
      event.target.style.display = 'none';
    },

    /** Gets the Minimum Value for a Range-slider control
    * @param type Type of Range  */
    getMinValue(type) {
      switch (type) {
        case '2X': return -1.0;
        case '4X':
          return 0.0;
        default:
          return 0.0;
      }
    },
    /** Gets the Maximum Value for a Range-slider control
     * @param type Type of Range     */
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
    intToRGBAstring(value) {
      return Util.intToRGBAstring(value);
    },

    // #endregion

    // #region METHODS TO UPDATE CHANGES IN THE CONTROLS 

    OnPresetValueChange(item, event) {
      const value = parseFloat(event.target.value);
      this.updateDataSourceValue(item, value);
    },
    OnBrightnessValueChange(item, event) {
      const value = parseFloat(event.target.value);
      this.updateDataSourceValue(item, value);
    },
    OnToggleValueChange(item, event) {
      const value = event.target.checked ? 1 : 0;
      this.updateDataSourceValue(item, value);
    },
    OnColorValueChange(item, event) {
      const value = event.int;
      this.updateDataSourceValue(item, value);
    },
    updateDataSourceValue(item, newValue) {
      const elementIndex = this.dataSource.Elements.findIndex(el => el.Key === item.Key);
      if (elementIndex !== -1) {
        this.dataSource.Elements[elementIndex].Value = newValue;
        item.Value = newValue; // Keep groupedElements in sync
        this.saveChanges();
      } else {
        console.error("Element not found in dataSource:", item.Key);
      }
    },
    saveChangesDebounced: null, // To store the debounced function
    saveChanges: function () {
      if (!this.saveChangesDebounced) {
        this.saveChangesDebounced = this.debounce(async function () {
          try {
            await window.api.SaveGlobalSettings(JSON.parse(JSON.stringify(this.dataSource)));
            this.$emit('OnGlobalSettings_Saved', JSON.parse(JSON.stringify(this.dataSource))); //<- Event listen on 'NavBars.vue'
            //console.log("Changes saved successfully!");
          } catch (error) {
            console.error("Error saving changes:", error);
          }
          this.saveChangesDebounced = null; // Reset after execution
        }, 500); // Adjust delay as needed
      }
      this.saveChangesDebounced();
    },
    debounce(func, delay) {
      let timeout;
      return function () {
        const context = this;
        const args = arguments;
        clearTimeout(timeout);
        timeout = setTimeout(() => {
          func.apply(context, args);
        }, delay);
      };
    },

    // #endregion

    // #region Search & Find

    DoFindAndSelectRow(key) {
      this.selectedKey = key;
      this.$nextTick(() => { // Use nextTick to ensure DOM is updated
        const row = document.getElementById('row-' + key);
        if (row) {
          row.scrollIntoView({ behavior: 'smooth', block: 'center' });
        } else {
          console.warn('Row with key "' + key + '" not found.');
        }
      });
    },
    /** Selects a Row of the Table by Key. 
     * @param key complete Key as is in the Ini files     */
    selectRow(key) {
      this.selectedKey = key;
      //console.log('Selected: ', key)
    },

    // #endregion

    // #region Context Menus

    /** When Fired, Shows the Context Menu for the Selected Theme and Selects it  * 
     * @param image The Selected Theme
     * @param event Mouse Event related    */
    onRightClick(event, element) {
      event.preventDefault(); // Prevent the default context menu
      this.selectedRow = JSON.parse(JSON.stringify(element)); 
      this.selectRow(element.Key);
      this.showContextMenu(event.clientX, event.clientY); // Show the custom context menu
    },
    showContextMenu(x, y) {
      // Position the context menu
      this.contextMenuStyle = {
        top: `${y - 60}px`,
        left: `${x}px`,
        display: 'block'
      };
      this.showContextMenuFlag = true;

      // Close context menu when clicking outside
      document.addEventListener('click', this.hideContextMenu);
    },
    hideContextMenu() {
      this.showContextMenuFlag = false;
      document.removeEventListener('click', this.hideContextMenu); // Remove the event listener
    },
    /** When the User clicks on one of the Context menus * 
    * @param action Name of the clicked menu    */
    async onContextMenu_Click(action) {
      try {
        const contextMenu = this.$refs.contextMenu;
        if (contextMenu) {
          contextMenu.classList.remove('show');
        }
        console.log('Context Menu:', action);
        // Handle the context menu action
        if (this.selectedRow) {
          switch (action) {
            case 'AddUserSettings':
              console.log('Adding to User Settings:', this.selectedRow);
              const _ret = await window.api.AddToUserSettings(JSON.parse(JSON.stringify(this.selectedRow)));
              if (_ret) {
                eventBus.emit('RoastMe', { type: 'Success', message: 'Added to the User Settings Tab!' });
                eventBus.emit('DoLoadUserSettings', null); //<- Event Listen in 'UserSettingsTab.vue'
              }
              break;

              case 'RemoveUserSettings':
                console.log('Removing from User Settings:', this.selectedRow);
                const _ret2 = await window.api.RemoveFromUserSettings(JSON.parse(JSON.stringify(this.selectedRow)));
                if (_ret2) {
                  eventBus.emit('RoastMe', { type: 'Success', message: 'Removed from User Settings Tab!' });
                  eventBus.emit('DoLoadUserSettings', null); //<- Event Listen in 'UserSettingsTab.vue'
                }
              break;
            default:
              break;
          }
        }
      } catch (error) {
        eventBus.emit('ShowError', error);
      }
    },

    // #endregion

  },
  mounted() {
    eventBus.on('DoLoadGlobalSettings', this.OnInitialize);
    eventBus.on('FindKeyInGlobalSettings', this.DoFindAndSelectRow);
  },
  beforeUnmount() {
    eventBus.off('DoLoadGlobalSettings', this.OnInitialize);
    eventBus.off('FindKeyInGlobalSettings', this.DoFindAndSelectRow);
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
.data-table::-webkit-scrollbar {
  width: 8px;
}
.data-table::-webkit-scrollbar-track {
  background: #333;
}
.data-table::-webkit-scrollbar-thumb {
  background-color: #555;
  border-radius: 10px;
}
.data-table::-webkit-scrollbar-thumb:hover {
  background-color: #777;
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
  cursor: default;
  font-size: 14px;
  color: lightgrey;
  align-items: center;
  padding: 0.5rem;
}

.info-icon {
  margin-left: 0.5rem;
  /* Space between text and icon */
  cursor: pointer;
  /* Indicate icon is clickable */
}

.range-container {
  position: relative;
  width: 100%;
  height: 38px;
}

.slider-value-label {
  display: block;
  margin-top: 2px;
  font-size: 12px;
  color: #f8f9fa;
  text-align: left;
}


</style>