import { ipcMain } from 'electron';
import path from 'node:path';
import fs from 'fs';
import fileHelper from './FileHelper'; 
import Log from './LoggingHelper.js';


let programSettings = null; // Holds the Program Settings
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

/**
 * Check if the Settings JSON exists, if it does not, creates a default file and returns 'false'
 */
export const initializeSettings = async () => {
    try {
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
        Log.Error(error);
    }
    return programSettings;
};

/**
 * Loads the Settings
 * @returns the settings data
 */
const loadSettings = () => {
    try {
        if (!fs.existsSync(userSettingsPath)) {
            throw new Error(`User settings file not found at: ${userSettingsPath}`);
        }
        const data = fs.readFileSync(userSettingsPath, 'utf-8');
        programSettings = JSON.parse(data);
        return programSettings;
    } catch (error) {
        Log.Error(error.message, error.stack);
        return null;
    }
};


/**
 * Save changes on the settings back to the JSON file
 * @param {*} settings must 'JSON.stringify' the object before sending it here
 */
const saveSettings = (settings) => {
    try {
        fs.writeFileSync(userSettingsPath, settings, 'utf-8');

        programSettings = JSON.parse(settings);
        //console.log('Settings file saved successfully.', settings);
    } catch (error) {
        Log.Error(error.message, error.stack);
    }
};

/**
 * Retrives the Active Instance from the Settings
 */
const getActiveInstance = () => {
    if (programSettings != null) {
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
  };

/*----------------------------------------------------------------------------------------------------------------------------*/
ipcMain.handle('InstallStatus', () => {
  return installationStatus;
});

ipcMain.handle('initialize-settings', async () => {
    return initializeSettings();
});

ipcMain.handle('load-settings', () => {
    return loadSettings();
});
ipcMain.handle('get-settings', () => {
    return programSettings;
});

ipcMain.handle('save-settings', (event, settings) => {
    saveSettings(settings);
    return true;
});

ipcMain.handle('active-instance', () => {
    try {
      return getActiveInstance();
    } catch (error) {
      console.error('Error retrieving active instance:', error);
      throw error;
    }
  });

/*----------------------------------------------------------------------------------------------------------------------------*/

export default { initializeSettings, loadSettings, saveSettings };