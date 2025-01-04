const { FusesPlugin } = require('@electron-forge/plugin-fuses');
const { FuseV1Options, FuseVersion } = require('@electron/fuses');
const path = require('path');

module.exports = {
    packagerConfig: {
      asar: true,
      extraResource: [
        'src/data',
        'src/images', // Keep images here if needed
        'public', // Keep public folder if needed
      ],
      icon: 'src/images/ED_TripleElite.ico',
      appCategoryType: 'public.app-category.developer-tools',
      win32metadata: {
        CompanyName: 'BlueMystic Corp.',
        FileDescription: 'Mod for Elite Dangerous to customize the HUD of any ship.',
        ProductName: 'EDHM-UI-V3',
        // "requested-execution-level": "highestAvailable"
      }
    },
    makers: [
      {
        name: '@electron-forge/maker-squirrel',
        config: {
          name: 'EDHM-UI-V3',
          exe: 'EDHM-UI.exe',
          iconUrl: path.join(__dirname, 'src/images/ED_TripleElite.ico'),
          setupIcon: path.join(__dirname, 'src/images/ED_TripleElite.ico'),
          setupExe: 'EDHM-UI-Installer.exe',
          shortcutFolderName: 'EDHM-UI',
          authors: 'Blue Mystic',
          description: 'Mod for Elite Dangerous to customize the HUD of any ship.',
          certificateFile: './EDHM-UI-V3.pfx',
          certificatePassword: '@Namllohj1975'
        }
      },
      {
        name: '@electron-forge/maker-deb',
        config: {
          options: {
            name: 'EDHM-UI-V3',
            maintainer: 'Blue Mystic <bluemystic.play@gmail.com>',
            homepage: 'https://bluemystic.com',
            productName: 'EDHM-UI',
            icon: path.join(__dirname, 'src/images/icon.png'), 
            setupIcon: path.join(__dirname, 'src/images/icon.png'),
            genericName: 'Modding Tool'
          }
        }
      },
      {
        name: '@electron-forge/maker-rpm',
        config: {
          options: {
            name: 'EDHM-UI-V3',
            maintainer: 'Blue Mystic <bluemystic.play@gmail.com>',
            homepage: 'https://bluemystic.com',
            productName: 'EDHM-UI',
            icon: path.join(__dirname, 'src/images/icon.png'), 
            setupIcon: path.join(__dirname, 'src/images/icon.png'),
            genericName: 'Modding Tool'
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
