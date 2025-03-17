import { app, ipcMain, dialog, shell, clipboard, net,  } from 'electron';
import { exec, spawn } from 'child_process';
import path from 'node:path';
import fs from 'node:fs';
import https from 'https';
import http from 'http';
import url from 'url';
import zl from 'zip-lib';
import os from 'os';
import Util from './Utils.js';


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
 *  @param {*} givenPath 
 * @returns 
 */
function getParentFolder(givenPath) {
    return path.dirname(givenPath);
}

function createWindowsShortcut() {
  try {
    const isDev = !app.isPackaged;
    const shortcutPath = path.join(os.homedir(), 'Desktop', 'EDHM-UI-V3.lnk');
    const targetPath = resolveEnvVariables('%LOCALAPPDATA%\\EDHM-UI-V3\\EDHM-UI-V3.exe'); //path.join(process.env.LOCALAPPDATA, 'EDHM-UI-V3', 'EDHM-UI-V3.exe'); // For production environment 
    const iconPath = getAssetPath('images/ED_TripleElite.ico');
    const comment = "Mod for Elite Dangerous to customize the HUD of any ship.";

    //if (!fs.existsSync(shortcutPath)) {
      const cmd = `powershell $s=(New-Object -COM WScript.Shell).CreateShortcut('${shortcutPath}');$s.TargetPath='${targetPath}';$s.IconLocation='${iconPath}';$s.Description='${comment}';$s.Save()`;

      exec(cmd, (err) => {
        if (err) {
          console.error('Failed to create shortcut:', err);
        } else {
          console.log('Shortcut created successfully');
        }
      });
    //} else {
    //  console.log('Shortcut already exists.');
    //}
  } catch (error) {
    console.log(error);
  }
}

function createLinuxShortcut() {
  try {
    const homeDir = os.homedir();
    const desktopFilePath = path.join(homeDir, 'Desktop', 'edhm-ui-v3.desktop');
    const appsFilePath = path.join(homeDir, '.local', 'share', 'applications', 'edhm-ui-v3.desktop');
    const execPath = app.getPath('exe');
    const iconPath = getAssetPath('images/icon.png');

    console.log('Attempting to create a Shortcut at ' + desktopFilePath);

    const desktopFileContent = `
        [Desktop Entry]
        Encoding=UTF-8
        Name=edhm-ui-v3
        Exec=${execPath}
        Icon=${iconPath}
        Terminal=false
        Type=Application
        Comment=Mod for Elite Dangerous to customize the HUD of any ship.
        StartupNotify=true
        Categories=Utility;`;

    fs.writeFileSync(desktopFilePath, desktopFileContent);
    fs.writeFileSync(appsFilePath, desktopFileContent);
    fs.chmodSync(desktopFilePath, '755');
    fs.chmodSync(appsFilePath, '755');

    console.log('Shortcut created successfully');

  } catch (error) {
    throw new Error(error.message + error.stack);
  }
}

/** Copies the given text to the clipboard.
 * @param {string} text - The text to copy. */
function copyToClipboard(text) {
  if (process.platform === 'linux') {
    // Linux requires using selection clipboard for some applications
    clipboard.writeText(text, 'selection');
  } else {
    // Windows and macOS use the default clipboard
    clipboard.writeText(text);
  }
}


// #endregion

// #region Assets Handling

/** Gets the absolute path to an asset, handling differences between development and production environments.
 * In development, it resolves the path relative to the project's root directory.
 * In production (when the application is packaged), it resolves the path relative to the 'resources' directory. *
 * @param {string} assetPath The relative path to the asset (e.g., 'data/Settings.json', 'images/icon.png', 'public/windows_installer.bat').
 * @returns {string} The absolute path to the asset. */
function getAssetPath(assetPath) {
  try {
    if (process.env.NODE_ENV === 'development') {
      if (assetPath.startsWith('public')) {
        return path.join(__dirname, '../../', assetPath); // Dev path
      } else {
        return path.join(__dirname, '../../src', assetPath); // Dev path
      }      
    } else {
      return path.join(process.resourcesPath, assetPath); // Correct Prod path
    }
  } catch (error) {
    throw new Error(error.message + error.stack);
  }  
}
function getAssetUrl(assetPath) {
  try {
    const resolvedPath = getAssetPath(assetPath);
    const fileUrl = url.pathToFileURL(resolvedPath).toString();
    return fileUrl;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }  
}

// #endregion

// #region Files & Directories

/** If the Directory doesn't exist, it is created.
 * @param {*} DirectoryPath Path to the Directory. */
function ensureDirectoryExists (DirectoryPath) {
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

/** Verifies is a File Exists
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

/** Copies files with specific extensions from one directory to another. 
 * @param {string} sourceDir - The path to the source directory.
 * @param {string} destDir - The path to the destination directory.
 * @param {string[]} extensions - An array of file extensions to copy (e.g., ['.jpg', '.json']). */
async function copyFiles(sourceDir, destDir, extensions) {
  let filesCopied = 0;

  if (!fs.existsSync(destDir)) {
    fs.mkdirSync(destDir, { recursive: true });
  }

  fs.readdirSync(sourceDir).forEach(file => {
    const currentPath = path.join(sourceDir, file);
    if (fs.lstatSync(currentPath).isFile()) {
      const ext = path.extname(file).toLowerCase();
      if (extensions.includes(ext)) {
        const destPath = path.join(destDir, file);
        fs.copyFileSync(currentPath, destPath);
        filesCopied++;
      }
    }
  });

  return filesCopied;
}
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

/** Moves a directory and its contents to a destination.
 * @param {string} sourceDir The source directory path.
 * @param {string} destinationDir The destination directory path. */
async function moveDirectory(sourceDir, destinationDir) {
  if (!fs.existsSync(sourceDir)) {
    console.error(`Source directory does not exist: ${sourceDir}`);
    return false;
  }

  try {
    fs.renameSync(sourceDir, destinationDir);
    console.log(`Moved directory from ${sourceDir} to ${destinationDir}`);
    return true; // Indicate success
  } catch (err) {
    // Handle cross-device linking issues.
    if (err.code === 'EXDEV') {
      console.log('Cross-device linking not supported. Copying and deleting instead.');

      try {
        copyDirectoryRecursive(sourceDir, destinationDir);
        fs.rmSync(sourceDir, { recursive: true, force: true });
        console.log(`Copied and deleted: ${sourceDir} -> ${destinationDir}`);
        return true;
      } catch (copyDeleteErr) {
        console.error('Error copying/deleting directory:', copyDeleteErr);
        return false;
      }
    } else {
      console.error('Error moving directory:', err);
      return false; // Indicate failure
    }
  }
}

/** Recursively copies a directory and its contents.
 * @param {string} source The source directory path.
 * @param {string} destination The destination directory path. */
async function copyDirectoryRecursive(source, destination) {
  fs.mkdirSync(destination, { recursive: true });
  fs.readdirSync(source, { withFileTypes: true }).forEach((entry) => {
    const sourcePath = path.join(source, entry.name);
    const destinationPath = path.join(destination, entry.name);

    if (entry.isDirectory()) {
      copyDirectoryRecursive(sourcePath, destinationPath);
    } else {
      fs.copyFileSync(sourcePath, destinationPath);
    }
  });
}

async function deleteFolderRecursive(folderPath) {
  if (fs.existsSync(folderPath)) {
    fs.readdirSync(folderPath).forEach((file) => {
      const currentPath = path.join(folderPath, file);

      if (fs.lstatSync(currentPath).isDirectory()) {
        deleteFolderRecursive(currentPath); // Recursively delete folder contents
      } else {
        fs.unlinkSync(currentPath); // Delete file
      }
    });
    //fs.rmdirSync(folderPath); // Remove the now-empty folder //<- DEPRECATED
    fs.rmdirSync(folderPath, { recursive: true, force: true });
    console.log('Folder Deleted.');
    return true;
  }
  return false; //<- 404 - The Folder doesnt exists
}
/** Deletes directories within a given directory, excluding specified exceptions.
 * @param {string} directoryPath The root directory path.
 * @param {string} exceptions A comma-separated string of directory names to exclude. */
async function deleteDirectoriesExcept(directoryPath, exceptions) {
  if (fs.existsSync(directoryPath)) {
    const exceptionList = exceptions.split(',').map(item => item.trim()); // Trim whitespace
    const directories = fs.readdirSync(directoryPath, { withFileTypes: true })
      .filter(dirent => dirent.isDirectory())
      .map(dirent => dirent.name);

    if (directories) {
      directories.forEach((dirName) => {
        if (!exceptionList.includes(dirName)) {
          const dirToDelete = path.join(directoryPath, dirName);
          try {
            fs.rmSync(dirToDelete, { recursive: true, force: true });
            console.log(`Deleted directory: ${dirToDelete}`);
          } catch (err) {
            console.error(`Error deleting directory: ${dirToDelete}`, err);
          }
        }
      });
    }
  } else {
      console.log(`directory ${directoryPath} does not exist`);
  }
}

/** Deletes the given File 
 * @param {*} filePath Absolute path to the File */
function deleteFileByAbsolutePath(filePath) {
  let _ret = false;
  try {
    fs.unlinkSync(filePath);
    _ret = true;     console.log('File deleted.');
  } catch (err) {
    throw err;
  }
  return _ret;
}

/** Deletes all Files of a certain Type  
 * @param {*} directoryPath Path to the directory containing the files
 * @param {*} extension File extension, example: '.txt' */
async function deleteFilesByType(directoryPath, extension) {
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

/** Deletes files matching a simplified wildcard pattern.
 * Supports '*' for any characters and '?' for a single character.
 * @param {string} wildcardPath Path with wildcard pattern, e.g., '/path/to/files/*.txt' */
async function deleteFilesByWildcard(wildcardPath) {
  const { dir, base } = path.parse(wildcardPath);
  const regexPattern = base.replace(/\./g, '\\.').replace(/\*/g, '.*').replace(/\?/g, '.');
  const regex = new RegExp(`^${regexPattern}$`);

  fs.readdir(dir, (err, files) => {
    if (err) {
      return console.error('Unable to scan directory:', err);
    }

    files.forEach((file) => {
      if (regex.test(file)) {
        const filePath = path.join(dir, file);
        try {
          fs.unlinkSync(filePath);
          console.log(`Deleted: ${filePath}`);
        } catch (deleteErr) {
          console.error('Error deleting file:', filePath, deleteErr);
        }
      }
    });
  });
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
    //fs.rmdirSync(symlinkPath, { recursive: true }); //<- DEPRECATED
    fs.rmSync(symlinkPath, { recursive: true, force: true });

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
    /*   USAGE: 
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
     const filePath = await window.api.ShowOpenDialog(options);
     if (filePath) {
       this.config.PlayerJournal = filePath[0];
     }
   */
    try {
      const result = dialog.showOpenDialogSync(options);
      return result;
    } catch (error) {
      throw new Error(error.message + error.stack);
    }
}

/** * Shows a save dialog and returns the file path if the user did not hit the Cancel button. 
 * @param {object} options - The options for the save dialog.
 * @returns {string|null} - The selected file path, or null if the user canceled the dialog. */
async function ShowSaveDialog(options) {
  /* USAGE:  
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
        const filePath = await ShowSaveDialog(options);
        if (filePath) {  console.log('Destination:', filePath); }

Another way:
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
    return result ? result : null;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
}

// #endregion

// #region JSON Files

/** Loads a JSON file from the specified path. *
 * @param {string} filePath The path to the JSON file.
 * @returns {Object|null} The parsed JSON object, or null if the file does not exist or cannot be parsed. */
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
    throw new Error(error.message + error.stack);
  }  
};

/** Takes a base64 image and writes it as a JPG image file
 * @param {*} base64Image image's data
 * @param {*} outputPath full path to save the image
 */
async function base64ToJpg(base64Image, outputPath) {
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
    throw new Error(error.message + error.stack);
  }
};

/** Loads an Image from disk * 
 * @param {*} imagePath Absolute path to the image
 * @returns Image data in Base64 format
 */
async function loadImageAsBase64(imagePath) {
  try {
    //console.log('Loading Image: ' + imagePath);
    if (fs.existsSync(imagePath)) {
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
    } else {
      console.log('404 - Not Found: ' + imagePath);
    }
  } catch (error) {
    return null;
  }
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
 * @param {*} callback  */
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
    throw new Error(error.message + error.stack);
  }
}

function terminateProgram(exeName, callback) {
  try {
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
  } catch (error) {
    // Program not running
    return callback(null, true);
    //console.log(error.message + error.stack);
  }
}

/** Open the File Explorer showing a selected Folder
 * @param {*} filePath Folder to select */
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

function runInstaller(installerPath) {
  let command;

  if (os.platform() === 'win32') {
    command = `start ${installerPath}`;
  } else if (os.platform() === 'linux') {
    if (installerPath.endsWith('.deb')) {
      command = `sudo dpkg -i ${installerPath}`;
    } else {
      // Handle other installer types as needed
      command = `chmod +x ${installerPath} && sudo ${installerPath}`;
    }
  } else {
    console.error('Unsupported OS');
    return;
  }

  exec(command, (error, stdout, stderr) => {
    if (error) {
      console.error(`Error running installer: ${error.message}`);
      return;
    }

    console.log(`Installer stdout: ${stdout}`);
    console.log(`Installer stderr: ${stderr}`);

    // Close the application
    app.quit();
  });
}


// #endregion

// #region Updates

/** [For Beta Testings] Check for the Latest Pre-Release published on Github
 * @param {string} owner 'BlueMystical'
 * @param {string} repo 'EDHM-UI'*/
async function getLatestPreReleaseVersion(owner, repo) {
  const options = {
    hostname: 'api.github.com',
    path: `/repos/${owner}/${repo}/releases`,
    method: 'GET',
    headers: { 'User-Agent': 'Node.js' }
  };
  //https://api.github.com/repos/BlueMystical/EDHM_UI/releases

  return new Promise((resolve, reject) => {
    const req = https.request(options, (res) => {
      let data = '';

      res.on('data', (chunk) => {
        data += chunk;
      });

      res.on('end', () => {
        const releases = JSON.parse(data);
        const preReleases = releases.filter(release => release.prerelease);
        const latestPreRelease = preReleases.sort((a, b) => new Date(b.created_at) - new Date(a.created_at))[0];

        if (latestPreRelease) {
          //console.log(latestPreRelease);
          const result = {
            release_id: latestPreRelease.id,
            version: latestPreRelease.tag_name,
            notes: latestPreRelease.body,
            zipball_url: latestPreRelease.zipball_url,
            html_url: latestPreRelease.html_url,
            assets: latestPreRelease.assets.map(asset => ({              
              content_type: asset.content_type,
              url: asset.browser_download_url,
              name: asset.name,
              size: asset.size,     
            }))
          };
          resolve(result);
        } else {
          resolve(null);
        }
      });
    });

    req.on('error', (error) => {
      reject(error);
    });

    req.end();
  });
}

/** Check for the Latest Release published on Github
 * @param {*} owner 'BlueMystical'
 * @param {*} repo 'EDHM-UI'
 * @returns  */
async function getLatestReleaseVersion(owner, repo) {
  const options = {
    hostname: 'api.github.com',
    path: `/repos/${owner}/${repo}/releases`,
    method: 'GET',
    headers: { 'User-Agent': 'Node.js' }
  };

  return new Promise((resolve, reject) => {
    const req = https.request(options, (res) => {
      let data = '';

      res.on('data', (chunk) => {
        data += chunk;
      });

      res.on('end', () => {
        const releases = JSON.parse(data);
        const latestRelease = releases.find(release => !release.prerelease && !release.draft);

        if (latestRelease) {
          const result = {
            release_id: latestRelease.id,
            version: latestRelease.tag_name,
            notes: latestRelease.body,
            html_url: latestRelease.html_url,
            zipball_url: latestRelease.zipball_url,            
            assets: latestRelease.assets.map(asset => ({              
              content_type: asset.content_type,
              url: asset.browser_download_url,
              name: asset.name,
              size: asset.size,     
            }))
          };
          resolve(result);
        } else {
          resolve(null);
        }
      });
    });

    req.on('error', (error) => {
      reject(error);
    });

    req.end();
  });
}



// #endregion

/*----------------------------------------------------------------------------------------------------------------------------*/
// #region ipc Handlers (Inter-Process Communication)

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
    throw new Error(error.message + error.stack);
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
    throw new Error(error.message + error.stack);
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

/** Launches a program or script file in a "fire and forget" manner.
 * Returns "Program started" immediately if the program is successfully launched.
 * Does not wait for the program to finish or capture its output.
 *
 * @param {Electron.IpcMainInvokeEvent} event - The IPC event.
 * @param {string} filePath - The absolute path to the program or script file.
 * @param {string[]} [args] - An optional array of arguments to pass to the program.
 * @returns {string} - "Program started" if launched, "Program could not start" on error. */
ipcMain.handle('run-program', async (event, filePath, args = []) => {
  try {
    console.log('Launching program:', filePath, args);

    if (process.platform === 'linux') {
      console.log('Linux platform detected');

      // Ensure the script has execute permissions
      fs.chmod(filePath, 0o755, (chmodError) => {
        if (chmodError) {
          console.warn(`Warning: Could not change file permissions for ${filePath}.`, chmodError);
        }
      });

      // Use spawn to run the shell script as a detached process
      const options = {
        detached: true, // Detach the process from the parent
        stdio: 'ignore', // Ignore input/output streams to let it run independently
        cwd: path.dirname(filePath) // Set the working directory of the script
      };

      const args = []; // Pass additional arguments to the shell script if needed
      const process = spawn('bash', [filePath, ...args], options);

      process.unref(); // Allow the parent app to exit without affecting the script
      console.log('Shell script launched in detached mode.');

    } else if (process.platform === 'win32') {
      const batPath = path.resolve(filePath); // Resolve absolute path of the .bat file
      const options = {
        detached: true, // Detach the process so it can run independently
        stdio: 'ignore', // Ignore standard input/output streams
        cwd: path.dirname(batPath) // Set the working directory to the .bat file's directory
      };

      // Use spawn to execute the batch file
      const batProcess = spawn('cmd.exe', ['/c', 'start', filePath], options);

      batProcess.unref(); // Allow the parent app to terminate without affecting the batch file
      console.log('Batch file launched in detached mode.');

      return 'Batch file started successfully';
    }
    else {
      console.error(`Unsupported platform: ${process.platform}`);
    }

    return "Program started";
  } catch (error) {
    console.error(`Error starting program: ${error}`);
    return "Program could not start";
  }
});

ipcMain.handle('openPathInExplorer', async (event, filePath) => {
  try {
    const result = openPathInExplorer(filePath);
    return result;
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

// Returns the path of an Asset (a file included with the program) ex: 'images\PREVIEW.jpg', 'data\Settings.json'
ipcMain.handle('get-asset-path', async (event, assetPath) => {
  try {
    const resolvedPath = getAssetPath(assetPath);
    return resolvedPath;
  } catch (error) {
    console.error('Failed to resolve asset path:', error);
    //logEvent(error.message, error.stack);
    throw new Error(error.message + error.stack);
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
    throw new Error(error.message + error.stack);
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
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('ensureDirectoryExists', async (event, fullPath) => {
  try {
    return ensureDirectoryExists(fullPath);
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

ipcMain.handle('is-not-null-obj', async (event, obj) => {
  return Util.isNotNullOrEmpty(obj);
});

ipcMain.handle('deleteFileByAbsolutePath', async (event, filePath) => {
  return deleteFileByAbsolutePath(filePath);
});
ipcMain.handle('deleteFilesByType', async (event, directoryPath, extension) => {
  return deleteFilesByType(directoryPath, extension);
});

ipcMain.handle('compress-files', async (event, files, outputPath) => {
  try {
    const result = await compressFiles(files, outputPath);
    return { success: true, message: result };
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('compress-folder', async (event, folderPath, outputPath) => {
  try {
    const result = await compressFolder(folderPath, outputPath);
    return { success: true, message: result };
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('decompress-file', async (event, zipPath, outputDir) => {
  try {
    const result = await decompressFile(zipPath, outputDir);
    return { success: true, message: result };
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('find-latest-file', async (event, folderPath, fileType) => {
  try {
    const result = await findLatestFile(folderPath, fileType);
    return { success: true, file: result };
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
}); 
ipcMain.handle('findFileWithPattern', async (event, folderPath, pattern) => {
  try {
    const result = await findFileWithPattern(folderPath, pattern);
    return { success: true, file: result };
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
}); 

ipcMain.handle('openUrlInBrowser', async (event, url) => {
  try {
    const result = openUrlInBrowser(url);
    return { success: true, file: result };
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('convertImageToJpg', async (event, base64Image) => {
  try {
    return convertImageToJpg(base64Image);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
}); 
ipcMain.handle('GetImageB64', async (event, filePath) => {
  try {
    return loadImageAsBase64(filePath);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('createWindowsShortcut', async (event) => {
  try {
    return createWindowsShortcut();
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('createLinuxShortcut', async (event) => {
  try {
    return createLinuxShortcut();
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('getLatestPreReleaseVersion', async (event, owner, repo) => {
  try {
    const latestPreRelease = await getLatestPreReleaseVersion(owner, repo);
    return latestPreRelease;
  } catch (error) {
    console.error('Error fetching pre-release version:', error);
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('getLatestReleaseVersion', async (event, owner, repo) => {
  try {
    const latestRelease = await getLatestReleaseVersion(owner, repo);
    return latestRelease;
  } catch (error) {
    console.error('Error fetching latest release version:', error);
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('download-asset', async (event, url, dest) => {
  return new Promise((resolve, reject) => {
    const file = fs.createWriteStream(dest);
    let receivedBytes = 0;

    https.get(url, response => {
      const totalBytes = parseInt(response.headers['content-length'], 10);

      response.pipe(file);
      response.on('data', chunk => {
        receivedBytes += chunk.length;
        event.sender.send('download-progress', receivedBytes, totalBytes);
      });

      file.on('finish', () => {
        file.close(resolve);
      });
    }).on('error', err => {
      fs.unlink(dest, () => reject(err));
    });
  });
});

ipcMain.handle('download-file', async (event, url, filePath) => {
  try {
    const request = net.request(url);

    return new Promise((resolve, reject) => { // Return the promise here.
      request.on('response', (response) => {
        if (response.statusCode !== 200) {
          reject(new Error(`HTTP error! status: ${response.statusCode}`));
          return;
        }

        const totalLength = parseInt(response.headers['content-length'], 10);
        let downloadedLength = 0;
        let lastUpdateTime = Date.now();
        let lastDownloadedLength = 0;

        const writer = fs.createWriteStream(filePath);

        response.on('data', (chunk) => {
          writer.write(chunk);
          downloadedLength += chunk.length;

          const now = Date.now();
          if (now - lastUpdateTime > 100) {
            const progress = (downloadedLength / totalLength) * 100;
            const timeDiff = now - lastUpdateTime;
            const bytesDiff = downloadedLength - lastDownloadedLength;
            const speed = bytesDiff / timeDiff;

            event.sender.send('download-progress', { progress, speed });
            lastUpdateTime = now;
            lastDownloadedLength = downloadedLength;
          }
        });

        response.on('end', () => {
          writer.end();
          event.sender.send('download-progress', { progress: 100, speed: 0 });
        });

        writer.on('finish', () => {
          console.log('Writer finished');
          resolve(); // Resolve the promise on finish
        });

        response.on('error', (err) => {
          console.error('Download response error:', err);
          writer.end();
          reject(err); // Reject the promise on error
        });

        writer.on('error', (err) => {
          console.error('Download writer error:', err);
          reject(err); // Reject the promise on error
        });
      });

      request.on('error', (err) => {
        console.error('Download request error:', err);
        reject(err); // Reject the promise on error
      });

      request.end();
    });
  } catch (error) {
    console.error('Download error:', error);
    throw error;
  }
});

ipcMain.handle('runInstaller', async (event, installerPath) => {
  try {
    const latestRelease = runInstaller(installerPath);
    return latestRelease;
  } catch (error) {
    console.error('Error fetching latest release version:', error);
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('copyToClipboard', (event, text) => {
  try {
    return copyToClipboard(text);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('getPublicFilePath', (event, relFilePath) => {
  try {
    return getPublicFilePath(relFilePath);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});



// #endregion

export default { 
  getAssetPath, getAssetUrl,
  resolveEnvVariables,  

  loadJsonFile, 
  writeJsonFile, 

  copyFiles, copyFile,
  checkFileExists, 
  openPathInExplorer,
  moveDirectory, copyDirectoryRecursive,
  deleteFilesByType,
  deleteFolderRecursive,
  deleteFilesByWildcard,
  deleteDirectoriesExcept,
  deleteFileByAbsolutePath,

  findLatestFile,
  findFileWithPattern,
  ensureDirectoryExists, 
  ensureSymlink,

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
  createLinuxShortcut,
  terminateProgram,
  runInstaller,
  
  copyToClipboard,

};