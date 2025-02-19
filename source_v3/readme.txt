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
│         ├── ED_TripleElite.ico        
│         └── [other files]
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
** APP'S HIERARCHY:
* Renderer Process:
├── renderer.js
│     ├── App.vue
│     │     ├── SettingsEditor.vue
│     │     ├── SearchBox.vue
│     │     ├── Notifications.vue
│     │     └── MainNavBars.vue
│     │          ├── HudImage.vue
│     │          ├── ThemeTab.vue
│     │          ├── PropertiesTab.vue
│     │          ├── UserSettingsTab.vue
│     │          └── GlobalSettingsTab.vue
│     └── EventBus.js
└── preload.js

* Main Process:
├── main.js
│     ├── LoggingHelper.js
│     ├── FileHelper.js
│     ├── SettingsHelper.js
│     ├── IniHelper.js
│     └── ThemeHelper.js
└── preload.js
------------------------------------------------------------------------------------------
** ENVIROMENT VARIABLES on Crossplatform:

Variable            Windows           macOS/Linux           Cross-Platform Equivalent
------------------------------------------------------------------------------------------
User's Home         %USERPROFILE%     $HOME                 os.homedir()
App Data (Roaming)  %APPDATA%         $HOME/.config         app.getPath('userData')
Local App Data      %LOCALAPPDATA%    $HOME/.local/share,   app.getPath('appData')
                                      $HOME/Library/Application Support' (macOS)	
------------------------------------------------------------------------------------------
Screen Resolutions:
720p  (HD):       1280 x 720 pixels
1080p (Full HD):  1920 x 1080 pixels  <-- Program is designed for 1080p
1440p (Quad HD):  2560 x 1440 pixels
4K   (Ultra HD):  3840 x 2160 pixels
------------------------------------------------------------------------------------------
** DATA STRUCTURE:
theme-template = {
...
ui_groups: [
  {
    Name:"Panel_UP",
    Title:"Panel (Upper)",
    Elements: [
      {
        Category: "Targeting Reticle",
        Title: "Mouse Dot",
        File: "Startup-Profile",
        Section: "Constants",
        Key: "x137",
        Value: 100.0,
        ValueType: "Preset",
        Type:"AdvancedMode",
        Description: "Mouse Dot (Ship & SRV)"
      },
      {
        Category: "Targeting Reticle",
        Title: "Rank Icon",
        File: "Advanced",
        Section: "Constants",
        Key: "x232|y232|z232|w232",
        Value: -16728065.0,
        ValueType: "Color",
        Type:"CustomColor",
        Description: "Rank icon that appears next to targeted ship (neutral / orange only)"
      },
      ...more elements..
    ]
  },
  ...more groups...
],
Presets:[
      {
         Type:"AdvancedMode",
         Name:"Custom Color",
         Index:100.0
      },
      {
         Type:"AdvancedMode",
         Name:"XML (User Defined)",
         Index:199.0
      },
      {
         Type:"AdvancedMode",
         Name:"Elite Default",
         Index:200.0
      },
  ... more presets..
],
xml_profile: [
    { key: "x150", value: 0.15 },
    { key: "y150", value: 0.3 },
    ...more key/values..
  ],
};

-------------------------------------------------------
var IniParser = {
    StartupProfile: [
        {
            Section: 'Constants',
            Comment: '',
            Keys: [
                {
                    Key: 'x228',
                    Value: 0.1248,
                    Comment: ''
                },
                { Key: 'z123', Value: 0, Comment: '' },
                //...more keys..
            ]
        },
        //...More Sections..
    ],
    Advanced: [
        {
            Section: 'Constants',
            Comment: '',
            Keys: [
                {
                    Key: 'x101',
                    Value: 1,
                    Comment: ''
                },
                { Key: 'x102', Value: 0, Comment: '' },
                //...more keys..
            ]
        },
        //...More Sections..
    ],
    SuitHud: [
        {
            Section: 'Constants',
            Comment: '',
            Keys: [
                {
                    Key: 'y101',
                    Value: 1,
                    Comment: ''
                },
                { Key: 'y123', Value: 0, Comment: '' },
                //...more keys..
            ]
        },
        //...More Sections..
    ],
    XmlProfile: [
        {
            Section: 'constants',
            Comment: '',
            Keys: [
                { Key: 'x150', Value: 0.15 },
                { Key: 'y150', Value: 0.3 },
                { Key: 'z150', Value: 1 },
                { Key: 'x151', Value: 0.5 },
                { Key: 'y151', Value: 1 },
                { Key: 'z151', Value: 0 },
                { Key: 'x152', Value: 1 },
                { Key: 'y152', Value: 0 },
                { Key: 'z152', Value: 0 }
            ]
        }
    ]
};
-------------------------------------------------------
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

npm uninstall axios //<- For Downloading Files
npm uninstall vanilla-picker --save  //<- For Color Picker

npm uninstall @lk77/vue3-color    https://www.npmjs.com/package/@lk77/vue3-color

-----------------------------------------------------------------------------------
********** CLEAR CACHE *************************
ie4uinit.exe -show          <- Refresca la Cache de Iconos
npm cache clean --force     <- Refresca la cache de Node

-----------------------------------------------------------------------------------
npm install split-pane
npm install mitt
npm install zip-lib
npm install find-process


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
npm install --save electron-squirrel-startup 


/* 	Create a ZIP archive for your Electron app using Electron Forge.
	The ZIP target builds basic .zip archives containing your packaged application. 
	There are no platform-specific dependencies for using this maker and it will run on any platform. */

npm install --save-dev @electron-forge/maker-zip

/* Create a package for Debian-based Linux distributions (Debian,Ubuntu,Mint,Raspbian,Kali Linux, etc..) */
npm install --save-dev @electron-forge/maker-deb
sudo apt-get install rpm

/* Create an RPM package for RedHat-based Linux distributions (Red Hat,Fedora,CentOS,Rocky,AlmaLinux,Oracle, etc)  */
npm install --save-dev @electron-forge/maker-rpm

/* Generate a DMG with Electron Forge to distribute your Electron app on macOS. */
npm install --save-dev @electron-forge/maker-dmg

/* The Flatpak target builds .flatpak files, which is a packaging format for Linux distributions that allows for sandboxed installation of applications in isolation from the rest of their system */
/* You can only build the Flatpak target if you have flatpak, flatpak-builder, and eu-strip (usually part of the elfutils package) installed on your system. */
https://www.electronforge.io/config/makers/flatpak
npm install --save-dev @electron-forge/maker-flatpak
npm uninstall electron-forge/maker-flatpak



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

// running the installer on Linux:
sudo apt install ./edhm-ui-v3_3.0.0_amd64.deb
sudo rpm -i EDHM-UI-V3-3.0.0-1.x86_64.rpm

// Un-install in Debian:
dpkg --list
sudo apt-get remove edhm-ui-v3
sudo apt-get purge edhm-ui-v3



