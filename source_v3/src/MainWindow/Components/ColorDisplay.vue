<template>
    <div ref="colorPickerContainer" class="color-display">
        <div class="transparency-grid"></div>
        <div class="color-layer" :style="colorStyle"></div>
        <div class="text-layer" :style="textStyle">{{ rgbaString }}</div>
    </div>
</template>

<script>
import Picker from './vanilla-picker.csp.mjs';
import Util from '../../Helpers/Utils.js';
import './vanilla-picker.csp.css';

export default {
    props: {
        key: '',
        color: {
            type: Number,
            required: true,
            description: "Signed integer representing the color.",
        },
    },
    data() {
        return {
            rgba: { r: 0, g: 0, b: 0, a: 0 },
            rgbaString: "",
            hex: "",
            intColor: -1,
            pickerInstance: null,
        };
    },
    computed: {
        colorStyle() {
            //console.log('Computed colorStyle:', this.rgbaString); // Debug log
            return {
                backgroundColor: this.rgbaString,
                borderRadius: '6px',
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                height: '100%'
            };
        },
        textStyle() {
            //console.log('Computed textStyle:', this.hex); // Debug log
            return {
                color: Util.GetForeColorFor(this.hex)
            };
        }
    },
    mounted() {
        this.initializePicker();
    },
    watch: {
        color(newColor, oldColor) {
            //console.log(`Color changed from ${oldColor} to ${newColor}`);
            this.updateColor(newColor);
        },
    },
    methods: {
        initializePicker() {
            try {
                this.updateColor(this.color);
                this.pickerInstance = new Picker({
                    parent: this.$refs.colorPickerContainer,
                    popup: 'middle',        //<- 'top' | 'bottom' | 'left' | 'right' | 'middle' | false
                    editorFormat: 'hex',    //<- 'hex' | 'hsl' | 'rgb'
                    alpha: true,
                    editor: true,
                    color: this.rgbaString, //<- 'rgba(255,0,0, 1)' | #FF0000FF  
                    cancelButton: false,

                    onChange: (color) => {
                        this.updateColor(Util.hexToSignedInt(color.hex));
                    },
                    onRecentColorsChange: (colors) => {
                        console.log("Recent colors changed:", colors);
                        // Do something with the colors
                    }
                });
                this.pickerInstance.setRecentColors(['#ff0000', '#00ff00', '#0000ff']);

            } catch (error) {
                console.log(error);
            }
        },
        updateColor(colorInt) {
            this.intColor = colorInt;
            this.rgbaString = Util.intToRGBAstring(colorInt); //<- for color box
            this.hex = Util.intToHexColor(colorInt);
            this.rgba = Util.intToRGBA(colorInt);

            // Force re-render
            this.$nextTick(() => {
                this.$forceUpdate();
            });

            // Emit the event with the new color:
            this.$emit('OncolorChanged', { int: colorInt, hex: this.hex, rgba: this.rgba });
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
.color-display {
    width: 100%;
    height: 100%;
    min-height: 34px;
    border: 1px solid #495057;
    border-radius: 6px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    position: relative; /* Create local stacking context */
}

.transparency-grid {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-image: url("./../../images/TransparencyGrid.jpg");
    background-repeat: repeat;
    border-radius: 6px;
    z-index: 0; /* Lowest */
}

.color-layer {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 1; /* Middle */
}

.text-layer {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 12px;
    color: #000;
    pointer-events: none;
    z-index: 2; /* Highest within .color-display */
}
</style>