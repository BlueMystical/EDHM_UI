import { app, BrowserWindow, Menu, ipcMain, nativeTheme } from 'electron';
import path from 'node:path';
import started from 'electron-squirrel-startup';
import FileHelper from './Helpers/FileHelper.js';

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (started) {
  app.quit();
}

// Desactiva solo la composición por GPU (no toda la aceleración)
app.commandLine.appendSwitch('disable-gpu-compositing');
//app.commandLine.appendSwitch('disable-gpu');

// Opcional: mantiene el factor de escala fijo
app.commandLine.appendSwitch('force-device-scale-factor', '1');


const createWindow = () => {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 1600, minWidth: 1160,
    height: 860, minHeight: 553,
    icon:  FileHelper.getAssetPath('images/icon_v3_a1.ico'), //  path.join(__dirname, 'images', 'icon_v3_a0.ico'),
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: true,
      webSecurity: false,
    },
  });

  // and load the index.html of the app.
  if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
    console.log('Running on Dev mode: ', MAIN_WINDOW_VITE_DEV_SERVER_URL);
    mainWindow.loadURL(MAIN_WINDOW_VITE_DEV_SERVER_URL);
    // Open the DevTools.
    mainWindow.webContents.openDevTools();
  } else {
    console.log('Production mode: ');
    mainWindow.loadFile(path.join(__dirname, `../renderer/${MAIN_WINDOW_VITE_NAME}/index.html`));
  }

  mainWindow.maximize();
};


// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.whenReady().then(() => {
  createWindow();

  // On OS X it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });

  //-- Disable the menu bar
  Menu.setApplicationMenu(null);
});

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

if (process.platform === 'win32') {
  // Register a user task for Windows 10/11
  // This allows users to create a new window from the taskbar
  // See: https://www.electronjs.org/docs/latest/tutorial/user-tasks
  // and https://learn.microsoft.com/en-us/windows/apps/design/shell/taskbar/user-tasks
  // Note: This feature is only available on Windows 10 and later.
  // Make sure to set the `app.setUserTasks` only once, ideally after the
  // app is ready and the main window is created.
  app.setUserTasks([
    {
      program: process.execPath,
      arguments: '--new-window',
      iconPath: process.execPath,
      iconIndex: 0,
      title: 'New Window',
      description: 'Create a new window'
    }
  ]);
}

ipcMain.handle('dark-mode:toggle', () => {
  if (nativeTheme.shouldUseDarkColors) {
    nativeTheme.themeSource = 'light'
  } else {
    nativeTheme.themeSource = 'dark'
  }
  return nativeTheme.shouldUseDarkColors
})

ipcMain.handle('dark-mode:system', () => {
  nativeTheme.themeSource = 'system'
})
