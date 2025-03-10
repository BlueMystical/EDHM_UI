import { app, BrowserWindow, Menu, ipcMain, process } from 'electron';
import path from 'node:path';
import started from 'electron-squirrel-startup';
import fileHelper from './Helpers/FileHelper.js';
import themeHelper from './Helpers/ThemeHelper.js';
import settingsHelper from './Helpers/SettingsHelper.js';

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (started) {
  app.quit();
}

const createWindow = () => {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 1600, minWidth: 1160,
    height: 800, minHeight: 553,

    icon: path.join(__dirname, 'images/ED_TripleElite.ico'),
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: true,
      webSecurity: false     
    },
  });

  console.log('App is Loading..');  

  // and load the index.html of the app.
  if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
    console.log('Running on Dev mode: ', MAIN_WINDOW_VITE_DEV_SERVER_URL);
    mainWindow.loadURL(MAIN_WINDOW_VITE_DEV_SERVER_URL);
  } else {
    console.log('Production mode: ');
    mainWindow.loadFile(path.join(__dirname, `../renderer/${MAIN_WINDOW_VITE_NAME}/index.html`));  
  }

  //-- Open the DevTools.
  mainWindow.webContents.openDevTools();
};

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.whenReady().then(() => {
  createWindow();

  console.log('App is Ready!');
  
  //-- Disable the menu bar
  Menu.setApplicationMenu(null);

  //-- Create Desktop Shortcut Icons:
  if (process.platform === 'win32') {
    fileHelper.createWindowsShortcut.call(this);
  } else if (process.platform === 'linux') {
    fileHelper.createLinuxShortcut.call(this);
  }

  // On OS X it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});
/*
ipcMain.handle('get-platform', () => {
  console.log(process.platform);
  return process.platform;
});*/

ipcMain.handle('quit-and-install', async (event, filePath) => {
  try {
      if(mainWindow){
          mainWindow.removeAllListeners('close');
          mainWindow.close();
      }
      if (process.platform === 'linux') {
        // Linux logic (as before)
      } else if (process.platform === 'win32') {
        // Windows logic (as before)
      } else {
        console.error(`Unsupported platform: ${process.platform}`);
        throw new Error(`Unsupported platform: ${process.platform}`);
      }
      return "Program started";
  } catch (error) {
      console.error(`Error starting program: ${error}`);
      throw error;
  }
});
