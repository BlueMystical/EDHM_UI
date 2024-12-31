


const { contextBridge, ipcRenderer } = require('electron');
const path = require('path');

const getParentFolder = (filePath) => {
  const parts = filePath.split(path.sep);
  parts.pop(); // Remove the last part
  return parts.join(path.sep);
};


contextBridge.exposeInMainWorld('api', {
  getAppVersion: async () => ipcRenderer.invoke('get-app-version'),
  logError: (...args) => ipcRenderer.invoke('logError', ...args),

  ShowMessageBox: (options) => ipcRenderer.invoke('ShowMessageBox', options),
  ShowOpenDialog: (options) => ipcRenderer.invoke('ShowOpenDialog', options),
  ShowSaveDialog: (options) => ipcRenderer.invoke('ShowSaveDialog', options),

  checkProcess: async (processName) => ipcRenderer.invoke('checkProcess', processName),

  joinPath: (basePath, ...segments) => path.join(basePath, ...segments),
  resolveEnvVariables: (inputPath) => ipcRenderer.invoke('resolve-env-variables', inputPath),
  getParentFolder: (filePath) => getParentFolder(filePath),
  openPathInExplorer: (filePath) => ipcRenderer.invoke('openPathInExplorer', filePath), 
  deleteFileByAbsolutePath: (filePath) => ipcRenderer.invoke('deleteFileByAbsolutePath', filePath), 

  findLatestFile: (folderPath, fileType) => ipcRenderer.invoke('find-latest-file', folderPath, fileType),

  getJsonFile: (jsonPath) => ipcRenderer.invoke('get-json-file', jsonPath),
  writeJsonFile: (filePath, data, prettyPrint) => ipcRenderer.invoke('writeJsonFile', filePath, data, prettyPrint),

  compressFiles: (files, outputPath) => ipcRenderer.invoke('compress-files', files, outputPath),
  compressFolder: (folderPath, outputPath) => ipcRenderer.invoke('compress-folder', folderPath, outputPath),
  decompressFile: (zipPath, outputDir) => ipcRenderer.invoke('decompress-file', zipPath, outputDir),
  
  getThemes: async (dirPath) => ipcRenderer.invoke('get-themes', dirPath),
  LoadThemeINIs: async (folderPath) => ipcRenderer.invoke('LoadThemeINIs', folderPath),
  SaveThemeINIs: async (folderPath, themeINIs) => ipcRenderer.invoke('SaveThemeINIs', folderPath, themeINIs),

  initializeSettings: async () => ipcRenderer.invoke('initialize-settings'),
  InstallStatus: async () => ipcRenderer.invoke('InstallStatus'),
  getSettings: async () => ipcRenderer.invoke('get-settings'),   
  getDefaultSettings: async () => ipcRenderer.invoke('getDefaultSettings'), 
  loadSettings: async () => ipcRenderer.invoke('load-settings'),
  saveSettings: async (settings) => ipcRenderer.invoke('save-settings', settings),
  
  addNewInstance: async (NewInstancePath, settings) => ipcRenderer.invoke('addNewInstance', NewInstancePath, settings),

  getActiveInstance: () => ipcRenderer.invoke('active-instance'), 
  getActiveInstanceEx: () => ipcRenderer.invoke('getActiveInstanceEx'),

  applyIniValuesToTemplate: async (template, iniValues) => ipcRenderer.invoke('apply-ini-values', template, iniValues),
  ApplyTemplateValuesToIni: async (template, iniValues) => ipcRenderer.invoke('ApplyTemplateValuesToIni', template, iniValues),
 
  
  loadIniFile: async (filePath) => ipcRenderer.invoke('loadIniFile', filePath),
  saveIniFile: async (filePath, iniData) => ipcRenderer.invoke('saveIniFile', filePath, iniData),
  getValueFromSection: (iniData, section, key, defaultValue) => ipcRenderer.invoke('getValueFromSection', iniData, section, key, defaultValue),
  setValueInSection: (iniData, section, key, value) => ipcRenderer.invoke('setValueInSection', iniData, section, key, value),

  isNotNullOrEmpty: async () => ipcRenderer.invoke('is-not-null-obj'),

  loadHistory: (historyFolder, numberOfSavesToRemember) => ipcRenderer.invoke('load-history', historyFolder, numberOfSavesToRemember),
  saveHistory: (historyFolder, theme) => ipcRenderer.invoke('save-history', historyFolder, theme),

  //resolvePath: (basePath, ...segments) => path.resolve(basePath, ...segments),
  
  GetGammaCorrected_RGBA: async (color, gammaValue) => ipcRenderer.invoke('GetGammaCorrected_RGBA', color, gammaValue),
  reverseGammaCorrected:  async (color, gammaValue) => ipcRenderer.invoke('reverseGammaCorrected',  color, gammaValue),
  intToRGBA:  async (colorValue) => ipcRenderer.invoke('intToRGBA',  colorValue),

  getAssetPath: (assetPath) => ipcRenderer.invoke('get-asset-path', assetPath),
  getAssetFileUrl: (assetPath) => ipcRenderer.invoke('get-asset-file-url', assetPath),
  getLocalFileUrl: (assetPath) => ipcRenderer.invoke('get-local-file-url', assetPath),

});




