/* THIS IS AN INTERMEDIATE LAYER BETWEEN THE 'MAIN PROCESS' AND THE 'RENDERER PROCESS'  */

const { contextBridge, ipcRenderer } = require('electron');
const path = require('path');

contextBridge.exposeInMainWorld('api', {

  // #region Assets

  getAssetPath: (assetPath) => ipcRenderer.invoke('get-asset-path', assetPath),
  getAssetFileUrl: (assetPath) => ipcRenderer.invoke('get-asset-file-url', assetPath),
  getLocalFileUrl: (assetPath) => ipcRenderer.invoke('get-local-file-url', assetPath),

  // #endregion

  // #region Files & Directories
 
  ShowMessageBox: (options) => ipcRenderer.invoke('ShowMessageBox', options),
  ShowOpenDialog: (options) => ipcRenderer.invoke('ShowOpenDialog', options),
  ShowSaveDialog: (options) => ipcRenderer.invoke('ShowSaveDialog', options),

  checkProcess: async (processName) => ipcRenderer.invoke('checkProcess', processName),

  joinPath: (basePath, ...segments) => path.join(basePath, ...segments),
  resolveEnvVariables: (inputPath) => ipcRenderer.invoke('resolve-env-variables', inputPath),
  getParentFolder: (filePath) => path.dirname(filePath),
  openPathInExplorer: (filePath) => ipcRenderer.invoke('openPathInExplorer', filePath), 
  deleteFileByAbsolutePath: (filePath) => ipcRenderer.invoke('deleteFileByAbsolutePath', filePath), 

  findLatestFile: (folderPath, fileType) => ipcRenderer.invoke('find-latest-file', folderPath, fileType), 
  findFileWithPattern: (folderPath, pattern) => ipcRenderer.invoke('findFileWithPattern', folderPath, pattern),

  getJsonFile: (jsonPath) => ipcRenderer.invoke('get-json-file', jsonPath),
  writeJsonFile: (filePath, data, prettyPrint) => ipcRenderer.invoke('writeJsonFile', filePath, data, prettyPrint),

  compressFiles: (files, outputPath) => ipcRenderer.invoke('compress-files', files, outputPath),
  compressFolder: (folderPath, outputPath) => ipcRenderer.invoke('compress-folder', folderPath, outputPath),
  decompressFile: (zipPath, outputDir) => ipcRenderer.invoke('decompress-file', zipPath, outputDir),
  
  // #endregion

  // #region Themes  

  getThemes: async (dirPath) => ipcRenderer.invoke('get-themes', dirPath), 
  LoadTheme: async (dirPath) => ipcRenderer.invoke('LoadTheme', dirPath),
  LoadThemeINIs: async (folderPath) => ipcRenderer.invoke('LoadThemeINIs', folderPath),
  SaveThemeINIs: async (folderPath, themeINIs) => ipcRenderer.invoke('SaveThemeINIs', folderPath, themeINIs),

  applyIniValuesToTemplate: async (template, iniValues) => ipcRenderer.invoke('apply-ini-values', template, iniValues),
  ApplyTemplateValuesToIni: async (template, iniValues) => ipcRenderer.invoke('ApplyTemplateValuesToIni', template, iniValues),

  loadIniFile: async (filePath) => ipcRenderer.invoke('loadIniFile', filePath),
  saveIniFile: async (filePath, iniData) => ipcRenderer.invoke('saveIniFile', filePath, iniData),
  getValueFromSection: (iniData, section, key, defaultValue) => ipcRenderer.invoke('getValueFromSection', iniData, section, key, defaultValue),
  setValueInSection: (iniData, section, key, value) => ipcRenderer.invoke('setValueInSection', iniData, section, key, value),
  
  // #endregion

  // #region Settings 
  
  initializeSettings: async () => ipcRenderer.invoke('initialize-settings'),
  InstallStatus: async () => ipcRenderer.invoke('InstallStatus'),

  getSettings: async () => ipcRenderer.invoke('get-settings'),   
  getDefaultSettings: async () => ipcRenderer.invoke('getDefaultSettings'), 

  loadSettings: async () => ipcRenderer.invoke('load-settings'),
  saveSettings: async (settings) => ipcRenderer.invoke('save-settings', settings),

  getActiveInstance: () => ipcRenderer.invoke('active-instance'), 
  getActiveInstanceEx: () => ipcRenderer.invoke('getActiveInstanceEx'),
  getInstanceByName: async (instanceName) => ipcRenderer.invoke('getInstanceByName', instanceName),
  addNewInstance: async (NewInstancePath, settings) => ipcRenderer.invoke('addNewInstance', NewInstancePath, settings),

  loadHistory: (historyFolder, numberOfSavesToRemember) => ipcRenderer.invoke('load-history', historyFolder, numberOfSavesToRemember),
  saveHistory: (historyFolder, theme) => ipcRenderer.invoke('save-history', historyFolder, theme), 
  
  installEDHMmod: (gameInstance) => ipcRenderer.invoke('installEDHMmod', gameInstance), 
  CheckEDHMinstalled: (gamePath) => ipcRenderer.invoke('CheckEDHMinstalled', gamePath),
  
  // #endregion

  // #region Utility Methods

  isNotNullOrEmpty: async () => ipcRenderer.invoke('is-not-null-obj'),
  getAppVersion: async () => ipcRenderer.invoke('get-app-version'),
  logError: (...args) => ipcRenderer.invoke('logError', ...args),
  
  // #endregion

  
  GetGammaCorrected_RGBA: async (color, gammaValue) => ipcRenderer.invoke('GetGammaCorrected_RGBA', color, gammaValue),
  reverseGammaCorrected:  async (color, gammaValue) => ipcRenderer.invoke('reverseGammaCorrected',  color, gammaValue),
  intToRGBA:  async (colorValue) => ipcRenderer.invoke('intToRGBA',  colorValue),

  

});