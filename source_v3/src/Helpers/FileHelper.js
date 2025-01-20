import { app, ipcMain, dialog, shell  } from 'electron'; 
import { exec } from 'child_process';
import path from 'node:path'; 
//import { writeFile , readFile } from 'node:fs/promises'; //
import { copyFileSync, constants } from 'node:fs';
import fs from 'node:fs'; 
import os from 'os'; 
import url from 'url'; 
import zl from 'zip-lib';


// #region Path Functions

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

/** Returns the Parent Folder of the given Path 
 *  @param {*} givenPath 
 * @returns 
 */
function getParentFolder(givenPath) {
    return path.dirname(givenPath);
}

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
    return resolvedPath;
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

/** Deletes the given File 
 * @param {*} filePath Absolute path to the File */
function deleteFileByAbsolutePath(filePath) {
  let _ret = false;
  try {
    fs.unlinkSync(filePath);
    _ret = true;     console.log('File deleted successfully.');
  } catch (err) {
    throw err;
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
    ensureDirectoryExists(targetFolder);

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

async function ShowOpenDialog(options) {
   /*  
  const options = {
      title: '',  //The dialog title. Cannot be displayed on some Linux desktop
      defaultPath: '', //Absolute directory path, absolute file path, or file name to use by default.
      buttonLabel : '',  //(optional) - Custom label for the confirmation button      
      filters: [
        { name: 'Images', extensions: ['jpg', 'jpeg', 'png', 'gif'] }
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
}
async function ShowSaveDialog(options) {
  /*    
        const options = {
          fileName: ThemeName, 
          title: `Exporting Theme '${ThemeName}':`, 
          defaultPath: path.join(app.getPath('desktop'), `${ThemeName}.zip`),
          filters: [
            { name: 'Zip Files', extensions: ['zip'] },
            { name: 'All Files', extensions: ['*'] }
          ],          
          properties: ['createDirectory', 'showOverwriteConfirmation ', 'dontAddToRecent']
        }; 
        let Destination = '';
        await fileHelper.ShowSaveDialog(options).then(filePath => {
          if (filePath) {
            Destination = filePath;
          }
        }); 
        console.log('Destination: ', Destination);
  */
    try {
      const result = dialog.showSaveDialogSync(options);
      return result;
    } catch (error) {
      throw error;
    }
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
    return true;

  } catch (error) {
    throw error;
  }
};


// #endregion

// #region Images

function SaveImageAsJpeg(base64Data, filePath) {
  try {
    // Remove the base64 header if present
    const base64String = base64Data.replace(/^data:image\/jpeg;base64,/, '');
    
    // Decode base64 string to buffer
    const buffer = Buffer.from(base64String, 'base64');
    
    // Write the buffer to disk as a JPEG file
    fs.writeFileSync(filePath, buffer);
    return true;

  } catch (error) {
    throw error;
  }  
};

/** Takes a base64 image and writes it as a JPG image file
 * @param {*} base64Image image's data
 * @param {*} outputPath full path to save the image
 */
function base64ToJpg(base64Image, outputPath) {
  try {
    // Split the base64 string to remove the "data:image/..." part
    const base64Data = base64Image.split(';base64,').pop();

    // Convert base64 string to binary data
    const binaryData = Buffer.from(base64Data, 'base64');

    // Write the binary data to a file
    fs.writeFile(outputPath, binaryData, { encoding: 'binary' }, (err) => {
      if (err) {
        console.error('Error writing file:', err);
        return false;
      } else {
        return true;
      }
    });
  } catch (error) {
    console.error(error);
    throw error;
  }
};

/** Loads an Image from disk * 
 * @param {*} imagePath Absolute path to the image
 * @returns Image data in Base64 format
 */
function loadImageAsBase64(imagePath) {
    return new Promise((resolve, reject) => {
        fs.readFile(imagePath, (err, data) => {
            if (err) {
                reject(err);
            } else {
                // Convert the image data to Base64
                const base64Image = data.toString('base64');
                // Create a Base64 image URL
                const base64ImageUrl = `data:image/${path.extname(imagePath).substring(1)};base64,${base64Image}`;
                resolve(base64ImageUrl);
            }
        });
    });
};

// #endregion

// #region ZIP Files

// DOCUMENTATION:  https://www.npmjs.com/package/zip-lib

/** Compress all files in the given Folder
 * @param {*} folderPath Absolute path to the Origin Folder to Compress
 * @param {*} outputPath Absolute path to the Destination ZIP file
 */
async function compressFiles(folderPath, outputPath) {
  if (!fs.existsSync(folderPath)) {
    throw new Error(`404 - Source Folder Not Found: '${folderPath}'`);
  }
  zl.archiveFolder(folderPath, outputPath, { addFolderToZip: true }).then(function () {
    console.log(`ZIP File Created! -> '${outputPath}'`);
    return true;
  }, function (err) {
    console.log(err);
    throw new Error(err.message + err.stack);
  });
}

/** Compress a Folder and all files inside. (the folder gets in the ZIP)
 * @param {*} folderPath Absolute path to the Origin Folder to Compress
 * @param {*} outputPath Absolute path to the Destination ZIP file
 */
async function compressFolder(folderPath, outputPath) {
  if (!fs.existsSync(folderPath)) {
    throw new Error(`404 - Source Folder Not Found: '${folderPath}'`);
  }

  const zip = new zl.Zip();
  const folderName = path.basename(folderPath);

  // Add the folder and its contents
  zip.addFolder(folderPath, folderName);

  // Generate the ZIP file
  let _ret = false;
  await zip.archive(outputPath).then(function () {
    console.log(`ZIP File Created! -> '${outputPath}'`);
    _ret =  true;
  }, function (err) {
    console.log(err);
    throw new Error(err.message + err.stack);
  });
  return _ret;
}

/** Un-compress the content of a ZIP file. 
 * @param {*} zipPath Absolute path to the ZIP file.
 * @param {*} outputDir Absolute path to the Destination Folder
 */
async function decompressFile(zipPath, outputDir) {
  if (!fs.existsSync(zipPath)) {
    throw new Error(`404 - ZIP file Not Found: '${zipPath}'`);
  }
  let _ret = false;
  await zl.extract(zipPath, outputDir).then(function () {
    console.log(`Uncompressed Files -> '${outputDir}'`);
    _ret =  true;
  }, function (err) {
    console.log(err);
    throw err;
  });
  return _ret;
}

// #endregion

// #region Processs & Programs

export function openUrlInBrowser(url) {
  shell.openExternal(url);
}

/** Detects a running Process and returns its full path. 
 * @param {*} exeName Program Exe Name: 'EliteDangerous64.exe'
 * @param {*} callback 
 */
function detectProgram(exeName, callback) {
  try {
    if (os.platform() === 'win32') {
      // Windows
      exec(`wmic process where name="${exeName}" get ExecutablePath`, (error, stdout) => {
        if (error) {
          return callback(error, null);
        }
        const lines = stdout.trim().split('\n');
        const exePath = lines[1] ? lines[1].trim() : null;
        callback(null, exePath);
      });
    } else {
      // Linux
      exec(`pgrep -f ${exeName}`, (error, stdout) => {
        if (error) {
          return callback(error, null);
        }
        const pid = stdout.trim();
        exec(`readlink -f /proc/${pid}/exe`, (error, stdout) => {
          if (error) {
            return callback(error, null);
          }
          const exePath = stdout.trim();
          callback(null, exePath);
        });
      });
    }
  } catch (error) {
    throw error;
  }
}

function terminateProgram(exeName, callback) {
  if (os.platform() === 'win32') {
      exec(`taskkill /F /IM ${exeName}`, (error, stdout) => {
          if (error) {
              return callback(error, null);
          }
          callback(null, stdout);
      });
  } else { //<- Linux
      exec(`pkill -f ${exeName}`, (error, stdout) => {
          if (error) {
              return callback(error, null);
          }
          callback(null, stdout);
      });
  }
}

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

// #endregion

/** Null-Empty-Uninstanced verification * 
 * @param {*} value Object, String or Array
 * @returns True or False
 */
function isNotNullOrEmpty(value) {
    if (value === null || value === undefined) {
        return false;
    }

    if (typeof value === 'string' || Array.isArray(value)) {
        return value.length > 0;
    }

    if (typeof value === 'object') {
        return Object.keys(value).length > 0;
    }

    return false;
}


function createWindowsShortcut() {
  try {
    const isDev = !app.isPackaged;
    const shortcutPath = path.join(os.homedir(), 'Desktop', 'EDHM-UI-V3.lnk');
    const targetPath = path.join(process.env.LOCALAPPDATA, 'EDHM-UI-V3', 'EDHM-UI-V3.exe'); // For production environment 
    const iconPath = getAssetPath('images/ED_TripleElite.ico');
    const comment = "Mod for Elite Dangerous to customize the HUD of any ship.";

    if (!fs.existsSync(shortcutPath)) {
      const cmd = `powershell $s=(New-Object -COM WScript.Shell).CreateShortcut('${shortcutPath}');$s.TargetPath='${targetPath}';$s.IconLocation='${iconPath}';$s.Description='${comment}';$s.Save()`;

      exec(cmd, (err) => {
        if (err) {
          console.error('Failed to create shortcut:', err);
        } else {
          console.log('Shortcut created successfully');
        }
      });
    } else {
      console.log('Shortcut already exists.');
    }
  } catch (error) {
    console.log(error);
  }
}

function createLinuxShortcut() {
  const desktopFilePath = path.join(os.homedir(), 'Desktop', 'EDHM-UI-V3.desktop');
  const execPath = resolveEnvVariables('%LOCALAPPDATA%\\EDHM-UI-V3\\EDHM-UI-V3.exe'); // path.join(__dirname, 'EDHM-UI-V3');
  const iconPath = getAssetPath('images/icon.png');
  const comment = "Mod for Elite Dangerous to customize the HUD of any ship.";

  if (!fs.existsSync(desktopFilePath)) {
    const desktopFileContent = `
      [Desktop Entry]
      Name=Your App
      Exec=${execPath}
      Icon=${iconPath}
      Terminal=false
      Type=Application
      Comment=${comment}
      Categories=Utility;
    `;

    fs.writeFileSync(desktopFilePath, desktopFileContent);
    fs.chmodSync(desktopFilePath, '755'); // Make the .desktop file executable

    console.log('Shortcut created successfully');
  }
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
        { name: 'Images', extensions: ['jpg', 'jpeg', 'png', 'gif'] }
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
/*  EXAMPLE:
    window.api.ShowOpenDialog(options).then(filePath => {
        if (filePath) {
            const FolderPath = window.api.getParentFolder(filePath[0]);
            this.config.GameInstances[instanceIndex].games[gameIndex].path = FolderPath;
            InstallGameInstance(FolderPath);
        }
    }); 
*/
});
ipcMain.handle('ShowSaveDialog', async (event, options) => {
  /*  
  const options = {
      title: '',  //The dialog title. Cannot be displayed on some Linux desktop
      defaultPath: app.getPath('desktop'),
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


ipcMain.handle('detect-program', async (event, exeName) => {
  try {
    return new Promise((resolve, reject) => {
      detectProgram(exeName, (error, exePath) => {
        if (error) {
          reject(error);
        } else {
          resolve(exePath);
        }
      });
    });
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('start-monitoring', (event, exeName) => {
  const interval = setInterval(() => {
      detectProgram(exeName, (error, exePath) => {
          if (exePath) {
              event.sender.send('program-detected', exePath);
              clearInterval(interval); // Stop monitoring once the program is detected
          }
      });
  }, 3000); // Check every 3 seconds

  return { intervalId: interval[Symbol.toStringTag] };
});
ipcMain.handle('terminate-program', async (event, exeName) => {
  return new Promise((resolve, reject) => {
      terminateProgram(exeName, (error, result) => {
          if (error) {
              reject(error);
          } else {
              resolve(result);
          }
      });
  });
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

ipcMain.handle('openUrlInBrowser', async (event, url) => {
  try {
    const result = openUrlInBrowser(url);
    return { success: true, file: result };
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('convertImageToJpg', async (event, base64Image) => {
  try {
    return convertImageToJpg(base64Image);
  } catch (error) {
    throw error;
  }
}); 
ipcMain.handle('GetImageB64', async (event, filePath) => {
  try {
    return loadImageAsBase64(filePath);
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('createWindowsShortcut', async (event) => {
  try {
    return createWindowsShortcut();
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('createLinuxShortcut', async (event) => {
  try {
    return createLinuxShortcut();
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
  getParentFolder,
  ShowOpenDialog,
  ShowSaveDialog,

  compressFiles,
  compressFolder,
  decompressFile,

  SaveImageAsJpeg,
  base64ToJpg,
  loadImageAsBase64,

  createWindowsShortcut,
  createLinuxShortcut
};