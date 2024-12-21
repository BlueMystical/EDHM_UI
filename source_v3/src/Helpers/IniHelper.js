import * as ini from 'ini';
import * as fs from 'fs';
import { ipcMain } from 'electron';

/**
 * Loads the INI file from the specified path.
 *
 * @param {string} filePath - The path to the INI file.
 * @returns {object} - The parsed INI data as an object, or an empty object on error.
 */
function loadIniFile(filePath) {
    try {
        const data = fs.readFileSync(filePath, 'utf-8');
        return ini.parse(data);
    } catch (error) {
        console.error(`Error loading INI file: ${filePath}`, error);
        return {};
    }
}

/**
 * Gets a specific value from a section in the INI object.
 *
 * @param {object} iniData - The parsed INI data as an object.
 * @param {string} section - The name of the section.
 * @param {string} key - The name of the key.
 * @param {*} defaultValue - The default value to return if the key is not found.
 * @returns {*} - The value of the key, or the default value if not found.
 */
function getValueFromSection(iniData, section, key, defaultValue) {
    return iniData[section] && iniData[section][key] || defaultValue;
}

/**
 * Sets a value in a section of the INI object.
 *
 * @param {object} iniData - The parsed INI data as an object.
 * @param {string} section - The name of the section.
 * @param {string} key - The name of the key.
 * @param {*} value - The new value for the key.
 */
function setValueInSection(iniData, section, key, value) {
    if (!iniData[section]) {
        iniData[section] = {};
    }
    iniData[section][key] = value;
}

/**
* Saves the modified INI object to the file.
*
* @param {string} filePath - The path to the INI file.
* @param {object} iniData - The modified INI data as an object.
*/
function saveIniFile(filePath, iniData) {
    try {
        fs.writeFileSync(filePath, ini.stringify(iniData));
        return true;
    } catch (error) {
        console.error(`Error saving INI file: ${filePath}`, error);
        return false;
    }
}
/* -----------------------------------------------------------------------------------------------------*/
// Expose functions through ipcMain

ipcMain.handle('loadIniFile', async (event, filePath) => {
    return await loadIniFile(filePath);
});
ipcMain.handle('saveIniFile', async (event, filePath) => {
    return await saveIniFile(filePath);
});

ipcMain.handle('getValueFromSection', async (event, iniData, section, key, defaultValue) => {
    return getValueFromSection(iniData, section, key, defaultValue);
});
ipcMain.handle('setValueInSection', async (event, iniData, section, key, value) => {
    return setValueInSection(iniData, section, key, value);
});

export default { loadIniFile, saveIniFile, getValueFromSection, setValueInSection }
