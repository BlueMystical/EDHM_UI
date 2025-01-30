import { ipcMain } from 'electron';
import path from 'node:path';

import fs from 'fs';
import { writeFile, unlink, access } from 'node:fs/promises';

import fileHelper from './FileHelper';
import Log from './LoggingHelper.js';
import Util from './Utils.js';


let programSettings = null; // Holds the Program Settings in memory
const defaultSettingsPath = fileHelper.getAssetPath('data/Settings.json');
const userSettingsPath = fileHelper.resolveEnvVariables('%USERPROFILE%\\EDHM_UI\\Settings.json');

const InstallationStatus = {
  NEW_SETTINGS: 'newSettings',
  UPGRADING_USER: 'upgradingUser',
  FRESH_INSTALL: 'freshInstall',
  EXISTING_INSTALL: 'existingInstall'
};
// Determine the Install Status of the V3 Program:
let installationStatus = InstallationStatus.EXISTING_INSTALL; // Default status



/** * Check if the Settings JSON exists, if it does not, creates a default file and returns 'false'
 */
export const initializeSettings = async () => {
  try {
    console.log('userSettingsPath',userSettingsPath);
    const userSettingsDir = path.dirname(userSettingsPath); // Get the directory path
    //console.log('Initializing Settings...Main');

    // Check if the user settings directory exists, if not, create it
    if (!fs.existsSync(userSettingsDir)) {
      fs.mkdirSync(userSettingsDir, { recursive: true });
      console.log(`Created directory: ${userSettingsDir}`);
    }

    // Check if the user settings file exists, if not, read and write the default settings JSON
    if (!fs.existsSync(userSettingsPath)) {
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
      fs.writeFileSync(userSettingsPath, JSON.stringify(programSettings, null, 4));

    } else {
      installationStatus = InstallationStatus.EXISTING_INSTALL; // Is a Normal Existing User
      programSettings = JSON.parse(fs.readFileSync(userSettingsPath, 'utf-8'));
      console.log('Settings Loaded from Existing Instance.');

      //Log.Info('This is a Test');
    }
  } catch (error) {
    throw error;
  }
  return programSettings;
};

/** * Loads the Settings
 * @returns the settings data
 */
const loadSettings = () => {
  try {
    if (!fs.existsSync(userSettingsPath)) {
      throw new Error(`User settings file not found at: ${userSettingsPath}`);
    }
    const data = fs.readFileSync(userSettingsPath, { encoding: "utf8", flag: 'r' });
    //flags: 'a' is append mode, 'w' is write mode, 'r' is read mode, 'r+' is read-write mode, 'a+' is append-read mode
    
    programSettings = JSON.parse(data);
    return programSettings;
  } catch (error) {
    throw error;
  }
};

/** * Adds a new Instance from the Game's Path.
 * @param {*} NewInstancePath Full path to the game
 * @param {*} settings Current Settings
 * @returns Updated Settings
 */
function addNewInstance(NewInstancePath, settings) {
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
    "elite-dangerous-64": { name: "Horizons (Legacy)", key: "ED_Horizons" },
    "FORC-FDEV-DO-38-IN-40": { name: "Horizons (Live)", key: "ED_Odissey" },
    "elite-dangerous-odyssey-64": { name: "Odyssey (Live)", key: "ED_Odissey" },
    "FORC-FDEV-DO-1000": { name: "Odyssey & Horizons (Live)", key: "ED_Odissey" },
    "FORC-FDEV-D-1010": { name: "Elite Dangerous Base", key: "ED_Horizons" },
    "FORC-FDEV-D-1012": { name: "Elite Dangerous Arena", key: "ED_Horizons" },
    "FORC-FDEV-D-1013": { name: "Horizons (Legacy)", key: "ED_Horizons" }
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

    settings.ActiveInstance = Instance.instance;
    console.log('New Instance Added: ', Instance.instance);
  }

  return JSON.parse(JSON.stringify(settings));
};

/** * Save changes on the settings back to the JSON file
 * @param {*} settings must 'JSON.stringify' the object before sending it here
 */
async function saveSettings (settings) {
  try {
    //console.log(settings);
    await writeFile(userSettingsPath, settings, { encoding: "utf8", flag: 'w' });

    //fs.writeFileSync(path, data, { encoding: "utf8", flag: 'a+' }); 
    //flags: 'a' is append mode, 'w' is write mode, 'r' is read mode, 'r+' is read-write mode, 'a+' is append-read mode

    programSettings = JSON.parse(settings); //<- Updates the Settings
    console.log('Settings Saved Successfully');

    return programSettings;
  } catch (error) {
    throw error;
  }
};

/** * Retrives the Active Instance from the Settings
 */
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
    throw error;
  }
};

/** * Re-load the Settings from file then retrieve the Active instance
 */
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
    throw error;
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
    throw error;
  }
};

/** Installs the Mod and themes in their respective locations
 * @param {*} gameInstance Game instance where to install the mod
 */
async function installEDHMmod(gameInstance) {
  console.log('------ Installing Mod --------');
  let Response = { game: '', version: '' };
  try {

    if (!Util.isNotNullOrEmpty(gameInstance.path)) {
      console.log('Instance.path Not Defined! ->', gameInstance);
      throw new Error('Instance.path Not Defined!');
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
    
    const SymlinkEdhmIni = await fileHelper.ensureSymlink(edhmSymLinkTarget,   path.join( gameInstance.path, 'EDHM-ini'));
    const SymlinkShaders = await fileHelper.ensureSymlink(shaderSymLinkTarget, path.join( gameInstance.path, 'ShaderFixes'));
    console.log('SymlinkEdhmIni: ', SymlinkEdhmIni);
    console.log('SymlinkShaders: ', SymlinkShaders);
    
    // #endregion

    // #region Un-Zipping Mod Files

    const edhmZipFile = await fileHelper.findFileWithPattern(AssetsPath, `${GameType}_EDHM-v*.zip`); //<- ODYSS_EDHM-v19.06.zip
    if (edhmZipFile) {
      console.log('edhmZipFile: ', edhmZipFile); 
      const unzipGamePath = gameInstance.path;
      const versionMatch =  edhmZipFile.match(/v\d+\.\d+/);      console.log('version', versionMatch);

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
    throw error;
  }
  return Response;
};

async function CheckEDHMinstalled(gamePath) {
  try {
    return fileHelper.checkFileExists(path.join(gamePath, 'd3dx.ini'));
  } catch (error) {
    throw error;
  }
};

/** Removes all EDHM Files and Folders from the Game path. * 
 * @param {*} gameInstance Instance of the Game where EDHM is installed.
 * @returns true/false
 */
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
    throw error;
  }

  return fileDeleted;
}

/*----------------------------------------------------------------------------------------------------------------------------*/
// #region ipcMain Handlers

ipcMain.handle('addNewInstance', (event, NewInstancePath, settings) => {
  try {
    return addNewInstance(NewInstancePath, settings);
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('InstallStatus', () => {
  try {
    return installationStatus;
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('initialize-settings', async () => {
  try {
    return initializeSettings();
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('load-settings', () => {
  try {
    return loadSettings();
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('get-settings', () => {
  try {
    return programSettings;
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('getDefaultSettings', () => {
  try {
    // Read the default settings JSON file
    const defaultSettings = fs.readFileSync(defaultSettingsPath, 'utf8');
    return JSON.parse(defaultSettings);
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('save-settings', (event, settings) => {
  try {
    return saveSettings(settings);
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('active-instance', () => {
  try {
    return getActiveInstance();
  } catch (error) {
    throw error;
  }
}); 
ipcMain.handle('getActiveInstanceEx', () => {
  try {
    return getActiveInstanceEx();
  } catch (error) {
    throw error;
  }
}); 
ipcMain.handle('getInstanceByName', (event, instanceName) => {
  try {
    return getInstanceByName(instanceName);
  } catch (error) {
    throw error;
  }
}); 

ipcMain.handle('installEDHMmod', (event, gameInstance) => {
  try {
    return installEDHMmod(gameInstance);
  } catch (error) {
    throw error;
  }
}); 
ipcMain.handle('CheckEDHMinstalled', (event, gamePath) => {
  try {
    return CheckEDHMinstalled(gamePath);
  } catch (error) {
    throw error;
  }
}); 
ipcMain.handle('UninstallEDHMmod', (event, gameInstance) => {
  try {
    return UninstallEDHMmod(gameInstance);
  } catch (error) {
    throw error;
  }
}); 

// #endregion
/*----------------------------------------------------------------------------------------------------------------------------*/

export default { initializeSettings, loadSettings, saveSettings, installEDHMmod, CheckEDHMinstalled, getInstanceByName, getActiveInstance };