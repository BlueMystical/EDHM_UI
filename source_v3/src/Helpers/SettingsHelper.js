import { ipcMain } from 'electron';
import path from 'node:path';

import fs from 'fs';
import { readdir, stat, readFile } from 'fs/promises';
import { writeFile, unlink, access } from 'node:fs/promises';

import fileHelper from './FileHelper';
import INIparser from './IniParser.js';
import Log from './LoggingHelper.js';
import Util from './Utils.js';

/*----------------------------------------------------------------------------------------------------------------------------*/
let programSettings = null; // Holds the Program Settings in memory
const defaultSettingsPath = fileHelper.getAssetPath('data/Settings.json');
const programSettingsPath = fileHelper.resolveEnvVariables('%USERPROFILE%\\EDHM_UI\\Settings.json');
const InstallationStatus = {
  NEW_SETTINGS: 'newSettings',
  UPGRADING_USER: 'upgradingUser',
  FRESH_INSTALL: 'freshInstall',
  EXISTING_INSTALL: 'existingInstall'
};
// Determine the Install Status of the V3 Program:
let installationStatus = InstallationStatus.EXISTING_INSTALL; // Default status
/*----------------------------------------------------------------------------------------------------------------------------*/

// #region Program Settings

/** * Check if the Settings JSON exists, if it does not, creates a default file and returns 'false'
 */
export const initializeSettings = async () => {
  try {
    console.log('programSettingsPath', programSettingsPath);
    const userSettingsDir = path.dirname(programSettingsPath); // Get the directory path
    //console.log('Initializing Settings...Main');

    // Check if the user settings directory exists, if not, create it
    if (!fs.existsSync(userSettingsDir)) {
      fs.mkdirSync(userSettingsDir, { recursive: true });
      console.log(`Created directory: ${userSettingsDir}`);
    }

    // Check if the user settings file exists, if not, read and write the default settings JSON
    if (!fs.existsSync(programSettingsPath)) {
      if (!fs.existsSync(defaultSettingsPath)) {
        throw new Error(`Default settings file not found at: ${defaultSettingsPath}`);
      }

      installationStatus = InstallationStatus.NEW_SETTINGS; // V3 is not installed

      // Read the default settings JSON file
      const defaultSettings = fs.readFileSync(defaultSettingsPath, 'utf8');
      programSettings = JSON.parse(defaultSettings);
      installationStatus = InstallationStatus.FRESH_INSTALL; // This means it's a fresh V3 install
      console.log('installationStatus:', installationStatus);

      // Write the JSON to the user settings file
      fs.writeFileSync(programSettingsPath, JSON.stringify(programSettings, null, 4));

    } else {
      installationStatus = InstallationStatus.EXISTING_INSTALL; // Is a Normal Existing User
      programSettings = JSON.parse(fs.readFileSync(programSettingsPath, 'utf-8'));
      console.log('Settings Loaded from Existing Instance.');

      //Log.Info('This is a Test');
    }
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
  return programSettings;
};

/** * Loads the Settings
 * @returns the settings data
 */
const loadSettings = () => {
  try {
    if (!fs.existsSync(programSettingsPath)) {
      throw new Error(`Program settings file not found at: ${programSettingsPath}`);
    }
    const data = fs.readFileSync(programSettingsPath, { encoding: "utf8", flag: 'r' });
    //flags: 'a' is append mode, 'w' is write mode, 'r' is read mode, 'r+' is read-write mode, 'a+' is append-read mode

    programSettings = JSON.parse(data);
    return programSettings;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

/** * Save changes on the settings back to the JSON file
 * @param {*} settings must 'JSON.stringify' the object before sending it here  */
async function saveSettings(settings) {
  try {
    //console.log(settings);
    await writeFile(programSettingsPath, settings, { encoding: "utf8", flag: 'w' });

    //fs.writeFileSync(path, data, { encoding: "utf8", flag: 'a+' }); 
    //flags: 'a' is append mode, 'w' is write mode, 'r' is read mode, 'r+' is read-write mode, 'a+' is append-read mode

    programSettings = JSON.parse(settings); //<- Updates the Settings
    console.log('Settings Saved Successfully');

    return programSettings;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

/** Returns the value of a setting from the settings JSON file. 
 * @param {*} key Name of a Key in the Settings
 * @returns The Value os the indicated Key */
function readSetting(key, defaultValue = null) {
  try {
    const data = fs.readFileSync(programSettingsPath, 'utf8');
    const settings = JSON.parse(data);
    return settings.hasOwnProperty(key) ? settings[key] : defaultValue;
  } catch (error) {
    console.error('Error reading setting:', error);
    return defaultValue;
  }
};

/** Writes a Key/Value into the Program Settings.
 * @param {*} key Name of a Key in the Settings
 * @param {*} value The Value os the indicated Key
 * @returns 'true' if Success */
function writeSetting(key, value) {
  try {
    const data = fs.readFileSync(programSettingsPath, 'utf8');
    const settings = JSON.parse(data);
    settings[key] = value;
    fs.writeFileSync(programSettingsPath, JSON.stringify(settings, null, 4), 'utf8');
    return true; // Indicate success
  } catch (error) {
    console.error('Error writing setting:', error);
    throw new Error(error.message + error.stack);
  }
};

// #endregion

// #region Global & User Settings

async function LoadGlobalSettings () {
  try {
    const _path_A = fileHelper.resolveEnvVariables('%USERPROFILE%/EDHM_UI/ED_Odissey_Global_Settings.json');
    const _path_B = fileHelper.getAssetPath('data/ODYSS/Global_Settings.json');
    const _path_C = fileHelper.getAssetPath('data/ODYSS/ThemeTemplate.json');
    
    let data = {};

    if (!fs.existsSync(_path_A)) {
      const dataRaw = fs.readFileSync(_path_B, { encoding: "utf8", flag: 'r' });
      data = JSON.parse(dataRaw);
      fileHelper.writeJsonFile(_path_A, data, true);
    } else {
      const dataRaw = fs.readFileSync(_path_A, { encoding: "utf8", flag: 'r' });
      data = JSON.parse(dataRaw);
    }

    if (fs.existsSync(_path_C)) {
      const dataRaw = fs.readFileSync(_path_C, { encoding: "utf8", flag: 'r' });
      const dataProc = JSON.parse(dataRaw);
      data.Presets = dataProc.Presets;
    }

    return data;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};
async function saveGlobalSettings(settings) {
  try {
    const _path_A = fileHelper.resolveEnvVariables('%USERPROFILE%/EDHM_UI/ED_Odissey_Global_Settings.json');
    return fileHelper.writeJsonFile(_path_A, settings, true);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

async function LoadUserSettings () {
  try {
    const _path_A = fileHelper.resolveEnvVariables('%USERPROFILE%/EDHM_UI/ED_Odissey_User_Settings.json');
    var data = {};
    if (fs.existsSync(_path_A)) {
      const dataRaw = fs.readFileSync(_path_A, { encoding: "utf8", flag: 'r' });
      data = JSON.parse(dataRaw);
    } else {
      //404 - File doesnt exists
      data = {
        Name: "UserSettings",
        Title: "User Settings",
        Description: "These settings are preserved between upgrades and take precedence over any applied themes.",
        Elements: []
      };
    }
    return data;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};
async function saveUserSettings(settings) {
  try {
    const _path_A = fileHelper.resolveEnvVariables('%USERPROFILE%/EDHM_UI/ED_Odissey_User_Settings.json');
    return fileHelper.writeJsonFile(_path_A, settings, true);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};
async function AddToUserSettings(newElement) {
  try {
    if (newElement) {
      try { delete newElement.iconVisible; } catch { }

      var userSettings = LoadUserSettings();
      //console.log('userSettings:', userSettings);
      if (userSettings) {
        // Check if an element with the same Key already exists
        const existingElementIndex = userSettings.Elements.findIndex(
          element => element.Key === newElement.Key
        );

        if (existingElementIndex !== -1) {
          // If an element exists, replace it
          userSettings.Elements[existingElementIndex] = newElement;
        } else {
          // If no element exists, add the new element
          userSettings.Elements.push(newElement);
        }

        return saveUserSettings(userSettings);
      }
    }
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};
async function RemoveFromUserSettings(settings) {
  try {
    var userSettings = LoadUserSettings();
    if (userSettings) {
      // Check if an element with the same Key already exists
      const indexToRemove = userSettings.Elements.findIndex(
        element => element.Key === settings.Key
      );

      if (indexToRemove !== -1) {
        // Remove the element using splice()
        userSettings.Elements.splice(indexToRemove, 1); // Remove 1 element at the found index
      } else {
        console.log(`Element with key ${keyToRemove} not found.`);
      }

      return saveUserSettings(userSettings);
    }
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

// #endregion

// #region Game Instances

/** * Adds a new Instance from the Game's Path.
 * @param {*} NewInstancePath Full path to the game
 * @param {*} settings Current Settings
 * @returns Updated Settings */
async function addNewInstance(NewInstancePath, settings) {
  let instanceName = "";

  const instances = [
    { name: "Steam", keyword: "steamapps" },
    { name: "Epic Games", keyword: "Epic Games" },
    { name: "Frontier", keyword: "Frontier" }
  ];

  for (const instance of instances) {
    if (NewInstancePath.includes(instance.keyword)) {
      instanceName = instance.name;
      break;
    }
  }

  const ExeName = path.basename(NewInstancePath, path.extname(NewInstancePath));
  const Publisher = settings.GameInstances.find(game => game.instance === instanceName);
  let Instance = {};

  const gameMappings = {
    "elite-dangerous-64": { name: "Horizons (Legacy)",      key: "ED_Horizons" },
    "FORC-FDEV-D-1013": { name: "Horizons (Legacy)",        key: "ED_Horizons" },
    "FORC-FDEV-D-1010": { name: "Elite Dangerous Base",     key: "ED_Horizons" },
    "FORC-FDEV-D-1012": { name: "Elite Dangerous Arena",    key: "ED_Horizons" },
    "FORC-FDEV-DO-38-IN-40": { name: "Horizons (Live)",     key: "ED_Odissey" },
    "elite-dangerous-odyssey-64": { name: "Odyssey (Live)", key: "ED_Odissey" },
    "FORC-FDEV-DO-1000": { name: "Odyssey & Horizons (Live)", key: "ED_Odissey" }       
  };

  if (Publisher) {
    if (gameMappings[ExeName]) {
      Instance = Publisher.games.find(inst => inst.name === gameMappings[ExeName].name) || {};
      Instance.path = NewInstancePath;
      Instance.name = gameMappings[ExeName].name;
      Instance.key = gameMappings[ExeName].key;
    } else {
      Instance = Publisher.games.find(inst => inst.name === "Odyssey (Live)") || {};
      Instance.path = NewInstancePath;
    }

    Instance.themes_folder = (Instance.key === "ED_Horizons")
      ? path.join(settings.UserDataFolder, "HORIZ", "Themes")
      : path.join(settings.UserDataFolder, "ODYSS", "Themes");

    //settings.ActiveInstance = Instance.instance;
    console.log('New Instance Added: ', Instance.instance);
  }

  return JSON.parse(JSON.stringify(settings));
};

/** * Retrives the Active Instance from the Settings */
const getActiveInstance = () => {
  try {
    if (programSettings != null) {
      //console.log('SettingsHelper.getActiveInstance.programSettings: ', programSettings);

      const instanceName = programSettings.ActiveInstance;
      const gameInstance = programSettings.GameInstances
        .flatMap(instance => instance.games)
        .find(game => game.instance === instanceName);

      if (!gameInstance) {
        throw new Error('Active instance not found');
      }

      return gameInstance;
    } else {
      throw new Error('programSettings is null');
    }
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

/** * Re-load the Settings from file then retrieve the Active instance */
const getActiveInstanceEx = () => {
  try {
    loadSettings();
    //console.log('programSettings', programSettings);

    const instanceName = programSettings.ActiveInstance;
    const gameInstance = programSettings.GameInstances
      .flatMap(instance => instance.games)
      .find(game => game.instance === instanceName);

    if (!gameInstance) {
      throw new Error('Active instance not found');
    }

    return gameInstance;

  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

const getInstanceByName = (InstanceFullName) => {
  try {
    const activeInstanceName = InstanceFullName.toString();
    console.log('InstanceFullName', InstanceFullName);

    if (programSettings != null) {
      const gameInstance = programSettings.GameInstances
        .flatMap(instance => instance.games)
        .find(game => game.instance === activeInstanceName);

      if (!gameInstance) {
        throw new Error('404 - Instance Name Not Found');
      }

      return gameInstance;
    }
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

/** Returns the list of all installed TPMods on the Game Instance. 
 * @param {*} gamePath full path to the Game Instance */
async function GetInstalledTPMods(gamePath) {
  try {
    const tpModsFolder = path.join(gamePath, 'EDHM-ini', '3rdPartyMods');
    const results = { mods: [], errors: [] };
    const files = await readdir(tpModsFolder); 

    for (const file of files) {
      const fullPath = path.join(tpModsFolder, file);
      const fileStats = await stat(fullPath); 

      if (fileStats.isDirectory()) {
        //- Mods inside a Folder
        const subfolderFiles = await readdir(fullPath);
        const jsonFiles = subfolderFiles.filter(f => path.extname(f) === '.json');

        for (const jsonFile of jsonFiles) {
          const jsonFilePath = path.join(fullPath, jsonFile);
          const baseName = path.basename(jsonFile, '.json');
          try {            
            const mod = {
              path: fullPath,
              basename: baseName,

              file_json: jsonFilePath,
              file_ini: path.join(fullPath, `${baseName}.ini`),
              file_thumb: path.join(fullPath, `${baseName}.png`),

              data: await fileHelper.loadJsonFile(jsonFilePath),
              data_ini: await INIparser.LoadIniFile(path.join(fullPath, `${baseName}.ini`)),
            };
            results.mods.push(mod);
          } catch (error) {
            results.errors.push({ msg: baseName + ': ' + error.message, stack: error.stack });
          }
        }

      } else {
        //- Lose mods on the root
        if (path.extname(file) === '.json') {
          try {
            const baseName = path.basename(file, '.json');
            const mod = {
              path: tpModsFolder,
              basename: baseName,

              file_json: fullPath,
              file_ini: path.join(tpModsFolder, `${baseName}.ini`),
              file_thumb: path.join(tpModsFolder, `${baseName}.png`),

              data: await fileHelper.loadJsonFile(fullPath),
              data_ini: await INIparser.LoadIniFile(path.join(tpModsFolder, `${baseName}.ini`))
            };
            results.mods.push(mod);
          } catch (error) {
            results.errors.push({ msg: error.message, stack: error.stack });
          }
        }
      }
    }
    return results;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};


// #endregion

// #region Mod Installing

/** Installs the Mod and themes in their respective locations
 * @param {*} gameInstance Game instance where to install the mod  */
async function installEDHMmod(gameInstance) {
  console.log('------ Installing Mod --------');
  let Response = { game: '', version: '' };
  try {

    if (!Util.isNotNullOrEmpty(gameInstance.path)) {
      console.log('Instance.path Not Defined! ->', gameInstance);
      throw new Error('Instance.path Not Defined!');
    }

    // Check if gameInstance.path is an array
    let gamePath = gameInstance.path;
    if (Array.isArray(gamePath) && gamePath.length > 0) {
      gamePath = gamePath[0]; // Take the first item
      console.log('Instance.path is an array, using first item:', gamePath);
    } else if (Array.isArray(gamePath) && gamePath.length === 0) {
      console.log('Instance.path is an empty array:', gamePath);
      throw new Error('Instance.path is an empty Array!');
    }

    const GameType = gameInstance.key === 'ED_Odissey' ? 'ODYSS' : 'HORIZ';
    const AssetsPath = fileHelper.getAssetPath(`data/${GameType}`);                       //console.log('AssetsPath', AssetsPath);
    const userDataPath = fileHelper.resolveEnvVariables(programSettings.UserDataFolder);

    // #region Un-Zipping Themes

    const unzipPath = path.join(userDataPath, GameType);
    const themesZipPath = path.join(AssetsPath, `${GameType}_EDHM-Themes.zip`); //<- ODYSS_EDHM-Themes.zip
    console.log('themesZipPath', themesZipPath);
    if (themesZipPath) {
      const _ret = await fileHelper.decompressFile(themesZipPath, unzipPath);
      if (_ret) {
        console.log('Themes Installed ->', unzipPath);
      }
    }

    // #endregion

    // #region SymLinks

    //Check for Game Symlinks for 'EDHM-ini' & ShaderFixes
    const Symlink_TargetFolder = path.join(userDataPath, GameType, 'EDHM'); console.log('Symlink_TargetFolder ->', Symlink_TargetFolder);
    const edhmSymLinkTarget = fileHelper.ensureDirectoryExists(path.join(Symlink_TargetFolder, 'EDHM-Ini'));
    const shaderSymLinkTarget = fileHelper.ensureDirectoryExists(path.join(Symlink_TargetFolder, 'ShaderFixes'));

    if (!fs.existsSync(edhmSymLinkTarget)) {
      throw new Error('Could not create the target folder for Simlink "EDHM-Ini".');
    }

    const SymlinkEdhmIni = await fileHelper.ensureSymlink(edhmSymLinkTarget, path.join(gamePath, 'EDHM-ini'));
    const SymlinkShaders = await fileHelper.ensureSymlink(shaderSymLinkTarget, path.join(gamePath, 'ShaderFixes'));
    console.log('SymlinkEdhmIni: ', SymlinkEdhmIni);
    console.log('SymlinkShaders: ', SymlinkShaders);

    // #endregion

    // #region Un-Zipping Mod Files

    const edhmZipFile = await fileHelper.findFileWithPattern(AssetsPath, `${GameType}_EDHM-v*.zip`); //<- ODYSS_EDHM-v19.06.zip
    if (edhmZipFile) {
      console.log('edhmZipFile: ', edhmZipFile);
      const unzipGamePath = gamePath;
      const versionMatch = edhmZipFile.match(/v\d+\.\d+/); console.log('version', versionMatch);

      console.log('Unziping into -> ', unzipGamePath);
      const _ret = await fileHelper.decompressFile(edhmZipFile, unzipGamePath);
      console.log('Zip Result: ', _ret);
      console.log('EDHM Installed!');

      Response.game = GameType;
      Response.version = versionMatch[0];

    } else {
      throw new Error('404 - Zip File Not Found');
    }

    // #endregion

  } catch (error) {
    throw new Error(error.message + error.stack);
  }
  return Response;
};


async function CheckEDHMinstalled(gamePath) {
  try {
    return fileHelper.checkFileExists(path.join(gamePath, 'd3dx.ini'));
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

/** Removes all EDHM Files and Folders from the Game path. * 
 * @param {*} gameInstance Instance of the Game where EDHM is installed.
 * @returns true/false  */
async function UninstallEDHMmod(gameInstance) {
  console.log('------ Un-Installing Mod --------');
  let fileDeleted = false;

  try {
    if (!Util.isNotNullOrEmpty(gameInstance.path)) {
      console.log('Instance.path Not Defined! ->', gameInstance);
      throw new Error('Instance.path Not Defined!');
    }

    // Close the Game if is Running:
    const _R = new Promise((resolve, reject) => {
      fileHelper.terminateProgram('EliteDangerous64.exe', (error, result) => {
        if (error) {
          reject(error);
        } else {
          resolve(result);
        }
      });
    });

    // List the Files for Deletion:
    const filesToUnlink = [
      'EDHM-ini',
      'ShaderFixes',
      'd3dx.ini',
      'd3d11.dll',
      'd3dcompiler_46.dll',
      'EDHM-Uninstall.bat',
    ];

    for (const file of filesToUnlink) {
      const filePath = path.join(gameInstance.path, file);
      try {
        //Delete the File if it Exists:
        if (await access(filePath).then(() => true).catch(() => false)) {
          await unlink(filePath);
          fileDeleted = true;
        }
      } catch (err) {
        //On Error Continue with the next file
        console.warn(`Failed to delete ${file}:`, err);
      }
    }

  } catch (error) {
    console.error('Error during uninstallation:', error);
    throw new Error(error.message + error.stack);
  }

  return fileDeleted;
};

/** This are actions to be run after an App Update is applied and Before EDHM mod is installed. */
async function DoHotFix() {
  try {
    const hotfixJsonPath = fileHelper.getAssetPath('data/EDHM_HOTFIX.json');
    if (hotfixJsonPath) {
      const hotFix = fileHelper.loadJsonFile(hotfixJsonPath);
      if (hotFix) {

        const AppExePath = fileHelper.resolveEnvVariables('%LOCALAPPDATA%\\EDHM-UI-V3');
        const UI_DOCUMENTS = fileHelper.resolveEnvVariables('%USERPROFILE%\\EDHM_UI');
        const GameInstances = readSetting('GameInstances');

        if (GameInstances?.length > 0) {
          for (const instance of GameInstances) {
            for (const game of instance.games) {
              if (game.path && game.path !== '') {

                console.log('------ Applying HotFixes on ', game.instance);
                const GamePath = game.path;

                for (const _job of hotFix.active_jobs) {

                  //- 1. Start by Resolving path Variables:
                  _job.file_path = _job.file_path.replace("%GAME_PATH%", GamePath);
                  _job.file_path = _job.file_path.replace("%UI_PATH%", AppExePath);
                  _job.file_path = _job.file_path.replace("%UI_DOCS%", UI_DOCUMENTS);
                  if (!Util.isEmpty(_job.destination)) {
                    _job.destination = _job.destination.replace("%GAME_PATH%", GamePath);
                    _job.destination = _job.destination.replace("%UI_PATH%", AppExePath);
                  }

                  //- 2. Resolve the Job Actions:
                  try {
                    const folder_path = path.dirname(_job.file_path); //Obtiene el Path: (Sin archivo ni extension:
                    const file_name = path.basename(_job.file_path); //<- Nombre del Archivo con Extension (Sin Ruta)

                    switch (_job.action) {
                      
                      case "COPY": //Copia un Archivo o Directorio de un lugar a otro, acepta comodines
                        console.log('- COPY: ' + file_name + ' -> ' + path.dirname(_job.destination));
                        await fileHelper.copyFile(_job.file_path, _job.destination, false);
                        break;

                      case "DEL": //Borra un Archivo
                        console.log('- DEL: ' + _job.file_path);
                        //Borra archivos usando comodines
                        if (file_name.includes("*")) {
                          await fileHelper.deleteFilesByWildcard(_job.file_path);
                        }
                        else {
                          //Borra el archivo indicado, si existe
                          fileHelper.deleteFileByAbsolutePath(_job.file_path);
                        }
                        break;

                      case "REPLACE": //Copia el Archivo sólo si existe previamente
                        console.log('- REPLACE: ' + file_name + ' -> ' + path.dirname(_job.destination));
                        if (fs.existsSync(_job.file_path) && fs.existsSync(_job.destination)) {
                          await fileHelper.copyFile(_job.file_path, _job.destination, false);
                        }
                        break;

                      case "MOVE": //Mueve un Archivo de un lugar a otro, acepta comodines
                        console.log('- MOVE: ' + file_name + ' -> ' + path.dirname(_job.destination));
                        if (fs.existsSync(_job.file_path)) {
                          if (!fs.existsSync(path.dirname(_job.destination))) {
                            Directory.CreateDirectory(path.dirname(_job.destination));
                          }
                          await fileHelper.copyFile(_job.file_path, _job.destination, true);
                        }
                        break;

                      case "RMDIR": //Borra un Directorio y todo su contenido
                        console.log('- RMDIR: ', _job.file_path);
                        await fileHelper.deleteFolderRecursive(_job.file_path);
                        break;

                      case "RMDIR-EX": //Borra las Carpetas de un Directorio salvo las Execpciones
                        //El nombre del directorio Raiz va en 'file_path', ej: "file_path":"%UI_DOCS%\\ODYSS",
                        //Las Excepciones van en 'destination', solo los nombres separados x comas. ej:  "destination":"Themes,History"
                        console.log('- RMDIR-EX: ', folder_path, _job.destination);
                        await fileHelper.deleteDirectoriesExcept(_job.file_path, _job.destination);
                        break;

                      case "MVDIR": //Mueve un Directorio de un lugar a otro
                        console.log('- MVDIR: ', folder_path, _job.destination);
                        await fileHelper.moveDirectory(_job.file_path, _job.destination);
                        break;

                      default:
                        break;
                    }
                  } catch (error) {
                    console.log('Error Applying HotFix -> ', error.message);
                  }
                };
              }
            }
          }
        }
      }
    }
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

// #endregion
/*----------------------------------------------------------------------------------------------------------------------------*/
// #region ipcMain Handlers

ipcMain.handle('GetInstalledTPMods', (event, gamePath) => {
  try {
    return GetInstalledTPMods(gamePath);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('addNewInstance', (event, NewInstancePath, settings) => {
  try {
    return addNewInstance(NewInstancePath, settings);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
}); 

ipcMain.handle('InstallStatus', () => {
  try {
    return installationStatus;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('initialize-settings', async () => {
  try {
    return initializeSettings();
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('load-settings', () => {
  try {
    return loadSettings();
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('LoadGlobalSettings', () => {
  try {
    return LoadGlobalSettings();
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('saveGlobalSettings', (event, settings) => {
  try {
    return saveGlobalSettings(settings);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('LoadUserSettings', () => {
  try {
    return LoadUserSettings();
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('saveUserSettings', (event, settings) => {
  try {
    return saveUserSettings(settings);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('AddToUserSettings', (event, newElement) => {
  try {
    return AddToUserSettings(newElement);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('RemoveFromUserSettings', (event, settings) => {
  try {
    return RemoveFromUserSettings(settings);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('get-settings', () => {
  try {
    return programSettings;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('getDefaultSettings', () => {
  try {
    // Read the default settings JSON file
    const defaultSettings = fs.readFileSync(defaultSettingsPath, 'utf8');
    return JSON.parse(defaultSettings);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('save-settings', (event, settings) => {
  try {
    return saveSettings(settings);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('active-instance', () => {
  try {
    return getActiveInstance();
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('getActiveInstanceEx', () => {
  try {
    return getActiveInstanceEx();
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('getInstanceByName', (event, instanceName) => {
  try {
    return getInstanceByName(instanceName);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('installEDHMmod', (event, gameInstance) => {
  try {
    return installEDHMmod(gameInstance);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('CheckEDHMinstalled', (event, gamePath) => {
  try {
    return CheckEDHMinstalled(gamePath);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('UninstallEDHMmod', (event, gameInstance) => {
  try {
    return UninstallEDHMmod(gameInstance);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('readSetting', (event, key, defaultValue = null) => {
  try {
    return readSetting(key, defaultValue);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('writeSetting', (event, key, value) => {
  try {
    return writeSetting(key, value);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('DoHotFix', async (event) => {
  try {
    return DoHotFix();
  } catch (error) {
    throw new Error(error.message + error.stack);
  }  
});

// #endregion
/*----------------------------------------------------------------------------------------------------------------------------*/

export default { 
  initializeSettings, loadSettings, saveSettings, 
  installEDHMmod, CheckEDHMinstalled, 
  getInstanceByName, getActiveInstance, 
  LoadGlobalSettings, saveGlobalSettings,
  readSetting, writeSetting, DoHotFix,
 };