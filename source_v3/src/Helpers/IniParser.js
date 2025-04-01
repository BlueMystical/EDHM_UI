import fs from 'node:fs';
import { ipcMain } from 'electron';

export async function LoadIniFile(filePath) {
  try {
    console.log('Loading ini..', filePath);
    if (!fs.existsSync(filePath)) {
      console.warn(`File not found: ${filePath}`);
      return null;
    }
    const data = fs.readFileSync(filePath, 'utf-8');
    return parseIni(data);
  } catch (error) {
    console.error('Error loading INI file:', error);
    return null;
  }
}

export async function SaveIniFile(filePath, iniData) {
  try {
    const iniString = stringifyIni(iniData);
    fs.writeFileSync(filePath, iniString, 'utf-8');
    return true;
  } catch (error) {
    console.error('Error saving INI file:', error);
    return false;
  }
}

// #region Private Functions

function parseIni(iniString) {
  const sections = [];
  let currentSection = null;
  let comments = [];
  let logic = { comments: [], lines: [] };
  let inLogicBlock = false;
  let sectionComments = [];
  let currentSectionData = {};

  const lines = iniString.split('\n');

  for (const line of lines) {
    const trimmedLine = line;

    if (trimmedLine.trim().startsWith(';')) {
      comments.push(trimmedLine);
      if (inLogicBlock) {
        logic.comments.push(trimmedLine);
      }

    } else if (trimmedLine.trim().startsWith('[') && trimmedLine.trim().endsWith(']')) {
      if (currentSection) {
        if (logic.lines.length > 0) {
          sections.push({
            name: currentSection,
            comments: sectionComments,
            keys: currentSectionData.keys || {},
            logic: logic,
          });
        } else {
          sections.push({
            name: currentSection,
            comments: sectionComments,
            keys: currentSectionData.keys || {},
            logic: { comments: [], lines: [] },
          });
        }
      }
      currentSection = trimmedLine.trim().slice(1, -1);
      currentSectionData = {};
      sectionComments = comments;
      comments = [];
      logic = { comments: [], lines: [] };
      inLogicBlock = false;

    } else if (trimmedLine.trim() === '') {
      comments.push(trimmedLine);

    } else if (currentSection) {
      if (trimmedLine.trim().startsWith('if') || trimmedLine.trim().startsWith('endif') || inLogicBlock) {
        logic.lines.push(trimmedLine);
        if (trimmedLine.trim().startsWith('if')) {
          inLogicBlock = true;
        }
        if (trimmedLine.trim().startsWith('endif')) {
          inLogicBlock = false;
        }

      } else {
        const match = trimmedLine.trim().match(/^(.+?)\s*=\s*(.+)$/);
        if (match) {
          const keyName = match[1].trim();
          let value = match[2].trim();

          if (!currentSectionData.keys) {
            currentSectionData.keys = {};
          }

          currentSectionData.keys[keyName] = {
            name: keyName, // Add the key name here
            value: String(value),
            comments: comments,
          };
          comments = [];
        }
      }
    }
  }

  if (currentSection) {
    if (logic.lines.length > 0) {
      sections.push({
        name: currentSection,
        comments: sectionComments,
        keys: currentSectionData.keys || {},
        logic: logic,
      });
    } else {
      sections.push({
        name: currentSection,
        comments: sectionComments,
        keys: currentSectionData.keys || {},
        logic: { comments: [], lines: [] },
      });
    }
  }

  return sections;
}

function stringifyIni(parsedIni) {
  let iniString = '';

  for (const section of parsedIni) {
    if (section.comments && section.comments.length > 0) {
      iniString += section.comments.join('\n') + '\n';
    }

    iniString += `[${section.name}]\n`;

    for (const keyName in section.keys) {
      const key = section.keys[keyName];
      if (key.comments && key.comments.length > 0) {
        iniString += key.comments.join('\n') + '\n';
      }
      iniString += `${key.name} = ${key.value}\n`;
    }

    if (section.logic && section.logic.lines.length > 0) {
      if (section.logic.comments && section.logic.comments.length > 0) {
        iniString += '\n';
        iniString += section.logic.comments.join('\n');
      }
      iniString += '\n';
      iniString += section.logic.lines.join('\n');
      //iniString += '\n';
    }

    iniString += '\n';
  }

  return iniString;
}

// #endregion

/** Returns the value of a key in the given INI data object.
 * If the key does not exist in the specified section, it will return undefined. 
 * @param {*} iniData Parsed Ini data
 * @param {*} section Name of the Section
 * @param {*} key Name of the Key */
function getKey(iniData, section, key) {
  if (iniData != undefined) {
    const sectionData = iniData.find(s => s.name === section);
    if (sectionData && sectionData.keys && sectionData.keys[key]) {
      return String(sectionData.keys[key].value);
    }
  }
  return undefined;
}

/** Sets the value of a key in the given INI data object.
 * If the key does not exist in the specified section, it will be added.
 * @param {*} iniData Parsed Ini data
 * @param {*} section Name of the Section
 * @param {*} key Name of the Key
 * @param {*} value Value to set
 * @param {*} comment [Optional] Comments
 * @returns  the whole iniData object modified */
function setKey(iniData, section, key, value, comment = undefined) {
  try {
    let sectionData = iniData.find(s => s.name === section);

    if (!sectionData) {
      sectionData = { name: section, comments: [], keys: {}, logic: { comments: [], lines: [] } };
      iniData.push(sectionData);
    }

    if (sectionData.keys[key]) {
      // Key found and updated
      sectionData.keys[key].value = value;
      if (comment !== undefined) {
        sectionData.keys[key].comments = comment.split('\n'); // Convert comment string to array of lines
      }
    } else {
      // Key not found in the section, add it
      sectionData.keys[key] = {
        name: key,
        value: value,
        comments: comment ? comment.split('\n') : [] // Convert comment string to array, or use empty array
      };
    }

  } catch (error) {
    console.log('setKey@IniParser:', error);
    throw new Error(error.message + error.stack);
  }
  return iniData;
}

/** Sets the value of a key in the given INI data object.
 * If the key does not exist in the specified section, it will be added.
 * @param {object[]} iniData - The INI data object (array of sections).
 * @param {string} sectionName - The name of the section (e.g., 'constants').
 * @param {string} keyName - The name of the key to set.
 * @param {any} newValue - The new value to assign to the key.
 * @param {string|undefined} comment - Optional comment for the key.
 * @returns {object[]} The modified INI data object.
 */
function setIniValue(iniData, sectionName, keyName, newValue, comment = undefined) {
  return setKey(iniData, sectionName, keyName, newValue, comment); // iniData is invalid
}

/** Gets the value of a key from the given INI data object.
* @param {object[]} iniData - The INI data object (array of sections).
* @param {string} sectionName - The name of the section (e.g., 'constants').
* @param {string} keyName - The name of the key to retrieve.
* @returns {any|undefined} The value of the key, or undefined if the key is not found. */
function getIniValue(iniData, sectionName, keyName) {
  return getKey(iniData, sectionName, keyName); // Not found
}



/** Finds a Key/Value in an INI file
 * @param {object[]} data Parsed data of all INI files
 * @param {object} searchItem One of the Elements from a Theme Template
 * @returns {any|any[]} Return results as an array of key-value pairs, or the single value if only one key */
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
  console.log('findValueByKey:', data);
  const results = [];

  try {
    const fileName = searchItem.File.replace(/-/g, ''); // Remove hyphens
    const section = searchItem.Section.toLowerCase(); // Convert section to lowercase
    const keys = searchItem.Key.split('|'); // Split multiple keys

    console.log('findValueByKey:', data[fileName]);
    const fileData = data[fileName].find(item => item.name.toLowerCase() === section); //<- Find section in array.

    if (fileData) {
      try {
        keys.forEach(keyName => {
          if (fileData.keys[keyName.toLowerCase()]) {
            results.push({ key: keyName, value: fileData.keys[keyName.toLowerCase()].value });
          }
        });
      } catch (error) {
        //console.log('Key: ' + keys + ' Not Found on ' + fileName + '/' + section);
      }
    } else {
      throw new Error(`Section Not Found: ${searchItem.Section}`);
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

/** Deletes a key from a section in the INI data.
 * @param {object[]} iniData - The INI data object (array of sections).
 * @param {string} sectionName - The name of the section.
 * @param {string} keyName - The name of the key to delete. */
export function deleteKey(iniData, sectionName, keyName) {
  const sectionData = iniData.find(s => s.name === sectionName);
  if (sectionData && sectionData.keys) {
    delete sectionData.keys[keyName];
  }
}

/** Adds a key-value pair to a section in the INI data.
 * @param {object[]} iniData - The INI data object (array of sections).
 * @param {string} sectionName - The name of the section.
 * @param {string} keyName - The name of the key to add.
 * @param {any} value - The value to associate with the key.
 * @param {string} comment - Optional comment for the key. */
export function addKey(iniData, sectionName, keyName, value, comment = "") {
  let sectionData = iniData.find(s => s.name === sectionName);
  if (!sectionData) {
    sectionData = { name: sectionName, comments: [], keys: {}, logic: { comments: [], lines: [] } };
    iniData.push(sectionData);
  }
  if (!sectionData.keys) {
    sectionData.keys = {};
  }
  sectionData.keys[keyName] = {
    name: keyName,
    value: value,
    comments: comment ? comment.split('\n') : []
  };
}


// ... (IPC Handlers)
ipcMain.handle('INI-LoadFile', async (event, filePath) => {
  return await LoadIniFile(filePath);
});
ipcMain.handle('INI-SaveFile', async (event, filePath, iniData) => {
  return await SaveIniFile(filePath, iniData);
});
ipcMain.handle('INI-GetKey', async (event, iniData, section, key) => {
  return await getKey(iniData, section, key);
});
ipcMain.handle('INI-SetKey', (event, iniData, section, key, value, comment) => {
  return setKey(iniData, section, key, value, comment);
});
ipcMain.handle('INI-SetValue', (event, iniData, section, key, value, comment) => {
  return setIniValue(iniData, fileName, sectionName, keyName, newValue);
});
ipcMain.handle('INI-AddKey', (event, iniData, section, key, value, comment) => {
  return addKey(iniData, section, key, value, comment);
});
ipcMain.handle('INI-DeleteKey', (event, iniData, section, key) => {
  return deleteKey(iniData, section, key);
});

export default {
  LoadIniFile, SaveIniFile,
  getKey, setKey, setIniValue, getIniValue,
  deleteKey, addKey,
  findValueByKey
}
