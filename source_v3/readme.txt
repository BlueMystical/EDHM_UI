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
│   ├── MainWindow                     <- Vue Components for the 'Renderer Process'
│   │     ├── Components
│   │     │      ├── Notifications.vue <- General purpose Notifications
│   │     │      └── ModalDialog.vue
│   │     ├── App.vue                  <- Root of the 'Renderer Process'
│   │     ├── MainNavBars.vue          <- Contains all Menus and Buttons 
│   │     └── [other .vue files]
│   ├── TPMods
│   │     ├── TPModsManager.html
│   │     ├── TPModsManager.js
│   │     ├── TPModsManager.vue
│   │     └── [other files]
│   ├── EventBus.js                   <- For comunication thru Events in the 'Renderer Process'
│   ├── preload.js                    <- Intermediate Layer between the 'Main Process' & 'Renderer Process'
│   └── main.js                       <- Root of the 'Main Process'
├── index.html                        <- Base over everithing is rendered.   
├── renderer.js                       <- 
├── router.js
├── package.json                      <- List of Dependencies and App's Version Number.
└── forge.config.js                   <- Configs for Build & Deploy
------------------------------------------------------------------------------------------
** ENVIROMENT VARIABLES on Crossplatform:
Variable            Windows           Linux           Cross-Platform Equivalent
------------------------------------------------------------------------------------------
User's Home         %USERPROFILE%     $HOME                 os.homedir()
App Data (Roaming)  %APPDATA%         $HOME/.config         app.getPath('userData')
Local App Data      %LOCALAPPDATA%    $HOME/.local/share,   app.getPath('appData')
------------------------------------------------------------------------------------------
Screen Resolutions:
720p  (HD):       1280 x 720 pixels
1080p (Full HD):  1920 x 1080 pixels  <-- Program is designed for 1080p
1440p (Quad HD):  2560 x 1440 pixels
4K   (Ultra HD):  3840 x 2160 pixels
------------------------------------------------------------------------------------------
** PROGRAM LOCATIONS:

%LOCALAPPDATA%\edhm_ui 	              <- where the old v2.x version lives
%LOCALAPPDATA%\EDHM-UI-V3             <- where the new v3.0 lives (temporarily)
%LOCALAPPDATA%\EDHM-UI-V3\app-{3.0.19}\resources\data\
%LOCALAPPDATA%\Temp\EDHM_UI           <- folder for Updates & Patches Downloads
%USERPROFILE%\EDHM_UI                 <- Themes & User files (User's data persisted thru re-installs)
├── HORIZ                             <- Horizons Legacy data (same structure as 'ODYSS')
├── ODYSS                             <- Odyssey & Horizons Live data
│     ├── 3PMods                      <- Themes for 3PMods
│     ├── EDHM                        <- Real Location for Mod Files: 'ShaderFixes' and 'EDHM-ini'
│     ├── History                     <- history of user actions
│     └── Themes                      <- Where Themes are stored
├── images                            <- Images and Icons
├── ED_Odissey_User_Settings.json     <- Settings the user dont want to be affected by themes
├── ED_Odissey_Global_Settings.json   <- Settings We dont want to be affected by themes
├── Shipyard_v3.json                  <- Data for the Shipyard, ships and asociated themes.
├── Settings.json                     <- the Program Settings
└── Errorlog.txt                      <- Error Logging
------------------------------------------------------------------------------------------
-- object for the Error Log:
{ "date":"28/12/2024 15:00:00", "message":"ERR 404 - not found..", "stack-trace":"" }
------------------------------------------------------------------------------------------
** GAME FILE STRUCTURE (Steam Odyssey):

C:\Program Files (x86)\Steam\steamapps\common\Elite Dangerous\Products\elite-dangerous-odyssey-64\
G:\SteamLibrary\steamapps\common\Elite Dangerous\Products\elite-dangerous-odyssey-64
├── ShaderFixes                       <- [Symlink] full of Shaders
├── EDHM-ini                          <- [Symlink] EDHM home dir
│     ├── 3rdPartyMods                <- Plugins for EDHM
│     │     ├── Keybindings.json
│     │     ├── Keybindings.ini
│     │     └── [other files]
│     ├── DevMode                     <- Don't stick your nose in here or else..
│     ├── Advanced.ini                <- INI config for Color Elements
│     ├── Startup-Profile.ini         <- INI config for non color Elements
│     ├── SuitHud.ini                 <- INI config for Suit and On foot elements
│     └── XML-Profile.ini             <- INI config for the XML Color Matrix
├── EliteDangerous64.exe              <- ED Program
├── d3dx.ini                          <- 3DMigoto INI config
└── d3d11.dll                         <- 3DMigoto

Player Journal:         %USERPROFILE%\Saved Games\Frontier Developments\Elite Dangerous\ 
Graphic Configurations: %LOCALAPPDATA%\Frontier Developments\Elite Dangerous\Options\Graphics\
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
        Description: "xxx"
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
xml_profile:  [
  { key: 'x150', value: 0.15 },
  { key: 'y150', value: 0.3 },
  { key: 'z150', value: 1 },
  { key: 'x151', value: 0.5 },
  { key: 'y151', value: 1 },
  { key: 'z151', value: 0 },
  { key: 'x152', value: 1 },
  { key: 'y152', value: 0 },
  { key: 'z152', value: 0 }
]
};

-------------------------------------------------------
var IniParser = {

  StartupProfile: {
   "sections":[
      {
         "name":"Constants",
         "comments":[
            "; This is a comment!",
            "; All comments will be written by the serializer over the object they belong to",
            "; -------------------------------------"
         ],
         "keys":[
            {
               "name":"x150",
               "value":"1",
               "comments":[
                  "; Key x150 has an integer value"
               ]
            },
            {
               "name":"x151",
               "value":"1.5",
               "comments":[
                  "; Key x151 has a decimal value"
               ]
            },
            {
               "name":"Key Name",
               "value":"Hello World",
               "comments":[
                  "; Key 'Key Name' has a string value",
                  "; both keys and values can have spaces"
               ]
            }
         ],
         "logic":[
            {
               "comments":[
                  "; this is a logic area inside an INI section",
                  "; logic areas contain 'if-endif' blocks",
                  "; logic areas will be written at the end of the section the belong to",
                  "; logic areas will be preserved exactly as they are!",
                  "; logic ares will be parsed as a bunch of string lines",
                  "; there may be nested if-endif blocks"
               ],
               "lines":[
                  "if x151 > 2",
                  "; this is a comment inside a logic area!",
                  "   x151 = 1.9",
                  "   x150 = x151",
                  "   if z150 < x150",
                  "      z150 = 0",
                  "   end if",
                  "endif"
               ]
            }
         ]
      },
      {
         "name":"Other Section",
         "comments":[],
         "keys":[],
         "logic":[]
      }
   ]
},
  Advanced: {
      sections: [
         {
            name: 'Constants',
            comments: [],
            keys: [],
            logic: []
         }
      ]
   },
  SuitHud: {
      sections: [
         {
            name: 'Constants',
            comments: [],
            keys: [],
            logic: []
         }
      ]
   },
  XmlProfile: {
    sections: [{
      name: 'Constants',
      comments: [],
      keys: {
        {
            "name":"x150",
            "value":"1",
            "comments":[ ]
         },
        y150: [Object],
        z150: [Object],
        x151: [Object],
        y151: [Object],
        z151: [Object],
        x152: [Object],
        y152: [Object],
        z152: [Object]
      },
      logic: { comments: [], lines: [] }
    }
    ]
  }
};
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

//------- Establecer el Proxy para NPM y Electron PERMANENTE: 
--setx HTTP_PROXY "http://jchacon:jchacon@192.168.10.1:8080"
--setx HTTPS_PROXY "http://jchacon:jchacon@192.168.10.1:8081"
--setx ELECTRON_GET_USE_PROXY "true"

//------- Establecer el Proxy para NPM y Electron TEMPORAL, solo para la sesion actual: 
set HTTP_PROXY=http://jchacon:jchacon@192.168.10.1:8080
set HTTPS_PROXY=http://jchacon:jchacon@192.168.10.1:8081

export http_proxy="http://jchacon:jchacon@192.168.10.1:8080"
export https_proxy="http://jchacon:jchacon@192.168.10.1:8081"


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
npm install mitt    <- for Events
npm install zip-lib <- for ZIP files
npm install -g asar <- for Build deploy
npm install @vitejs/plugin-vue --save-dev
npm install vue-router@4   <- For multi-windows https://github.com/vuejs/router
npm install chokidar   <- For file watching (Shipyard)

npm install robot-js    <- for keyboard events  https://github.com/Robot/robot-js
- On Windows, global keyboard events should work without issues.
- On Linux, users may need to allow accessibility permissions for the app to interact with input events (this varies by distro).


-----------------------------------------------------------------------------------
********** CLEAR CACHE *************************
ie4uinit.exe -show          <- Refresca la Cache de Iconos
npm cache clean --force     <- Refresca la cache de Node

-----------------------------------------------------------------------------------
// ASAR:
npm install -g asar
npm install -g @electron/asar

// Ver el Contenido del archivo ASAR:
asar list app.asar
asar extract app.asar extracted_app


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

-------------------------------------------------------------------------------------------
/* 	Create a ZIP archive for your Electron app using Electron Forge.
	The ZIP target builds basic .zip archives containing your packaged application. 
	There are no platform-specific dependencies for using this maker and it will run on any platform. */

npm install --save-dev @electron-forge/maker-zip
npm run make -- --arch="x64" --targets="@electron-forge/maker-zip"


-------------------------------------------------------------------------------------------
/* Create a package for Debian-based Linux distributions (Debian,Ubuntu,Mint,Raspbian,Kali Linux, etc..) */
npm install --save-dev @electron-forge/maker-deb
npm run make -- --arch="x64" --targets="@electron-forge/maker-deb"

// Installing the .deb:
sudo apt install ./edhm-ui-v3_3.0.0_amd64.deb
or
sudo dpkg -i edhm-ui-v3_3.0.0_amd64.deb
or
sudo apt install gdebi
sudo gdebi edhm-ui-v3_3.0.0_amd64.deb

-------------------------------------------------------------------------------------------

/* Create an RPM package for RedHat-based Linux distributions (Red Hat,Fedora,CentOS,Rocky,AlmaLinux,Oracle, etc)  */
sudo apt-get install rpm
npm install --save-dev @electron-forge/maker-rpm
npm run make -- --arch="x64" --targets="@electron-forge/maker-rpm"

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

//-------------------------------------------------------------------------------------------
// running the installer on Linux:
sudo apt install ./edhm-ui-v3_3.0.0_amd64.deb
sudo rpm -i EDHM-UI-V3-3.0.0-1.x86_64.rpm

// Un-install in Debian:
dpkg --list
sudo apt-get remove edhm-ui-v3
sudo apt-get purge edhm-ui-v3
//-------------------------------------------------------------------------------------------


