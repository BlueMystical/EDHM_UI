import { app, Menu, BrowserWindow, globalShortcut, ipcMain, shell, Tray, screen  } from 'electron';
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
let StartMinimizedToTray = false;
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
    app.commandLine.appendSwitch('high-dpi-support', '1');
    //app.commandLine.appendSwitch('force-device-scale-factor', '1');
    
    //#endregion

    // This method will be called when Electron has finished
    // initialization and is ready to create browser windows.
    // Some APIs can only be used after this event occurs.
    app.whenReady().then(async () => {
      // Ajustar el Escalado de imagen a la pantalla:
      const { size, scaleFactor: systemScale } = screen.getPrimaryDisplay();
      let userScale = settingsHelper.readSetting('UiScaleFactor', 0); // 0 = automático
      let finalScale;
      if (userScale && userScale > 0) {        
        finalScale = userScale; //<- Usuario forzó un valor
      } else {        
        finalScale = systemScale; //<- Automático: usa el del sistema
        // Ejemplo de regla extra: si es 4K, mínimo 1.5
        if (size.width >= 3840) {
          finalScale = Math.max(finalScale, 1.5);
        }
      }

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
      }
      
      // Font Size Options:
      const FontSize = await settingsHelper.readSetting('FontSize', '14px'); 
      const { scaleFactor } = screen.getPrimaryDisplay();      
      // Send arguments to the renderer process: App.vue
      mainWindow.webContents.on('did-finish-load', () => {
        mainWindow.webContents.send('app-args', args);
        mainWindow.webContents.send('font-size-setting', FontSize);
        mainWindow.webContents.setZoomFactor(finalScale);
      });

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

    show: false, //<- will be decided by the 'StartMinimizedToTray' prop

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
    mainWindow.webContents.openDevTools( { mode: 'detach'});

  } else {
    console.log('Production mode: ');
    mainWindow.loadFile(path.join(__dirname, `../renderer/${MAIN_WINDOW_VITE_NAME}/index.html`));
  }

  HideToTray = settingsHelper.readSetting('HideToTray', false); //<- Hide to System Tray on close
  console.log('HideToTray:', HideToTray);

  StartMinimizedToTray = settingsHelper.readSetting('StartMinimizedToTray', false);
  console.log('StartMinimizedToTray:', StartMinimizedToTray);

  shipyard = new Shipyard(mainWindow);


  // Register the shortcut to open DevTools
  globalShortcut.register('Shift+F1', () => {
    if (!mainWindow) return
    const wc = mainWindow.webContents

    if (wc.isDevToolsOpened()) {
      wc.closeDevTools()
    } else {
      wc.openDevTools({ mode: 'detach' })
    }
  });
  mainWindow.once('ready-to-show', () => {
    if (StartMinimizedToTray && process.platform === 'win32') {
      mainWindow.hide(); // arranca minimizada en tray
    } else {
      mainWindow.show(); // arranca visible normalmente
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
  try {
    //- Create the Tray Icon:
    console.log('Creating the Tray icon..')
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
  } catch (error) {
    console.error(error);
  }
};

const windows = new Map()

function openSettingsWindow(initData, options = {}) {
  let win = windows.get('settings')

  if (!win || win.isDestroyed()) {
    win = new BrowserWindow({
      width: 800,
      height: 650,
      backgroundColor: '#1F1F1F',
      show: false,
      webPreferences: {
        preload: path.join(__dirname, 'preload.js'),
        contextIsolation: true,
        nodeIntegration: false
      },
      ...options
    });

    if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
      win.loadURL(`${MAIN_WINDOW_VITE_DEV_SERVER_URL}/src/SettingsWindow/settings.html`);
      win.webContents.openDevTools( { mode: 'detach'});
    } else {
      const settingsPath = path.join(process.resourcesPath, 'settings_window', 'settings.html');
      win.loadFile(settingsPath);
      win.webContents.openDevTools( { mode: 'detach'});
    }

    win.once('ready-to-show', () => {
      win.show()
      // Enviar datos iniciales al renderer
      win.webContents.send('init-data', initData)
    })

    win.on('closed', () => {
      windows.delete('settings')
    })
    windows.set('settings', win)
  } else {
    win.focus()
    // refrescar datos si querés
    win.webContents.send('init-data', initData)
  }

  return win
}


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

ipcMain.on('settings:open', (event, initData) => {
  console.log('Opening Settings Window..');
  try {
    let win = new BrowserWindow({
      width: 800,
      height: 750,
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
    })

    if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
      win.loadURL(`${MAIN_WINDOW_VITE_DEV_SERVER_URL}/src/SettingsWindow/settings.html`);
      win.webContents.openDevTools( { mode: 'detach'});
    } else {
      const settingsPath = path.join(process.resourcesPath, 'settings_window', 'settings.html');
      win.loadFile(settingsPath);
      //win.webContents.openDevTools( { mode: 'detach'});
    }


    win.webContents.once('did-finish-load', () => {
      win.webContents.send('settings:init-data', initData)
    })
    win.on('closed', () => {
      event.sender.send('settings:closed', { ok: true })
    })
  } catch (error) {
    console.error(error)
  }
});
ipcMain.on('settings:close', (event) => {
  const win = BrowserWindow.fromWebContents(event.sender)
  if (win) {
    win.close()
  }
})

ipcMain.on('event:forward', (event, { channel, payload }) => {
  // reenviar a todas las ventanas abiertas
  for (const win of BrowserWindow.getAllWindows()) {
    if (win.webContents.id !== event.sender.id) {
      win.webContents.send(channel, payload)
    }
  }
});

ipcMain.on('ui-scale-changed', (event, value) => {
  const { screen } = require('electron');
  const { size, scaleFactor: systemScale } = screen.getPrimaryDisplay();

  let finalScale;
  if (value && value > 0) {
    finalScale = value;
  } else {
    finalScale = systemScale;
    if (size.width >= 3840) {
      finalScale = Math.max(finalScale, 1.5);
    }
  }

  // Aplica a todas las ventanas abiertas
  BrowserWindow.getAllWindows().forEach(win => {
    win.webContents.setZoomFactor(finalScale);
  });
});



// #endregion