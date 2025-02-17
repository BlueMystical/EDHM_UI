import { app, BrowserWindow, ipcMain, Menu } from 'electron';
import path from 'node:path';

import Log from './Helpers/LoggingHelper.js';
import fileHelper from './Helpers/FileHelper.js'; 
import settingsHelper from './Helpers/SettingsHelper.js'; 
import iniHelper from './Helpers/IniHelper.js'; 
import themeHelper from './Helpers/ThemeHelper.js'; 

function createWindow() {  
  const mainWindow = new BrowserWindow({
    width: 1600, minWidth: 1160,
    height: 800, minHeight: 553,        
    
    icon: path.join(__dirname, 'images/ED_TripleElite.ico'), 
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      enableRemoteModule: false,
      nodeIntegration: true,
      contextIsolation: true,
      webSecurity: false
    },
  });
  //log.info('Loading main window content...');
  mainWindow.loadURL(
    MAIN_WINDOW_VITE_DEV_SERVER_URL || path.join(__dirname, `../renderer/${MAIN_WINDOW_VITE_NAME}/index.html`)
  ).catch(err => {
    Log.Error('Failed to load main window content:', err);
  });

  mainWindow.webContents.on('did-finish-load', () => {

  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription) => {
   // log.error('Failed to load:', errorCode, errorDescription);
   Log.Error(errorCode, errorDescription);
  });

  mainWindow.webContents.on('crashed', () => {
    //log.error('Window crashed');
    Log.Error('Window crashed');
  });

  try {
    if (require('electron-squirrel-startup')) app.quit(); 
  } catch (error) {
    console.log(error);
  }  
}

app.on('ready', () => { 
  console.log('App is ready');
  createWindow(); 

      // Open the DevTools.
      mainWindow.webContents.openDevTools(); 
      // Disable the menu bar
      //Menu.setApplicationMenu(null);

  if (process.platform === 'win32') { 
    fileHelper.createWindowsShortcut.call(this); 
  } else if (process.platform === 'linux') { 
    fileHelper.createLinuxShortcut.call(this);
  } 
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

