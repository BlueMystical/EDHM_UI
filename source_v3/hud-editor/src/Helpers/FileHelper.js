import { app, ipcMain, dialog } from 'electron';
import path from 'node:path';
import fs from 'node:fs';
import os from 'os';

// #region Path Finders

function getLocalAppDataPath() {
    switch (process.platform) {
        case 'win32':
            return path.join(os.homedir(), 'AppData', 'Local');
        case 'darwin':
            return path.join(os.homedir(), 'Library', 'Application Support');
        case 'linux':
            return path.join(os.homedir(), '.local', 'share');
        default:
            throw new Error('Unsupported operating system');
    }
}

/** Resolves environment variables in a given path string.
 * @param {string} inputPath The path string containing environment variables.
 * @returns {string} The resolved path, or the original path if no replacements were made. */
const resolveEnvVariables = (inputPath) => {
    try {
        if (typeof inputPath !== 'string') {
            console.warn("Input path must be a string. Returning original input:", inputPath);
            return inputPath;
        }

        const OriginalPath = inputPath;
        let resolvedPath = ''; // Final resolved path

        // #region Resolve Environmental Variables

        const envVars = {
            '%USERPROFILE%': os.homedir(),
            '%APPDATA%': app.getPath('userData'),
            '%LOCALAPPDATA%': getLocalAppDataPath(),
            '%PROGRAMFILES%': process.env.PROGRAMFILES || process.env['ProgramFiles'],
            '%PROGRAMFILES(X86)%': process.env['PROGRAMFILES(X86)'] || process.env['ProgramFiles(x86)'],
            '%PROGRAMDATA%': process.env.PROGRAMDATA,
            '%APPDIR%': app.getAppPath(),
            '$HOME': os.homedir(),
            '~': os.homedir(),
            '%TEMP%': process.env.TEMP || process.env.TMP,
            '$TMPDIR': process.platform === 'win32' ? (process.env.TEMP || process.env.TMP) : (process.env.TMPDIR || '/tmp'),
            '$XDG_CONFIG_HOME': process.env.XDG_CONFIG_HOME || path.join(os.homedir(), '.config'),
            '$XDG_DATA_HOME': process.env.XDG_DATA_HOME || path.join(os.homedir(), '.local', 'share'),
        };

        // Replace environment variables and tilde (~) in the path
        resolvedPath = inputPath.replace(/(~|\$[A-Z_]+|%[^%]+%)/g, (match) => {
            console.log(`Resolving environment variable: ${match} -> ${envVars[match] || match}`);
            return envVars[match] || match;
        });

        // #endregion

        // Normalize and clean the resolved path
        const isWindows = process.platform === 'win32';
        resolvedPath = isWindows
            ? resolvedPath.replace(/\//g, '\\') // Convert forward slashes to backslashes for Windows
            : resolvedPath.replace(/\\/g, '/'); // Convert backslashes to forward slashes for Linux/Mac

        resolvedPath = path.normalize(resolvedPath); // Normalize path to remove redundancies
        //console.log(`Resolving environment variable: ${OriginalPath} -> ${resolvedPath}`);
        return resolvedPath;

    } catch (error) {
        throw new Error(error.message + '\n' + error.stack);
    }
};

/** Returns the Parent Folder of the given Path 
 *  @param {*} givenPath  */
function getParentFolder(givenPath) {
    return path.dirname(givenPath);
}

// #endregion

// #region Files & Directories

/** Verifies is a File or Directory Exists
 * @param {*} filePath Full path to the File */
const checkFileExists = (filePath) => {
    try {
        if (fs.existsSync(filePath)) {
            return true;
        } else {
            return false; //File does not exist
        }
    } catch (error) {
        throw new Error(error.message + error.stack);
    }
};

/** If the Directory doesn't exist, it is created.
 * @param {*} DirectoryPath Path to the Directory. */
function ensureDirectoryExists(DirectoryPath) {
  try {
    const resolvedPath = resolveEnvVariables(DirectoryPath);
    if (!fs.existsSync(resolvedPath)) {
      fs.mkdirSync(resolvedPath, { recursive: true });
    }
    return resolvedPath;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

/** Copy a file from one location to another.
 * @param {*} sourcePath Full path of the source file
 * @param {*} destinationPath full path of the destination, ensures the folder exists.
 * @param {*} move [Optional] If true, the file is moved instead of copied. Default is false. */
async function copyFile(sourcePath, destinationPath, move = false) {
    try {
        const sourceFileName = path.basename(sourcePath);
        const destinationDir = path.dirname(destinationPath);

        // Ensure destination directory exists
        if (!fs.existsSync(destinationDir)) {
            fs.mkdirSync(destinationDir, { recursive: true });
        }

        if (move) {
            fs.renameSync(sourcePath, destinationPath);
            console.log(`File moved from ${sourcePath} to ${destinationPath}`);
        } else {
            fs.copyFileSync(sourcePath, destinationPath);
            console.log(`File copied from ${sourcePath} to ${destinationPath}`);
        }
    } catch (err) {
        console.error(`Error ${move ? 'moving' : 'copying'} file:`, err);
        throw err; // Re-throw the error for the caller to handle
    }
}

// #endregion

// #region Settings

/** Returns the value of a setting from the settings JSON file. 
 * @param {*} key Name of a Key in the Settings
 * @returns The Value os the indicated Key */
function readSetting(key, defaultValue = null) {
  try {
    const programSettingsPath = resolveEnvVariables('%USERPROFILE%\\EDHM_UI\\Settings.json');
    const data = fs.readFileSync(programSettingsPath, 'utf8');
    const settings = JSON.parse(data);

    return settings.hasOwnProperty(key) && settings[key] !== "" ? settings[key] : defaultValue;
  } catch (error) {
    console.error('Error reading setting:', error);
    return defaultValue;
  }
}
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

// #region JSON Files

/** Loads a JSON file from the specified path. *
 * @param {string} filePath The path to the JSON file.
 * @returns {Object|null} The parsed JSON object, or null if the file does not exist or cannot be parsed. */
const loadJsonFile = (filePath) => {
    const resolvedPath = resolveEnvVariables(filePath);
    console.log('Intentando leer JSON desde:', resolvedPath);
    try {
        if (!fs.existsSync(resolvedPath)) {
            console.warn(`File not found: ${resolvedPath}`);
            return null;
        }

        const data = fs.readFileSync(resolvedPath, 'utf8');
        return JSON.parse(data);
    } catch (err) {
        console.error(`Error loading JSON file: ${resolvedPath}`, err);
        throw err;
    }
};

/** Writes an object to a JSON file.
 * @param {string} filePath The path to the JSON file.
 * @param {Object} data The object to be written to the file.
 * @param {boolean} [prettyPrint=true] Whether to pretty-print the JSON output. */
const writeJsonFile = (filePath, data, prettyPrint = true) => {
    const resolvedPath = resolveEnvVariables(filePath);
    try {
        ensureDirectoryExists(path.dirname(resolvedPath)); // Ensure parent directory exists

        const options = prettyPrint ? 4 : null;
        fs.writeFileSync(resolvedPath, JSON.stringify(data, null, options));
        return true;

    } catch (error) {
        throw new Error(error.message + error.stack);
    }
};

// #endregion

// #region Common Dialogs



// #endregion


/** Returns the path to the EDHM data directory. */
ipcMain.handle('GetAppDataDirectory', (event) => {
    try {
        return resolveEnvVariables(
            readSetting('UserDataFolder', '%USERPROFILE%\\EDHM_UI')
        );
    } catch (error) {
        throw new Error(error.message + error.stack);
    }
});
ipcMain.handle('get-json-file', async (event, jsonPath) => {
  try {
    return loadJsonFile(jsonPath);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('writeJsonFile', async (event, filePath, data, prettyPrint) => {
  try {
    return writeJsonFile(filePath, data, prettyPrint);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('checkFileExists', async (event, fullPath) => {
  try {
    return fs.existsSync(fullPath);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('copyFile', async (event, sourcePath, destinationPath, move = false) => {
  try {
    return copyFile(sourcePath, destinationPath, move);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
// Returns the local path (Cross-platform compatible) of the given path who is using Env.Vars.
ipcMain.handle('resolve-env-variables', async (event, inputPath) => {
  try {
    const resolvedPath = resolveEnvVariables(inputPath);
    return resolvedPath;
  } catch (error) {
    console.error('Failed to resolve environment variables:', error);
    //logEvent(error.message, error.stack);
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

ipcMain.handle('ShowOpenDialog', async (event, options) => {
  /*  
  const homeDir = await window.api.resolveEnvVariables('%USERPROFILE%');
  const LastFolderUsed = await window.api.readSetting('LastFolderUsed', homeDir);
  const options = {
      title: '',  //The dialog title. Cannot be displayed on some Linux desktop
      defaultPath: LastFolderUsed, //Absolute directory path, absolute file path, or file name to use by default.
      buttonLabel : '',  //(optional) - Custom label for the confirmation button
      
      filters: [
        { name: 'Images', extensions: ['jpg', 'jpeg', 'png', 'gif'] },
        { name: 'Movies', extensions: ['mkv', 'avi', 'mp4'] },
        { name: 'Custom File Type', extensions: ['as'] },
         { name: 'Zip Files', extensions: ['zip'] },
        { name: 'All Files', extensions: ['*'] }
      ],
      //--- Choose only one: 'openFile', 'openDirectory':
      properties: ['openFile', 'openDirectory', 'multiSelections', 'showHiddenFiles', 'createDirectory', 'promptToCreate', 'dontAddToRecent'],
      message: 'This message will only be shown on macOS', // (optional)
    }; 
  */
  try {
    const result = dialog.showOpenDialogSync(options);
    return result;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
  /*  EXAMPLE:
      window.api.ShowOpenDialog(options).then(filePath => {
          if (filePath) {
              const FolderPath = window.api.getParentFolder(filePath[0]);
              this.config.GameInstances[instanceIndex].games[gameIndex].path = FolderPath;
              InstallGameInstance(FolderPath);
          }
      }); 
  o tambien asi:
      const filePath = await window.api.ShowOpenDialog(options);
      if (filePath && filePath.filePaths && filePath.filePaths.length > 0) {
        const folderPath = window.api.getParentFolder(filePath.filePaths[0]);
      }
  */
});
ipcMain.handle('ShowSaveDialog', async (event, options) => {
  /*  
    const homeDir = await window.api.resolveEnvVariables('%USERPROFILE%');
  const LastFolderUsed = await window.api.readSetting('LastFolderUsed', homeDir);
  const options = {
      title: '',  //The dialog title. Cannot be displayed on some Linux desktop
      defaultPath: LastFolderUsed,
      buttonLabel : '',  //(optional) - Custom label for the confirmation button
      filters: [
        { name: 'Images', extensions: ['jpg', 'png', 'gif'] },
        { name: 'Movies', extensions: ['mkv', 'avi', 'mp4'] },
        { name: 'Custom File Type', extensions: ['as'] },
        { name: 'All Files', extensions: ['*'] }
      ],
      //--- Choose only one: 'openFile', 'openDirectory':
      properties: ['showHiddenFiles', 'createDirectory', 'showOverwriteConfirmation ', 'dontAddToRecent'],
      message: 'This message will only be shown on macOS', // (optional)
    }; 
  */
  try {
    const result = dialog.showSaveDialogSync(options);
    return result;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
  /*  EXAMPLE:
   await window.api.ShowSaveDialog(options).then(filePath => {
        if (filePath) {
            const FolderPath = window.api.getParentFolder(filePath[0]);
            this.config.GameInstances[instanceIndex].games[gameIndex].path = FolderPath;
            InstallGameInstance(FolderPath);
        }
    }); 
*/
});
ipcMain.handle('ShowMessageBox', async (event, options) => {
  /*  MODO DE USO:
      const options = {
        type: 'question', //<- none, info, error, question, warning
        buttons: ['Cancel', "Yes, It's a Favorite", 'No, thanks.'],
        defaultId: 1,
        title: 'Favorite?',
        message: 'Do you want to Favorite this new theme?',
        detail: '',
        cancelId: 0
      };
      window.api.ShowMessageBox(options).then(result => {
        if (result && result.response === 1) {
          // DO SOMEHTING 
        }
      });
      o tambien asi:
      const result = await window.api.ShowMessageBox(options);
      if (result && result.response === 1) { }
  */
  try {
    const result = await dialog.showMessageBox(options);
    return result;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

export default {
    resolveEnvVariables,
    loadJsonFile, writeJsonFile,
    copyFile,
    checkFileExists, ensureDirectoryExists,
    getParentFolder, getLocalAppDataPath,
    readSetting, writeSetting
}