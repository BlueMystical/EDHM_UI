import { app, BrowserWindow, Menu, ipcMain, globalShortcut } from 'electron';
import path from 'node:path';
import started from 'electron-squirrel-startup';
import fileHelper from './Helpers/FileHelper.js';
import themeHelper from './Helpers/ThemeHelper.js';
import settingsHelper from './Helpers/SettingsHelper.js';
import { console } from 'node:inspector';

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (started) {
  app.quit();
}

let mainWindow; // Declare mainWindow in the outer scope
let TPModsManagerWindow; // Declare TPModsManagerWindow in the outer scope

const createWindow = () => {
  // Create the browser window.
  mainWindow = new BrowserWindow({ // Assign to the outer scope variable
    width: 1600, minWidth: 1160,
    height: 800, minHeight: 553,

    icon: path.join(__dirname, 'images/ED_TripleElite.ico'),
    backgroundColor: '#1F1F1F',

    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: true,
      webSecurity: false
    }
  });

  console.log('App is Loading..');

  // and load the index.html of the app.
  if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
    console.log('Running on Dev mode: ', MAIN_WINDOW_VITE_DEV_SERVER_URL);
    mainWindow.loadURL(MAIN_WINDOW_VITE_DEV_SERVER_URL);
    //-- Open the DevTools.
    mainWindow.webContents.openDevTools();
  } else {
    console.log('Production mode: ');
    mainWindow.loadFile(path.join(__dirname, `../renderer/${MAIN_WINDOW_VITE_NAME}/index.html`));
  }

  // Register the shortcut to open DevTools
  globalShortcut.register('Control+Shift+I', () => {
    if (mainWindow) {
      mainWindow.webContents.toggleDevTools();
    }
  });

  app.on('will-quit', () => {
    globalShortcut.unregisterAll(); // Clean up shortcuts on app quit
  });
};

const createTPModsManagerWindow = () => {
  TPModsManagerWindow = new BrowserWindow({
      width: 1000,
      height: 900,
      parent: mainWindow,
      modal: false,
      icon: path.join(__dirname, 'images/ED_TripleElite.ico'),
      webPreferences: {
          preload: path.join(__dirname, 'preload.js'),
          contextIsolation: true,
          nodeIntegration: true,
          webSecurity: false,
      },
      backgroundColor: '#1F1F1F'
  });

  if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
      // Load the new entry point in development mode
      console.log('Loading: ', `${MAIN_WINDOW_VITE_DEV_SERVER_URL}/src/TPMods/TPModsManager.html`);
      TPModsManagerWindow.loadURL(`${MAIN_WINDOW_VITE_DEV_SERVER_URL}/src/TPMods/TPModsManager.html`);
      //TPModsManagerWindow.webContents.openDevTools();
  } else {
      // Load the new HTML file in production mode
      TPModsManagerWindow.loadFile(
          path.join(__dirname, `../renderer/${MAIN_WINDOW_VITE_NAME}/TPMods/TPModsManager.html`)
      );
  }

  TPModsManagerWindow.once('ready-to-show', () => {
      TPModsManagerWindow.show();
  });

  TPModsManagerWindow.webContents.on('did-finish-load', () => {
      console.log('TPModsManager window loaded URL:', TPModsManagerWindow.webContents.getURL());
  });

  TPModsManagerWindow.on('closed', () => {
      TPModsManagerWindow = null;
  });
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

  // Handle command-line arguments
  const args = process.argv.slice(2);
  if (args.length > 0) {
    console.log('Command-line arguments:', args);

    // Handle your arguments here
    if (args.includes('--my-flag')) {
      console.log('my flag was passed');
      // Do something.
    }
    if (args[0] === '--file' && args[1]) {
      const filePath = args[1];
      console.log(`Opening file: ${filePath}`);
      // Handle opening the file
    }
    // Send arguments to the renderer process
    mainWindow.webContents.on('did-finish-load', () => {
      mainWindow.webContents.send('app-args', args);
    });

    
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

ipcMain.handle('get-platform', () => {
  console.log(process.platform);
  return process.platform;
});

//---------------------------------------------------------------
ipcMain.handle('quit-program', async (event) => {
  try {
    if (mainWindow) {
      mainWindow.removeAllListeners('close');
      mainWindow.close();
    }
    return true;
  } catch (error) {
    console.error(`Error starting program: ${error}`);
    throw error;
  }
});

ipcMain.handle('open3PModsManager', () => {
  if (!TPModsManagerWindow) {
    createTPModsManagerWindow();
  } else {
    TPModsManagerWindow.focus();
  }
});

