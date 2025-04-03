/*  COLECTION OF UTILITY METHODS
 *  They can be used in both Process (Main & Renderer) */

function containsWord(str, word) { return str.includes(word); };

function containsCharacter(str, char) {
    if (typeof str !== 'string' || typeof char !== 'string' || char.length !== 1) {
        return false; // Handle invalid input
    }
    return str.indexOf(char) !== -1;
}

/** Checks if a string exists within an array of strings.
 * @param {string} compareString - The string to search for.
 * @param {string[]} stringList - The array of strings to search within.
 * @returns {boolean} - True if the string is found in the array, false otherwise. */
function stringIn(compareString, stringList) {
    if (!Array.isArray(stringList) || typeof compareString !== 'string') {
        return false; // Handle invalid input
    }
    return stringList.includes(compareString);
}

function trimAllSpaces(str) {
    return str.replace(/\s+/g, '');
}

/** To Check is something is Empty
 * @param obj Object to check */
function isEmpty(obj) { Object.keys(obj).length === 0 };

/** Checks if a string is defined and has a value, and returns the string or a default value.
 * @param {string} str - The string to check.
 * @param {any} defaultValue - The default value to return if the string is not valid.
 * @returns {string|any} - The original string if it's defined and has a value, or the default value otherwise. */
function NVL(str, defaultValue) {
    if (typeof str === 'string' && str.trim() !== '') {
        return str;
    } else {
        return defaultValue;
    }
}

function safeRound(value) { return isNaN(value) ? 0 : Math.round(value); }

/** Null-Empty-Uninstanced verification * 
 * @param {*} value Object, String or Array
 * @returns 'true' if the object is NOT Null or Empty */
function isNotNullOrEmpty(value) {
    if (value === null || value === undefined) {
        return false;
    }

    if (typeof value === 'string' || Array.isArray(value)) {
        return value.length > 0;
    }

    if (typeof value === 'object') {
        return Object.keys(value).length > 0;
    }

    return false;
}

function copyToClipboard(text) {
    navigator.clipboard.writeText(text)
        .then(() => {
            console.log('Copied to clipboard successfully!');
        })
        .catch((err) => {
            console.error('Failed to copy to clipboard: ', err);
        });
}

function compareVersions(serverVersion, localVersion) {
    // Remove non-numeric characters from the beginning
    serverVersion = serverVersion.replace(/^[^\d]+/, '');  console.log('serverVersion:', serverVersion);
    localVersion = localVersion.replace(/^[^\d]+/, '');     console.log('localVersion:', localVersion);

    // Split the versions into parts
    const serverParts = serverVersion.split('.').map(Number);
    const localParts = localVersion.split('.').map(Number);

    // Compare each part of the versions
    for (let i = 0; i < serverParts.length; i++) {
      if (serverParts[i] > (localParts[i] || 0)) {
        return true;
      } else if (serverParts[i] < (localParts[i] || 0)) {
        return false;
      }
    }

    return false; // versions are equal or local version is higher
  }

// #region Color Conversion Methods

/** Helper function to determine if a color is dark  * 
 * @param {string} color String for the Color, can be a HEX or a 'rgb(0,0,0,1)'
 * @returns true or false */
function isColorDark(color) {
    if (color) {
        // Convert color to RGB if it's a hex code
        let r, g, b;
        //console.log('isColorDark: ', color);
        if (color.startsWith('#')) {
            // Hex to RGB conversion (simplified - adapt for #RRGGBBAA if needed)
            r = parseInt(color.slice(1, 3), 16);
            g = parseInt(color.slice(3, 5), 16);
            b = parseInt(color.slice(5, 7), 16);
        } else if (color.startsWith('rgb')) {
            // Extract RGB values from rgba or rgb string
            const rgbValues = color.match(/\d+/g);
            r = parseInt(rgbValues[0]);
            g = parseInt(rgbValues[1]);
            b = parseInt(rgbValues[2]);
        } else {
            console.error("Invalid color format. Please provide a valid color string.");
            return false; // Or handle the error as needed
        }

        // Calculate luminance (a common way to determine darkness)
        const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;

        return luminance < 0.5; // Adjust threshold as needed (0.5 is a common value)
    }
}

/** Returns a text color appropriate to the given background color. 
 * @param {string} color String for the Color, can be a HEX or a 'rgb(0,0,0,1)'
 * @returns {string} a HEX color adecuate to the back color */
function GetForeColorFor(color) {
    const textColor = isColorDark(color) ? '#fff' : '#000'; // White for dark, black for light
    return textColor;
}

/** Converts an array of Integers to an RGBA string
 * @param {*} colorComponents Array of Integer Numbers
 * @returns a rgba(..) string*/
function RGBAtoColor(colorComponents) {
    console.log(colorComponents);
    if (colorComponents.length === 3) {
        const [r, g, b] = colorComponents.map(parseFloat);
        return `rgb(${r}, ${g}, ${b})`;
    } else if (colorComponents.length === 4) {
        const [r, g, b, a] = colorComponents.map(parseFloat);
        return `rgba(${r}, ${g}, ${b}, ${a})`;
    } else {
        throw new Error("Invalid number of color components. Expected 3 or 4.");
    }
};
/** Converts an RGBA object to a rgba(..) string.
 * @param {object} rgba - The RGBA object.
 * @returns {string} - The rgba(..) string.
 */
function rgbaToString(rgba) {
    const { r, g, b, a } = rgba;

    // Default alpha value to 1 if it's missing
    var alpha = 1;
    if (a !== undefined) {
        if (a > 1) {
            alpha = a / 255;
        } else {
            alpha = a
        }
    } else {
        alpha = 1
    }

    // Return the rgba string
    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}

/** Converts an RGB or RGBA or individual r,g,b,a components into a HEX color String with transparency. * 
 * @param {*} r If this is a number then is the Red component (0-255). If is an Object the it must contain the rgba components and the other parameters are ignored.
 * @param {*} g Green component in the range of 0 to 255.
 * @param {*} b Blue component in the range of 0 to 255.
 * @param {*} a Alpha can be in the ranges of 0.0 to 1.0 or 0 to 255
* @returns A HEX color string in #RRGGBBAA format. */
function rgbaToHex(r, g, b, a) {
    let rVal = r;
    let gVal = g;
    let bVal = b;
    let aVal = a;

    if (typeof r === 'object' && r !== null) {
        ({ r: rVal, g: gVal, b: bVal, a: aVal = 1.0 } = r); // Destructure, using different variable names, default alpha to 1.0
    } else {
        aVal = a === undefined ? 1.0 : a; // Default alpha to 1.0 if not provided
    }

    // Scale alpha if it is in the range of 0-255
    if (aVal > 1 && aVal <= 255) {
        aVal = aVal / 255;
    }

    const componentToHex = (c) => {
        if (typeof c !== 'number' || isNaN(c) || c < 0 || c > 255) {
            return "00";
        }
        const hex = c.toString(16);
        return hex.length === 1 ? "0" + hex : hex;
    };

    const rHex = componentToHex(rVal);
    const gHex = componentToHex(gVal);
    const bHex = componentToHex(bVal);
    const aHex = componentToHex(Math.round(aVal * 255)); // Scale alpha to 0-255 for hex conversion

    return `#${rHex}${gHex}${bHex}${aHex}`; // Always include alpha
}

/** Converts an array or RGBA color componets into a HEX string with transparency.
 * @param {*} rgba Array of Integers, except the A componet which is Decimal from 0.0 to 1.0
 * @returns a HEX string in #RRGGBBAA format  */
function rgbaArrayToHex(rgba) {
    const componentToHex = (c) => {
        if (typeof c !== 'number' || isNaN(c) || c < 0 || c > 255) {
            return "00";
        }
        const hex = c.toString(16).toUpperCase();
        return hex.length === 1 ? "0" + hex : hex;
    };

    const rHex = componentToHex(rgba[0]);
    const gHex = componentToHex(rgba[1]);
    const bHex = componentToHex(rgba[2]);
    const aHex = componentToHex(Math.round(rgba[3] * 255));

    return `#${rHex}${gHex}${bHex}${aHex}`;
}

function rgbaToInt(color) {
    const { r, g, b, a } = color;
    const alpha = a !== undefined ? a : 255; // If alpha is not provided, assume it's 255 (fully opaque)

    // Combine RGBA values into a single integer in the correct order
    const intVal = (alpha << 24) | (r << 16) | (g << 8) | b;

    // Convert to signed 32-bit integer
    return intVal >> 0;
}

//---------------------------------------------

/**
 * Converts a signed integer to a HEX color string in #RRGGBBAA format.
 * Handles two's complement for negative numbers.
 * @param {number} number - The signed integer representing the color.
 * @returns {string} - The HEX color string in #RRGGBBAA format.
 */
function intToHexColor(number) {
    let hexValue = number;

    // Handle negative numbers using two's complement (if needed for your system)
    if (number < 0) {
        hexValue = (number >>> 0); // Convert to unsigned 32bit integer
    }


    const a = (hexValue >> 24) & 0xFF;
    const r = (hexValue >> 16) & 0xFF;
    const g = (hexValue >> 8) & 0xFF;
    const b = hexValue & 0xFF;

    const componentToHex = (c) => {
        const hex = c.toString(16).toUpperCase();
        return hex.length === 1 ? "0" + hex : hex;
    };

    const hexColor = `#${componentToHex(r)}${componentToHex(g)}${componentToHex(b)}${componentToHex(a)}`;
    return hexColor;
}

/** Converts a signed integer to an RGBA object.
 * @param {number} number - The signed integer number.
 * @returns {object} - An object with properties r, g, b, and a. */
function intToRGBA(number) {
    if (number < 0) {
        number = 4294967296 + number; // 2^32
    }

    const a = (number >> 24) & 0xFF;
    const r = (number >> 16) & 0xFF;
    const g = (number >> 8) & 0xFF;
    const b = number & 0xFF;

    const rgba = {
        r: r,
        g: g,
        b: b,
        a: (a / 255).toFixed(3)
    };
    //console.log(rgba);
    return rgba;
}
function intToRGBAstring(number) {
    const RGBvalue = this.intToRGBA(number);
    return this.rgbaToString(RGBvalue);
}


//---------------------------------------------

/** Converts a HEX color string to RGBA color components. 
 * @param {string} hexColor - '#ff0000', 'ff0000', 'f00', '#ff000080', '#FF9EF703'
 * @returns {object} An object having the RGBA color components. */
function hexToRgba(hexColor) {
    // Remove the hash sign if present and convert to lowercase for uniformity
    hexColor = hexColor.replace("#", "").toLowerCase();

    if (hexColor.length === 3) {
        hexColor = hexColor.replace(/(.)/g, "$1$1"); // Expand shorthand
    }

    if (hexColor.length === 4) {
        hexColor = hexColor.replace(/(.)/g, "$1$1"); // Expand 4-character shorthand to 8 characters
    }

    const r = parseInt(hexColor.slice(0, 2), 16);
    const g = parseInt(hexColor.slice(2, 4), 16);
    const b = parseInt(hexColor.slice(4, 6), 16);
    const a = hexColor.length === 8 ? parseInt(hexColor.slice(6, 8), 16) : 255; // Alpha last

    if (isNaN(r) || isNaN(g) || isNaN(b) || isNaN(a)) {
        return null; // Invalid hex characters
    }

    return { r, g, b, a };
}
/** Converts a HEX color string to a signed (can be negative) Integer number. * 
 * @param {*} hexColor '#ff0000, ff0000, f00, #ff000080 
 * @returns a signed Integer number represnting the color  */
function hexToSignedInt(hexColor) {
    const rgba = hexToRgba(hexColor);
    if (!rgba) {
        return null;
    }
    return (rgba.a << 24) | (rgba.r << 16) | (rgba.g << 8) | rgba.b;
}
/** Converts a HEX color string to a un-signed (can NOT be negative) Integer number.  
 * @param {*} hexColor '#ff0000, ff0000, f00, #ff000080 
 * @returns a un-signed Integer number represnting the color  */
function hexToUnsignedInt(hexColor) {
    const rgba = hexToRgba(hexColor);
    if (!rgba) {
        return null;
    }
    return ((rgba.a << 24) | (rgba.r << 16) | (rgba.g << 8) | rgba.b) >>> 0; // The >>> 0 is essential
}

//---------------------------------------------

// Function to convert sRGB to Linear RGB using gamma correction
function Convert_sRGB_ToLinear(thesRGBValue, gammaValue = 2.4) {
    return thesRGBValue <= 0.04045
        ? thesRGBValue / 12.92
        : Math.pow((thesRGBValue + 0.055) / 1.055, gammaValue);
}
function convert_sRGB_FromLinear(theLinearValue, _GammaValue = 2.4) {
    return theLinearValue <= 0.0031308
        ? theLinearValue * 12.92
        : Math.pow(theLinearValue, 1.0 / _GammaValue) * 1.055 - 0.055;
};

// Function to get gamma corrected RGBA
function GetGammaCorrected_RGBA(color, gammaValue = 2.4) {
    const normalize = value => value / 255;

    const gammaCorrected = {
        r: Math.round(this.Convert_sRGB_ToLinear(normalize(color.r), gammaValue) * 10000) / 10000,
        g: Math.round(this.Convert_sRGB_ToLinear(normalize(color.g), gammaValue) * 10000) / 10000,
        b: Math.round(this.Convert_sRGB_ToLinear(normalize(color.b), gammaValue) * 10000) / 10000,
        a: Math.round(color.a * 10000) / 10000 // Alpha remains linear
        //a: Math.round(normalize(color.a) * 10000) / 10000 // Alpha remains linear
    };

    return gammaCorrected;
}
// Function to reverse the gamma correction for comparison
function reverseGammaCorrected(gammaR, gammaG, gammaB, gammaA = 1.0, gammaValue = 2.4) {
    const result = { r: 255, g: 255, b: 255, a: 255 }; // Initialize with white and full alpha

    try {
        // Undo gamma correction (assuming power function)
        const invR = convert_sRGB_FromLinear(gammaR, gammaValue);
        const invG = convert_sRGB_FromLinear(gammaG, gammaValue);
        const invB = convert_sRGB_FromLinear(gammaB, gammaValue);

        // Approximate linear sRGB (assuming conversion to sRGB space)
        const linearSrgb = { r: invR, g: invG, b: invB };

        // Convert to RGB (assuming 0-255 range)
        result.r = this.safeRound(linearSrgb.r * 255);
        result.g = this.safeRound(linearSrgb.g * 255);
        result.b = this.safeRound(linearSrgb.b * 255);

        // Handle alpha (if provided)
        if (gammaA !== undefined) {
            result.a = this.safeRound(gammaA * 255);
        }
    } catch (error) {
        throw new Error(error.message + error.stack);
    }

    return result;
}
function reverseGammaCorrectedList(gammaComponents, gammaValue = 2.4) {
    try {
        if (!Array.isArray(gammaComponents) || gammaComponents.length < 3) {
            console.log(gammaComponents);
            throw new Error("Invalid gamma components list (requires at least 3 elements)");
        }

        const [gammaR, gammaG, gammaB, ...remaining] = gammaComponents;
        const gammaA = remaining.length > 0 ? remaining[0] : 1.0;

        return this.reverseGammaCorrected(gammaR, gammaG, gammaB, gammaA, gammaValue);
    } catch (error) {
        throw new Error(error.message + error.stack);
    }
};

// #endregion

/** Timer class to measure the time taken by a task.
 * USAGE:
 * const timer = new Timer();
 * timer.Start();
 * // Perform some task
 * const duration = timer.Stop();
 * console.log(`Task took ${timer.ElapsedTime} seconds to complete.`);
 */
export class Timer {
    constructor() {
        this.startTime = null;
    }
    Start() { // Changed startTimer to start, to match your example.
        this.startTime = performance.now();
    }

    Stop() { // Changed stopTimer to stop, to match your example.
        if (this.startTime === null) {
            console.error("Timer was not started.");
            return;
        }
        const endTime = performance.now();
        const duration = (endTime - this.startTime) / 1000;
        console.log(`The task took ${duration.toFixed(4)} seconds to complete.`);
        this.startTime = null; // Reset for next use
        return duration; //return the duration so that it can be used.
    }

    ElapsedTime() {
        if (this.startTime === null) {
            console.error("Timer was not started.");
            return 0; // or throw an error, depending on your needs.
        }
        const currentTime = performance.now();
        const elapsedTime = (currentTime - this.startTime) / 1000;
        return elapsedTime;
    }
}

export default {
    containsWord, containsCharacter, stringIn,
    trimAllSpaces, NVL,
    isEmpty, isNotNullOrEmpty,
    copyToClipboard, safeRound,

    isColorDark, GetForeColorFor,
    intToHexColor, intToRGBA, intToRGBAstring,
    rgbaToHex, RGBAtoColor, rgbaToInt, rgbaToString, rgbaArrayToHex,
    hexToRgba, hexToSignedInt, hexToUnsignedInt,

    GetGammaCorrected_RGBA, reverseGammaCorrected,
    reverseGammaCorrectedList,
    convert_sRGB_FromLinear, Convert_sRGB_ToLinear,
    convert_sRGB_FromLinear,

    compareVersions,
    Timer
}