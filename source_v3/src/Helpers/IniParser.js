//import fs from 'node:fs';
import fs from 'fs/promises';
import { ipcMain } from 'electron';

/** Load and returns a Parsed ini file.
 * @param {string} filePath Absolute path to the file
 * @returns {Promise<Object|null>} Parsed INI data or null if error occurs */
async function LoadIniFile(filePath) {
    try {
        if (!filePath) throw new Error('Invalid file path.');
        
        console.log('Loading INI...', filePath);
        await fs.access(filePath); // Verifica si el archivo existe antes de leer
        
        const data = await fs.readFile(filePath, 'utf-8');
        return parseIni(data); // Asumiendo que `parseIni` es una función válida
    } catch (error) {
        console.error(`Error loading INI file: ${error.message}`);
        return null;
    }
}

/** Writes the INI data to a file.
 * @param {string} filePath Absolute path to the file.
 * @param {Object} iniData Data to be written to the INI file.
 * @returns {Promise<boolean>} True if success, false otherwise */
async function SaveIniFile(filePath, iniData) {
    try {
        if (!filePath || !iniData) throw new Error('Invalid file path or INI data.');

        const iniString = stringifyIni(iniData); // Asumiendo que `stringifyIni` es una función válida
        await fs.writeFile(filePath, iniString, 'utf-8');
        return true;
    } catch (error) {
        console.error(`Error saving INI file: ${error.message}`);
        return false;
    }
}

//-----------------------------------------------------------------;

/** De-Serializes an INI formatted string into a JSON object.
 * The JSON object will have sections, keys, and logic blocks.
 * @param {*} iniString Raw data string in INI format.
 * @returns {*} the parsed object  */
function parseIni(iniString) {
    const lines = iniString.split(/\r?\n/);
    const result = { sections: [] };
    let currentSection = null;
    let currentComments = [];
    let logicStack = [];
    let indentLevel = 0;

    lines.forEach(line => {
        line = line.trim();
        if (!line) return;

        if (line.startsWith(';')) {
            currentComments.push(line);
            return;
        }

        if (line.startsWith('[') && line.endsWith(']')) {
            currentSection = {
                name: line.slice(1, -1),
                comments: [...currentComments],
                keys: [],
                logic: []
            };
            result.sections.push(currentSection);
            currentComments = [];
            logicStack = [];
            indentLevel = 0;

        } else if (/^if .*$/i.test(line)) {
            const logicBlock = { condition: line, content: [], comments: [...currentComments], indent: indentLevel };
            currentComments = [];
            if (logicStack.length > 0) {
                logicStack[logicStack.length - 1].content.push(logicBlock);
            } else {
                currentSection.logic.push(logicBlock);
            }
            logicStack.push(logicBlock);
            indentLevel++;

        } else if (/^endif$/i.test(line)) {
            indentLevel--;
            if (logicStack.length > 0) {
                logicStack.pop();
            }
        } else if (logicStack.length > 0) {
            logicStack[logicStack.length - 1].content.push({ line, indent: indentLevel });

        } else if (line.includes('=')) {
            const [key, value] = line.split('=').map(str => str.trim());
            currentSection.keys.push({
                name: key,
                value: value,
                comments: [...currentComments]
            });
            currentComments = [];
        }
    });

    return result;
}

/** Serializes an INI JSON object into a string.
 * @param {*} obj Data object with sections, keys, and logic blocks.
 * @returns {string} an INI formatted string. */
function stringifyIni(obj) {
    if (!obj || !obj.sections) return ''; 

    let iniString = '';

    obj.sections.forEach(section => {
        if (section?.comments?.length) {
            iniString += section.comments.join('\n') + '\n';
        }
        iniString += `[${section.name}]\n`;

        section.keys?.forEach(key => {
            if (key?.comments?.length) {
                iniString += key.comments.join('\n') + '\n';
            }
            iniString += `${key.name} = ${key.value}\n`;
        });

        function processLogic(logicBlock) {
            let indent = ' '.repeat(logicBlock.indent * 4);
            if (logicBlock?.comments?.length) {
                iniString += logicBlock.comments.map(comment => indent + comment).join('\n') + '\n';
            }
            iniString += indent + logicBlock.condition + '\n';

            logicBlock?.content?.forEach(item => {
                if (item?.line) {
                    iniString += ' '.repeat(item.indent * 4) + item.line + '\n';
                } else if (item?.condition) {
                    processLogic(item); 
                }
            });

            iniString += indent + 'endif\n';
        }

        section.logic?.forEach(logicBlock => {
            processLogic(logicBlock);
        });

        iniString += '\n';
    });

    return iniString.trim();
}

/*function parseIni(iniString) {
    const lines = iniString.split(/\r?\n/);
    const result = { sections: [] };
    let currentSection = null;
    let currentComments = [];
    let logicArea = null;
    let insideLogicBlock = false;
    let indentLevel = 0;

    lines.forEach(line => {
        line = line.trim();
        if (!line) return;

        if (line.startsWith(';')) {
            currentComments.push(line);
            return;
        }

        if (line.startsWith('[') && line.endsWith(']')) {
            currentSection = {
                name: line.slice(1, -1),
                comments: [...currentComments],
                keys: [],
                logic: []
            };
            result.sections.push(currentSection);
            currentComments = [];
            insideLogicBlock = false;
            indentLevel = 0;
        } else if (/^if .*$/i.test(line)) {
            if (!insideLogicBlock) {
                logicArea = { comments: [...currentComments], lines: [] };
                currentComments = [];
                currentSection.logic.push(logicArea);
                insideLogicBlock = true;
            }
            logicArea.lines.push(' '.repeat(indentLevel * 4) + line); // Indentar según nivel
            indentLevel++; // Incrementar nivel de anidación
        } else if (/^endif$/i.test(line)) {
            indentLevel--; // Reducir nivel de anidación
            logicArea.lines.push(' '.repeat(indentLevel * 4) + line);
            if (indentLevel === 0) {
                insideLogicBlock = false;
            }
        } else if (insideLogicBlock) {
            logicArea.lines.push(' '.repeat(indentLevel * 4) + line);
        } else if (line.includes('=')) {
            const [key, value] = line.split('=').map(str => str.trim());
            currentSection.keys.push({
                name: key,
                value: value,
                comments: [...currentComments]
            });
            currentComments = [];
        }
    });

    return result;
}*/

/*function stringifyIni(obj) {
    let iniString = '';

    obj.sections.forEach(section => {
        if (section.comments.length) {
            iniString += section.comments.join('\n') + '\n';
        }
        iniString += `[${section.name}]\n`;

        section.keys.forEach(key => {
            if (key.comments.length) {
                iniString += key.comments.join('\n') + '\n';
            }
            iniString += `${key.name} = ${key.value}\n`;
        });

        section.logic.forEach(logicBlock => {
            if (logicBlock.comments.length) {
                iniString += logicBlock.comments.join('\n') + '\n';
            }
            logicBlock.lines.forEach(line => {
                iniString += line + '\n';
            });
        });

        iniString += '\n'; // Separación entre secciones
    });

    return iniString.trim();
}*/

//-----------------------------------------------------------------;

/** Returns the value of a key in the given INI data object.
 * If the key does not exist in the specified section, it will return undefined.
 * @param {Object} iniObject Parsed INI data
 * @param {string} sectionName Name of the Section
 * @param {string} keyName Name of the Key */
function getKey(iniObject, sectionName, keyName) {
    if (!iniObject || !iniObject.sections) return undefined;

    const section = iniObject.sections.find(sec => sec.name.toLowerCase() === sectionName.toLowerCase());
    if (!section) return undefined;

    // Exclude logic areas from lookup
    const key = section.keys.find(k => k.name.toLowerCase() === keyName.toLowerCase());
    return key ? String(key.value) : undefined;
}

/** Sets the value of a key in the given INI data object.
 * If the key does not exist in the specified section, it will be added.
 * @param {Object} iniObject Parsed INI data
 * @param {string} sectionName Name of the section
 * @param {string} keyName Name of the key
 * @param {string} newValue Value to set
 * @param {boolean} [addNewKey=false] If true, adds the key if it doesn't exist
 * @returns {Object|null} The modified iniObject, or NULL if failed. */
function setKey(iniObject, sectionName, keyName, newValue, addNewKey = false) {
    if (!iniObject || !iniObject.sections) return null;

    try {
        const sectionIndex = iniObject.sections.findIndex(sec => sec.name.toLowerCase() === sectionName.toLowerCase());
        let section = iniObject.sections[sectionIndex];

        if (!section) {
            if (!addNewKey) return null;
            section = { name: sectionName, comments: [], keys: [] }; // Removed `logic`
            iniObject.sections.push(section);
        }

        // Find and update key without interacting with logic areas
        const keyIndex = section.keys.findIndex(k => k.name.toLowerCase() === keyName.toLowerCase());
        let key = section.keys[keyIndex];

        if (!key) {
            if (!addNewKey) return null;
            key = { name: keyName, value: String(newValue), comments: [] };
            section.keys.push(key);
        } else {
            key.value = String(newValue);
        }

        return iniObject;
    } catch (error) {
        console.warn(`Error in setKey: ${error.message}`);
        return null;
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

/** Finds a Key/Value in an INI-like JSON structure
 * @param {object} data Parsed JSON structure
 * @param {object} searchItem Element from a Theme Template -> { Section: "SectionName", Key: "KeyName|KeyName2" }
 * @returns {any|any[]} Results as key-value pairs array, or single value if only one key found */
function findValueByKey(data, searchItem) {
    console.log('findValueByKey:', data);
    const results = [];

    try {
        const sectionName = searchItem.Section.toLowerCase();
        const keys = searchItem.Key.split('|').map(k => k.toLowerCase()); // Normalize keys

        // Find the section in JSON structure
        const sectionData = data.sections.find(sec => sec.name.toLowerCase() === sectionName);
        if (!sectionData) throw new Error(`Section Not Found: ${searchItem.Section}`);

        // Search for each key in the section
        keys.forEach(keyName => {
            const keyData = sectionData.keys.find(k => k.name.toLowerCase() === keyName);
            if (keyData) {
                results.push({ key: keyName, value: keyData.value });
            } else {
                console.warn(`Key Not Found: ${keyName} in Section: ${sectionName}`);
            }
        });

    } catch (error) {
        console.error(`Error in findValueByKey: ${error.message}`);
    }

    // Return results as an array of key-value pairs, or the single value if only one key
    return results.length === 1 ? results[0].value : results.length > 0 ? results : null;
}

/** Deletes a key from a section in the INI-like JSON structure.
 * @param {object} data - The INI JSON object.
 * @param {string} sectionName - The name of the section.
 * @param {string} keyName - The name of the key to delete.
 * @returns {boolean} Returns true if the key was deleted, false if not found.
 */
function deleteKey(data, sectionName, keyName) {
    if (!data || !data.sections) return false; // Validación básica

    const sectionData = data.sections.find(s => s.name.toLowerCase() === sectionName.toLowerCase());
    if (!sectionData || !sectionData.keys) return false; // Si no encuentra la sección o no tiene claves, salir

    // Filtrar la clave a eliminar
    const initialLength = sectionData.keys.length;
    sectionData.keys = sectionData.keys.filter(k => k.name.toLowerCase() !== keyName.toLowerCase());

    // Retorna true si se eliminó la clave, false si no existía
    return sectionData.keys.length < initialLength;
}

/** Adds a key-value pair to a section in the INI-like JSON structure.
 * @param {object} data - The INI JSON object.
 * @param {string} sectionName - The name of the section.
 * @param {string} keyName - The name of the key to add.
 * @param {any} value - The value to associate with the key.
 * @param {string} comment - Optional comment for the key.
 * @returns {boolean} Returns true if the key was added successfully, false otherwise.
 */
function addKey(data, sectionName, keyName, value, comment = "") {
    if (!data || !data.sections) return false; // Validación básica

    // Buscar o crear la sección
    let sectionData = data.sections.find(s => s.name.toLowerCase() === sectionName.toLowerCase());
    if (!sectionData) {
        sectionData = { name: sectionName, comments: [], keys: [], logic: [] };
        data.sections.push(sectionData);
    }

    // Validar que la sección tenga la propiedad keys
    if (!sectionData.keys) {
        sectionData.keys = [];
    }

    // Verificar si la clave ya existe
    const existingKey = sectionData.keys.find(k => k.name.toLowerCase() === keyName.toLowerCase());
    if (existingKey) {
        console.warn(`Key "${keyName}" already exists in section "${sectionName}".`);
        return false;
    }

    // Agregar la clave con su valor y comentarios
    sectionData.keys.push({
        name: keyName,
        value: String(value),
        comments: comment ? comment.split('\n') : []
    });

    return true;
}
//-----------------------------------------------------------------;

// ... (IPC Handlers)
ipcMain.handle('INI-LoadFile', async (event, filePath) => {
    try {
        return await LoadIniFile(filePath);
    } catch (error) {
        console.error(`Error in INI-LoadFile: ${error.message}`);
        return null;
    }
});

ipcMain.handle('INI-SaveFile', async (event, filePath, iniData) => {
    try {
        return await SaveIniFile(filePath, iniData);
    } catch (error) {
        console.error(`Error in INI-SaveFile: ${error.message}`);
        return false;
    }
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
