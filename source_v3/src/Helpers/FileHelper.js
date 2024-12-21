const { app, ipcMain } = require('electron');
const path = require('path');
const fs = require('fs');
const os = require('os');
const ini = require('ini');

/**
 * Resolves environment variables in a given path string.
 *
 * @param {string} inputPath The path string containing environment variables.
 * @returns {string} The resolved path, or the original path if no replacements were made.
 */
const resolveEnvVariables = (inputPath) => {
    if (typeof inputPath !== 'string') {
        console.warn("Input path must be a string. Returning original input:", inputPath);
        return inputPath;
    }

    if (!inputPath.includes('%')) {
        // Optimization: if no % characters, no need to do any replacements
        return inputPath;
    }

    const envVars = {
        '%USERPROFILE%': os.homedir(),
        '%APPDATA%': process.env.APPDATA,
        '%LOCALAPPDATA%': process.env.LOCALAPPDATA,
        '%PROGRAMFILES%': process.env.PROGRAMFILES,
        '%PROGRAMFILES(X86)%': process.env['PROGRAMFILES(X86)'],
        '%PROGRAMDATA%': process.env.PROGRAMDATA, // Added %PROGRAMDATA%
        '%APPDIR%': app.getAppPath(),
    };

    let resolvedPath = inputPath;

    for (const [key, value] of Object.entries(envVars)) {
        if (value) {
            // Use a regular expression for more robust replacement
            const regex = new RegExp(key.replace(/%/g, '\\%'), 'gi'); // Escape % for regex
            resolvedPath = resolvedPath.replace(regex, value);
        }
    }

    console.log('Resolved path:', resolvedPath);
    return resolvedPath;
};

/**
 * Gets the absolute path to an asset, handling differences between development and production environments.
 * In development, it resolves the path relative to the project's root directory.
 * In production (when the application is packaged), it resolves the path relative to the 'resources' directory.
 *
 * @param {string} assetPath The relative path to the asset (e.g., 'data/config.json', 'images/logo.png').
 * @returns {string} The absolute path to the asset.
 */
function getAssetPath(assetPath) {
    if (process.env.NODE_ENV === 'development') {
        return path.join(__dirname, '../../src', assetPath); // Dev path
    } else {
        return path.join(process.resourcesPath, assetPath); // Correct Prod path
    }
}

/**
 * If the Directory doesn't exist, it is created.
 * @param {*} DirectoryPath Path to the Directory.
 */
const ensureDirectoryExists = (DirectoryPath) => {
    try {
      const resolvedPath = resolveEnvVariables(DirectoryPath); 
      if (!fs.existsSync(resolvedPath)) {
        fs.mkdirSync(resolvedPath, { recursive: true });
      }
    } catch (err) {
      console.error('Failed to create log directory:', err);
    }
  };


/**
 * Loads a JSON file from the specified path.
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
      return null;
    }
  };
  
  /**
   * Writes an object to a JSON file.
   *
   * @param {string} filePath The path to the JSON file.
   * @param {Object} data The object to be written to the file.
   * @param {boolean} [prettyPrint=true] Whether to pretty-print the JSON output.
   */
  const writeJsonFile = (filePath, data, prettyPrint = true) => {
    const resolvedPath = resolveEnvVariables(filePath);
    try {
      ensureDirectoryExists(path.dirname(resolvedPath)); // Ensure parent directory exists
  
      const options = prettyPrint ? { spaces: 2 } : null; 
      fs.writeFileSync(resolvedPath, JSON.stringify(data, null, options));
    } catch (err) {
      console.error(`Error writing JSON file: ${resolvedPath}`, err);
    }
  };

  function isNotNullOrEmpty(obj) {
    return obj && Object.keys(obj).length > 0; 
  }

/*----------------------------------------------------------------------------------------------------------------------------*/
ipcMain.handle('get-app-path', () => {
    return app.getAppPath();
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
        logEvent(error.message, error.stack);
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
        logEvent(error.message, error.stack);
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
        logEvent(error.message, error.stack);
        throw error;
    }
});



ipcMain.handle('get-json-file', async (event, jsonPath) => {
    return loadJsonFile(jsonPath);
  });
  
ipcMain.handle('is-not-null-obj', async (event, obj) => {
    return isNotNullOrEmpty(obj);
});

export default { getAssetPath, resolveEnvVariables, ensureDirectoryExists, loadJsonFile, writeJsonFile };