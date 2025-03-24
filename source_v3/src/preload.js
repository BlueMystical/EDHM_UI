/* THIS IS AN INTERMEDIATE LAYER BETWEEN THE 'MAIN PROCESS' AND THE 'RENDERER PROCESS'  */

const { contextBridge, ipcRenderer, shell } = require('electron');
const path = require('path');

contextBridge.exposeInMainWorld('api', {

  // #region Assets

  joinPath: (basePath, ...segments) => path.join(basePath, ...segments),
  getParentFolder: (filePath) => path.dirname(filePath),
  getBaseName: (filePath, extension) => path.basename(filePath, extension),
  resolveEnvVariables: (inputPath) => ipcRenderer.invoke('resolve-env-variables', inputPath),

  getAssetPath: (assetPath) => ipcRenderer.invoke('get-asset-path', assetPath),
  getAssetFileUrl: (assetPath) => ipcRenderer.invoke('get-asset-file-url', assetPath),
  getLocalFileUrl: (assetPath) => ipcRenderer.invoke('get-local-file-url', assetPath),
  getPublicFilePath: (relFilePath) => ipcRenderer.invoke('getPublicFilePath', relFilePath),

  getJsonFile: (jsonPath) => ipcRenderer.invoke('get-json-file', jsonPath),
  writeJsonFile: (filePath, data, prettyPrint) => ipcRenderer.invoke('writeJsonFile', filePath, data, prettyPrint),

  GetImageB64: (filePath) => ipcRenderer.invoke('GetImageB64', filePath),
  GetElementsImage: async (key) => ipcRenderer.invoke('GetElementsImage', key),

  // #endregion

  // #region Files & Directories

  ShowMessageBox: (options) => ipcRenderer.invoke('ShowMessageBox', options),
  ShowOpenDialog: (options) => ipcRenderer.invoke('ShowOpenDialog', options),
  ShowSaveDialog: (options) => ipcRenderer.invoke('ShowSaveDialog', options),

  detectProgram: (exeName) => ipcRenderer.invoke('detect-program', exeName),
  startMonitoring: (exeName) => ipcRenderer.invoke('start-monitoring', exeName),
  onProgramDetected: (callback) => ipcRenderer.on('program-detected', callback),
  terminateProgram: (exeName) => ipcRenderer.invoke('terminate-program', exeName),

  openPathInExplorer: (filePath) => ipcRenderer.invoke('openPathInExplorer', filePath),
  openUrlInBrowser: (url) => ipcRenderer.invoke('openUrlInBrowser', url),
  deleteFileByAbsolutePath: (filePath) => ipcRenderer.invoke('deleteFileByAbsolutePath', filePath),
  deleteFilesByType: (directoryPath, extension) => ipcRenderer.invoke('deleteFilesByType', directoryPath, extension),

  findLatestFile: (folderPath, fileType) => ipcRenderer.invoke('find-latest-file', folderPath, fileType),
  findFileWithPattern: (folderPath, pattern) => ipcRenderer.invoke('findFileWithPattern', folderPath, pattern),

  fileExists: (filePath) => ipcRenderer.invoke('checkFileExists', filePath), 
  ensureDirectoryExists: (fullPath) => ipcRenderer.invoke('ensureDirectoryExists', fullPath),
  copyFile: (sourcePath, destinationPath, move)=> ipcRenderer.invoke('copyFile', sourcePath, destinationPath, move = false),

  compressFiles: (files, outputPath) => ipcRenderer.invoke('compress-files', files, outputPath),
  compressFolder: (folderPath, outputPath) => ipcRenderer.invoke('compress-folder', folderPath, outputPath),
  decompressFile: (zipPath, outputDir) => ipcRenderer.invoke('decompress-file', zipPath, outputDir),

  // #endregion

  // #region Themes  

  getThemes: async (dirPath) => ipcRenderer.invoke('get-themes', dirPath),
  LoadTheme: async (dirPath) => ipcRenderer.invoke('LoadTheme', dirPath),
  LoadThemeINIs: async (folderPath) => ipcRenderer.invoke('LoadThemeINIs', folderPath),
  SaveThemeINIs: async (folderPath, themeINIs) => ipcRenderer.invoke('SaveThemeINIs', folderPath, themeINIs),

  FavoriteTheme: async (theme) => ipcRenderer.invoke('FavoriteTheme', theme),
  UnFavoriteTheme: async (theme) => ipcRenderer.invoke('UnFavoriteTheme', theme),

  applyIniValuesToTemplate: async (template, iniValues) => ipcRenderer.invoke('apply-ini-values', template, iniValues),
  ApplyTemplateValuesToIni: async (template, iniValues) => ipcRenderer.invoke('ApplyTemplateValuesToIni', template, iniValues),

  loadIniFile: async (filePath) => ipcRenderer.invoke('loadIniFile', filePath),
  saveIniFile: async (filePath, iniData) => ipcRenderer.invoke('saveIniFile', filePath, iniData),
  getValueFromSection: (iniData, section, key, defaultValue) => ipcRenderer.invoke('getValueFromSection', iniData, section, key, defaultValue),
  setValueInSection: (iniData, section, key, value) => ipcRenderer.invoke('setValueInSection', iniData, section, key, value),

  GetCurrentSettingsTheme: async (filePath) => ipcRenderer.invoke('GetCurrentSettingsTheme', filePath),
  CreateNewTheme: async (themeData) => ipcRenderer.invoke('CreateNewTheme', themeData),
  ExportTheme: async (themeData) => ipcRenderer.invoke('ExportTheme', themeData),
  UpdateTheme: async (themeData, source) => ipcRenderer.invoke('UpdateTheme', themeData, source),
  SaveTheme: async (themeData) => ipcRenderer.invoke('SaveTheme', themeData),
  DeleteTheme: async (themeData) => ipcRenderer.invoke('DeleteTheme', themeData),  

  // #endregion

  // #region Settings 

  initializeSettings: async () => ipcRenderer.invoke('initialize-settings'),
  InstallStatus: async () => ipcRenderer.invoke('InstallStatus'),

  getSettings: async () => ipcRenderer.invoke('get-settings'),
  getDefaultSettings: async () => ipcRenderer.invoke('getDefaultSettings'),

  /** Writes a Key/Value into the Program Settings.
 * @param {*} key Name of a Key in the Settings
 * @param {*} value The Value os the indicated Key
 * @returns 'true' if Success */
  writeSetting: async (key, value) => ipcRenderer.invoke('writeSetting', key, value),
  readSetting: async (key, defaultValue) => ipcRenderer.invoke('readSetting', key, defaultValue),

  loadSettings: async () => ipcRenderer.invoke('load-settings'),
  saveSettings: async (settings) => ipcRenderer.invoke('save-settings', settings),
  LoadGlobalSettings: async () => ipcRenderer.invoke('LoadGlobalSettings'),
  SaveGlobalSettings: async (settings) => ipcRenderer.invoke('saveGlobalSettings', settings),
  LoadUserSettings: async () => ipcRenderer.invoke('LoadUserSettings'),
  saveUserSettings: async (settings) => ipcRenderer.invoke('saveUserSettings', settings),
  AddToUserSettings: async (settings) => ipcRenderer.invoke('AddToUserSettings', settings),
  RemoveFromUserSettings: async (settings) => ipcRenderer.invoke('RemoveFromUserSettings', settings),

  getActiveInstance: () => ipcRenderer.invoke('active-instance'),
  getActiveInstanceEx: () => ipcRenderer.invoke('getActiveInstanceEx'),
  getInstanceByName: async (instanceName) => ipcRenderer.invoke('getInstanceByName', instanceName),
  addNewInstance: async (NewInstancePath, settings) => ipcRenderer.invoke('addNewInstance', NewInstancePath, settings),
  GetCurrentSettings: async (folderPath) => ipcRenderer.invoke('GetCurrentSettings', folderPath),

  loadHistory: (historyFolder, numberOfSavesToRemember) => ipcRenderer.invoke('load-history', historyFolder, numberOfSavesToRemember),
  saveHistory: (historyFolder, theme) => ipcRenderer.invoke('save-history', historyFolder, theme),

  installEDHMmod: (gameInstance) => ipcRenderer.invoke('installEDHMmod', gameInstance),
  CheckEDHMinstalled: (gamePath) => ipcRenderer.invoke('CheckEDHMinstalled', gamePath),
  UninstallEDHMmod: (gameInstance) => ipcRenderer.invoke('UninstallEDHMmod', gameInstance),
  DoHotFix: async () => ipcRenderer.invoke('DoHotFix'),

  GetInstalledTPMods: async (folderPath) => ipcRenderer.invoke('GetInstalledTPMods', folderPath),

  // #endregion

  // #region Utility Methods

  isNotNullOrEmpty: async () => ipcRenderer.invoke('is-not-null-obj'),
  getAppVersion: async () => ipcRenderer.invoke('get-app-version'),
  logError: (...args) => ipcRenderer.invoke('logError', ...args),

  GetGammaCorrected_RGBA: async (color, gammaValue) => ipcRenderer.invoke('GetGammaCorrected_RGBA', color, gammaValue),
  reverseGammaCorrected: async (color, gammaValue) => ipcRenderer.invoke('reverseGammaCorrected', color, gammaValue),
  intToRGBA: async (colorValue) => ipcRenderer.invoke('intToRGBA', colorValue),

  openUrlInBrowser: (url) => { shell.openExternal(url); },

  // #endregion

  getColorMatrixFilters: async () => ipcRenderer.invoke('GetColorMatrixFilters'),

  getLatestPreReleaseVersion: async (owner, repo) => ipcRenderer.invoke('getLatestPreReleaseVersion', owner, repo),
  getLatestReleaseVersion: async (owner, repo) => ipcRenderer.invoke('getLatestReleaseVersion', owner, repo),

  runInstaller: async (url, dest) => ipcRenderer.invoke('runInstaller', installerPath),
  copyToClipboard: (text) => ipcRenderer.invoke('copyToClipboard', text),

  downloadFile: (url, filePath) => ipcRenderer.invoke('download-file', url, filePath),
  onDownloadProgress: (callback) => ipcRenderer.on('download-progress', callback),
  removeDownloadProgressListener: (callback) => ipcRenderer.removeListener('download-progress', callback),
  downloadAsset: async (url, dest) => ipcRenderer.invoke('download-asset', url, dest),

  getPlatform: () => ipcRenderer.invoke('get-platform'),
  quitProgram: () => ipcRenderer.invoke('quit-program'),
  runProgram: (filePath, args = []) => ipcRenderer.invoke('run-program', filePath, args),

  open3PModsManager: () => ipcRenderer.invoke('open3PModsManager'),
  navigate: (callback) => ipcRenderer.on('navigate', callback),

});