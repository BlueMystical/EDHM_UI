<template>
    <div class="data-table table-responsive" v-if="processedData.length > 0">
        <div v-for="(section, sectionIndex) in processedData" :key="sectionIndex">
            <p class="category-name">{{ section.title }}</p>
            <table class="table table-bordered table-hover align-middle">
                <tbody>
                    <!-- Table Row -->
                    <tr v-for="(key, keyIndex) in section.keys" :key="keyIndex" :id="'row-' + key.key"
                        @mouseover="showIcon(sectionIndex, keyIndex)" @mouseleave="hideIcon(sectionIndex, keyIndex)"
                        @click="selectRow(key.key)" :class="rowClass(key)" @contextmenu="onRightClick($event, key)">

                        <!-- Left Column -->
                        <td class="fixed-width title-cell">
                            {{ key.name }}
                            <span class="info-icon" v-show="key.iconVisible" @mouseover="showPopover(key, $event)"
                                @mouseleave="hidePopover">
                                <i class="bi bi-info-circle text-info"></i>
                            </span>
                        </td>

                        <!-- Right Column -->
                        <td class="fixed-width cell-content">

                            <!-- On/Off Swap control -->
                            <template v-if="key.type === 'toggle'">
                                <div class="form-check form-switch" :id="'element-' + key.key">
                                    <input class="form-check-input larger-switch" type="checkbox"
                                        :checked="key.value === 1"
                                        @change="OnToggleValueChange(sectionIndex, key, $event)" />
                                </div>
                            </template>

                            <!-- Range Slider Control -->
                            <template v-else-if="key.type === 'decimal'">
                                <div class="range-container" :id="'element-' + key.Key">
                                    <input type="range" class="form-range range-input" v-model="key.value"
                                        :min="getMinValue('2x')" :max="getMaxValue('2X')" step="0.01"
                                        @input="OnBrightnessValueChange(sectionIndex, key, $event)"
                                        style="height: 10px;" />
                                    <label class="slider-value-label">{{ key.value }}</label>
                                </div>
                            </template>

                            <!-- Custom Color Picker Control -->
                            <template v-else-if="key.type === 'color'">
                                <ColorDisplay :id="'element-' + key.key" :color="key.value" :recentColors="recentColors"
                                    @OncolorChanged="OnColorValueChange(sectionIndex, key, $event)"
                                    @OnRecentColorsChange="OnRecentColorsChange($event)" />
                            </template>

                            <!-- Standard TextBox Input -->
                            <template v-else-if="key.type === 'text'">
                                <input type="text" :id="'element-' + key.Key" class="form-control" aria-describedby="" v-model="key.value"
                                @change="OnTextValueChange(sectionIndex, key, $event)">
                            </template>

                            <!-- Standard Numeric Input -->
                            <template v-else-if="key.type === 'number'">
                                <input type="number" :id="'element-' + key.Key" class="form-control" aria-describedby="" v-model="key.value"
                                @change="OnTextValueChange(sectionIndex, key, $event)">
                            </template>

                            <!-- Dynamic Preset Select Combo -->
                            <template v-else>
                                <select class="form-select select-combo" :id="'element-' + key.key" v-model="key.value"
                                    @change="OnPresetValueChange(sectionIndex, key, $event)">
                                    <option v-for="preset in getCustomTypes(key.type)" 
                                        :key="preset.name" :value="preset.value">
                                        {{ preset.name }}
                                    </option>
                                </select>
                            </template>

                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>

<script>
import ColorDisplay from '../MainWindow/Components/ColorDisplay.vue';
import Util from '../Helpers/Utils';
import eventBus from '../EventBus';

// Activate all Popovers:
const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
const popoverList = [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl));

export default {
    name: 'TPModProperties',
    components: {
        ColorDisplay
    },
    data() {
        return {
            themeName: '',
            themeTemplate: null,  //<- Loaded Theme
            dataSource: null,     //<- Raw Datasource directly from the File
            groupedElements: [],  //<- Processed Datasource from 'loadGroupData()'
            presets: [],          //<- Presets for the Combo Selects, from Raw Datasource
            recentColors: [],     //<- Array of colors (Hex) for the Recent Colors boxes, up to 8
            
            activePopover: null,
            selectedKey: null,    //<- key of the selected row
            selectedRow: null,

            contextMenuStyle: {},
            showContextMenuFlag: false, //<- Flag to show the Context Menu
            componentKey: 0,     //<- flag to force component re-render

            modData: {},
            processedData: [],
        };
    },
/*    watch: {
        dataSource: {
            immediate: true,
            handler() {
                //this.loadGroupData()
            }
        }
    },*/
    computed: {
        rowClass: function () {
            return function (element) {
                return { 'table-active': element.key === this.selectedKey };
            }
        }
    },
    methods: {
        async OnInitialize(theme) {
            if (theme) {
                this.themeName = theme.mod_name;
                console.log('Initializing Properties for:', theme.mod_name);
                this.dataSource = theme;
                this.modData = null;
                
                this.modData = { ...JSON.parse(JSON.stringify(theme.data)) };
                this.presets = theme.Presets;

                this.componentKey++; // Increment key to force re-render
                
                this.processData();
            }
        },

        // #region Load Data
        processData() {
            this.processedData = this.modData.sections.map((section) => ({
                title: section.title,
                keys: section.keys.map(key => ({ ...key, iconVisible: false })),
            }));
        },

        /** Gets all Presets for the selected Type
         * @param type Type of Preset  */
        getCustomTypes(type) {
            return this.modData.custom_types.filter((item) => item.type === type);
        },

        // #endregion

        // #region Info Icon / Popover

        showIcon(sectionIndex, keyIndex) {
            const newProcessedData = this.processedData.map((section, sIndex) => {
                if (sIndex === sectionIndex) {
                    return {
                        ...section,
                        keys: section.keys.map((key, kIndex) => {
                            if (kIndex === keyIndex) {
                                return { ...key, iconVisible: true };
                            }
                            return key;
                        }),
                    };
                }
                return section;
            });
            this.processedData = newProcessedData;
        },
        hideIcon(sectionIndex, keyIndex) {
            const newProcessedData = this.processedData.map((section, sIndex) => {
                if (sIndex === sectionIndex) {
                    return {
                        ...section,
                        keys: section.keys.map((key, kIndex) => {
                            if (kIndex === keyIndex) {
                                return { ...key, iconVisible: false };
                            }
                            return key;
                        }),
                    };
                }
                return section;
            });
            this.processedData = newProcessedData;
        },
        showPopover(key, event) {
            if (this.activePopover) {
                this.activePopover.dispose();
                this.activePopover = null;
            }
            this.$nextTick(async () => {
                const imagePath = await this.getImagePath(key.key);
                const content = `
          <p>${key.description}</p>
          <img src="${imagePath}" alt="No Image" class="popover-image" />
          <p>Key: ${key.key}</p>`;
                const popover = new bootstrap.Popover(event.target, {
                    title: key.name,
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
                case '2X': return -2.0;
                case '4X': return 0.0;
                default:   return 0.0;
            }
        },
        /** Gets the Maximum Value for a Range-slider control
         * @param type Type of Range     */
        getMaxValue(type) {
            switch (type) {
                case '2X': return 2.0;
                case '4X': return 4.0;
                default:   return 1.0;
            }
        },
        intToRGBAstring(value) {
            return Util.intToRGBAstring(value);
        },

        // #endregion

        // #region METHODS TO UPDATE CHANGES IN THE CONTROLS 

        OnTextValueChange(sectionIndex, item, event) {
            const value = parseFloat(event.target.value);
            this.updateDataSourceValue(sectionIndex, item, value);
        },
        OnPresetValueChange(sectionIndex, item, event) {
            //console.log(event);
            const value = event.target.value; 
            this.updateDataSourceValue(sectionIndex, item, value);
        },
        OnBrightnessValueChange(sectionIndex, item, event) {
            const value = parseFloat(event.target.value);
            this.updateDataSourceValue(sectionIndex, item, value);
        },
        OnToggleValueChange(sectionIndex, item, event) {
            const value = event.target.checked ? 1 : 0;
            this.updateDataSourceValue(sectionIndex, item, value);
        },
        OnColorValueChange(sectionIndex, item, event) {
            const value = event.int;
            this.updateDataSourceValue(sectionIndex, item, value);
        },
        OnRecentColorsChange (colors) {
            this.recentColors = [...colors]; // Update the array reactively
            console.log('Recent colors updated in parent:', colors);
        },
        async updateDataSourceValue(sectionIndex, item, newValue) {            
            const elementIndex = this.modData.sections[sectionIndex].keys.findIndex(el => el.key === item.key);
            if (elementIndex !== -1) {
                //console.log('Updating:',  item, newValue);
                if (this.modData.sections[sectionIndex].keys[elementIndex].value != newValue) {
                    
                    //- Update the key/value on the Mod data:
                    this.modData.sections[sectionIndex].keys[elementIndex].value = newValue;
                    item.value = newValue;
                    
                    //- Update the key/value on the Ini data:  TODO: if is a Color (multi-key)
                    const _key = this.modData.sections[sectionIndex].keys[elementIndex].key;
                    const _val = this.modData.sections[sectionIndex].keys[elementIndex].value;
                    const keys = _key.split('|');  //<- iniKey === "x159|y159|z159" or "x159|y155|z153|w200"
                    
                    if (Array.isArray(keys) && keys.length > 2) {
                        //- Multi Key: Colors
                        const RGBAcolor = Util.intToRGBA(_val); //<- color: { r: 81, g: 220, b: 255, a: 255 }
                        const sRGBcolor = Util.GetGammaCorrected_RGBA(RGBAcolor);
                        const values = [sRGBcolor.r, sRGBcolor.g, sRGBcolor.b, sRGBcolor.a]; //<- [ 0.082, 0.716, 1.0, 1.0 ]

                        for (const [index, key] of keys.entries()) { // using entries to get the index
                            const color = parseFloat(values[index]);
                            try {
                                this.dataSource.data_ini = await window.api.setIniKey(
                                    JSON.parse(JSON.stringify(this.dataSource.data_ini)),
                                    this.modData.sections[sectionIndex].ini_section,
                                    key,
                                    color
                                );
                            } catch (error) {
                                console.log(stackTrace + values[index], error.message);
                            }
                        }

                    } else {
                        //- Single Key:
                        this.dataSource.data_ini = await window.api.setIniKey(
                            JSON.parse(JSON.stringify(this.dataSource.data_ini)), 
                            this.modData.sections[sectionIndex].ini_section,
                            this.modData.sections[sectionIndex].keys[elementIndex].key,
                            this.modData.sections[sectionIndex].keys[elementIndex].value
                        );
                    }                    
                    //console.log(this.dataSource.data_ini);

                    this.saveChanges();
                }
            } else {
                console.error("Element not found in dataSource:", item.Key);
            }
        },
        saveChangesDebounced: null, // To store the debounced function
        saveChanges: function () {
            if (!this.saveChangesDebounced) {
                this.saveChangesDebounced = this.debounce(async function () {
                    try {
                        //Sends the changes to the Current Settings:
                        this.dataSource.data = this.modData;
                        this.$emit("OnValuesChanged", JSON.parse(JSON.stringify(this.dataSource))); //<- pasar el objeto al padre.

                    } catch (error) {
                        console.error("Error saving changes:", error);
                    }
                    this.saveChangesDebounced = null; // Reset after execution
                }, 1000); // Adjust delay as needed
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
        eventBus.on('InitializeProperties', this.OnInitialize);
        eventBus.on('areaClicked', this.loadProperties);
        eventBus.on('FindKeyInGlobalSettings', this.DoFindAndSelectRow);
    },
    beforeUnmount() {
        eventBus.off('InitializeProperties', this.OnInitialize);
        eventBus.off('areaClicked', this.loadProperties);
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
  table-layout: fixed;
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
  cursor: pointer;
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

.table td:first-child {
  width: 50%;
}

/* Added these rules */
.cell-content select {
  width: 100%; /* Or set a specific width, e.g., 200px */
  max-width: 250px; /* Optional: prevent it from becoming too wide */
  box-sizing: border-box; /* Include padding and border in the width */
}
</style>