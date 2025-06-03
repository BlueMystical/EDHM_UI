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
    const trimmedLine = line.trimEnd(); // 🔥 No elimina espacios iniciales

    if (trimmedLine.startsWith(';')) {
      comments.push(line); // 🔥 Usa `line` en lugar de `trimmedLine` para preservar espacios

    } else if (trimmedLine.startsWith('[') && trimmedLine.endsWith(']')) {
      if (currentSection) {
        sections.push({
          name: currentSection,
          comments: sectionComments,
          keys: currentSectionData.keys || [],
          logic: logic.blocks.length > 0 ? logic : { blocks: [] },
        });
      }
      currentSection = trimmedLine.slice(1, -1);
      currentSectionData = {};
      sectionComments = comments;
      comments = [];
      logic = { blocks: [] };
      inLogicBlock = false;

    } else if (trimmedLine === '') {
      comments.push(line);

    } else if (trimmedLine.startsWith('if') || trimmedLine.startsWith('endif') || inLogicBlock) {
      if (!inLogicBlock) {
        logic.blocks.push({ comments: [...comments], lines: [] });
        comments = [];
      }
      logic.blocks[logic.blocks.length - 1].lines.push(line); // 🔥 Guarda la línea EXACTAMENTE como fue leída

      if (trimmedLine.startsWith('if')) inLogicBlock = true;
      if (trimmedLine.startsWith('endif')) inLogicBlock = false;

    } else if (currentSection) {
      // 🛠 Nueva expresión regular: Acepta espacios en nombres de clave
      const match = line.match(/^(.+?)\s*=\s*(.*)$/);
      if (match) {
        const keyName = match[1]; // 🔥 Guarda la clave exactamente como está
        const value = match[2] !== undefined ? match[2].trimEnd() : ""; // 🔥 No elimina espacios iniciales

        if (!currentSectionData.keys) currentSectionData.keys = [];

        currentSectionData.keys.push({
          name: keyName,
          value: value,
          comments: comments,
        });

        comments = [];
      }
    }
  }

  if (currentSection) {
    sections.push({
      name: currentSection,
      comments: sectionComments,
      keys: currentSectionData.keys || [],
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
    if (section.comments.length > 0) {
      iniString += section.comments.join('\n') + '\n';
    }

    iniString += `[${section.name}]\n`;

    for (const key of section.keys) {
      if (key.comments.length > 0) {
        iniString += key.comments.join('\n') + '\n';
      }
      iniString += `${key.name} = ${key.value}\n`; // 🔥 No se cambia nada de la clave
    }

    // 🔥 Conservar bloques lógicos sin alterar formato ni indentación
    if (section.logic.blocks.length > 0) {
      for (const block of section.logic.blocks) {
        if (block.comments.length > 0) {
          iniString += block.comments.join('\n') + '\n';
        }
        iniString += block.lines.join('\n') + '\n'; // 🔥 No se toca la indentación original
      }
    }
  }

  return iniString.trim(); // ⚡ Solo elimina líneas en blanco al final, NO en el contenido
}

// #endregion

/** Returns the value of a key in the given INI data object.
 * If the key does not exist in the specified section, it will return undefined.
 * @param {Array} iniData Parsed INI data
 * @param {string} section Name of the Section
 * @param {string} key Name of the Key */
function getKey(iniData, section, key) {
  if (iniData !== undefined) {
    const sectionData = iniData.find(s => s.name && s.name.toLowerCase() === section.toLowerCase()); 
    if (sectionData && sectionData.keys.length > 0) {
      const keyData = sectionData.keys.find(k => k.name.toLowerCase() === key.toLowerCase());
      if (keyData) return String(keyData.value);
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
      let sectionData = iniData.find(s => s.name && s.name.toLowerCase() === section.toLowerCase()); 

      if (!sectionData && addNewKey) {
        sectionData = { name: section, comments: [], keys: [], logic: { blocks: [] } };
        iniData.push(sectionData);
      }

      if (!sectionData || !sectionData.keys) {
        console.warn('Error: La sección no tiene estructura válida:', sectionData);
        return null;
      }

      // Buscar la clave dentro de la lista de keys
      let keyData = sectionData.keys.find(k => k.name.toLowerCase() === key.toLowerCase());

      if (keyData) {
        keyData.value = String(value);
        if (typeof comment === 'string') {
          keyData.comments = comment.split('\n'); // Convertir comentario a array de líneas
        }
        return iniData;
      }

      // Si la clave no existe, agregarla si se permite `addNewKey`
      if (addNewKey) {
        sectionData.keys.push({
          name: key,
          value: String(value),
          comments: typeof comment === 'string' ? comment.split('\n') : [],
        });
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

//-----------------------------------------------------------------;

/** Finds a Key/Value in an INI file
 * @param {object[]} data Parsed data of all INI files
 * @param {object} searchItem One of the Elements from a Theme Template
 * @returns {any|any[]} Return results as an array of key-value pairs, or the single value if only one key */
function findValueByKey(data, searchItem) {
  console.log('findValueByKey:', data);
  const results = [];

  try {
    const fileName = searchItem.File.replace(/-/g, ''); // Remove hyphens
    const section = searchItem.Section.toLowerCase(); // Convert section to lowercase
    const keys = searchItem.Key.split('|'); // Split multiple keys

    console.log('findValueByKey:', data[fileName]);
    const fileData = data[fileName].find(item => item.name.toLowerCase() === section); // Find section in array.

    if (fileData) {
      try {
        keys.forEach(keyName => {
          const keyData = fileData.keys.find(k => k.name.toLowerCase() === keyName.toLowerCase()); // Fix for array-based keys
          if (keyData) {
            results.push({ key: keyName, value: keyData.value });
          }
        });
      } catch (error) {
        console.warn(`Key(s) ${keys} Not Found in ${fileName}/${section}`);
      }
    } else {
      throw new Error(`Section Not Found: ${searchItem.Section}`);
    }
  } catch (error) {
    console.error(error);
  }

  // Return results as an array of key-value pairs, or the single value if only one key
  return results.length === 1 ? results[0].value : results.length > 0 ? results : null;
}

/** Deletes a key from a section in the INI data.
 * @param {object[]} iniData - The INI data object (array of sections).
 * @param {string} sectionName - The name of the section.
 * @param {string} keyName - The name of the key to delete. */
export function deleteKey(iniData, sectionName, keyName) {
  const sectionData = iniData.find(s => s.name.toLowerCase() === sectionName.toLowerCase());
  if (sectionData && sectionData.keys) {
    sectionData.keys = sectionData.keys.filter(k => k.name.toLowerCase() !== keyName.toLowerCase()); // Filter out the key
  }
}

/** Adds a key-value pair to a section in the INI data.
 * @param {object[]} iniData - The INI data object (array of sections).
 * @param {string} sectionName - The name of the section.
 * @param {string} keyName - The name of the key to add.
 * @param {any} value - The value to associate with the key.
 * @param {string} comment - Optional comment for the key. */
export function addKey(iniData, sectionName, keyName, value, comment = "") {
  let sectionData = iniData.find(s => s.name.toLowerCase() === sectionName.toLowerCase());
  
  if (!sectionData) {
    sectionData = { name: sectionName, comments: [], keys: [], logic: { blocks: [] } }; // Fix for array-based keys
    iniData.push(sectionData);
  }

  if (!sectionData.keys) {
    sectionData.keys = [];
  }

  sectionData.keys.push({
    name: keyName,
    value: String(value),
    comments: comment ? comment.split('\n') : []
  });
}
//-----------------------------------------------------------------;

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
