import { app, ipcMain } from 'electron';
import path from 'node:path';
import fs from 'fs';
//import { copyFileSync, constants } from 'node:fs';
import INIparser from './IniParser.js';
import Log from './LoggingHelper.js';
import fileHelper from './FileHelper';
import Util from './Utils.js';
import settingsHelper from './SettingsHelper.js'
import { writeFile } from 'node:fs/promises';
import FileHelper from './FileHelper';


/** * Retrieves themes from a specified directory path.
 * @async
 * @param {string} dirPath - The directory path to search for themes.
 * @returns {Promise<Array>} A promise that resolves to an array of themes.
 */
const getThemes = async (dirPath) => {
  try {
    // Get all Folders in the Themes Directory, each folder is a Theme:
    const subfolders = await fs.promises.readdir(dirPath, { withFileTypes: true });
    const files = [];

    for (const dirent of subfolders) {
      if (dirent.isDirectory()) {
        const subfolderPath = path.join(dirPath, dirent.name); //<- Full Theme path

        try {
          // Here we Load the Data of the Theme either from the JSON (if present) or from the INIs:
          const template = await LoadTheme(subfolderPath);

          // Check if the folder contains a Large Preview file:
          let preview_url = '';
          if (fileHelper.checkFileExists(path.join(subfolderPath, dirent.name + '.jpg'))) {
            preview_url = `file:///${path.join(subfolderPath, dirent.name + '.jpg')}`;
          }

          // Assemble the Data to return:
          files.push({
            theme: template,
            path: subfolderPath,
            preview: preview_url,
            thumbnail: 'PREVIEW.jpg',
            credits: template.credits,
            name: template.credits.theme,
            isFavorite: template.isFavorite
          });

          // Writes the JSON in the theme folder:
          if (!fileHelper.checkFileExists(path.join(subfolderPath, 'ThemeSettings.json'))) {
            const JsonString = JSON.stringify(template, null, 4);
            await writeFile(
              path.join(subfolderPath, 'ThemeSettings.json'),
              JsonString,
              { encoding: "utf8", flag: 'w' }
            );
          }

          // Theme Migration :
          const ThemeCleansing = true; //<- Swap to 'true' to save the json and cleanse old files
          try {
            if (ThemeCleansing) {
              // Sanitization: For Themes Exportings
              //fileHelper.deleteFilesByType(subfolderPath, '.ini');
              //fileHelper.deleteFilesByType(subfolderPath, '.credits');
              //fileHelper.deleteFilesByType(subfolderPath, '.fav');
              //fileHelper.deleteFilesByType(subfolderPath, '.json'); //<- BEWARE !
            }
          } catch { }

        } catch (error) {
          Log.Error(error.message, error.stack);
          console.error(error);
        }
      }
    }
    return files;

  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

/** Loads a Theme from a specified folder path. * 
 * @param {*} themeFolder Path to the folder containing the Theme files
 */
const LoadTheme = async (themeFolder) => {
  let template = {};
  try {
    const themeJSON = path.join(themeFolder, 'ThemeSettings.json');
    if (fs.existsSync(themeJSON)) {
      // New v3 Format for Themes, single File JSON:
      template = await fileHelper.loadJsonFile(themeJSON);
      template.path = themeFolder;
      template.isFavorite = fileHelper.checkFileExists(path.join(themeFolder, 'IsFavorite.fav'));

    } else {
      // Old fashion format for Themes, Multiple INI files:
      const ThemeINIs = await LoadThemeINIs(themeFolder);
      const defaultThemePath = fileHelper.getAssetPath('./data/ODYSS/ThemeTemplate.json');

      template = await fileHelper.loadJsonFile(defaultThemePath);
      template = await ApplyIniValuesToTemplate(template, ThemeINIs);

      template.credits = await GetCreditsFile(themeFolder);
      template.path = themeFolder;
      template.isFavorite = fileHelper.checkFileExists(path.join(themeFolder, 'IsFavorite.fav'));
    }
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
  return template;
};

const ApplyIniValuesToTemplate = (template, iniValues) => {
  //console.log('ApplyIniValuesToTemplate..');
  try {
    if (Array.isArray(template.ui_groups) && template.ui_groups.length > 0) {
      for (const group of template.ui_groups) {
        if (group.Elements != null) {
          for (const element of group.Elements) {

            const iniKey = element.Key;
            const defaultValue = element.Value;

            const foundValue = INIparser.findValueByKey(iniValues, element);
            // #region Data Example:

            /* foundValue can be either an Array of Key/Values or a Single Decimal Value:  

            foundValue: null, //<- Key: "x157" Not Found
            foundValue: 0,
            foundValue: 100,
            foundValue: [
              { key: 'x33', value: 0.6376 },
              { key: 'y33', value: 0.6376 },
              { key: 'z33', value: 0.6376 },
              { key: 'w33', value: 1 }
            ] */

            // #endregion

            if (foundValue != null && foundValue != undefined) {
              if (Array.isArray(foundValue) && foundValue.length > 0) {

                const colorKeys = iniKey.split("|");            //<- iniKey === "x159|y159|z159" or "x159|y155|z153|w200"
                const colorComponents = colorKeys.map(key => {  
                  const foundValueObj = foundValue.find(obj => obj.key === key);
                  return foundValueObj ? foundValueObj.value : undefined;
                }); //<- colorComponents: [ '0.063', '0.7011', '1' ]

                if (colorComponents != undefined && !colorComponents.includes(undefined)) {
                  const color = Util.reverseGammaCorrectedList(colorComponents); //<- color: { r: 81, g: 220, b: 255, a: 255 }
                  element.Value = parseFloat(Util.rgbaToInt(color).toFixed(1));

                  /*  if (iniKey === 'x159|y159|z159') {
                      console.log('x159|y159|z159: ' + colorComponents );
                      console.log('RGB reversed: ', color);
                      console.log('INT value: ', element.Value);
                    }*/

                } else {
                  element.Value = parseFloat(defaultValue.toFixed(1));
                  console.log('Color Conversion Error:', path.join(element.File, element.Section, element.Key), 'Val: ', element.Value);
                }

              } else {
                // No hay Multi-Keys, foundValue: 100.0
                element.Value = parseFloat(foundValue ?? defaultValue);
              }
            } else {
              //If the Key in the Theme is not found in the Template, the template's value is used
              element.Value = parseFloat(defaultValue);
            }
          }
        }
      }
    }

    // Update the XMLs:
    if (template.xml_profile && iniValues.XmlProfile) {
      // #region Data Example:
      /* watch out for the Caps!! yeah yeah i know..
        template.xml_profile: [
          { key: 'x150', value: 0.15 },
          { key: 'y150', value: 0.3 },
          { key: 'z150', value: 1 },
          { key: 'x151', value: 0.5 },
          { key: 'y151', value: 1 },
          { key: 'z151', value: 0 },
          { key: 'x152', value: 1 },
          { key: 'y152', value: 0 },
          { key: 'z152', value: 0 }
        ];
        iniValues.XmlProfile: [
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
        ];      */
      // #endregion

      const iniProfile = iniValues.XmlProfile[0].Keys; // Assuming only one section in iniValues
      template.xml_profile.forEach(templateItem => {
        const iniItem = iniProfile.find(item => item.Key === templateItem.key);
        if (iniItem) {
          templateItem.value = parseFloat(iniItem.Value);  // Parse to decimal
        } // else, templateItem.value remains unchanged (Default Value from Template).
      });

    }
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
  return template;
};

const ApplyTemplateValuesToIni = (template, iniValues) => {
  let stackTrace = '';
  try {

    if (Array.isArray(template.ui_groups) && template.ui_groups.length > 0) {
      for (const group of template.ui_groups) {
        if (group.Elements != null) {
          for (const element of group.Elements) {

            const fileName = element.File.replace(/-/g, '');  // Remove hyphens
            const section = element.Section;  // Convert section to lowercase
            const iniKey = element.Key;

            stackTrace = path.join(element.File, element.Section, element.Key) + ': ';

            if (iniKey.includes('|')) {
              //Multi Key: Colors
              const keys = element.Key.split('|');  //<- iniKey === "x159|y159|z159" or "x159|y155|z153|w200"
              if (Array.isArray(keys) && keys.length > 2) {

                const RGBAcolor = Util.intToRGBA(element.Value); //<- color: { r: 81, g: 220, b: 255, a: 255 }
                const sRGBcolor = Util.GetGammaCorrected_RGBA(RGBAcolor);
                const values = [sRGBcolor.r, sRGBcolor.g, sRGBcolor.b, sRGBcolor.a]; //<- [ 0.082, 0.716, 1.0, 1.0 ]

                // DEBUG:  Check Color Conversion on a given Key:
                /*if (iniKey === 'x232|y232|z232|w232') {
                  console.log(`Key: ${iniKey}, Int:${element.Value} -> sRGB:`, values);
                }*/

                keys.forEach((key, index) => {
                  const value = parseFloat(values[index]);
                  try {
                    iniValues = INIparser.setIniValue(iniValues, fileName, section, key, value);
                  } catch (error) {
                    console.log(stackTrace + value, error.message);
                  }
                });
              }
            } else {
              //Single Key:
              try {                
                iniValues = INIparser.setIniValue(iniValues, fileName, section, iniKey, parseFloat(element.Value));
              } catch (error) {
                console.log(stackTrace + value, error.message);
              }
            }
          }
        }
      }
    }

    // Set values in the XML:
    if (template.xml_profile && iniValues.XmlProfile) {

      const section = iniValues.XmlProfile[0].Section;
      template.xml_profile.forEach(element => {
        iniValues = INIparser.setIniValue(iniValues, 'XmlProfile', section, element.key, parseFloat(element.value));
      });

    }

  } catch (error) {
    throw new Error('At ThemeHelper.js/ApplyTemplateValuesToIni(): ' + stackTrace + error.message);
  }
  return iniValues;
};

async function GetCreditsFile(themePath) {
  let creditsJson = {};
  try {
    // Find any .credits file
    const themeName = path.basename(themePath);
    const allFiles = await fs.promises.readdir(themePath);
    const creditsFile = allFiles.find(file => file.endsWith('.credits'));

    if (creditsFile) {
      const creditsFilePath = path.join(themePath, creditsFile);
      const creditsData = await fs.promises.readFile(creditsFilePath, 'utf8');
      try {
        creditsJson = JSON.parse(creditsData);
      } catch {
        creditsJson = {
          "theme": themeName,
          "author": "Unknown",
          "description": "** .Credits file missing or Invalid **",
          "preview": ""
        };
      }
    }

  } catch (error) {
    console.log(error);
  }
  return creditsJson;
};

/** Marks the given theme as a Favorite * 
 * @param {*} themePath Absolute path to the Theme
 */
async function FavoriteTheme(themePath) {
  try {
    const dummy = { isFavorite: true };
    const favFilePath = path.join(themePath, 'IsFavorite.fav');
    const _ret = fileHelper.writeJsonFile(favFilePath, dummy, false);
    return _ret;
  } catch (error) {
    console.log(error);
    throw new Error(error.message + error.stack);
  }
};
/** Removes the given theme from Favorites * 
 * @param {*} themePath Absolute path to the Theme
 * @returns 
 */
async function UnFavoriteTheme(themePath) {
  try {
    const favFilePath = path.join(themePath, 'IsFavorite.fav');
    const _ret = fileHelper.deleteFileByAbsolutePath(favFilePath);
    return _ret;
  } catch (error) {
    console.log(error);
    throw new Error(error.message + error.stack);
  }
};

/** Returns the Currently Applied Theme Settings as a ThemeTemplate
 * @param {*} themePath Full path to the Game Instance
 */
async function GetCurrentSettingsTheme(themePath) {
  try {
    const ThemeINIs = LoadThemeINIs(themePath);
    const defaultSettingsPath = fileHelper.getAssetPath('data/ODYSS/ThemeTemplate.json');

    let themeTemplate = await fileHelper.loadJsonFile(defaultSettingsPath);
    themeTemplate = await ApplyIniValuesToTemplate(themeTemplate, ThemeINIs);
    themeTemplate.name = "Current Settings";
    themeTemplate.credits = {
      theme: "Current Settings",
      author: "User",
      description: "Currently Applied Colors in Game",
      preview: "",
      path: themePath
    };

    return themeTemplate;

  } catch (error) {
    console.log(error.message + error.stack);
    throw new Error(error.message + error.stack);
  }
  return null;
};

/** Makes a new Theme, saved on the Themes Folder
 * @param {*} credits Meta-data for the new theme
 * @returns true is success
 */
async function CreateNewTheme(credits) {
  try {
    //console.log('credits: ', credits);  

    //1. RESOLVE THE THEMES PATH:
    const Credits = credits.credits;
    const gameInstance = await settingsHelper.getActiveInstance();              //console.log('gameInstance: ', gameInstance);  
    const GameType = gameInstance.key === 'ED_Odissey' ? 'ODYSS' : 'HORIZ';     //console.log('GameType: ', GameType);  
    const settings = await settingsHelper.loadSettings();                       //console.log('settings: ', settings);    
    const dataPath = fileHelper.resolveEnvVariables(settings.UserDataFolder);   //console.log('dataPath: ', dataPath);     //<- %USERPROFILE%\EDHM_UI  
    const themesPath = path.join(dataPath, GameType, 'Themes', Credits.theme);  //console.log('themesPath: ', themesPath); //<- %USERPROFILE%\EDHM_UI\ODYSS\Themes\MyTheme   

    //2. CREATE THE NEW THEME FOLDER IF IT DOESNT EXIST:
    if (fileHelper.ensureDirectoryExists(themesPath)) {

      //3. LOAD THE CURRENTLY APPLIED THEME SETTINGS:
      const CurrentSettings = await GetCurrentSettingsTheme(path.join(gameInstance.path, 'EDHM-ini'));
      CurrentSettings.credits.theme = Credits.theme;
      CurrentSettings.credits.author = Credits.author;
      CurrentSettings.credits.description = Credits.description;
      CurrentSettings.version = settings.Version_ODYSS;
      CurrentSettings.game = gameInstance.key;
      CurrentSettings.path = '';

      //4. WRITE THE NEW THEME FILES:
      fileHelper.writeJsonFile(path.join(themesPath, 'ThemeSettings.json'), CurrentSettings);
      fileHelper.base64ToJpg(Credits.preview, path.join(themesPath, `${Credits.theme}.jpg`));
      fileHelper.base64ToJpg(Credits.thumb, path.join(themesPath, 'PREVIEW.jpg'));

      if (fileHelper.checkFileExists(path.join(themesPath, 'ThemeSettings.json'))) {
        return true;
      } else {
        return false;
      }
    }
  } catch (error) {
    console.log(error);
    throw new Error(error.message + error.stack);
  }
}

/**
 * Updates the theme with the provided theme data.
 * 
 * @param {Object} themeData - The data for the theme to be updated.
 * @param {Object} themeData.credits - The credits information for the theme.
 * @param {string} themeData.credits.theme - The name of the theme.
 * @param {string} themeData.credits.author - The author of the theme.
 * @param {string} themeData.credits.description - The description of the theme.
 * @param {string} themeData.credits.preview - The base64 encoded preview image of the theme.
 * @param {string} themeData.credits.thumb - The base64 encoded thumbnail image of the theme.
 * 
 * @returns {Promise<boolean>} - Returns true if the theme was successfully updated, otherwise false.
 * 
 * @throws {Error} - Throws an error if the theme update process fails.
 */
async function UpdateTheme(themeData, source) {
  try {
    console.log('UpdateTheme: ', themeData.credits.theme);

    //1. RESOLVE THE THEMES PATH:
    const Credits = themeData.credits;
    const gameInstance = await settingsHelper.getActiveInstance();              //console.log('gameInstance: ', gameInstance);  
    const GameType = gameInstance.key === 'ED_Odissey' ? 'ODYSS' : 'HORIZ';     //console.log('GameType: ', GameType);  
    const settings = await settingsHelper.loadSettings();                       //console.log('settings: ', settings);    
    const dataPath = fileHelper.resolveEnvVariables(settings.UserDataFolder);   //console.log('dataPath: ', dataPath);     //<- %USERPROFILE%\EDHM_UI  
    const themesPath = path.join(dataPath, GameType, 'Themes', Credits.theme);  //console.log('themesPath: ', themesPath); //<- %USERPROFILE%\EDHM_UI\ODYSS\Themes\MyTheme   

    //2. CREATE THE NEW THEME FOLDER IF IT DOESNT EXIST:
    if (fileHelper.ensureDirectoryExists(themesPath)) {

      //3. LOAD THE CURRENTLY APPLIED THEME SETTINGS:
      const CurrentSettings = source;
      CurrentSettings.credits.theme = Credits.theme;
      CurrentSettings.credits.author = Credits.author;
      CurrentSettings.credits.description = Credits.description;
      CurrentSettings.version = settings.Version_ODYSS;
      CurrentSettings.game = gameInstance.key;
      CurrentSettings.path = '';

      //4. WRITE THE NEW THEME FILES:
      fs.writeFileSync(path.join(themesPath, 'ThemeSettings.json'), JSON.stringify(CurrentSettings, null, 4));

      if (fileHelper.checkFileExists(path.join(themesPath, 'ThemeSettings.json'))) {
        return true;
      } else {
        return false;
      }
    }
  } catch (error) {
    console.log(error);
    throw new Error(error.message + error.stack);
  }
}

/**
 * Saves Theme Changes directly into the 'ThemeSettings.json'
 * @param {*} themeData Data of the Theme
 * @returns 
 */
async function SaveTheme(themeData) {
  try {
    //1. RESOLVE THE THEMES PATH:
    const themesPath = themeData.path;
    themeData.path = '';

    //2. CREATE THE NEW THEME FOLDER IF IT DOESNT EXIST:
    if (fileHelper.ensureDirectoryExists(themesPath)) {

      //4. WRITE THE NEW THEME FILES:
      fileHelper.writeJsonFile(path.join(themesPath, 'ThemeSettings.json'), themeData);
      if (fileHelper.checkFileExists(path.join(themesPath, 'ThemeSettings.json'))) {
        return true;
      } else {
        return false;
      }
    }
  } catch (error) {
    console.log(error);
    throw new Error(error.message + error.stack);
  }
}

async function DeleteTheme(themePath) {
  try {
    return fileHelper.deleteFolderRecursive(themePath);
  } catch (error) {
    console.log(error);
    throw new Error(error.message + error.stack);
  }
}

/** Exports the given theme into a ZIP file 
 * @param {*} themeData Theme to Export
 * @returns 
 */
async function ExportTheme(themeData) { // 
  try {
    console.log('Exporting Theme .....');

    if (themeData && themeData.path) {
      //1. RESOLVE THE THEMES PATH:
      const ThemeName = themeData.credits.theme;
      const ThemePath = themeData.path;
      const TempPath = fileHelper.resolveEnvVariables(`%LOCALAPPDATA%\\Temp\\EDHM_UI\\${ThemeName}`);

      //2. CREATE THE NEW THEME FOLDER IF IT DOESNT EXIST:
      if (fileHelper.ensureDirectoryExists(TempPath)) {

        //3. COPY THE THEME FILES TO A TEMP FOLDER:
        const _ret = await fileHelper.copyFiles(ThemePath, TempPath, ['.jpg', '.json']); //<- 'PREVIEW.jpg', 'ThemeName.jpg', 'ThemeSettings.json'
        console.log(_ret + ' Files Copied.');

        //4. Ask the User for Destination Zip File:
        const options = {
          fileName: ThemeName,
          title: `Exporting Theme '${ThemeName}':`,
          defaultPath: path.join(app.getPath('desktop'), `${ThemeName}.zip`),
          filters: [
            { name: 'Zip Files', extensions: ['zip'] },
            { name: 'All Files', extensions: ['*'] }
          ],
          properties: ['createDirectory', 'showOverwriteConfirmation ', 'dontAddToRecent']
        };
        const Destination = await fileHelper.ShowSaveDialog(options);
        if (Destination) {
          console.log('Destination:', Destination);
          //5. COMPRESS THEME FILES:
          await fileHelper.compressFolder(TempPath, Destination);

          //6. Clean the Temp trash:
          await fileHelper.deleteFolderRecursive(TempPath);
          return true;
        }
        return false;
      }
    }
  } catch (error) {
    console.log(error);
    throw new Error(error.message + error.stack);
  }
}

// #region Ini File Handling

const getIniFilePath = (basePath, fileName) => {
  const joinedPath = path.join(basePath, fileName);
  return fileHelper.resolveEnvVariables(joinedPath);
};

/**  * Retrieve the INI files asociated to a Theme
 * @param {string} folderPath Full path to the Folder containing the INI files
 * @returns Object */
const LoadThemeINIs = async (folderPath) => {
  try {

    const [Startup_Profile, Advanced, SuitHud, XML_Profile] = await Promise.all([
      await INIparser.LoadIniFile(getIniFilePath(folderPath, 'Startup-Profile.ini')),
      await INIparser.LoadIniFile(getIniFilePath(folderPath, 'Advanced.ini')),
      await INIparser.LoadIniFile(getIniFilePath(folderPath, 'SuitHud.ini')),
      await INIparser.LoadIniFile(getIniFilePath(folderPath, 'XML-Profile.ini')),
    ]);

    return {
      path: folderPath,
      StartupProfile: Startup_Profile,
      Advanced: Advanced,
      SuitHud: SuitHud,
      XmlProfile: XML_Profile
    }

  } catch (error) {
    throw new Error(error.message + error.stack);
  }
};

/** * Save the modified INI files back to their original location
 * @param {string} folderPath Full path to the Folder containing the INI files
 * @param {object} themeINIs Object containing the INI data
 * @returns boolean
 */
const SaveThemeINIs = async (folderPath, themeINIs) => {
  try {
    await Promise.all([
      await INIparser.SaveIniFile(getIniFilePath(folderPath, 'Startup-Profile.ini'), themeINIs.StartupProfile),
      await INIparser.SaveIniFile(getIniFilePath(folderPath, 'Advanced.ini'), themeINIs.Advanced),
      await INIparser.SaveIniFile(getIniFilePath(folderPath, 'SuitHud.ini'), themeINIs.SuitHud),
      await INIparser.SaveIniFile(getIniFilePath(folderPath, 'XML-Profile.ini'), themeINIs.XmlProfile),
    ]);
    return true;

  } catch (error) {
    console.error('Error at ThemeHelper/SaveThemeINIs():', error);
    throw new Error(error.message + error.stack);
  }
};



// #endregion



// #region --------- Expose Methods via IPC Handlers: ---------------------
//  they can be accesed like this:   const files = await window.api.getThemes(dirPath);


ipcMain.handle('load-history', async (event, historyFolder, numberOfSavesToRemember) => {
  try {
    historyFolder = fileHelper.resolveEnvVariables(historyFolder);
    // Ensure History folder exists
    if (!fs.existsSync(historyFolder)) {
      fs.mkdirSync(historyFolder, { recursive: true });
    }

    // Read and sort .json files by modification date
    const files = fs.readdirSync(historyFolder)
      .filter(file => file.endsWith('.json'))
      .map(file => ({
        name: file,
        path: path.join(historyFolder, file),
        time: fs.statSync(path.join(historyFolder, file)).mtime.getTime()
      }))
      .sort((a, b) => b.time - a.time)
      .slice(0, numberOfSavesToRemember);

    return files.map(file => ({
      name: file.name,
      path: file.path,
      date: new Date(file.name.substring(0, 4), file.name.substring(4, 6) - 1, file.name.substring(6, 8), file.name.substring(8, 10), file.name.substring(10, 12), file.name.substring(12, 14)).toLocaleString()
    }));
  } catch (error) {
    console.error('Failed to load history elements:', error);
    Log.Error(error.message, error.stack);
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('save-history', async (event, historyFolder, theme) => {
  try {
    historyFolder = fileHelper.resolveEnvVariables(historyFolder);
    // Ensure History folder exists
    if (!fs.existsSync(historyFolder)) {
      fs.mkdirSync(historyFolder, { recursive: true });
    }

    // File with timestamp in the name
    const filePath = path.join(historyFolder, `${new Date().toISOString().replace(/[:.-]/g, '')}.json`);

    // Save the data in JSON format
    fs.writeFileSync(filePath, JSON.stringify(theme, null, 2));
    console.log('Theme added to history:', filePath);

    return true;
  } catch (error) {
    console.error('Failed to add theme to history:', error);
    Log.Error(error.message, error.stack);
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('get-themes', async (event, dirPath) => {
  try {
    const resolvedPath = fileHelper.resolveEnvVariables(dirPath);
    const files = await getThemes(resolvedPath);
    return files;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('LoadTheme', async (event, dirPath) => {
  try {
    const resolvedPath = fileHelper.resolveEnvVariables(dirPath);
    const template = await LoadTheme(resolvedPath);
    return template;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('LoadThemeINIs', async (event, folderPath) => {
  try {
    return LoadThemeINIs(folderPath);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('SaveThemeINIs', async (event, folderPath, themeINIs) => {
  try {
    return SaveThemeINIs(folderPath, themeINIs);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('reverseGammaCorrected', async (event, color, gammaValue) => {
  try {
    return Util.reverseGammaCorrected(color.r, color.g, color.b, color.a, gammaValue);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('GetGammaCorrected_RGBA', async (event, color, gammaValue) => {
  try {
    return Util.GetGammaCorrected_RGBA(color, gammaValue);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('intToRGBA', async (event, colorInt) => {
  try {
    return Util.intToRGBA(colorInt);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});


ipcMain.handle('apply-ini-values', async (event, template, iniValues) => {
  try {
    const updatedTemplate = await ApplyIniValuesToTemplate(template, iniValues);
    return updatedTemplate;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('ApplyTemplateValuesToIni', async (event, template, iniValues) => {
  try {
    const updatedTemplate = await ApplyTemplateValuesToIni(template, iniValues);
    return updatedTemplate;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('FavoriteTheme', async (event, theme) => {
  try {
    return FavoriteTheme(theme);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('UnFavoriteTheme', async (event, theme) => {
  try {
    return UnFavoriteTheme(theme);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('CreateNewTheme', async (event, credits) => {
  try {
    return CreateNewTheme(credits);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('UpdateTheme', async (event, theme, source) => {
  try {
    return UpdateTheme(theme, source);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});
ipcMain.handle('SaveTheme', async (event, theme) => {
  try {
    return SaveTheme(theme);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
}); 
ipcMain.handle('DeleteTheme', async (event, theme) => {
  try {
    return DeleteTheme(theme);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('ExportTheme', async (event, themeData) => {
  try {
    return ExportTheme(themeData);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('GetCurrentSettings', async (event, folderPath) => {
  try {
    return GetCurrentSettingsTheme(folderPath);
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('GetElementsImage', (event, key) => {
  try {
    const imageKey = key.replace(/\|/g, '_');
    const rawPath = FileHelper.getAssetPath(`images/Elements_ODY/${imageKey}.png`);
    const defaultImg = FileHelper.getAssetPath('images/Elements_ODY/empty.png');

    if (fs.existsSync(rawPath)) {
      return rawPath;// Return the image path if it exists

    } else {
      // If the image doesn't exist, return the default image path
      const defaultImagePath = new URL(defaultImg, import.meta.url).href;
      return defaultImg;
    }

  } catch (error) {
    console.error("Error in GetElementsImage:", error); // Log the error for debugging
    throw new Error(error.message + error.stack); // Re-throw the error to be handled by the caller
  }
});


// #endregion

export default {
  getThemes,
  LoadThemeINIs, SaveThemeINIs,
  ApplyIniValuesToTemplate, ApplyTemplateValuesToIni,
  FavoriteTheme, UnFavoriteTheme,
  CreateNewTheme, UpdateTheme,
  GetCurrentSettingsTheme,
  DeleteTheme,
};