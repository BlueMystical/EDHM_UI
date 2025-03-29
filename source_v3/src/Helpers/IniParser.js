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

function parseIni(data) {
    const result = [];
    const lines = data.split('\n');
    let currentSection = null;
    let currentSectionComment = "";
    let currentKeyComment = "";
    let insideLogicBlock = false;

    for (const line of lines) {
        const trimmedLine = line; // Do NOT trim the line here

        if (trimmedLine.trim().startsWith(';')) {
            if (!currentSection) {
                currentSectionComment += trimmedLine.trim().substring(1).trim() + "\n";
            } else {
                currentKeyComment += trimmedLine.trim().substring(1).trim() + "\n";
            }
            continue;
        }

        if (trimmedLine.trim().startsWith('[')) {
            const sectionName = trimmedLine.trim().slice(1, -1);
            currentSection = { Section: sectionName, Comment: currentSectionComment.trim(), Keys: [], Logic: [] };
            currentSectionComment = "";
            result.push(currentSection);
            currentKeyComment = "";
            insideLogicBlock = false;
        } else if (currentSection && trimmedLine.trim()) {
            if (trimmedLine.trim().startsWith('if')) {
                currentSection.Logic.push(trimmedLine);
                insideLogicBlock = true;
                continue;
            } else if (trimmedLine.trim().startsWith('endif')) {
                currentSection.Logic.push(trimmedLine);
                insideLogicBlock = false;
                continue;
            }

            let key, value;
            if (trimmedLine.trim().startsWith('global ')) {
                const parts = trimmedLine.trim().substring(7).trim().split('=');
                key = 'global ' + parts[0].trim();
                value = parts[1] ? parts[1].trim() : '';
            } else {
                const parts = trimmedLine.trim().split('=');
                key = parts[0].trim();
                value = parts[1] ? parts[1].trim() : '';
            }

            if (insideLogicBlock) {
                currentSection.Logic.push(trimmedLine); // Add the entire line, including indents
            } else {
                currentSection.Keys.push({ Key: key, Value: parseValue(value), Comment: currentKeyComment.trim() });
            }

            currentKeyComment = "";

        } else if (currentSection && !trimmedLine.trim() && currentKeyComment) {
            if (currentSection.Keys.length === 0) {
                currentSection.Comment += (currentSection.Comment ? "\n" : "") + currentKeyComment.trim();
            } else {
                currentSection.Keys[currentSection.Keys.length - 1].Comment += (currentSection.Keys[currentSection.Keys.length - 1].Comment ? "\n" : "") + currentKeyComment.trim();
            }
            currentKeyComment = "";
        }
    }
    return result;
}

function parseValue(value) {
    return value;
}

function stringifyIni(iniData) {
    let output = '';
    for (const sectionData of iniData) {
        if (sectionData.Comment) {
            output += sectionData.Comment.split("\n").map(line => `; ${line.trim()}`).join("\n") + "\n";
        }
        output += `[${sectionData.Section}]\n`;

        // Combine Keys and Logic into a single array for output
        const allLines = [];
        for (const keyData of sectionData.Keys) {
            if (keyData.Comment) {
                allLines.push(...keyData.Comment.split("\n").map(line => `; ${line.trim()}`));
            }
            allLines.push(`${keyData.Key} = ${keyData.Value}`);
        }
        if (sectionData.Logic && sectionData.Logic.length > 0) {
            allLines.push(...sectionData.Logic);
        }
        output += allLines.join("\n") + "\n";
        output += '\n'; // Add an extra newline between sections for better readability
    }
    return output;
}

// #endregion

function getKey(iniData, section, key) {
    const sectionData = iniData.find(s => s.Section === section);
    if (sectionData) {
        const keyData = sectionData.Keys.find(k => k.Key === key);
        return keyData ? keyData.Value : undefined;
    }
    return undefined;
}

function setKey(iniData, section, key, value, comment = undefined) {
    try {
        let sectionData = iniData.find(s => s.Section === section);
        if (!sectionData) {
            sectionData = { Section: section, Comment: "", Keys: [] };
            iniData.push(sectionData); //<- Key not found in the section, add it
        }
        let keyData = sectionData.Keys.find(k => k.Key === key);
        if (keyData) {
            // Key found and updated
            keyData.Value = value;
            keyData.Comment = comment || keyData.Comment; // Preserve old comment if not provided
        } else {
            sectionData.Keys.push({ Key: key, Value: value, Comment: comment || "" });
        }

    } catch (error) {
        console.log('setKey@IniParser:', error);
        throw new Error(error.message + error.stack);
    }
    return iniData; 
}

/** Sets the value of a key in the given INI data object.
 * If the key does not exist in the specified section, it will be added.
 * @param {object} iniData - The INI data object.
 * @param {string} fileName - The name of the profile (e.g., 'XmlProfile').
 * @param {string} sectionName - The name of the section (e.g., 'constants').
 * @param {string} keyName - The name of the key to set.
 * @param {any} newValue - The new value to assign to the key.
 * @returns {object} The modified INI data object. */
function setIniValue(iniData, fileName, sectionName, keyName, newValue) {
    if (iniData && iniData[fileName]) {
        for (let section of iniData[fileName]) {
            if (section.Section === sectionName) {
                for (let key of section.Keys) {
                    if (key.Key === keyName) {
                        key.Value = newValue;
                        //key.Comment = comment || key.Comment; // Preserve old comment if not provided
                        return iniData; // Key found and updated
                    }
                }
                // Key not found in the section, add it
                section.Keys.push({ Key: keyName, Value: newValue, Comment: 'Key Not Found, Added as new:' });
                return iniData;
            }
        }
    }
    return iniData; // Profile or section not found, or iniData is invalid
}

/** Gets the value of a key from the given INI data object.
 * @param {object} iniData - The INI data object.
 * @param {string} fileName - The name of the profile (e.g., 'XmlProfile').
 * @param {string} sectionName - The name of the section (e.g., 'constants').
 * @param {string} keyName - The name of the key to retrieve.
 * @returns {any|undefined} The value of the key, or undefined if the key is not found. */
function getIniValue(iniData, fileName, sectionName, keyName) {
    if (iniData && iniData[fileName]) {
        for (let section of iniData[fileName]) {
            if (section.Section === sectionName) {
                for (let key of section.Keys) {
                    if (key.Key === keyName) {
                        return key.Value;
                    }
                }
            }
        }
    }
    return undefined; // Not found
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


export function deleteKey(iniData, section, key) {
    const sectionData = iniData.find(s => s.Section === section);
    if (sectionData) {
        sectionData.Keys = sectionData.Keys.filter(k => k.Key !== key);
    }
}

export function addKey(iniData, section, key, value, comment = "") {
    let sectionData = iniData.find(s => s.Section === section);
    if (!sectionData) {
        sectionData = { Section: section, Comment: "", Keys: [] };
        iniData.push(sectionData);
    }
    sectionData.Keys.push({ Key: key, Value: value, Comment: comment });
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
