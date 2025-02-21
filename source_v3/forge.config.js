const { FusesPlugin } = require('@electron-forge/plugin-fuses');
const { FuseV1Options, FuseVersion } = require('@electron/fuses');
const path = require('path');

/*  // For Linux
module.exports = {
  packagerConfig: {
    // set config executableName
    executableName: "edhm-ui-v3",
    icon: path.join(__dirname, 'public/images/icon.png'),
  },
  makers: [
    {
      name: '@electron-forge/maker-deb',
      executableName: "edhm-ui-v3",
      config: {
        options: {
          icon: path.join(__dirname, 'src/images/icon.png'),
          setupIcon: path.join(__dirname, 'src/images/icon.png'), 
          name: 'edhm-ui-v3',
          productName: 'edhm-ui-v3',
          genericName: 'Modding Tool',
          maintainer: 'Blue Mystic <bluemystic.play@gmail.com>',
          description: 'Mod for Elite Dangerous to customize the HUD of any ship.',
        }
      }
    }
  ],
  // set output directory name
  buildIdentifier: 'edhm-ui-v3',
}
*/
module.exports = {
    packagerConfig: {
      asar: true,
      extraResource: [
        'src/data',
        'src/images', 
        'public', 
      ],
      icon: path.join(__dirname, 'src/images/ED_TripleElite.ico'), //'public/images/ED_TripleElite.ico'
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
          setupExe: 'EDHM-UI-V3 Setup.exe',

          iconUrl: 'file:///' + path.join(__dirname, 'src/images/ED_TripleElite.ico'),   
          setupIcon: path.join(__dirname, 'src/images/ED_TripleElite.ico'),       //setupIcon: 'src/images/ED_TripleElite.ico',         
          icon: path.join(__dirname, 'src/images/ED_TripleElite.ico'),            //icon: 'src/images/ED_TripleElite.ico',

          shortcutFolderName: 'EDHM-UI-V3',
          shortcutName: 'EDHM-UI-V3', 
          createDesktopShortcut: true, 
          createStartMenuShortcut: true,

          loadingGif: path.join(__dirname, 'src/images/EDHNUIv3B.gif'),

          certificateFile: './EDHM-UI-V3.pfx',
          certificatePassword: '@Namllohj1975'
        }
      },
/*      {
        name: '@electron-forge/maker-deb',
        config: {
          options: {
            name: 'EDHM-UI-V3',
            productName: 'EDHM-UI-V3',
            genericName: 'Modding Tool',
            maintainer: 'Blue Mystic <bluemystic.play@gmail.com>',
            description: 'Mod for Elite Dangerous to customize the HUD of any ship.',
            icon: path.join(__dirname, 'src/images/icon.png'), 
            setupIcon: path.join(__dirname, 'src/images/icon.png'), 
            version: '3.0.0'
          }
        }
      },
      {
        name: '@electron-forge/maker-rpm',
        config: {
          options: {
            name: 'EDHM-UI-V3',
            productName: 'EDHM-UI-V3',
            genericName: 'Modding Tool',
            maintainer: 'Blue Mystic <bluemystic.play@gmail.com>',
            description: 'Mod for Elite Dangerous to customize the HUD of any ship.',
            icon: path.join(__dirname, 'src/images/icon.png'), 
            setupIcon: path.join(__dirname, 'src/images/icon.png'), 
            version: '3.0.0'
          }
        }
      },
      {
        name: '@electron-forge/maker-flatpak',
        config: {
          options: {
            name: 'EDHM-UI-V3',
            categories: ['Tools', 'Game'],
            setupIcon: path.join(__dirname, 'src/images/ED_TripleElite.ico'),
            icon: path.join(__dirname, 'src/images/icon.png'), 
          }
        }
      },
      {
        "name": "@electron-forge/maker-zip",
        "platforms": ["darwin", "linux"], // optional
        "config": {
            // Config here
        }
      }*/
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
          ],
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
    ]
  };
