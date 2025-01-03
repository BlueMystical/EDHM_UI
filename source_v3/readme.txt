┌────────────────────────────────────────────────────────────────────────────────────────┐
│ ** THIS ARE JUST SOME RANDOM DEV NOTES **                                              │
│ Insanity may arise, dare to keep reading at your own risk, you've been warned (•̀ᴗ•́)و   │
│                                                            -- Blue Mystic.             │
└────────────────────────────────────────────────────────────────────────────────────────┘
------------------------------------------------------------------------------------------
** PROJECT STRUCTURE:

[root] SOURCE-V3 
├── public                             <- Files for Public Access
│   └─ images
├── node_modules                       <- Platform Aware Dependencies
├── out                                <- Compiled Program & Compiled Installer
├── src                                <- Source Code
│   ├── data                           <- Default Data files
│   │     ├── HUD_Type8.json           <- Template for the HUD image and clickable areas
│   │     ├── *_ThemeTemplate.json     <- Template for Themes & Data Shown in the Properties Tab
│   │     ├── Settings.json            <- Default Program Settings
│   │     └── [other files]
│   ├── Helpers                        <- Methods & stuff for the 'Main Process'
│   │     ├── FileHelper.js
│   │     └── [other files]
│   ├── MainWindow                     <- Vue Components in the Renderer Process
│   │     ├── Components
│   │     │      ├── Notifications.vue <- General purpose Notifications
│   │     │      └── ModalDialog.vue
│   │     ├── App.vue                  <- Starting file for 'Renderer Process'
│   │     ├── MainNavBars.vue
│   │     └── [other .vue files]
│   ├── EventBus.js                   <- For comunication thru Events in the 'Renderer Process'
│   ├── preload.js                    <- Intermediate Layer between the 'Main Process' & 'Renderer Process'
│   └── main.js                       <- Root of the 'Main Process'
├── index.html                              
├── renderer.js                       <- Root of the 'Renderer Process'
├── package.json                      <- List of Dependencies and App's Version.
└── forge.config.js                   <- Config for Build & Deploy
------------------------------------------------------------------------------------------
** PROGRAM LOCATIONS:

%LOCALAPPDATA%\edhm_ui 	              <- where the old v2.x version lives
%LOCALAPPDATA%\EDHM-UI-V3             <- where the new v3.0 lives (temporarily)
%LOCALAPPDATA%\Temp\EDHM_UI           <- folder for Updates & Patches Downloads

%USERPROFILE%\EDHM_UI                 <- Themes & User files (User's data persisted thru re-installs)
├── HORIZ                             <- Horizons Legacy data (same structure as 'ODYSS')
├── ODYSS                             <- Odyssey & Horizons Live data
│     ├── 3PMods                      <- Plugins for the App
│     ├── EDHM                        <- Real Location for Mod Files 'ShaderFixes' and 'EDHM-ini'
│     ├── History                     <- history of user actions
│     └── Themes                      <- Where Themes are stored
├── ED_Odissey_User_Settings.json     <- Mod's User Settings
├── Settings.json                     <- the actual User modified Program Settings
└── Errorlog.txt                      <- Error Logging
------------------------------------------------------------------------------------------
-- object for the Log:
{ "date":"28/12/2024 15:00:00", "message":"ERR 404 - not found..", "stack-trace":"" }
------------------------------------------------------------------------------------------
** GAME FILE STRUCTURE (Steam Odyssey):
G:\SteamLibrary\steamapps\common\Elite Dangerous\Products\elite-dangerous-odyssey-64
C:\Program Files (x86)\Steam\steamapps\common\Elite Dangerous\Products\elite-dangerous-odyssey-64\
├── ShaderFixes                       <- [Symlink] full of Shaders
├── EDHM-ini                          <- [Symlink] EDHM home dir
│     ├── 3rdPartyMods                <- Plugins for EDHM
│     │     ├── Keybindings.json
│     │     └── [other files] 
│     ├── DevMode                     <- Don't stick your nose in here or else..
│     ├── Advanced.ini                <- INI config for Color Elements
│     ├── Startup-Profile.ini         <- INI config for non color Elements
│     ├── SuitHud.ini                 <- INI config for Suit and On foot elements
│     └── XML-Profile.ini             <- INI config for the XML Color Matrix
├── EliteDangerous64.exe              <- ED Program
├── d3dx.ini                          <- 3DMigoto INI config
└── d3d11.dll                         <- 3DMigoto
------------------------------------------------------------------------------------------
** ENVIROMENT VARIABLES on Crossplatform:

Variable            Windows           macOS/Linux           Cross-Platform Equivalent
------------------------------------------------------------------------------------------
User's Home         %USERPROFILE%     $HOME                 os.homedir()
App Data (Roaming)  %APPDATA%         $HOME/.config         app.getPath('userData')
Local App Data      %LOCALAPPDATA%    $HOME/.local/share,   app.getPath('appData')
                                      $HOME/Library/Application Support' (macOS)	
------------------------------------------------------------------------------------------
** DATA STRUCTURE:
theme-template = {
...
ui_groups: [
  {
    Elements: [
      {
        File: "Startup-Profile",
        Section: "Constants",
        Key: "x137",
        Value: 100.0,
        ValueType: "Preset",
      },
      {
        File: "Advanced",
        Section: "Constants",
        Key: "x232|y232|z232|w232",
        Value: -16728065.0,
        ValueType: "Color",
      },
      ...
    ]
  },
  ...
],
xml_profile: [
    { key: "x150", value: 0.15 },
    { key: "y150", value: 0.3 },
    ...
  ],
};


iniReader = 
{
    StartupProfile: {
        constants: {
            "z105": "101",
            "y106": "100",
            "x138": "100",
            "y138": "100",
            "w136": "199",
            "y139": "100",
	          ...
        }
    },
    Advanced: {
        Constants: {
            "x228": "0.3663",
            "y228": "0.1248",
            "z228": "1",
            "y87": "1.3",
	          ...
        }
    },
    SuitHud: {
        "Constants": {
            "x30": "199",
            "x31": "1",
            "y31": "0",
            "z31": "1",
            ....
        }
    },
    XmlProfile: {
        "constants": {
            "x150": "0.2",
            "y150": "0.8",
            "z150": "0.3",
            "x151": "0.5",
            "y151": "1",
            "z151": "0.2",
            "x152": "-0.5",
            "y152": "-1",
            "z152": "1"
        }
    }
}
------------------------------------------------------------------------------------------------------------


//------------- INSTALAR ELECTRON: --------------------
https://www.electronjs.org/docs/latest/tutorial/quick-start
node -v
npm -v
npm install -g npm


cd G:\@Proyectos\Electron && G:

//-------------- Desarrollo-----------------------------
cd G:\@Proyectos\EDHM_UI\source_v3 && G:
explorer . && code .
npm start

//-------------------------------

//------- Establecer el Proxy para NPM y Electron: 
setx HTTP_PROXY "http://jchacon:jchacon@192.168.10.1:8080"
setx HTTPS_PROXY "http://jchacon:jchacon@192.168.10.1:8081"
setx ELECTRON_GET_USE_PROXY "true"


//------- Instalar Electron con Template de Electron Forge: https://www.electronforge.io/
//------ VITE:      https://vite.dev/
// ----- VITE+VUE:  https://www.electronforge.io/guides/framework-integration/vue-3
// ----- Bootstrap: https://getbootstrap.com/docs/5.3/components/dropdowns/#via-javascript

npm init electron-app@latest edhm-ui -- --template=vite
cd edhm-ui
npm install --save-dev vue
npm install --save-dev @vitejs/plugin-vue
explorer . && code .

//------- Instalar Bootstrap para usar con Electron
npm install --save bootstrap 
npm install bootstrap-vue@latest
npm install bootstrap-icons	

npm install --save ini 		//<- For editing INI files
npm uninstall regedit --save	//<- For accesing Windows Registry


npm uninstall electron-log --save

npm uninstall @floating-ui/vue  https://floating-ui.com/docs/getting-started
npm uninstall dotenv

npm install @lk77/vue3-color    https://www.npmjs.com/package/@lk77/vue3-color


ie4uinit.exe -show          <- Refresca la Cache de Iconos
npm cache clean --force     <- Refresca la cache de Node

npm install split-pane
npm install mitt




// ------- RUNNING THE APP ----------------
# By default, the start command corresponds to a start npm script:
npm start
npm start --enable-logging
npm run start -- --inspect-electron

# if there is no start script
npx electron-forge start --enable-logging

// ------- INSTALLING BUILD PACKAGES -------------------
/* Create a Windows installer for your Electron app using Electron Forge: */

npm install --save-dev @electron/packager
npm install --save-dev @electron-forge/maker-squirrel

/* 	Create a ZIP archive for your Electron app using Electron Forge.
	The ZIP target builds basic .zip archives containing your packaged application. 
	There are no platform-specific dependencies for using this maker and it will run on any platform. */
npm install --save-dev @electron-forge/maker-zip

/* Create a package for Debian-based Linux distributions (Debian,Ubuntu,Mint,Raspbian,Kali Linux, etc..) */
npm install --save-dev @electron-forge/maker-deb

/* Create an RPM package for RedHat-based Linux distributions (Red Hat,Fedora,CentOS,Rocky,AlmaLinux,Oracle, etc)  */
npm install --save-dev @electron-forge/maker-rpm

/* Generate a DMG with Electron Forge to distribute your Electron app on macOS. */
npm install --save-dev @electron-forge/maker-dmg

// ------------------------------ BUILDING FOR LINUX FROM WINDOWS -------------------------
// SE REQUIERE UNA MAQUINA CON LINUX PARA CREAR EL PAQUETE DE DISTRIBUCION PARA LINUX
// Setting up WSL (Windows Subsystem for Linux) will allow you to create a Linux environment directly on your Windows PC.
// Open PowerShell as an administrator and run the following command:
wsl --install

// ------------------Installing PUBLISHERS-------------------------------------------------
https://www.electronforge.io/config/publishers/github
/* The GitHub target publishes all your artifacts to GitHub releases, this allows your users to download the files straight from your repository. If your repository is open source you can use update.electronjs.org and get a free hosted update service.

npm install --save-dev @electron-forge/publisher-github


// ------- Building distributables -------------------------
npm run make 	//<- Defaults to the arch that you're running on (the "host" arch).
npm run package

# By default, the make command corresponds to a make npm script:
npm run make -- --arch="ia32,x64"  			//<- "ia32","x64","armv7l","arm64","universal","mips64el".
npm run make -- --arch="x64" --targets="@electron-forge/maker-deb"

# If there is no make script:
npx electron-forge make --arch="ia32"
//-------------------------------------------------------------------------------------------
run after cloning the Repo:
npm install

Resolved path: home/bluemystic/edhmv3/src/data/HUD_Type8.json
File not found: home/bluemystic/edhmv3/src/data/HUD_Type8.json
