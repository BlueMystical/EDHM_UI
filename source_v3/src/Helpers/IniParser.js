import fs from 'node:fs';
import { ipcMain } from 'electron';

/** Load and returns a Parsed ini file.
 * @param {*} filePath absolute path to the file */
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

/** Writes the INI data to a file.
 * @param {*} filePath Absolute path to the file.
 * @param {*} iniData Data to be written to the INI file.*/
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

/** Parses an INI formatted string into a structured JavaScript object.
 * @param {string} iniString - The INI formatted string to parse.
 * @returns - An array of section objects representing the parsed INI data.*/
function parseIni(iniString) {
  const sections = []; // Array to store parsed sections.
  let currentSection = null; // Name of the current section being parsed.
  let comments = []; // Array to store comments encountered before keys.
  let logic = { comments: [], lines: [] }; // Object to store logic block data.
  let inLogicBlock = false; // Flag to indicate if currently parsing a logic block.
  let sectionComments = []; // Array to store comments encountered before a section.
  let currentSectionData = {}; // Object to store key-value pairs for the current section.

  const lines = iniString.split('\n'); // Split the INI string into lines.

  // Iterate through each line of the INI string.
  for (const line of lines) {
    const trimmedLine = line; // No need to trim yet, trim only when needed.

    // Check if the line is a comment.
    if (trimmedLine.trim().startsWith(';')) {
      comments.push(trimmedLine); // Store the comment.
      if (inLogicBlock) {
        logic.comments.push(trimmedLine); // Store logic block comments.
      }

      // Check if the line is a section header.
    } else if (trimmedLine.trim().startsWith('[') && trimmedLine.trim().endsWith(']')) {
      // If a section was being parsed, add it to the sections array.
      if (currentSection) {
        // If the logic block contains lines, include it in the section.
        if (logic.lines.length > 0) {
          sections.push({
            name: currentSection,
            comments: sectionComments,
            keys: currentSectionData.keys || {},
            logic: logic,
          });
        } else {
          // If the logic block is empty, create an empty logic object.
          sections.push({
            name: currentSection,
            comments: sectionComments,
            keys: currentSectionData.keys || {},
            logic: { comments: [], lines: [] },
          });
        }
      }
      // Start parsing a new section.
      currentSection = trimmedLine.trim().slice(1, -1);
      currentSectionData = {}; // Reset section data.
      sectionComments = comments; // Store section comments.
      comments = []; // Reset comments array.
      logic = { comments: [], lines: [] }; // Reset logic object.
      inLogicBlock = false; // Reset logic block flag.

      // Check if the line is an empty line.
    } else if (trimmedLine.trim() === '') {
      comments.push(trimmedLine); // Store empty lines as comments.

      // If a section is being parsed, process key-value pairs and logic blocks.
    } else if (currentSection) {
      // Check for logic block lines.
      if (trimmedLine.trim().startsWith('if') || trimmedLine.trim().startsWith('endif') || inLogicBlock) {
        logic.lines.push(trimmedLine); // Store logic line.
        if (trimmedLine.trim().startsWith('if')) {
          inLogicBlock = true; // Enter logic block.
        }
        if (trimmedLine.trim().startsWith('endif')) {
          inLogicBlock = false; // Exit logic block.
        }
      } else {
        // Check for key-value pair lines.
        const match = trimmedLine.trim().match(/^(.+?)\s*=\s*(.+)$/);
        if (match) {
          const keyName = match[1].trim(); // Extract key name.
          let value = match[2].trim(); // Extract value.

          // Create keys object if it doesn't exist.
          if (!currentSectionData.keys) {
            currentSectionData.keys = {};
          }

          // Store key-value pair with comments.
          currentSectionData.keys[keyName] = {
            name: keyName,
            value: String(value),
            comments: comments,
          };
          comments = []; // Reset comments array.
        }
      }
    }
  }

  // Add the last section if it exists.
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

  // Return the parsed sections.
  return sections;
}

/** Stringifies a parsed INI file structure into a string so it can be written on file. *
 * @param {Array<{
* name: string,
* comments?: string[],
* keys: Record<string, { name: string, value: string, comments?: string[] }>,
* logic?: { comments?: string[], lines: string[] }
* }>} parsedIni - The parsed INI data structure.
* @returns {string} - The stringified INI file.*/
function stringifyIni(parsedIni) {
  let iniString = '';

  // Iterate over each section in the parsed INI data.
  for (const section of parsedIni) {
    // Add section comments if they exist.
    if (section.comments && section.comments.length > 0) {
      iniString += section.comments.join('\n') + '\n';
    }

    // Add the section name.
    iniString += `[${section.name}]\n`;

    // Iterate over each key in the section.
    for (const keyName in section.keys) {
      const key = section.keys[keyName];

      // Add key comments if they exist.
      if (key.comments && key.comments.length > 0) {
        iniString += key.comments.join('\n') + '\n';
      }

      // Add the key-value pair.
      iniString += `${key.name} = ${key.value}\n`;
    }

    // Add logic section if it exists.
    if (section.logic && section.logic.lines.length > 0) {
      // Add logic comments if they exist.
      if (section.logic.comments && section.logic.comments.length > 0) {
        iniString += '\n'; // Add a newline before logic comments.
        iniString += section.logic.comments.join('\n') + '\n';
      } else {
        // Only add a newline if there are logic lines, but no comments.
        iniString += '\n';
      }
      // Add logic lines.
      iniString += section.logic.lines.join('\n');
    }

    // Add a newline to separate sections.
    iniString += '\n';
  }

  // Return the complete stringified INI file.
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
    const sectionData = iniData.find(s => s.name.toLowerCase() === section.toLowerCase()); // Case insensitive search
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
 * @param {*} addNewKey [Optional] If true, adds the key if it doesn't exist 
 * @returns  the whole iniData object modified, returns NULL if fails. */
function setKey(iniData, section, key, value, comment = undefined, addNewKey = false) {
  try {
    if (key !== undefined && value !== undefined) {
      let sectionData = iniData.find(s => s.name.toLowerCase() === section.toLowerCase()); // Case insensitive search
      if (!sectionData && addNewKey) {
        sectionData = { name: section, comments: [], keys: {}, logic: { comments: [], lines: [] } };
        iniData.push(sectionData);
      }

      if (sectionData.keys[key]) {
        // Key found and updated
        sectionData.keys[key].value = value;
        if (comment !== undefined) {
          sectionData.keys[key].comments = comment.split('\n'); // Convert comment string to array of lines
        }
        return iniData;
      } else {
        // Key not found in the section, add it
        if (addNewKey) {
          sectionData.keys[key] = {
            name: key,
            value: value,
            comments: comment ? comment.split('\n') : [], // Convert comment string to array, or use empty array
            logic: []
          };
          return iniData;
        } 
        //console.log('Key not found:', { section, key, value });   
        return null;    
      }
    } else {
      //console.warn('Key or Value is undefined:', { section, key, value });
      return null;
    }
  } catch (error) {
    console.log('setKey@IniParser:', error);
    throw new Error(error.message + error.stack);
  }
  return iniData;
}

//- Legacy Method:
function setIniValue(iniData, sectionName, keyName, newValue, comment = undefined) {
  return setKey(iniData, sectionName, keyName, newValue, comment); 
}

//- Legacy Method:
function getIniValue(iniData, sectionName, keyName) {
  return getKey(iniData, sectionName, keyName);
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
ipcMain.handle('INI-GetKey', (event, iniData, section, key) => {
  return getKey(iniData, section, key);
});
ipcMain.handle('INI-SetKey', (event, iniData, section, key, value, comment, addNewKey) => {
  return setKey(iniData, section, key, value, comment, addNewKey);
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
