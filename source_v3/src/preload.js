


const { contextBridge, ipcRenderer } = require('electron');
const path = require('path');

const getParentFolder = (filePath) => {
  const parts = filePath.split(path.sep);
  parts.pop(); // Remove the last part
  return parts.join(path.sep);
};


contextBridge.exposeInMainWorld('api', {
  getAppVersion: async () => ipcRenderer.invoke('get-app-version'),
  logEvent: (message, stackTrace = '') => ipcRenderer.invoke('log-event', message, stackTrace),

  joinPath: (basePath, ...segments) => path.join(basePath, ...segments),
  resolveEnvVariables: (inputPath) => ipcRenderer.invoke('resolve-env-variables', inputPath),
  getParentFolder: (filePath) => getParentFolder(filePath),

  getJsonFile: (jsonPath) => ipcRenderer.invoke('get-json-file', jsonPath),
  writeJsonFile: (filePath, data, prettyPrint) => ipcRenderer.invoke('writeJsonFile', filePath, data, prettyPrint),
  
  getThemes: async (dirPath) => ipcRenderer.invoke('get-themes', dirPath),
  LoadThemeINIs: async (folderPath) => ipcRenderer.invoke('LoadThemeINIs', folderPath),

  initializeSettings: async () => ipcRenderer.invoke('initialize-settings'),
  InstallStatus: async () => ipcRenderer.invoke('InstallStatus'),
  getSettings: async () => ipcRenderer.invoke('get-settings'),  
  loadSettings: async () => ipcRenderer.invoke('load-settings'),
  saveSettings: async (settings) => ipcRenderer.invoke('save-settings', settings),

  getActiveInstance: () => ipcRenderer.invoke('active-instance'),

  applyIniValuesToTemplate: async (template, iniValues) => ipcRenderer.invoke('apply-ini-values', template, iniValues),
  applyTemplateToGame: async (template, gamePath) => ipcRenderer.invoke('applyTemplateToGame', template, gamePath),
  //reverseGammaCorrected: async (gammaR, gammaG, gammaB) => ipcRenderer.invoke('reverseGammaCorrected', gammaR, gammaG, gammaB),
 
  
  loadIniFile: async (filePath) => ipcRenderer.invoke('loadIniFile', filePath),
  saveIniFile: async (filePath) => ipcRenderer.invoke('saveIniFile', filePath),
  getValueFromSection: (iniData, section, key, defaultValue) => ipcRenderer.invoke('getValueFromSection', iniData, section, key, defaultValue),
  setValueInSection: (iniData, section, key, value) => ipcRenderer.invoke('setValueInSection', iniData, section, key, value),

  isNotNullOrEmpty: async () => ipcRenderer.invoke('is-not-null-obj'),

  loadHistory: (historyFolder, numberOfSavesToRemember) => ipcRenderer.invoke('load-history', historyFolder, numberOfSavesToRemember),
  saveHistory: (historyFolder, theme) => ipcRenderer.invoke('save-history', historyFolder, theme),

  //resolvePath: (basePath, ...segments) => path.resolve(basePath, ...segments),
  



  getAssetPath: (assetPath) => ipcRenderer.invoke('get-asset-path', assetPath),
  getAssetFileUrl: (assetPath) => ipcRenderer.invoke('get-asset-file-url', assetPath),
  getLocalFileUrl: (assetPath) => ipcRenderer.invoke('get-local-file-url', assetPath),

});




