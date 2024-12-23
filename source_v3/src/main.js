import { app, BrowserWindow, ipcMain, Menu } from 'electron';
import path from 'node:path';
import fs from 'fs';

import Log from './Helpers/LoggingHelper.js';
import fileHelper from './Helpers/FileHelper'; 
import settingsHelper from './Helpers/SettingsHelper.js'; 
import ini from './Helpers/IniHelper.js'; 
import Tmanager from './Helpers/ThemeHelper.js'; 

function createWindow() {
  const mainWindow = new BrowserWindow({
    width: 1600,
    height: 800,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      enableRemoteModule: false,
      worldSafeExecuteJavaScript: false, 
      nodeIntegration: true,
      webSecurity: false
    },
    icon: path.join(__dirname, 'images/icon.png')
  });
  //log.info('Loading main window content...');
  mainWindow.loadURL(
    MAIN_WINDOW_VITE_DEV_SERVER_URL || path.join(__dirname, `../renderer/${MAIN_WINDOW_VITE_NAME}/index.html`)
  ).catch(err => {
    Log.Error('Failed to load main window content:', err);
  });

  // Disable the menu bar
 // Menu.setApplicationMenu(null);

  mainWindow.webContents.on('did-finish-load', () => {
    // Open the DevTools.
    mainWindow.webContents.openDevTools(); 
  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription) => {
   // log.error('Failed to load:', errorCode, errorDescription);
   Log.Error(errorCode, errorDescription);
  });

  mainWindow.webContents.on('crashed', () => {
    //log.error('Window crashed');
    Log.Error('Window crashed');
  });
}

app.on('ready', () => {
  //log.info('App is ready');
  createWindow();
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

// Handle errors in main process
process.on('uncaughtException', (err) => {
  Log.Error(`Uncaught Exception: ${err.message}`, err.stack);
  // Handle the error gracefully (e.g., display an error message to the user)
});

process.on('unhandledRejection', (reason, promise) => {
  Log.Error(`Uncaught Exception: ${reason}`, '');
});


/* ==================================================================================================*/

/*--------- Expose Methods via IPC Handlers: ---------------------*/
//  they can be accesed like this:   const files = await window.api.getThemes(dirPath);

ipcMain.handle('get-app-version', async () => {
  const appPath = app.getAppPath();
  const packageJsonPath = path.join(appPath, 'package.json');
  const packageJson = JSON.parse(fs.readFileSync(packageJsonPath, 'utf8'));
  return packageJson.version;
});

ipcMain.handle('load-history', async (event, historyFolder, numberOfSavesToRemember) => {
  try {
    historyFolder = fileHelper.resolveEnvVariables(historyFolder);
    // Ensure History folder exists
    if (!fs.existsSync(historyFolder)) {
      fs.mkdirSync(historyFolder, { recursive: true });
    }

    // Read and sort .json files by modification date
    const files = fs.readdirSync(historyFolder)
      .filter(file => file.endsWith('.json'))
      .map(file => ({
        name: file,
        path: path.join(historyFolder, file),
        time: fs.statSync(path.join(historyFolder, file)).mtime.getTime()
      }))
      .sort((a, b) => b.time - a.time)
      .slice(0, numberOfSavesToRemember);

    return files.map(file => ({
      name: file.name,
      path: file.path,
      date: new Date(file.name.substring(0, 4), file.name.substring(4, 6) - 1, file.name.substring(6, 8), file.name.substring(8, 10), file.name.substring(10, 12), file.name.substring(12, 14)).toLocaleString()
    }));
  } catch (error) {
    console.error('Failed to load history elements:', error);
    Log.Error(error.message, error.stack);
    throw error;
  }
});

ipcMain.handle('save-history', async (event, historyFolder, theme) => {
  try {
    historyFolder = fileHelper.resolveEnvVariables(historyFolder);
    // Ensure History folder exists
    if (!fs.existsSync(historyFolder)) {
      fs.mkdirSync(historyFolder, { recursive: true });
    }

    // File with timestamp in the name
    const filePath = path.join(historyFolder, `${new Date().toISOString().replace(/[:.-]/g, '')}.json`);

    // Save the data in JSON format
    fs.writeFileSync(filePath, JSON.stringify(theme, null, 2));
    console.log('Theme added to history:', filePath);

    return true;
  } catch (error) {
    console.error('Failed to add theme to history:', error);
    Log.Error(error.message, error.stack);
    throw error;
  }
});


