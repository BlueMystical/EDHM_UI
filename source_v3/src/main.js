import { app, BrowserWindow, Menu, ipcMain, shell, globalShortcut, Tray } from 'electron';
import path from 'node:path';
import started from 'electron-squirrel-startup';
import fileHelper from './Helpers/FileHelper.js';
import themeHelper from './Helpers/ThemeHelper.js';
import settingsHelper from './Helpers/SettingsHelper.js';
import Shipyard from './MainWindow/ShipyardNew.js';


// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (started) { app.quit(); } //<- This is a Squirrel event, so we quit the app

// Declare variables for windows and tray
let mainWindow;
let tray;
let BalloonShown = false;
let HideToTray = false;
let WatchMe = false;
let shipyard;
let CustomIcon;
let programSettings;


//- Check for Single Instance:
const gotTheLock = app.requestSingleInstanceLock();
if (!gotTheLock) {
  app.quit(); // Si otra instancia ya está corriendo, cerramos esta.
} else {
  app.on('second-instance', (event, commandLine, workingDirectory) => {
    if (mainWindow) {
      if (mainWindow.isMinimized()) {
        mainWindow.restore(); // Restaura si estaba minimizada.
      }
      mainWindow.show();  // Muestra la ventana si estaba oculta.
      mainWindow.focus(); // Asegura que la ventana tenga el foco.
    }
  });
  Start();
}

async function Start() {
  try {
    programSettings = await settingsHelper.initializeSettings();

    //- Set the default Icon for the app
    CustomIcon = settingsHelper.readSetting('CustomIcon', fileHelper.getAssetPath('images/Icon_v3_a0.ico'));
    console.log('CustomIcon:', CustomIcon);

    //#region Graphic Options

    //- Rendering Backend: Vulkan / OpenGL / Direct3D:
    const GpuRenderer = settingsHelper.readSetting('GpuRenderer', 'Vulkan');
    switch (GpuRenderer) {
      case 'Vulkan': app.commandLine.appendSwitch('use-vulkan'); break; // Force Vulkan
      case 'OpenGL': app.commandLine.appendSwitch('use-angle', 'gl'); break; // Force ANGLE with OpenGL
      case 'Direct3D': app.commandLine.appendSwitch('use-angle', 'd3d11'); break; // Force ANGLE with Direct3D 11
      default:
        app.commandLine.appendSwitch('use-vulkan'); break;
    }

    // Desactiva solo la composición por GPU (no toda la aceleración)
    const GpuComposite = settingsHelper.readSetting('GpuComposite', true);
    if (!GpuComposite) {
      app.commandLine.appendSwitch('disable-gpu-compositing');
    }
    const GpuAcceleration = settingsHelper.readSetting('GpuAcceleration', true);
    if (!GpuAcceleration) {
      app.commandLine.appendSwitch('disable-gpu');
    }
    //- Hardware-Accelerated Video Decoding
    const GpuVideoDecode = settingsHelper.readSetting('GpuVideoDecode', true);
    if (!GpuVideoDecode) {
      app.commandLine.appendSwitch('disable-accelerated-video-decode');
    }
    //- Enable GPU-Accelerated 2D Canvas
    const GpuCanvas2D = settingsHelper.readSetting('GpuCanvas2D', true);
    if (!GpuCanvas2D) {
      app.commandLine.appendSwitch('disable-accelerated-2d-canvas');
    }
    //- Enable WebGL” / “Enable WebGL2
    const GpuUseWebGL = settingsHelper.readSetting('GpuUseWebGL', true);
    if (!GpuUseWebGL) {
      app.commandLine.appendSwitch('disable-webgl');
      app.commandLine.appendSwitch('disable-webgl2');
    }
    //- Disable Smooth Scrolling / Animations
    const GpuSmoothScrolling = settingsHelper.readSetting('GpuSmoothScrolling', true);
    if (!GpuSmoothScrolling) {
      app.commandLine.appendSwitch('disable-smooth-scrolling');
    }
    // Opcional: mantiene el factor de escala fijo
    app.commandLine.appendSwitch('force-device-scale-factor', '1');
    
    //#endregion

    // This method will be called when Electron has finished
    // initialization and is ready to create browser windows.
    // Some APIs can only be used after this event occurs.
    app.whenReady().then(async () => {
      createWindow();

      //-- Disable the menu bar
      Menu.setApplicationMenu(null);


      //-- Create Desktop Shortcut Icons:
      if (process.platform === 'win32') {
        createTray(); // Create the tray icon
        /* Shortcut creation is no longer needed
        const makeShortcut = await settingsHelper.readSetting('CreateShortcutOnDesktop', true);
        if (makeShortcut) {
          fileHelper.createWindowsShortcut.call(this, CustomIcon);
        }*/
      } else if (process.platform === 'linux') {
        //- Linux users prefer their desktop clean, so no shortcut is created by default
        //- Uncomment the next line to create a shortcut on Linux as well
        //fileHelper.createLinuxShortcut.call(this);
      }

      // Handle command-line arguments
      const args = process.argv.slice(2);
      if (args.length > 0) {
        console.log('Command-line arguments:', args);

        // Handle your arguments here
        if (args.includes('--hide')) {
          console.log('Program started with --hide argument.');
          // Hide the main window immediately
          mainWindow.hide();
        }

        // Send arguments to the renderer process
        mainWindow.webContents.on('did-finish-load', () => {
          mainWindow.webContents.send('app-args', args);
        });
      }

      // Ensure tray works on both Windows and Linux
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
      try {
        if (process.platform !== 'darwin') {
          if (mainWindow) {
            if (tray) tray.destroy(); // Destroy the tray icon
            globalShortcut.unregisterAll(); // Clean up shortcuts on app quit
            mainWindow.removeAllListeners('close');
            app.quit();
          }
        }
      } catch (error) {
        console.error('Error during window-all-closed:', error);
      }
    });
  } catch (error) {
    console.log(error);
  }
}

const createWindow = () => {
  // Create the browser window.
  mainWindow = new BrowserWindow({ // Assign to the outer scope variable
    width: 1600, minWidth: 800,
    height: 800, minHeight: 553,

    icon: CustomIcon, //path.join(__dirname, 'images/ED_TripleElite.ico'),
    backgroundColor: '#1F1F1F',

    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: true,
      webSecurity: false,
      sandbox: false, // Asegura que el contenido no esté limitado dentro de Electron
      allowRunningInsecureContent: false
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

  HideToTray = settingsHelper.readSetting('HideToTray', false); //<- Hide to System Tray on close
  WatchMe = settingsHelper.readSetting('WatchMe', false); //<- Whatch for changes in the Player Journal
  console.log('HideToTray:', HideToTray);

  shipyard = new Shipyard(mainWindow);

  // Register the shortcut to open DevTools
  globalShortcut.register('Control+Shift+I', () => {
    if (mainWindow) {
      mainWindow.webContents.toggleDevTools();
    }
  });
  // Handle external links
  mainWindow.webContents.on('will-navigate', (event, url) => {
    if (url !== mainWindow.webContents.getURL()) {
      event.preventDefault(); // Prevent Electron from navigating
      shell.openExternal(url); // Open the URL in the default browser
    }
  });
  // Handle new window creation (if you have links with target="_blank")
  mainWindow.webContents.setWindowOpenHandler(({ url }) => {
    shell.openExternal(url);
    return { action: 'deny' }; // Prevent Electron from creating a new window
  });
  // Handle window close event
  mainWindow.on('close', (event) => {
    if (HideToTray && process.platform === 'win32') {
      event.preventDefault(); // Prevent the default close behavior
      mainWindow.hide(); // Hide the window instead of closing it

      //- Show a balloon notification informing the user that the app is still running in the background
      //- This is only for Windows, as Linux have different tray behavior
      if (tray && BalloonShown === false) {
        const BallonOptions = {
          title: 'EDHM-UI',
          icon: path.join(__dirname, 'images/ED_TripleElite.ico'),
          content: 'The App is still running in the background.'
        };
        tray.displayBalloon(BallonOptions);
        BalloonShown = true; // Shows the Balloon only once per session
      }
    } else {
      //- Here the Program Terminates Normally
      console.log('Quiting..');
      if (tray) {
        tray.destroy(); // Destroy the tray icon
      }
      app.isQuiting = true; // Signal that the app is quitting
      globalShortcut.unregisterAll(); // Clean up shortcuts on app quit
      if (mainWindow) {
        mainWindow.removeAllListeners('close');
        mainWindow.close();
      }
      app.quit();
    }
  });
};

const createTray = () => {
  //- https://www.electronjs.org/docs/latest/api/tray

  //- Create the Tray Icon:
  //tray = new Tray(path.join(__dirname, 'images/ED_TripleElite.ico')); CustomIcon
  tray = new Tray(CustomIcon);

  //- Create Context Menu for the Tray Icon:
  const contextMenu = Menu.buildFromTemplate([
    {
      label: 'Restore',
      click: () => {
        mainWindow.show(); // Restore the main window
      }
    },
    {
      label: 'Quit',
      click: () => {
        //- Here the Program Terminates Normally
        console.log('Quiting..');
        tray.destroy(); // Destroy the tray icon
        app.isQuiting = true; // Signal that the app is quitting        
        globalShortcut.unregisterAll(); // Clean up shortcuts on app quit
        if (mainWindow) {
          mainWindow.removeAllListeners('close');
          mainWindow.close();
        }
        app.quit();
      }
    }
  ]);

  tray.setContextMenu(contextMenu);
  tray.setToolTip('EDHM-UI');

  // Add the double-click event listener
  tray.on('double-click', () => {
    if (mainWindow) {
      mainWindow.show(); // Show the main window
    }
  });

};


//---------------------------------------------------------------
// #region ipc Handlers (Inter-Process Communication)


ipcMain.handle('get-platform', () => {
  return process.platform;
});
ipcMain.handle('quit-program', async (event) => {
  try {
    if (mainWindow) {
      globalShortcut.unregisterAll(); // Clean up shortcuts on app quit
      mainWindow.removeAllListeners('close');
      app.quit();
    }
    return true;
  } catch (error) {
    console.error(error);
    throw error;
  }
});


// #endregion