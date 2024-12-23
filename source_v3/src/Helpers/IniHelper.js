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

function findValueByKey(data, searchItem) {
    const fileName = searchItem.File.replace(/-/g, '');  // Remove hyphens
    const section = searchItem.Section.toLowerCase();  // Convert section to lowercase
    const keys = searchItem.Key.split('|');  // Split multiple keys
  
    const fileData = data[fileName];
    const results = [];
    let keyCount = 0;
  
    try {
      if (fileData) {
        // Find the correct section in a case-insensitive manner
        const sectionKey = Object.keys(fileData).find(
          key => key.toLowerCase() === section
        );
  
        if (sectionKey && fileData[sectionKey]) {
          keys.forEach(key => {
            const value = fileData[sectionKey][key];
            if (value !== undefined) {
              results.push({ key, value });
              keyCount++; // Increment the counter for each found key
            } else {
              throw new Error(`Key not found: ${key}`);
            }
          });
        } else {
          throw new Error(`Section not found: ${fileName} / ${section}`);
        }
      } else {
        throw new Error(`File not found: ${fileName}`);
      }
    } catch (error) {
      console.error(error.message);
    }
  
    //console.log(`Total keys found: ${keyCount}`);
  
    // Return results as an array of key-value pairs, or the single value if only one key
    return results.length === 1 ? results[0].value : results;
  }
/*
function findValueByKey(data, searcher) {
    const { File, Section, Key } = searcher;
  
    for (const profileName in data) {
      const normalizedProfileName = profileName.replace(/-/g, ''); 

      if (normalizedProfileName === File) {
        if (data[normalizedProfileName][Section]) {
          if (Key.includes("|")) { 
            const keys = Key.split("|");
            const values = keys.map(key => data[normalizedProfileName][Section][key]);
            return values.filter(value => value !== undefined); 
          } else {
            return data[normalizedProfileName][Section][Key];
          }
        }
      }
    }
  
    console.log('Not Found: ', File, Section, Key);
    return undefined; // Key not found
  }
*/
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

export default { loadIniFile, saveIniFile, getValueFromSection, setValueInSection, findValueByKey }
