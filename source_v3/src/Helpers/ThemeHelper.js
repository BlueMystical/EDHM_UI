import { app, ipcMain } from 'electron';
import path from 'node:path';
import fs from 'fs';
//import { copyFileSync, constants } from 'node:fs';
import INIparser from './IniParser.js';
import Log from './LoggingHelper.js';
import Util from './Utils.js';
import settingsHelper from './SettingsHelper.js'
import { writeFile } from 'node:fs/promises';
import FileHelper from './FileHelper';

let LoadedThemes = null; //<- Cache for the loaded themes
// #region --------- Theme Helper Functions: ---------------------  


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
          if (FileHelper.checkFileExists(path.join(subfolderPath, dirent.name + '.jpg'))) {
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
          if (!FileHelper.checkFileExists(path.join(subfolderPath, 'ThemeSettings.json'))) {
            const JsonString = JSON.stringify(template, null, 4);
            await writeFile(
              path.join(subfolderPath, 'ThemeSettings.json'),
              JsonString,
              { encoding: "utf8", flag: 'w' }
            );
          }

          LoadedThemes = files; //<- Cache the loaded themes

          // Theme Migration :
          const ThemeCleansing = true; //<- Swap to 'true' to save the json and cleanse old files
          try {
            if (ThemeCleansing) {
              // Sanitization: For Themes Exportings
              //FileHelper.deleteFilesByType(subfolderPath, '.ini');
              //FileHelper.deleteFilesByType(subfolderPath, '.credits');
              //FileHelper.deleteFilesByType(subfolderPath, '.fav');
              //FileHelper.deleteFilesByType(subfolderPath, '.json'); //<- BEWARE !
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

function GetLoadedThemes() {
  return LoadedThemes;
}

/** Loads a Theme from a specified folder path.
 * @param {*} themeFolder Path to the folder containing the Theme files */
const LoadTheme = async (themeFolder) => {
  let template = {};
  try {
    const templatePath = FileHelper.getAssetPath('data/ODYSS/ThemeTemplate.json'); //<- Default Template
    template = await FileHelper.loadJsonFile(templatePath);
    const themeJSON = path.join(themeFolder, 'ThemeSettings.json');

    if (fs.existsSync(themeJSON)) {
      // New v3 Format for Themes, single File JSON:
      const themeData = await FileHelper.loadJsonFile(themeJSON);

      // Create a new object by merging themeData into the template
      const mergedData = { ...template };
      for (const key in themeData) {
        if (mergedData.hasOwnProperty(key) && key !== 'Presets') { // Only update existing properties, exclude 'Presets'
          mergedData[key] = themeData[key];
        }
      }

      template = {
        ...mergedData,
        path: themeFolder,
        isFavorite: FileHelper.checkFileExists(path.join(themeFolder, 'IsFavorite.fav')),
      };

    } else {
      // Old fashion format for Themes, Multiple INI files:
      const ThemeINIs = await LoadThemeINIs(themeFolder);

      template.credits = await GetCreditsFile(themeFolder);
      template = await ApplyIniValuesToTemplate(template, ThemeINIs);
      template.path = themeFolder;
      template.isFavorite = FileHelper.checkFileExists(path.join(themeFolder, 'IsFavorite.fav'));
    }
    //console.log('Loaded Theme:', template.credits.theme, 'from', template.ui_groups[2].Elements[8]);
  } catch (error) {
    throw new Error(`Error loading theme from ${themeFolder}: ${error.message}\n${error.stack}`);
  }
  return template;
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
          theme: themeName,
          author: "Unknown",
          description: "** .Credits file missing or Invalid **",
          preview: ""
        };
      }
    }
    else {
      // in case there is no credits file:
      creditsJson = {
        theme: "Current Settings", 
        author: "You",
        description: "** THESE ARE THE CURRENTLY APPLIED COLORS **",
        preview: ""                     
      };
    }

  } catch (error) {
    console.log(error);
  }
  return creditsJson;
};

/** Marks the given theme as a Favorite * 
 * @param {*} themePath Absolute path to the Theme */
async function FavoriteTheme(themePath) {
  try {
    const dummy = { isFavorite: true };
    const favFilePath = path.join(themePath, 'IsFavorite.fav');
    const _ret = FileHelper.writeJsonFile(favFilePath, dummy, false);
    return _ret;
  } catch (error) {
    console.log(error);
    throw new Error(error.message + error.stack);
  }
};
/** Removes the given theme from Favorites * 
 * @param {*} themePath Absolute path to the Theme */
async function UnFavoriteTheme(themePath) {
  try {
    const favFilePath = path.join(themePath, 'IsFavorite.fav');
    const _ret = FileHelper.deleteFileByAbsolutePath(favFilePath);
    return _ret;
  } catch (error) {
    console.log(error);
    throw new Error(error.message + error.stack);
  }
};

/** Returns the Currently Applied Theme Settings as a ThemeTemplate
 * @param {*} themePath Full path to the Game Instance */
async function GetCurrentSettingsTheme(themePath) {
  try {
    const ThemeINIs = await LoadThemeINIs(themePath);
    //console.log('Default INIs: ', ThemeINIs);
    // Load the default template from the assets folder
    const defaultSettingsPath = FileHelper.getAssetPath('data/ODYSS/ThemeTemplate.json');

    let themeTemplate = await FileHelper.loadJsonFile(defaultSettingsPath);
    themeTemplate.credits = {
      theme: "Current Settings",
      author: "User",
      description: "Currently Applied Colors in Game",
      preview: "",
      path: themePath
    };
    themeTemplate = await ApplyIniValuesToTemplate(themeTemplate, ThemeINIs);
    themeTemplate.name = "Current Settings";

    return themeTemplate;

  } catch (error) {
    console.log(error.message + error.stack);
    throw new Error(error.message + error.stack);
  }
  return null;
};

/** Makes a new Theme, saved on the Themes Folder
 * @param {*} credits Meta-data for the new theme
 * @returns true is success */
async function CreateNewTheme(credits) {
  try {
    //console.log('credits: ', credits);  //<- credits: { theme: 'ThemeName', author: '', description: '', preview: 'Base64image', thumb: 'Base64image' }

    //1. RESOLVE THE THEMES PATH:
    const Credits = credits.credits;
    const gameInstance = await settingsHelper.getActiveInstance();              //console.log('gameInstance: ', gameInstance);  
    const GameType = gameInstance.key === 'ED_Odissey' ? 'ODYSS' : 'HORIZ';     //console.log('GameType: ', GameType);  
    const settings = await settingsHelper.loadSettings();                       //console.log('settings: ', settings);    
    const dataPath = FileHelper.resolveEnvVariables(settings.UserDataFolder);   //console.log('dataPath: ', dataPath);     //<- %USERPROFILE%\EDHM_UI  
    const themesPath = path.join(dataPath, GameType, 'Themes', Credits.theme);  //console.log('themesPath: ', themesPath); //<- %USERPROFILE%\EDHM_UI\ODYSS\Themes\MyTheme   

    //2. CREATE THE NEW THEME FOLDER IF IT DOESNT EXIST:
    if (FileHelper.ensureDirectoryExists(themesPath)) {

      //3. LOAD THE CURRENTLY APPLIED THEME SETTINGS:
      //const CurrentSettings = await GetCurrentSettingsTheme(path.join(gameInstance.path, 'EDHM-ini'));
      const CurrentSettings = await LoadTheme(path.join(gameInstance.path, 'EDHM-ini'));
      CurrentSettings.credits.theme = Credits.theme;
      CurrentSettings.credits.author = Credits.author;
      CurrentSettings.credits.description = Credits.description;
      CurrentSettings.version = settings.Version_ODYSS;
      CurrentSettings.game = gameInstance.key;
      CurrentSettings.path = '';

      //4. WRITE THE NEW THEME FILES:
      FileHelper.writeJsonFile(path.join(themesPath, 'ThemeSettings.json'), CurrentSettings);
      FileHelper.base64ToJpg(Credits.preview, path.join(themesPath, `${Credits.theme}.jpg`));
      FileHelper.base64ToJpg(Credits.thumb, path.join(themesPath, 'PREVIEW.jpg'));

      if (FileHelper.checkFileExists(path.join(themesPath, 'ThemeSettings.json'))) {
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

/** Updates the theme with the provided theme data.
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
 * @throws {Error} - Throws an error if the theme update process fails. */
async function UpdateTheme(themeData, source) {
  try {
    console.log('UpdateTheme: ', themeData.credits.theme);

    //1. RESOLVE THE THEMES PATH:
    const Credits = themeData.credits;
    const gameInstance = await settingsHelper.getActiveInstance();              //console.log('gameInstance: ', gameInstance);  
    const GameType = gameInstance.key === 'ED_Odissey' ? 'ODYSS' : 'HORIZ';     //console.log('GameType: ', GameType);  
    const settings = await settingsHelper.loadSettings();                       //console.log('settings: ', settings);    
    const dataPath = FileHelper.resolveEnvVariables(settings.UserDataFolder);   //console.log('dataPath: ', dataPath);     //<- %USERPROFILE%\EDHM_UI  
    const themesPath = path.join(dataPath, GameType, 'Themes', Credits.theme);  //console.log('themesPath: ', themesPath); //<- %USERPROFILE%\EDHM_UI\ODYSS\Themes\MyTheme   

    //2. CREATE THE NEW THEME FOLDER IF IT DOESNT EXIST:
    if (FileHelper.ensureDirectoryExists(themesPath)) {

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

      if (FileHelper.checkFileExists(path.join(themesPath, 'ThemeSettings.json'))) {
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

/** Saves Theme Changes directly into the 'ThemeSettings.json'
 * @param {*} themeData Data of the Theme */
async function SaveTheme(themeData) {
  try {
    //1. RESOLVE THE THEMES PATH:
    const themesPath = themeData.path;
    themeData.path = '';

    //2. CREATE THE NEW THEME FOLDER IF IT DOESNT EXIST:
    if (FileHelper.ensureDirectoryExists(themesPath)) {

      //4. WRITE THE NEW THEME FILES:
      FileHelper.writeJsonFile(path.join(themesPath, 'ThemeSettings.json'), themeData);
      if (FileHelper.checkFileExists(path.join(themesPath, 'ThemeSettings.json'))) {
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
    return FileHelper.deleteFolderRecursive(themePath);
  } catch (error) {
    console.log(error);
    throw new Error(error.message + error.stack);
  }
}

/** Exports the given theme into a ZIP file 
 * @param {*} themeData Theme to Export */
async function ExportTheme(themeData) { // 
  try {
    console.log('Exporting Theme .....');

    if (themeData && themeData.path) {
      //1. RESOLVE THE THEMES PATH:
      const ThemeName = themeData.credits.theme;
      const ThemePath = themeData.path;
      const TempPath = FileHelper.resolveEnvVariables(`%LOCALAPPDATA%\\Temp\\EDHM_UI\\${ThemeName}`);

      //2. CREATE THE NEW THEME FOLDER IF IT DOESNT EXIST:
      if (FileHelper.ensureDirectoryExists(TempPath)) {

        //3. COPY THE THEME FILES TO A TEMP FOLDER:
        const _ret = await FileHelper.copyFiles(ThemePath, TempPath, ['.jpg', '.json']); //<- 'PREVIEW.jpg', 'ThemeName.jpg', 'ThemeSettings.json'
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
        const Destination = await FileHelper.ShowSaveDialog(options);
        if (Destination) {
          console.log('Destination:', Destination);
          //5. COMPRESS THEME FILES:
          await FileHelper.compressFolder(TempPath, Destination);

          //6. Clean the Temp trash:
          await FileHelper.deleteFolderRecursive(TempPath);
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

/** Reads the data from the ini file and applies it to the JSON data.
 * @param {*} template Data of the Theme template
 * @param {*} iniValues Values from the Ini file 
 * @returns template data with ini data applied */
async function ApplyIniValuesToTemplate(template, iniValues) {
  try {
    if (Array.isArray(template.ui_groups) && template.ui_groups.length > 0) {
      for (const group of template.ui_groups) {
        if (group.Elements != null) {
          for (const element of group.Elements) {
            /*element: {
              ..
              File: 'Startup-Profile',  <- 'Startup-Profile', 'Advanced', 'SuitHud', 'XML-Profile'
              Section: 'Constants',     
              Key: 'x137',              <- 'x157' or 'x159|y159|z159' or 'x159|y155|z153|w200'
              Value: 100,
              ..
            }*/
            const iniSection = element.Section;   //<- iniSection === 'constants'
            const iniKey = element.Key;                         //<- 'x157' or 'x159|y159|z159' or 'x159|y155|z153|w200'
            const defaultValue = element.Value;                 //<- 100.0             
            const iniFileName = element.File.replace(/-/g, ''); //<- Remove hyphens
            const iniData = iniValues[iniFileName];             //<- 'StartupProfile', 'Advanced', 'SuitHud', 'XmlProfile'

            if (iniData) {
              try {
                const colorKeys = iniKey.split('|');            //<-  colorKeys [ 'x232', 'y232', 'z232' ]  OR [ 'x204', 'y204', 'z204', 'w204' ]
                if (Array.isArray(colorKeys) && colorKeys.length > 2) {
                  //- Multi Key: Colors
                  let colorComponents = [];
                  for (const [index, rgbKey] of colorKeys.entries()) {
                    const iniValue = INIparser.getKey(iniData, iniSection, rgbKey);
                    if (iniValue != undefined) {
                      colorComponents.push(iniValue);           //<- colorComponents: [ '0.063', '0.7011', '1' ]
                    } else {
                      console.log(`404 - Ini Value Not Found: '${template.credits.theme}/${iniFileName}/${iniSection}/${rgbKey}'`);
                    }
                  }
                  if (colorComponents != undefined && !colorComponents.includes(undefined) && colorComponents.length > 0) {
                    const color = Util.reverseGammaCorrectedList(colorComponents); //<- color: { r: 81, g: 220, b: 255, a: 255 }
                    element.Value = parseFloat(Util.rgbaToInt(color).toFixed(1));
                  }
                } else {
                  //- Single Key: Text, Numbers, etc.
                  const iniValue = INIparser.getKey(iniData, iniSection, iniKey);
                  if (iniValue != undefined) {
                    element.Value = parseFloat(iniValue ?? defaultValue);
                  } else {
                    console.log(`404 - Ini Value Not Found: '${template.credits.theme}/${iniFileName}/${iniSection}/${iniKey}'`);
                  }                  
                }
              } catch (error) {
                console.log('Error:', error);
              }
            }
          }
        }
      }
    }

    // Update the XMLs:
    if (template.xml_profile && iniValues.XmlProfile) {
      const iniData = iniValues.XmlProfile;  
      //console.log('iniData: ', iniData);
      //console.log('template.xml_profile: ', template.xml_profile);
      for (const element of template.xml_profile) {
        try {
          const defaultValue = element.value;
          const iniValue = INIparser.getKey(iniData, 'Constants', element.key); 
          //console.log('iniValue: ', iniValue, 'defaultValue: ', defaultValue);
          element.value = parseFloat(iniValue ?? defaultValue);
        } catch (error) {
          console.log(error);
        }
      }
    }
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
  return template;
};

/** Writes the values from the template to the ini files.
 * @param {*} template Data of the Theme template
 * @param {*} iniValues Values from the Ini file 
 * @returns the iniValues with the new values applied */
async function ApplyTemplateValuesToIni(template, iniValues) {
  let stackTrace = '';
  try {
    console.log('Applying ' + template.credits.theme);
    if (Array.isArray(template.ui_groups) && template.ui_groups.length > 0) {
      for (const group of template.ui_groups) {
        if (group.Elements != null) {
          for (const element of group.Elements) {
            /*element: {
              ..
              File: 'Startup-Profile',  <- 'Startup-Profile', 'Advanced', 'SuitHud', 'XML-Profile'
              Section: 'Constants',     
              Key: 'x137',              <- 'x157' or 'x159|y159|z159' or 'x159|y155|z153|w200'
              Value: 100,
              ..
            }*/
            const iniSection = element.Section.toLowerCase();   //<- iniSection === 'constants'
            const iniKey = element.Key;                         //<- 'x157' or 'x159|y159|z159' or 'x159|y155|z153|w200'
            const defaultValue = element.Value;                 //<- 100.0             
            const iniFileName = element.File.replace(/-/g, ''); //<- Remove hyphens
            const iniData = iniValues[iniFileName];             //<- 'StartupProfile', 'Advanced', 'SuitHud', 'XmlProfile'

            if (iniData) {
              const colorKeys = iniKey.split('|');            //<-  colorKeys [ 'x232', 'y232', 'z232' ]  OR [ 'x204', 'y204', 'z204', 'w204' ]

              if (Array.isArray(colorKeys) && colorKeys.length > 2) {
                //- Multi Key: Colors
                const RGBAcolor = Util.intToRGBA(element.Value); //<- color: { r: 81, g: 220, b: 255, a: 255 }
                const sRGBcolor = Util.GetGammaCorrected_RGBA(RGBAcolor);
                const values = [sRGBcolor.r, sRGBcolor.g, sRGBcolor.b, sRGBcolor.a]; //<- [ 0.082, 0.716, 1.0, 1.0 ]

                colorKeys.forEach((key, index) => {
                  const value = parseFloat(values[index]);
                  try {
                    const _ret = INIparser.setKey(iniData, iniSection, key, value);
                    if (_ret) {
                      iniValues[iniFileName] = _ret;
                    }
                    else {
                      console.log(`404 - Ini Value Not Found*: ${template.credits.theme}/${iniFileName}/${iniSection}/${key}`);
                    }
                  } catch (error) {
                    console.log(stackTrace + value, error.message);
                  }
                });

              } else {
                //- Single Key: Text, Numbers, etc.
                const iniValue = INIparser.setKey(iniData, iniSection, iniKey, defaultValue);
                if (iniValue) {
                  iniValues[iniFileName] = iniValue;
                } else {
                  console.log('404 - Ini Value Not Found-: ', iniFileName, iniSection, iniKey, defaultValue);
                }                
              }
            }
          }
        }
      }

      // Update the XMLs:
      if (template.xml_profile && iniValues.XmlProfile) {
        const iniData = iniValues.XmlProfile;
        //console.log('iniData: ', iniData);
        //console.log('template.xml_profile: ', template.xml_profile);

        for (const element of template.xml_profile) {
          try {
            const _ret = INIparser.setKey(iniData, 'Constants', element.key, element.value);
            if (_ret) {
              iniValues.XmlProfile = _ret;
            } else {
              console.log('404 - Not Found: ', 'xml_profile', 'Constants', element.key, element.value);
            }
          } catch (error) {
            console.log(error);
          }
        }
      }
    }
  } catch (error) {
    throw new Error('At ThemeHelper.js/ApplyTemplateValuesToIni(): ' + stackTrace + error.message);
  }
  return iniValues;
};

const getIniFilePath = (basePath, fileName) => {
  const joinedPath = path.join(basePath, fileName);
  return FileHelper.resolveEnvVariables(joinedPath);
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
 * @returns boolean 'true' is save is successful. */
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

/** Uncompress a ZIP file containing a Theme into the Themes folder * 
 * @param {*} zip_path Full path to the ZIP file
 * @returns 'true' is success. */
async function ImportTheme(zip_path) {
  try {
    if (zip_path != undefined && FileHelper.checkFileExists(zip_path)) {      
      const ThemeName = path.basename(zip_path, '.zip');  
      console.log('Importing Theme .....', ThemeName);
      
      const ActiveInstance = await settingsHelper.getActiveInstance();
      const GameType = ActiveInstance.key === 'ED_Odissey' ? 'ODYSS' : 'HORIZ';
      const DataPath = FileHelper.resolveEnvVariables(
            settingsHelper.readSetting('UserDataFolder', '%USERPROFILE%\\EDHM_UI') );      
      const themes_path = path.join(DataPath, GameType, 'Themes');
      FileHelper.ensureDirectoryExists(themes_path);

      const _ret = await FileHelper.decompressFile(zip_path, themes_path);
      if (_ret) {
        console.log('Theme Installed ->', ThemeName);
        return _ret;
      }
    }
  } catch (error) {
    console.error('Error at ThemeHelper/ImportTheme():', error);
    throw new Error(error.message + error.stack);    
  }
}

// #endregion



// #region --------- Expose Methods via IPC Handlers: ---------------------
//  they can be accesed like this:   const files = await window.api.getThemes(dirPath);

ipcMain.handle('load-history', async (event, historyFolder, numberOfSavesToRemember) => {
  try {
    historyFolder = FileHelper.resolveEnvVariables(historyFolder);
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
    historyFolder = FileHelper.resolveEnvVariables(historyFolder);
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
    const resolvedPath = FileHelper.resolveEnvVariables(dirPath);
    const files = await getThemes(resolvedPath);
    return files;
  } catch (error) {
    throw new Error(error.message + error.stack);
  }
});

ipcMain.handle('LoadTheme', async (event, dirPath) => {
  try {
    const resolvedPath = FileHelper.resolveEnvVariables(dirPath);
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

ipcMain.handle('ImportTheme', async (event, theme_path) => {
  try {
    return ImportTheme(theme_path);
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
    const jpgFilePath = FileHelper.getAssetPath(`images/Elements_ODY/${imageKey}.jpg`);
    const pngFilePath = FileHelper.getAssetPath(`images/Elements_ODY/${imageKey}.png`);
    const defaultImg = FileHelper.getAssetPath('images/Elements_ODY/empty.png');

    if (fs.existsSync(jpgFilePath)) {
      return jpgFilePath; // Return the JPG image path if it exists
    } else if (fs.existsSync(pngFilePath)) {
      return pngFilePath; // Return the PNG image path if it exists
    } else {
      // If neither JPG nor PNG exists, return the default image path
      //const defaultImagePath = new URL(defaultImg, import.meta.url).href;
      return defaultImg;
    }

  } catch (error) {
    console.error("Error in GetElementsImage:", error); // Log the error for debugging
    throw new Error(error.message + error.stack); // Re-throw the error to be handled by the caller
  }
});
ipcMain.handle('GetElementsImageTPM', (event, filePath, key) => {
  try {
    const imageKey = key.replace(/\|/g, '_');
    const jpgFilePath = path.join(filePath, 'assets', `${imageKey}.jpg`);
    const pngFilePath = path.join(filePath, 'assets', `${imageKey}.png`);
    const defaultImg = FileHelper.getAssetPath('images/Elements_ODY/empty.png');

    if (fs.existsSync(jpgFilePath)) {
      return jpgFilePath; // Return the JPG image path if it exists
    } else if (fs.existsSync(pngFilePath)) {
      return pngFilePath; // Return the PNG image path if it exists
    } else {
      // If neither JPG nor PNG exists, return the default image path
      //const defaultImagePath = new URL(defaultImg, import.meta.url).href;
      return defaultImg;
    }
  } catch (error) {
    console.error("Error in GetElementsImage:", error);
    throw new Error(error.message + error.stack);
  }
});


// #endregion

export default {
  getThemes, GetLoadedThemes,
  LoadThemeINIs, SaveThemeINIs, SaveTheme,
  ApplyIniValuesToTemplate, ApplyTemplateValuesToIni,
  FavoriteTheme, UnFavoriteTheme,
  CreateNewTheme, UpdateTheme,
  GetCurrentSettingsTheme,
  DeleteTheme, ImportTheme,
};