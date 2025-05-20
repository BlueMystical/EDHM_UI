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
 * @returns {Array} - An array of section objects representing the parsed INI data. */
function parseIni(iniString) {
  const sections = [];
  let currentSection = null;
  let comments = [];
  let logic = { blocks: [] };
  let inLogicBlock = false;
  let sectionComments = [];
  let currentSectionData = {};

  const lines = iniString.split('\n');

  for (const line of lines) {
    const trimmedLine = line;

    if (trimmedLine.trim().startsWith(';')) {
      comments.push(trimmedLine);

    } else if (trimmedLine.trim().startsWith('[') && trimmedLine.trim().endsWith(']')) {
      if (currentSection) {
        sections.push({
          name: currentSection,
          comments: sectionComments,
          keys: currentSectionData.keys || {},
          logic: logic.blocks.length > 0 ? logic : { blocks: [] },
        });
      }
      currentSection = trimmedLine.trim().slice(1, -1);
      currentSectionData = {};
      sectionComments = comments;
      comments = [];
      logic = { blocks: [] };
      inLogicBlock = false;

    } else if (trimmedLine.trim() === '') {
      comments.push(trimmedLine);

    } else if (trimmedLine.trim().startsWith('if') || trimmedLine.trim().startsWith('endif') || inLogicBlock) {
      if (!inLogicBlock) {
        logic.blocks.push({ comments: [...comments], lines: [] });
        comments = [];
      }

      logic.blocks[logic.blocks.length - 1].lines.push(trimmedLine);

      if (trimmedLine.trim().startsWith('if')) inLogicBlock = true;
      if (trimmedLine.trim().startsWith('endif')) inLogicBlock = false;

    } else if (currentSection) {
      const match = trimmedLine.trim().match(/^(.+?)\s*=\s*(.+)$/);
      if (match) {
        const keyName = match[1].trim();
        const value = match[2].trim();

        if (!currentSectionData.keys) currentSectionData.keys = {};
        currentSectionData.keys[keyName] = {
          name: keyName,
          value: String(value),
          comments: comments,
        };
        comments = [];
      }
    }
  }

  if (currentSection) {
    sections.push({
      name: currentSection,
      comments: sectionComments,
      keys: currentSectionData.keys || {},
      logic: logic.blocks.length > 0 ? logic : { blocks: [] },
    });
  }

  return sections;
}

/** Stringifies a parsed INI file structure into a string so it can be written to a file.
 * @param {Array} parsedIni - The parsed INI data structure.
 * @returns {string} - The stringified INI file. */
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

    if (section.logic && section.logic.blocks.length > 0) {
      for (const block of section.logic.blocks) {
        if (block.comments.length > 0) {
          iniString += block.comments.join('\n') + '\n';
        }
        iniString += block.lines.join('\n') + '\n';
      }
    }

    iniString += '\n';
  }

  return iniString;
}

// #endregion

/** Returns the value of a key in the given INI data object.
 * If the key does not exist in the specified section, it will return undefined.
 * @param {Array} iniData Parsed INI data
 * @param {string} section Name of the Section
 * @param {string} key Name of the Key */
function getKey(iniData, section, key) {
  if (iniData !== undefined) {
    //console.log('getKey:', { section, key });
    const sectionData = iniData.find(s => s.name && s.name.toLowerCase() === section.toLowerCase()); 
    if (sectionData && sectionData.keys && sectionData.keys[key]) {
      return String(sectionData.keys[key].value);
    }
  }
  return undefined;
}

/** Sets the value of a key in the given INI data object.
 * If the key does not exist in the specified section, it will be added.
 * @param {Array} iniData Parsed Ini data
 * @param {string} section Name of the Section
 * @param {string} key Name of the Key
 * @param {string} value Value to set
 * @param {string} [comment] Optional comments
 * @param {boolean} [addNewKey=false] If true, adds the key if it doesn't exist
 * @returns {Array|null} The modified iniData object, returns NULL if fails. */
function setKey(iniData, section, key, value, comment = undefined, addNewKey = false) {
  try {
    if (typeof key === 'string' && typeof value !== 'undefined') {
      //console.log('setKey:', { section, key, value });

      // Buscar la sección en el INI data
      let sectionData = iniData.find(s => s.name && s.name.toLowerCase() === section.toLowerCase()); 

      // Si la sección no existe y se permite agregar una nueva clave, crearla
      if (!sectionData && addNewKey) {
        sectionData = { name: section, comments: [], keys: {}, logic: { comments: [], lines: [] } };
        iniData.push(sectionData);
      }

      // Validar que `keys` exista antes de acceder a ella
      if (!sectionData || !sectionData.keys) {
        console.warn('Error: La sección no tiene estructura válida:', sectionData);
        return null;
      }

      // Si la clave ya existe, actualizar su valor
      if (sectionData.keys[key]) {
        sectionData.keys[key].value = String(value);
        if (typeof comment === 'string') {
          sectionData.keys[key].comments = comment.split('\n'); // Convertir comentario a array de líneas
        }
        return iniData;
      }

      // Si la clave no existe, agregarla si se permite `addNewKey`
      if (addNewKey) {
        sectionData.keys[key] = {
          name: key,
          value: String(value),
          comments: typeof comment === 'string' ? comment.split('\n') : [],
          logic: [],
        };
        return iniData;
      }

      console.warn('Clave no encontrada y no se permite agregar nuevas:', { section, key });
      return null;
    } else {
      console.warn('Error: Clave o valor no válidos:', { section, key, value });
      return null;
    }
  } catch (error) {
    console.error('Error en setKey@IniParser:', error);
    throw new Error(error.message + error.stack);
  }
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
