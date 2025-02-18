import path from 'node:path';
import { ipcMain } from 'electron';
import { stringify , parse } from 'ini';
import { writeFile , readFile } from 'node:fs/promises';


/**
 * Loads the INI file from the specified path.
 *
 * @param {string} filePath - The path to the INI file.
 * @returns {object} - The parsed INI data as an object, or an empty object on error.
 */
async function loadIniFile(filePath) {
  let _ret = {};
    try {
        //  Read INI file as text
        let text = await readFile(filePath, {
          encoding : 'utf-8'
        });

        //  Parse text data to object
        _ret = parse(text);

    } catch (error) {
        //console.error(`Error loading INI file: ${filePath}`, error);
        throw new Error(error.message + error.stack);
    }
    return _ret;
}

/**
* Saves the modified INI object to the file.
*
* @param {string} filePath - The path to the INI file.
* @param {object} iniData - The modified INI data as an object.
*/
async function saveIniFile(filePath, iniData) {
  let _ret = false;
    try { 
      if (iniData) {
        const text = stringify(iniData,{ 
            whitespace : true ,     //<- Whether to insert spaces before & after `=`
            sort : false ,          //<- Whether to sort all sections & their keys alphabetically.
            bracketedArray : true   //<- Whether to append `[]` to array keys.
        });

        //console.log(text);
        
        //  Write INI file as text      
        await writeFile(filePath, text); 
        _ret = true;

      } else {
        throw new Error('404 - No Data to save!');
      }
    } catch (error) {
        console.log(error);
        throw new Error(error.message + error.stack);
    }
    return _ret;
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

function getXmlValueByKey(xml_profile, targetKey) {
  for (let i = 0; i < xml_profile.length; i++) {
      if (xml_profile[i].key === targetKey) {
          return xml_profile[i].value;
      }
  }
  return null; // If the key is not found
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
  let _ret = false;
  try {
    if (!iniData[section]) {
        iniData[section] = {};
    }
    iniData[section][key] = value;
    _ret = true;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
  return _ret;
}



/** Finds a Key/Value in an INI file
 * @param {*} data Parsed data of all INI files
 * @param {*} searchItem One of the Elements from a Theme Template
 * @returns Return results as an array of key-value pairs, or the single value if only one key */
function findValueByKey(data, searchItem) {
    /* searchItem: {
        ...
        "File":"Startup-Profile",
        "Section":"Constants",
        "Key":"x137", //<- or 'x138|y138|z138|w138'
        "Value":100,
        "ValueType":"Preset",
        ...
      } 
    */
    const results = [];
  
    try {
      const fileName = searchItem.File.replace(/-/g, '');  // Remove hyphens
      const section = searchItem.Section.toLowerCase();   // Convert section to lowercase
      const keys = searchItem.Key.split('|');             // Split multiple keys  
      
      const fileData = data[fileName]; //console.log('fileName:', fileName);

      if (fileData) {
        // Find the correct section in a case-insensitive manner:
        const sectionData = fileData.find(element => element.Section.toLowerCase() === section);
        if (sectionData && sectionData.Keys) {

          try {
            keys.forEach(keyName => {
              const key = sectionData.Keys.find(item => item.Key.toLowerCase() === keyName.toLowerCase());
              results.push({ key: key.Key, value: key.Value });
            });
          } catch (error) {
            //console.log('Key: ' + keys + ' Not Found on ' + fileName + '/' + section);
          }

        } else {
          throw new Error(`Section Not Found: ${path.join(fileName, searchItem.Section)} `);
        }
      } else {
        //console.log('data:', data);
        //throw new Error(`File Not Found: findValueByKey/${path.join(fileName, searchItem.Section)}`);
      }
    } catch (error) {
      console.error(error);
      //throw new Error(error.message + error.stack);
    }
  
    // Return results as an array of key-value pairs, or the single value if only one key
    let _ret = results.length === 1 ? results[0].value : results;
    if (Array.isArray(_ret) && _ret.length < 1) _ret = null; //<- Empty Arrays not allowed

    return _ret;
  }

/* -----------------------------------------------------------------------------------------------------*/
// Expose functions through ipcMain

ipcMain.handle('loadIniFile', async (event, filePath) => {
  try {
    return await loadIniFile(filePath);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }    
});
ipcMain.handle('saveIniFile', async (event, filePath, iniData) => {
  try {
    return saveIniFile(filePath, iniData);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }    
});

ipcMain.handle('getValueFromSection', async (event, iniData, section, key, defaultValue) => {
  try {
    return getValueFromSection(iniData, section, key, defaultValue);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }    
});
ipcMain.handle('setValueInSection', async (event, iniData, section, key, value) => {
  try {
    return setValueInSection(iniData, section, key, value);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }    
});

export default { loadIniFile, saveIniFile, getValueFromSection, setValueInSection, findValueByKey }
