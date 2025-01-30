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

/** Convert RGB to a single decimal value
 * @param {*} color { r, g, b }
 * @returns 
 */
function getColorDecimalValue(color) {
    // Convert RGB to a single decimal value (e.g., for use with some UI libraries)
    return (color.r << 16) + (color.g << 8) + color.b;
};

function intToHex(int) {
    try {
        // Ensure unsigned 32-bit
        int >>>= 0;

        // Convert each component to a two-digit hex string
        const r = ((int >> 16) & 0xFF).toString(16).padStart(2, '0');
        const g = ((int >> 8) & 0xFF).toString(16).padStart(2, '0');
        const b = (int & 0xFF).toString(16).padStart(2, '0');

        return `#${r}${g}${b}`;
    } catch (error) {
        throw error;
    }
};

/** Convert a Color from Number to a Hex HTML string.
* @param number Integer Value representing a Color.
*/
function intToHexColor(int) {
    // Ensure unsigned 32-bit
    int >>>= 0;

    // Convert each component to a two-digit hex string
    const r = ((int >> 16) & 0xFF).toString(16).padStart(2, '0');
    const g = ((int >> 8) & 0xFF).toString(16).padStart(2, '0');
    const b = (int & 0xFF).toString(16).padStart(2, '0');

    return `#${r}${g}${b}`;
}
function intToRGB(num) {
    // Convert the signed integer to an unsigned integer
    const unsignedNum = num >>> 0;

    // Extract red, green, and blue components
    const r = (unsignedNum >> 16) & 0xFF;
    const g = (unsignedNum >> 8) & 0xFF;
    const b = unsignedNum & 0xFF;

    return { r, g, b };
}
// Function to convert an integer value to RGBA components
function intToRGBA(colorInt) {
    const r = (colorInt >> 16) & 0xFF;
    const g = (colorInt >> 8) & 0xFF;
    const b = colorInt & 0xFF;
    const a = ((colorInt >> 24) & 0xFF) || 255; // Default alpha to 255 if not present

    return { r, g, b, a };
}
function rgbToHex(r, g, b, a = 1) {
    // Ensure values are within valid range
    r = Math.max(0, Math.min(255, r));
    g = Math.max(0, Math.min(255, g));
    b = Math.max(0, Math.min(255, b));
    a = Math.max(0, Math.min(1, a));

    // Convert alpha to hexadecimal
    const alphaHex = Math.round(a * 255).toString(16).padStart(2, '0');

    // Convert RGB to hexadecimal
    const hex = "#" +
        r.toString(16).padStart(2, '0') +
        g.toString(16).padStart(2, '0') +
        b.toString(16).padStart(2, '0');

    return a < 1 ? hex + alphaHex : hex; // Return hex with alpha if alpha is less than 1
}

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
    copyToClipboard, safeRound, intToHex,
    intToHexColor, intToRGB, intToRGBA, rgbToHex, RGBAtoColor,
    GetGammaCorrected_RGBA, reverseGammaCorrected,
    getColorDecimalValue, reverseGammaCorrectedList,
    convert_sRGB_FromLinear, Convert_sRGB_ToLinear,
    convert_sRGB_FromLinear,
}