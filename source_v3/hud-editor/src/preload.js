/* THIS IS AN INTERMEDIATE LAYER BETWEEN THE 'MAIN PROCESS' AND THE 'RENDERER PROCESS'  */

const { contextBridge, ipcRenderer, shell } = require('electron');
const path = require('path');

console.log('Preload.js loaded!');

contextBridge.exposeInMainWorld('api', {

    joinPath: (basePath, ...segments) => path.join(basePath, ...segments),
    getParentFolder: (filePath) => path.dirname(filePath),
    getBaseName: (filePath, extension) => path.basename(filePath, extension),
    getBaseNameNoExt: (filePath) => path.parse(filePath).name,
    resolveEnvVariables: (inputPath) => ipcRenderer.invoke('resolve-env-variables', inputPath),

    /** Writes a Key/Value into the Program Settings.
    * @param {*} key Name of a Key in the Settings
    * @param {*} value The Value os the indicated Key
    * @returns 'true' if Success */
    writeSetting: async (key, value) => ipcRenderer.invoke('writeSetting', key, value),
    readSetting: async (key, defaultValue) => ipcRenderer.invoke('readSetting', key, defaultValue),

    getJsonFile: async (jsonPath) => ipcRenderer.invoke('get-json-file', jsonPath),
    writeJsonFile: async (filePath, data, prettyPrint) => ipcRenderer.invoke('writeJsonFile', filePath, data, prettyPrint),

    copyFile: async (source, destination, move = false) => ipcRenderer.invoke('copyFile', source, destination, move),
    checkFileExists: async (filePath) => ipcRenderer.invoke('checkFileExists', filePath),
    deleteFile: async (filePath) => ipcRenderer.invoke('deleteFile', filePath),

    /** Returns the path to the EDHM data directory. */
    GetProgramDataDirectory: async () => ipcRenderer.invoke('GetAppDataDirectory'),

    ShowMessageBox: (options) => ipcRenderer.invoke('ShowMessageBox', options),
    ShowOpenDialog: (options) => ipcRenderer.invoke('ShowOpenDialog', options),
    ShowSaveDialog: (options) => ipcRenderer.invoke('ShowSaveDialog', options),

});
