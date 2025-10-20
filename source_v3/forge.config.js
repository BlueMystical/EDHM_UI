// forge.config.js
const { FusesPlugin } = require('@electron-forge/plugin-fuses');
const { FuseV1Options, FuseVersion } = require('@electron/fuses');
const fs = require('node:fs');
const path = require('path');

function reveal(r) { return atob(r).split("").map((r => String.fromCharCode(r.charCodeAt(0) - 3))).join("").split("").reverse().join("") };
function safeInclude(n) { return fs.existsSync(n) ? n : null }



/* ---  FOR WINDOWS --- */
module.exports = {
  packagerConfig: {
    asar: true,
    extraResource: [
      'src/data',
      'src/images',
      'public',
      'out/renderer/settings_window'
    ],
    icon: path.join(__dirname, 'src', 'images', 'Icon_v3_a0.ico'), //'public/images/Icon_v3_a0.ico'
    appCategoryType: 'public.app-category.developer-tools',
    win32metadata: {
      FileDescription: 'Mod for Elite Dangerous to customize the HUD of any ship.',
      ProductName: 'EDHM-UI-V3',
      CompanyName: 'Blue Mystic',
      "requested-execution-level": "highestAvailable"
    }
  },
  makers: [
    {
      name: '@electron-forge/maker-squirrel',
      config: {
        name: 'EDHM-UI-V3',
        authors: 'Blue Mystic',
        appCopyright: 'Blue Mystic - 2025',
        description: 'Mod for Elite Dangerous to customize the HUD of any ship.',
        setupExe: 'edhm-ui-v3-windows-x64.exe',

        iconUrl: 'file:///' + path.join(__dirname, 'src', 'images', 'Icon_v3_a0.ico'),
        setupIcon: path.join(__dirname, 'src', 'images', 'Icon_v3_a0.ico'),       //setupIcon: 'src/images/Icon_v3_a0.ico',         
        icon: path.join(__dirname, 'src', 'images', 'Icon_v3_a0.ico'),       //icon: 'src/images/Icon_v3_a0.ico',
        loadingGif: path.join(__dirname, 'src', 'images', 'EDHNUIv3B.gif'),

        shortcutFolderName: 'EDHM-UI-V3',
        shortcutName: 'EDHM-UI-V3',
        createDesktopShortcut: true,
        createStartMenuShortcut: true,

        //certificateFile: path.join(__dirname, 'src','data','etc','EDHM-UI-V3.pfx'), // './src/data/etc/EDHM-UI-V3.pfx',
        //certificateFile:  './src/data/etc/EDHM-UI-V3.pfx',
        //certificatePassword:  reveal('ODo8NG1rcm9vcGRRQw==') 
      }
    },
    {
      name: '@electron-forge/maker-zip'
    }
  ],
  plugins: [
    {
      name: '@electron-forge/plugin-vite',
      config: {
        build: [
          {
            entry: 'src/main.js',
            config: 'vite.main.config.mjs',
            target: 'main',
          },
          {
            entry: 'src/preload.js',
            config: 'vite.preload.config.mjs',
            target: 'preload',
          },
        ],
        renderer: [
          {
            name: 'main_window',
            config: 'vite.renderer.config.mjs',
          },
          {
            name: 'settings',
            config: 'vite.settings.config.mjs',
          },
        ]

      },
    },
    new FusesPlugin({
      version: FuseVersion.V1,
      [FuseV1Options.RunAsNode]: false,
      [FuseV1Options.EnableCookieEncryption]: true,
      [FuseV1Options.EnableNodeOptionsEnvironmentVariable]: false,
      [FuseV1Options.EnableNodeCliInspectArguments]: false,
      [FuseV1Options.EnableEmbeddedAsarIntegrityValidation]: true,
      [FuseV1Options.OnlyLoadAppFromAsar]: true
    })
  ],
  //build: {
  //  files: [
  //    'dist/electron/**/*',
  //    'dist/renderer/**/*', // Ensure this line is present and correct
  //    'resources/**/*',
  //    'node_modules/**/*',
  //  ],
  //},
};

/*  // For Linux
module.exports = {
  packagerConfig: {
    asar: true,
    executableName: 'edhm-ui-v3',
    icon: 'public/images/icon.png',
    extraResource: [
      'src/data',
      'src/images',
      'public',
    ],
    buildIdentifier: 'production' // Add build identifier
  },
  rebuildConfig: {},
  makers: [
    {
      name: '@electron-forge/maker-squirrel',
      config: {},
    },
    {
      name: '@electron-forge/maker-zip',
    },
    {
      name: '@electron-forge/maker-deb',
      executableName: "edhm-ui-v3",
      config: {
        options: {
          icon: 'public/images/icon.png',
          setupIcon: 'public/images/icon.png',
          name: 'edhm-ui-v3',
          productName: 'edhm-ui-v3',
          genericName: 'Modding Tool',
          maintainer: 'Blue Mystic <bluemystic.play@gmail.com>',
          description: 'Mod for Elite Dangerous to customize the HUD of any ship.',
        }
      }
    },
    {
      name: '@electron-forge/maker-rpm',
      config: {},
    },
  ],
  plugins: [
    {
      name: '@electron-forge/plugin-vite',
      config: {
        // `build` can specify multiple entry builds, which can be Main process, Preload scripts, Worker process, etc.
        // If you are familiar with Vite configuration, it will look really familiar.
        build: [
          {
            // `entry` is just an alias for `build.lib.entry` in the corresponding file of `config`.
            entry: 'src/main.js',
            config: 'vite.main.config.mjs',
            target: 'main',
          },
          {
            entry: 'src/preload.js',
            config: 'vite.preload.config.mjs',
            target: 'preload',
          },
        ],
        renderer: [
          {
            name: 'main_window',
            config: 'vite.renderer.config.mjs',
          },
        ],
      },
    },
    // Fuses are used to enable/disable various Electron functionality
    // at package time, before code signing the application
    new FusesPlugin({
      version: FuseVersion.V1,
      [FuseV1Options.RunAsNode]: false,
      [FuseV1Options.EnableCookieEncryption]: true,
      [FuseV1Options.EnableNodeOptionsEnvironmentVariable]: false,
      [FuseV1Options.EnableNodeCliInspectArguments]: false,
      [FuseV1Options.EnableEmbeddedAsarIntegrityValidation]: true,
      [FuseV1Options.OnlyLoadAppFromAsar]: true,
    }),
  ],
};
*/