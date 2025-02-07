/*  COLECTION OF UTILITY METHODS
 *  They can be used in both Process (Main & Renderer) */

function containsWord(str, word) { return str.includes(word); };

/** To Check is something is Empty
 * @param obj Object to check
 */
function isEmpty(obj) { Object.keys(obj).length === 0 };

function safeRound(value) { return isNaN(value) ? 0 : Math.round(value); }

/** Null-Empty-Uninstanced verification * 
 * @param {*} value Object, String or Array
 * @returns True or False
 */
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

// #region Color Conversion Methods

/**
 * Converts an array of Integers to an RGBA string
 * @param {*} colorComponents Array of Integer Numbers
 * @returns 
 */
function RGBAtoColor(colorComponents) {
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
/** Converts an RGB or RGBA or individual r,g,b,a components into a HEX color String with transparency. * 
 * @param {*} r 
 * @param {*} g 
 * @param {*} b 
 * @param {*} a 
 * @returns 
 */
function rgbaToHex(r, g, b, a) {
    let rVal = r;
    let gVal = g;
    let bVal = b;
    let aVal = a;

    if (typeof r === 'object' && r !== null) {
        ({ r: rVal, g: gVal, b: bVal, a: aVal = 255 } = r); // Destructure, using different variable names
    } else {
        aVal = a === undefined ? 255 : a;
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
    const aHex = componentToHex(aVal);

    return aVal === 255 ? `#${rHex}${gHex}${bHex}` : `#${rHex}${gHex}${bHex}${aHex}`;
}

function rgbaToInt(rgba) {
    // Ensure values are within the valid range (0-255)
    const r = Math.max(0, Math.min(255, rgba.r));
    const g = Math.max(0, Math.min(255, rgba.g));
    const b = Math.max(0, Math.min(255, rgba.b));
    const a = Math.max(0, Math.min(255, rgba.a));

    // Convert each component to its hexadecimal representation (2 digits)
    const rHex = r.toString(16).padStart(2, '0');
    const gHex = g.toString(16).padStart(2, '0');
    const bHex = b.toString(16).padStart(2, '0');
    const aHex = a.toString(16).padStart(2, '0');

    // Concatenate the hex values to form the ARGB hex string
    const argbHex = aHex + rHex + gHex + bHex;

    // Convert the ARGB hex string to a signed 32-bit integer
    const intValue = parseInt(argbHex, 16);

    return intValue;
}

//---------------------------------------------

/** Converts a signed integer to a HEX color string with optional alpha information.
 * @param {number} number - The signed integer number.
 * @returns {string} - The HEX color string. */
function intToHexColor(number) {
    // Handle negative numbers by adding 2^32 to ensure positive value within range
    if (number < 0) {
        number = 4294967296 + number; // 2^32
    }

    // Extract the RGBA components
    const aVal = (number >> 24) & 0xFF; // Extract alpha
    const rVal = (number >> 16) & 0xFF; // Extract red
    const gVal = (number >> 8) & 0xFF; // Extract green
    const bVal = number & 0xFF; // Extract blue

    // Convert components to HEX strings
    const componentToHex = (c) => {
        const hex = c.toString(16).toUpperCase();
        return hex.length === 1 ? "0" + hex : hex;
    };

    const rHex = componentToHex(rVal);
    const gHex = componentToHex(gVal);
    const bHex = componentToHex(bVal);
    const aHex = componentToHex(aVal);

    // Return HEX color string, including alpha even if it's 255 (opaque)
    return `#${aHex}${rHex}${gHex}${bHex}`;
}

/** Converts a signed integer to an RGBA object.
 * @param {number} number - The signed integer number.
 * @returns {object} - An object with properties r, g, b, and a. */
function intToRGBA(number) {
    // Handle negative numbers by adding 2^32 to ensure positive value within range
    if (number < 0) {
        number = 4294967296 + number; // 2^32
    }

    // Extract the RGBA components
    const aVal = (number >> 24) & 0xFF; // Extract alpha
    const rVal = (number >> 16) & 0xFF; // Extract red
    const gVal = (number >> 8) & 0xFF; // Extract green
    const bVal = number & 0xFF; // Extract blue

    // Return an object with RGBA properties
    return {
        r: rVal,
        g: gVal,
        b: bVal,
        a: aVal // Alpha value remains within the range of 0 to 255
    };
}

//---------------------------------------------

/** Converts a HEX color string to RGBA color components. * 
 * @param {*} hexColor '#ff0000, ff0000, f00, #ff000080 
 * @returns an object having the RGBA color components. */
function hexToRgba(hexColor) {
    hexColor = hexColor.replace("#", "");

    const validLengths = [3, 6, 8];
    if (!validLengths.includes(hexColor.length)) {
        return null; // Invalid length
    }

    if (hexColor.length === 3) {
        hexColor = hexColor.replace(/(.)/g, "$1$1"); // Expand shorthand
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
        a: Math.round(normalize(color.a) * 10000) / 10000 // Alpha remains linear
    };

    return gammaCorrected;
}
// Function to reverse the gamma correction for comparison
function reverseGammaCorrected(gammaR, gammaG, gammaB, gammaA = 1.0, gammaValue = 2.4) {
    const result = { r: 255, g: 255, b: 255, a: 255 }; // Initialize with white and full alpha

    try {
        // Undo gamma correction (assuming power function)
        const invR = Math.pow(gammaR, 1 / gammaValue);
        const invG = Math.pow(gammaG, 1 / gammaValue);
        const invB = Math.pow(gammaB, 1 / gammaValue);

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
        throw error;
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
        throw error;
    }
};

// #endregion


export default {
    containsWord, isEmpty, isNotNullOrEmpty,
    copyToClipboard, safeRound, 

    intToHexColor, intToRGBA, 
    rgbaToHex, RGBAtoColor, rgbaToInt,
    hexToRgba, hexToSignedInt, hexToUnsignedInt, 

    GetGammaCorrected_RGBA, reverseGammaCorrected, 
    reverseGammaCorrectedList,
    convert_sRGB_FromLinear, Convert_sRGB_ToLinear,
    convert_sRGB_FromLinear,
}