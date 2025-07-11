const { FusesPlugin } = require('@electron-forge/plugin-fuses');
const { FuseV1Options, FuseVersion } = require('@electron/fuses');
const path = require('path');

module.exports = {
  packagerConfig: {
    asar: true,
    extraResource: [
      'src/images'
    ],
    icon: path.join(__dirname, 'src/images/Icon_v3_a1.ico'), 
    appCategoryType:    'public.app-category.developer-tools',
    win32metadata: {
      FileDescription:  'HUD Editor for EDHM-UI v3',
      ProductName:      'hud-editor',
      CompanyName:      'Blue Mystic'
    }
  },
  rebuildConfig: {},
  makers: [
    {
      name: '@electron-forge/maker-squirrel',
      config: {
        name:         'hud-editor',
        authors:      'Blue Mystic',
        appCopyright: 'Blue Mystic - 2025',
        description:  'Editor for the HUD of EDHM-UI.',
        iconUrl: 'file:///' + path.join(__dirname, 'src/images/Icon_v3_a1.ico'),
        setupIcon:            path.join(__dirname, 'src/images/Icon_v3_a1.ico'),         
        icon:                 path.join(__dirname, 'src/images/Icon_v3_a1.ico'), 
        loadingGif:           path.join(__dirname, 'src/images/loading.gif'),
        shortcutFolderName: 'EDHM-UI Hud Editor',
        shortcutName: 'EDHM-UI Hud Editor',
        createDesktopShortcut: true,
        createStartMenuShortcut: true,
        certificateFile: './src/data/EDHM-UI-V3.pfx',
        certificatePassword: '@Namllohj1975'
      },
    },
    {
      name: '@electron-forge/maker-zip',
      platforms: ['darwin'],
    },
    {
      name: '@electron-forge/maker-deb',
      config: {},
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
