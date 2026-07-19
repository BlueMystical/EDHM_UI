## This is the code for new version 3.x+

v3 is not an update but a Complete Rework of all functionalities.<br>
App is made with Electron Forge Vite-Vue and Bootstrap

[License: GPL-3.0+](https://raw.githubusercontent.com/BlueMystical/EDHM_UI/main/license.txt)

## Frameworks Documentation:

* [Electron JS](https://www.electronjs.org)
* [Electron Forge](https://www.electronforge.io)
* [Electron Vite](https://electron-vite.org)
* [Electron Vue](https://github.com/SimulatedGREG/electron-vue)
* [Bootstrap](https://getbootstrap.com)

## Frontier Fleet Integration

The Shipyard can import a Commander's owned ships from Frontier CAPI using OAuth 2.0 Authorization Code with PKCE. See [FRONTIER_OAUTH.md](FRONTIER_OAUTH.md) for the implementation, security, storage, and testing details.

## Requeriments (For Development):
- NODE:   [Download](https://nodejs.org/en/download/prebuilt-installer)
- NPM:    Comes with Node, check the [Documentation](https://docs.npmjs.com/cli/v11/commands/npm)
- Visual Studio Code [Download](https://code.visualstudio.com/)
- Vue VSCode Extension:  Click Yes when VSCode asks for this.
- GitHub Desktop: [Download](https://github.com/apps/desktop) Only If you intent to contribute with Coding.
- Open a Terminal (System Symbol NOT PowerShell) as Administrator.

Check your Node and NPM Versions:
```
node -v
npm -v
```

Update Node & NPM:
```
npm install -g node
npm install -g npm
```

## Usage

To use this project, you can do the following:

1. Clone the repository:
   
    You only need the [source_v3](https://github.com/BlueMystical/EDHM_UI/tree/main/source_v3) folder.

2. On the Terminal, navigate to the folder where you unpacked the repo:
   ```
   cd [Full Path to the Repo]\EDHM_UI\source_v3
   ```
3. Install dependencies: (having your Terminal at the source_v3 folder)
   ```
   npm install
   ```
4. Open the Project files and Visual Studio Code:
   ```
   explorer . && code .
   ```  
5. Run the application:
   ```
   npm start
   ```
## DEPLOY STEPS:

- Copy ```EDHM_Odyssey_v22.00.zip``` to ```..\EDHM_UI\source_v3\src\data\ODYSS\```
- Rename file to 'ODYSS_EDHM-v[number].zip'
- Delete old ZIP from there
- Extract .ini files on same directory.
- Add New/Updated Themes into the 'ODYSS_EDHM-Themes.zip' file.
- Add any (if need) works to the ```..\EDHM_UI\source_v3\src\data\EDHM_HOTFIX.json```

## COMPILING ON WINDOWS
- change version number on package.json
- npm run start
- npm run make
- Open Installer Proyect from ```..\EDHM_UI\source_v3\out\Installer\EDHM-UI-V3.aip```
- Under 'Product Details' increase the version number, clikc 'Generate New' when Prompt.
- Add any new file into 'Files and Folders'
- Remove old 'ODYSS_EDHM-v[number].zip' and add the new one
- Build the Project

## COMPILING ON LINUX

See [Linux Users](../Linux%20Users.md#build-from-source) for the current Linux dependencies, test procedure, build command, output location, and Frontier authorization requirements.

## PUBLISHING
- Make a new Release on GitHub
- Upload the ```..\EDHM_UI\source_v3\out\Installer\Build\edhm-ui-v3-windows-x64.exe``` into GitHub.
- Upload the ```..\EDHM_UI\source_v3\out\Installer\Build\edhm-ui-v3-linux-x64.zip``` into GitHub.
- Test the Installer

![image](https://github.com/user-attachments/assets/c6e5950f-9039-45f3-b9f5-09ffa508fde2)



