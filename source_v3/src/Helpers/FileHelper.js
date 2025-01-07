const { app, ipcMain, dialog } = require('electron');
const path = require('path');
const fs = require('fs');
const os = require('os');

//const { exec } = require('child_process');
const findProcess = require('find-process');
const url = require('url');
const zl = require("zip-lib");

// #region Path Functions


/** Open the File Explorer showing a selected Folder
 * @param {*} filePath Folder to select
 */
function openPathInExplorer(filePath) {
  const normalizedPath = resolveEnvVariables(
    path.normalize(filePath)); // Normalize path to avoid issues

  let command;

  if (os.platform() === 'win32') {
      command = `start "" "${normalizedPath}"`;
  } else if (os.platform() === 'darwin') {
      command = `open "${normalizedPath}"`;
  } else {
      command = `xdg-open "${normalizedPath}"`;
  }

  exec(command, (error, stdout, stderr) => {
      if (error) {
          console.error(`Error opening path: ${error.message}`);
          return;
      }
      if (stderr) {
          console.error(`stderr: ${stderr}`);
          return;
      }
      console.log(`Path opened successfully: ${stdout}`);
  });
};

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
 *
 * @param {string} inputPath The path string containing environment variables.
 * @returns {string} The resolved path, or the original path if no replacements were made.
 */
const resolveEnvVariables = (inputPath) => {
  try {
    if (typeof inputPath !== 'string') {
      console.warn("Input path must be a string. Returning original input:", inputPath);
      return inputPath;
    }

    let envVarPath = ''; // Path with resolved Env.Vars
    let normalPath = ''; // Path with resolved normal Path
    let resolvedPath = ''; // Final resolved Path

    // #region Resolve Enviromental Variables if Present

    // Check if we have Environment Variables that we need to resolve
    let isEnvVar = inputPath.includes('%'); // True if inputPath contains '%'
    if (isEnvVar) {

      const envVars = {
        '%USERPROFILE%': os.homedir(), 
        '%APPDATA%': app.getPath('userData'), 
        '%LOCALAPPDATA%': getLocalAppDataPath(), 
        '%PROGRAMFILES%': process.env.PROGRAMFILES || process.env['ProgramFiles'], 
        '%PROGRAMFILES(X86)%': process.env['PROGRAMFILES(X86)'] || process.env['ProgramFiles(x86)'], 
        '%PROGRAMDATA%': process.env.PROGRAMDATA,
        '%APPDIR%': app.getAppPath(),
      };
      
      // Logging environment variables for debugging
      //console.log('Environment Variables:', envVars);
      
      // Extract and resolve the Env.Var portion from the inputPath
      envVarPath = inputPath.split('%')[1];   
      envVarPath = '%' + envVarPath + '%'; // Re-add the % to the Env.Var
      envVarPath = envVarPath.replace(/%([^%]+)%/g, (match) => {
        console.log(`Resolving environment variable: ${match} -> ${envVars[match] || ''}`);
        return envVars[match] || '';
      });
    }

    // #endregion

    // #region Resolve Absolute paths, if present

    // Extract the non Env.Var portion of the path
    let nonEnvVarPart = isEnvVar ? inputPath.replace(/%[\w]+%/, '') : inputPath;
    let separator = nonEnvVarPart.includes('\\') ? '\\' : '/';  // Detect separator type
    let pathComponents = nonEnvVarPart.split(separator);

    // re-assemble the path using the platform path separator
    normalPath = path.join(...pathComponents);

    // #endregion

    // Combine envVarPath and normalPath
    resolvedPath = isEnvVar ? path.join(envVarPath, normalPath) : normalPath;

    // Ensure the resolved path has the initial slash if needed
    if (os.platform() === 'linux' && (pathComponents[0] === '' || resolvedPath.startsWith('/'))) {
      resolvedPath = '/' + resolvedPath.replace(/^\/+/, '');
    }

    // Handle initial slash for Linux 
    if (os.platform() !== 'win32' && resolvedPath.startsWith('/') && !resolvedPath.startsWith('//')) { 
      resolvedPath = '/' + resolvedPath.replace(/^\/+/, ''); 
    }

    return resolvedPath;

  } catch (error) {
    throw error;
  }
};


// #endregion

// #region Assets Handling

/** Gets the absolute path to an asset, handling differences between development and production environments.
 * In development, it resolves the path relative to the project's root directory.
 * In production (when the application is packaged), it resolves the path relative to the 'resources' directory.
 *
 * @param {string} assetPath The relative path to the asset (e.g., 'data/config.json', 'images/logo.png').
 * @returns {string} The absolute path to the asset.
 */
function getAssetPath(assetPath) {
  try {
    if (process.env.NODE_ENV === 'development') {
      return path.join(__dirname, '../../src', assetPath); // Dev path
    } else {
      return path.join(process.resourcesPath, assetPath); // Correct Prod path
    }
  } catch (error) {
    throw error;
  }  
}

// #endregion

// #region Files & Directories

/** If the Directory doesn't exist, it is created.
 * @param {*} DirectoryPath Path to the Directory.
 */
const ensureDirectoryExists = (DirectoryPath) => {
  try {
    const resolvedPath = resolveEnvVariables(DirectoryPath);
    if (!fs.existsSync(resolvedPath)) {
      fs.mkdirSync(resolvedPath, { recursive: true });
    }
  } catch (error) {
    throw error;
  }
};

/** Verifies is a File Exists
 * @param {*} filePath Full path to the File
 */
const checkFileExists = (filePath) => {
  try {
    if (fs.existsSync(filePath)) {
      return true;
    } else {
      return false; //File does not exist
    }
  } catch (error) {
    throw error;
  }
};

function deleteFolderRecursive(folderPath) {
  if (fs.existsSync(folderPath)) {
    fs.readdirSync(folderPath).forEach((file) => {
      const currentPath = path.join(folderPath, file);

      if (fs.lstatSync(currentPath).isDirectory()) {
        deleteFolderRecursive(currentPath); // Recursively delete folder contents
      } else {
        fs.unlinkSync(currentPath); // Delete file
      }
    });
    fs.rmdirSync(folderPath); // Remove the now-empty folder
  }
}

/** Deletes the given Directory 
 * @param {*} filePath Absolute path to the Directory
 * @returns 
 */
function deleteFileByAbsolutePath(filePath) {
  let _ret = false;
  try {
    fs.unlinkSync(filePath);
    _ret = true;
    console.log('File deleted successfully.');
  } catch (err) {
    console.error('Error deleting file:', err);
  }
  return _ret;
}

/** Deletes all Files of a certain Type  
 * @param {*} directoryPath Path to the directory containing the files
 * @param {*} extension File extension, example: '.txt'
 */
function deleteFilesByType(directoryPath, extension) {
  fs.readdir(directoryPath, (err, files) => {
    if (err) {
      return console.error('Unable to scan directory:', err);
    }

    files.forEach((file) => {
      if (path.extname(file) === extension) {
        const filePath = path.join(directoryPath, file);
        try {
          fs.unlinkSync(filePath);
          console.log(`Deleted: ${filePath}`);
        } catch (err) {
          console.error('Error deleting file:', filePath, err);
        }
      }
    });
  });
  /* Usage example:
  const dirPath = '/path/to/your/directory'; // Directory path
  const fileExtension = '.txt'; // File extension to delete
  deleteFilesByType(dirPath, fileExtension); */
}

/** Busca archivos de un tipo específico en una carpeta y devuelve el archivo con la fecha de modificación o creación más reciente.
 * 
 * @param {string} folderPath - La ruta de la carpeta en la que buscar.
 * @param {string} fileType - El tipo de archivo a buscar (por ejemplo, '.txt' para archivos de texto).
 * @returns {Promise<string>} - El archivo más reciente encontrado.
 */
async function findLatestFile(folderPath, fileType) {
  const files = fs.readdirSync(folderPath)
    .filter(file => path.extname(file) === fileType)
    .map(file => path.join(folderPath, file));
  
  let latestFile = null;
  let latestTime = 0;

  for (const file of files) {
    const stats = fs.statSync(file);
    const modifiedTime = stats.mtimeMs;

    if (modifiedTime > latestTime) {
      latestTime = modifiedTime;
      latestFile = file;
    }
  }

  if (!latestFile) {
    throw new Error(`No files of type ${fileType} found in folder ${folderPath}`);
  }

  return latestFile;
}

/** Busca un archivo en una carpeta utilizando un patrón con comodines.
 * 
 * @param {string} folderPath - La ruta de la carpeta en la que buscar.
 * @param {string} pattern - El patrón de búsqueda con comodines (por ejemplo, 'EDHM_Odyssey_*.zip').
 * @returns {Promise<string>} - El nombre del archivo encontrado.
 */
async function findFileWithPattern(folderPath, pattern) {
  const regexPattern = new RegExp('^' + pattern.replace('*', '.*') + '$');
  const files = fs.readdirSync(folderPath)
    .filter(file => regexPattern.test(file))
    .map(file => path.join(folderPath, file));

  if (files.length === 0) {
    throw new Error(`No files matching pattern ${pattern} found in folder ${folderPath}`);
  }

  // Return the first match
  return files[0];

  /* Ejemplo de uso:
  findFileWithPattern('/ruta/a/tu/carpeta', 'EDHM_Odyssey_*.zip')
    .then(file => console.log(`Archivo encontrado: ${file}`))
    .catch(err => console.error(err)); */
}

/** Verifica si un symlink existe y lo crea si no existe.
 * 
 * @param {string} targetFolder - La carpeta objetivo a la que debe apuntar el symlink.
 * @param {string} symlinkPath - La ruta donde se debe crear el symlink.
 */
async function ensureSymlink(targetFolder, symlinkPath) {
  try {
    // Check if the target folder exists, create it if it doesn't
    if (!fs.existsSync(targetFolder)) {
      console.log(`Target folder does not exist, creating: ${targetFolder}`);
      fs.mkdirSync(targetFolder, { recursive: true });
    }

    const stats = fs.lstatSync(symlinkPath);

    if (stats.isSymbolicLink()) {
      console.log(`Symlink already exists: ${symlinkPath}`);
      return 'exists';
    }

    console.log(`Path exists but is not a symlink: ${symlinkPath}`);
    fs.rmdirSync(symlinkPath, { recursive: true });

  } catch (err) {
    if (err.code !== 'ENOENT') { throw err; }
    // Symlink does not exist, proceed to create it
  }

  fs.symlinkSync(targetFolder, symlinkPath, 'junction');
  console.log(`Symlink created: ${symlinkPath} -> ${targetFolder}`);
  return 'created';

  /* Ejemplo de uso:
ensureSymlink('/ruta/al/target', '/ruta/al/symlink')
  .then(() => console.log('Proceso completado'))
  .catch(err => console.error(err)); */
}




// #endregion

// #region JSON Files

/** Loads a JSON file from the specified path.
 *
 * @param {string} filePath The path to the JSON file.
 * @returns {Object|null} The parsed JSON object, or null if the file does not exist or cannot be parsed.
 */
const loadJsonFile = (filePath) => {
  const resolvedPath = resolveEnvVariables(filePath);
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
 *
 * @param {string} filePath The path to the JSON file.
 * @param {Object} data The object to be written to the file.
 * @param {boolean} [prettyPrint=true] Whether to pretty-print the JSON output.
 */
const writeJsonFile = (filePath, data, prettyPrint = true) => {
  const resolvedPath = resolveEnvVariables(filePath);
  try {
    ensureDirectoryExists(path.dirname(resolvedPath)); // Ensure parent directory exists

    const options = prettyPrint ? { spaces: 4 } : null;
    fs.writeFileSync(resolvedPath, JSON.stringify(data, null, options));
  } catch (error) {
    throw error;
  }
};

// #endregion

// #region ZIP Files

async function compressFiles(files, outputPath) {
//TODO: Implement this function
}

async function compressFolder(folderPath, outputPath) {
  zl.archiveFolder(folderPath, outputPath).then(function () {
      console.log("done");
      return true;
  }, function (err) {
      console.log(err);
      throw err;
  });
}

async function decompressFile(zipPath, outputDir) {
  if (!fs.existsSync(zipPath)) {
    throw new Error(`ZIP file not found: ${zipPath}`);
  }

  zl.extract(zipPath, outputDir).then(function () {
    console.log("done");
    return true;

  }, function (err) {
    console.log(err);
    throw err;
  });

}

// #endregion


function isNotNullOrEmpty(obj) {
  return obj && Object.keys(obj).length > 0;
}


/*----------------------------------------------------------------------------------------------------------------------------*/
// #region ipcMain Handlers

ipcMain.handle('get-app-version', async () => {
  const appPath = app.getAppPath();
  const packageJsonPath = path.join(appPath, 'package.json');
  const packageJson = JSON.parse(fs.readFileSync(packageJsonPath, 'utf8'));
  return packageJson.version;
});


ipcMain.handle('ShowMessageBox', async (event, options) => {
  /*
const options = {
    type: 'warning', //<- none, info, error, question, warning
    buttons: ['Cancel', 'Yes, please', 'No, thanks'],
    defaultId: 1,
    title: 'Question',
    message: 'Do you want to proceed?',
    detail: 'It does not really matter',
    cancelId: 0,
    checkboxLabel: 'Remember my answer', checkboxChecked: false,
  }; 
*/
  try {
    const result = await dialog.showMessageBox(options);
    return result;
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('ShowOpenDialog', async (event, options) => {
  /*  
  const options = {
      title: '',  //The dialog title. Cannot be displayed on some Linux desktop
      defaultPath: '', //Absolute directory path, absolute file path, or file name to use by default.
      buttonLabel : '',  //(optional) - Custom label for the confirmation button
      filters: [
        { name: 'Images', extensions: ['jpg', 'png', 'gif'] },
        { name: 'Movies', extensions: ['mkv', 'avi', 'mp4'] },
        { name: 'Custom File Type', extensions: ['as'] },
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
    throw error;
  }
});
ipcMain.handle('ShowSaveDialog', async (event, options) => {
  /*  
  const options = {
      title: '',  //The dialog title. Cannot be displayed on some Linux desktop
      defaultPath: '', //Absolute directory path, absolute file path, or file name to use by default.
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
    throw error;
  }
});

ipcMain.handle('checkProcess', async (event, processName) => {
  try {
    const processes = await findProcess('name', processName);
    if (processes.length > 0) {
      const process = processes[0];
      console.log(`${processName} is running at path: ${process.bin}`);
      return process.bin;
    } else {
      console.log(`${processName} is not running`);
      return null;
    }
  } catch (error) {
    console.error('Error occurred in handler for checkProcess:', error);
    throw error;
  }
});





ipcMain.handle('openPathInExplorer', async (event, filePath) => {
  try {
    const result = openPathInExplorer(filePath);
    return result;
  } catch (error) {
    throw error;
  }
});

// Returns the local path (Cross-platform compatible) of the given path who is using Env.Vars.
ipcMain.handle('resolve-env-variables', async (event, inputPath) => {
  try {
    const resolvedPath = resolveEnvVariables(inputPath);
    return resolvedPath;
  } catch (error) {
    console.error('Failed to resolve environment variables:', error);
    logEvent(error.message, error.stack);
    throw error;
  }
});

// Returns the path of an Asset (a file included with the program) ex: 'images\PREVIEW.jpg', 'data\Settings.json'
ipcMain.handle('get-asset-path', async (event, assetPath) => {
  try {
    const resolvedPath = getAssetPath(assetPath);
    return resolvedPath;
  } catch (error) {
    console.error('Failed to resolve asset path:', error);
    //logEvent(error.message, error.stack);
    throw error;
  }
});

// Returns the URL of an Asset (a file included with the program) ex: 'images\PREVIEW.jpg', 'data\Settings.json'
ipcMain.handle('get-asset-file-url', async (event, assetPath) => {
  try {
    const resolvedPath = getAssetPath(assetPath);
    const fileUrl = url.pathToFileURL(resolvedPath).toString();
    return fileUrl;
  } catch (error) {
    console.error('Failed to resolve asset path:', error);
    //logEvent(error.message, error.stack);
    throw error;
  }
});

// Returns the URL of a local file, resolves Env.Vars and Cross-platform paths
ipcMain.handle('get-local-file-url', async (event, localPath) => {
  try {
    const resolvedPath = resolveEnvVariables(localPath);
    const fileUrl = url.pathToFileURL(resolvedPath).toString();
    return fileUrl;
  } catch (error) {
    console.error('Failed to resolve local path:', error);
    //logEvent(error.message, error.stack);
    throw error;
  }
});

ipcMain.handle('get-json-file', async (event, jsonPath) => {
  try {
    return loadJsonFile(jsonPath);
  } catch (error) {
    throw error;
  }  
});
ipcMain.handle('writeJsonFile', async (event, filePath, data, prettyPrint) => {
  try {
    return writeJsonFile(filePath, data, prettyPrint);
  } catch (error) {
    throw error;
  }  
});

ipcMain.handle('is-not-null-obj', async (event, obj) => {
  return isNotNullOrEmpty(obj);
});

ipcMain.handle('deleteFileByAbsolutePath', async (event, filePath) => {
  return deleteFileByAbsolutePath(filePath);
});

ipcMain.handle('compress-files', async (event, files, outputPath) => {
  try {
    const result = await compressFiles(files, outputPath);
    return { success: true, message: result };
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('compress-folder', async (event, folderPath, outputPath) => {
  try {
    const result = await compressFolder(folderPath, outputPath);
    return { success: true, message: result };
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('decompress-file', async (event, zipPath, outputDir) => {
  try {
    const result = await decompressFile(zipPath, outputDir);
    return { success: true, message: result };
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('find-latest-file', async (event, folderPath, fileType) => {
  try {
    const result = await findLatestFile(folderPath, fileType);
    return { success: true, file: result };
  } catch (error) {
    throw error;
  }
}); 
ipcMain.handle('findFileWithPattern', async (event, folderPath, pattern) => {
  try {
    const result = await findFileWithPattern(folderPath, pattern);
    return { success: true, file: result };
  } catch (error) {
    throw error;
  }
});


// #endregion

export default { 
  getAssetPath, 
  resolveEnvVariables,  

  loadJsonFile, 
  writeJsonFile, 

  checkFileExists, 
  openPathInExplorer ,
  deleteFilesByType,
  deleteFolderRecursive,
  deleteFileByAbsolutePath,

  findLatestFile,
  findFileWithPattern,
  ensureDirectoryExists, 
  ensureSymlink,

  isNotNullOrEmpty,
  

  compressFiles,
  compressFolder,
  decompressFile
};