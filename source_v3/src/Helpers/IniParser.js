import fs from 'node:fs'; 
import { ipcMain } from 'electron';

export async function LoadIniFile(filePath) {
    try {
        console.log('Loading ini..', filePath);     
        if (!fs.existsSync(filePath)) {
            console.warn(`File not found: ${resolvedPath}`);
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
    let currentSectionComment = ""; // Store comments *before* the first key in a section
    let currentKeyComment = "";

    for (const line of lines) {
        const trimmedLine = line.trim();

        if (trimmedLine.startsWith(';')) { // Comment
            if (!currentSection) {
                // Comment before any section
                currentSectionComment += trimmedLine.substring(1).trim() + "\n";
            } else {
                currentKeyComment += trimmedLine.substring(1).trim() + "\n";
            }
            continue;
        }

        if (trimmedLine.startsWith('[')) { // Section
            const sectionName = trimmedLine.slice(1, -1);
            currentSection = { Section: sectionName, Comment: currentSectionComment.trim(), Keys: [] }; // Assign accumulated section comment
            currentSectionComment = ""; // Reset for the new section
            result.push(currentSection);
            currentKeyComment = "";
        } else if (currentSection && trimmedLine) { // Key-Value pair
            const [key, value] = trimmedLine.split('=').map(s => s.trim());
            currentSection.Keys.push({ Key: key, Value: parseValue(value), Comment: currentKeyComment.trim() });
            currentKeyComment = "";
        } else if (currentSection && !trimmedLine && currentKeyComment) { //Handles comments without a key after them.
            if (currentSection.Keys.length === 0) { //If it's the first comment in the section.
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
    const num = Number(value);
    return isNaN(num) ? value : num;
}

function stringifyIni(iniData) {
    let output = '';
    for (const sectionData of iniData) {
        if (sectionData.Comment) {
            output += sectionData.Comment.split("\n").map(line => `; ${line.trim()}`).join("\n") + "\n";
        }
        output += `[${sectionData.Section}]\n`;
        for (const keyData of sectionData.Keys) {
            if (keyData.Comment) {
                output += keyData.Comment.split("\n").map(line => `; ${line.trim()}`).join("\n") + "\n";
            }
            output += `${keyData.Key} = ${keyData.Value}\n`;
        }
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
            iniData.push(sectionData);
        }
        let keyData = sectionData.Keys.find(k => k.Key === key);
        if (keyData) {
            keyData.Value = value;
            keyData.Comment = comment || keyData.Comment; // Preserve old comment if not provided
        } else {
            sectionData.Keys.push({ Key: key, Value: value, Comment: comment || "" });
        }
    } catch (error) {
        console.log('setKey@IniParser:', error);
        throw new Error(error.message + error.stack);
    }
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
ipcMain.handle('INI-AddKey', (event, iniData, section, key, value, comment) => {
    return addKey(iniData, section, key, value, comment);
});
ipcMain.handle('INI-DeleteKey', (event, iniData, section, key) => {
    return deleteKey(iniData, section, key);
});

export default { LoadIniFile, SaveIniFile, getKey, setKey, deleteKey, addKey }
