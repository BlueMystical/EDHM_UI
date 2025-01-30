import { app, ipcMain } from 'electron';
import path from 'node:path';
import fs from 'fs';
import { copyFileSync, constants } from 'node:fs';
import ini from './IniHelper.js';
import Log from './LoggingHelper.js';
import fileHelper from './FileHelper';
import Util from './Utils.js';
import settingsHelper from './SettingsHelper.js'
import { writeFile } from 'node:fs/promises';


/** * Retrieves themes from a specified directory path.
 * @async
 * @param {string} dirPath - The directory path to search for themes.
 * @returns {Promise<Array>} A promise that resolves to an array of themes.
 */
const getThemes = async (dirPath) => {
  try {
    console.log('getThemes dirPath: ', dirPath);
    const subfolders = await fs.promises.readdir(dirPath, { withFileTypes: true });
    const files = []; //console.log('subfolders', subfolders);

    for (const dirent of subfolders) {
      if (dirent.isDirectory()) {
        const subfolderPath = path.join(dirPath, dirent.name);

        try {
            // Check if the folder contains a Large Preview file
            let preview_url = '';
            const template = await LoadTheme(subfolderPath);
            if (fileHelper.checkFileExists(path.join(subfolderPath, dirent.name + '.jpg'))) {
              preview_url = `file:///${path.join(subfolderPath, dirent.name + '.jpg')}`;
            }

            files.push({
              theme: template,
              path: subfolderPath,
              preview: preview_url,
              thumbnail: 'PREVIEW.jpg',              
              credits: template.credits,              
              name: template.credits.theme,
              isFavorite: template.isFavorite
            });

            // Theme Migration Tool:
            const ThemeCleansing = true; //<- Swap to 'true' to save the json and cleanse old files
            if (ThemeCleansing) {
              try {
                // Writes the JSON in the theme folder:
                if (!fileHelper.checkFileExists(path.join(subfolderPath, 'ThemeSettings.json'))) {
                  const JsonString = JSON.stringify(template, null, 4);
                  await writeFile(
                    path.join(subfolderPath, 'ThemeSettings.json'), 
                    JsonString, 
                    { encoding: "utf8", flag: 'w' }
                  );
                }
  
                // Sanitization: For Themes Exportings
               /* fileHelper.deleteFilesByType(subfolderPath, '.ini');
                fileHelper.deleteFilesByType(subfolderPath, '.credits');
                fileHelper.deleteFilesByType(subfolderPath, '.fav');*/
                //fileHelper.deleteFilesByType(subfolderPath, '.json'); //<- BEWARE !
  
              } catch (error) {
                console.error(error);
              } 
            }

        } catch (error) {
          Log.Error(error.message, error.stack);
          console.error(error);
        }
      }
    }
    return files;

  } catch (error) {
    throw error;
  }
};

/** Loads a Theme from a specified folder path. * 
 * @param {*} themeFolder Path to the folder containing the Theme files
 */
const LoadTheme = async (themeFolder) => {
  let template = {};
  try {
    if (fs.existsSync(path.join(themeFolder, 'ThemeSettings.json'))) {
      // New v3 Format for Themes, single File:
      template = fileHelper.loadJsonFile( path.join(themeFolder, 'ThemeSettings.json') );
      template.path = themeFolder;
      template.isFavorite = fileHelper.checkFileExists(path.join(themeFolder, 'IsFavorite.fav'));

    } else {
      // Old fashion format for Themes:
      const defaultThemePath = fileHelper.getAssetPath('./data/ODYSS/ThemeTemplate.json');  
      const ThemeINIs = await LoadThemeINIs(themeFolder); 

      template = fileHelper.loadJsonFile(defaultThemePath);
      template = await ApplyIniValuesToTemplate(template, ThemeINIs);      
      template.credits = await GetCreditsFile(themeFolder);
      template.path = themeFolder;
      template.isFavorite = fileHelper.checkFileExists(path.join(themeFolder, 'IsFavorite.fav'));
    }
  } catch (error) {
    throw error;
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
    throw error;
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
    throw error;
  }
};

/** Returns the Currently Applied Theme Settings as a ThemeTemplate
 * @param {*} themePath Full path to the Game Instance
 */
async function GetCurrentSettingsTheme(themePath) {
  try {
    const ThemeINIs = LoadThemeINIs(themePath);
    const defaultSettingsPath = fileHelper.getAssetPath('data/ODYSS/ThemeTemplate.json');

    let themeTemplate = fileHelper.loadJsonFile(defaultSettingsPath);
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
    console.log(error);
    throw error;    
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
    throw error;
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
async function UpdateTheme(themeData) {
  try {
    //console.log('credits: ', credits);  

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
      const CurrentSettings = await GetCurrentSettingsTheme(path.join(gameInstance.path, 'EDHM-ini'));
      CurrentSettings.credits.theme = Credits.theme;
      CurrentSettings.credits.author = Credits.author;
      CurrentSettings.credits.description = Credits.description;      
      CurrentSettings.version = settings.Version_ODYSS; 
      CurrentSettings.game = gameInstance.key;
      CurrentSettings.path = '';  

      //4. WRITE THE NEW THEME FILES:
      fileHelper.writeJsonFile(path.join(themesPath, 'ThemeSettings.json'), CurrentSettings);
      //fileHelper.base64ToJpg(Credits.preview, path.join(themesPath, `${Credits.theme}.jpg`));      
      //fileHelper.base64ToJpg(Credits.thumb, path.join(themesPath, 'PREVIEW.jpg'));

      if (fileHelper.checkFileExists(path.join(themesPath, 'ThemeSettings.json'))) {
        return true;
      } else {
        return false;
      }      
    }
  } catch (error) {
    console.log(error);
    throw error;
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
    throw error;
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
        copyFileSync(path.join(ThemePath, `${ThemeName}.jpg`),    path.join(TempPath, `${ThemeName}.jpg`));
        copyFileSync(path.join(ThemePath, 'PREVIEW.jpg'),         path.join(TempPath, 'PREVIEW.jpg'));
        copyFileSync(path.join(ThemePath, 'ThemeSettings.json'),  path.join(TempPath, 'ThemeSettings.json'));    
        
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
        let Destination = '';
        await fileHelper.ShowSaveDialog(options).then(filePath => {
          if (filePath) {
            Destination = filePath;
          }
        });
        console.log('Destination: ', Destination);

        //5. COMPRESS THEME FILES:
        await fileHelper.compressFolder(TempPath, Destination);

        //6. Clean the Temp trash:
        fileHelper.deleteFolderRecursive(TempPath);

        return true;  
      }
    }
  } catch (error) {
    console.log(error);
    throw error;
  }
}

// #region Ini File Handling

const getIniFilePath = (basePath, fileName) => {
  const joinedPath = path.join(basePath, fileName);
  return fileHelper.resolveEnvVariables(joinedPath);
};

/**  * Retrieve the INI files asociated to a Theme
 * @param {string} folderPath Full path to the Folder containing the INI files
 * @returns Object
 */
const LoadThemeINIs = async (folderPath) => {
  try {

    //console.log('Loading Inis from: ', folderPath);
    const [Startup_Profile, Advanced, SuitHud, XML_Profile] = await Promise.all([
      await ini.loadIniFile(getIniFilePath(folderPath, 'Startup-Profile.ini')),
      await ini.loadIniFile(getIniFilePath(folderPath, 'Advanced.ini')),
      await ini.loadIniFile(getIniFilePath(folderPath, 'SuitHud.ini')),
      await ini.loadIniFile(getIniFilePath(folderPath, 'XML-Profile.ini')),
    ]);

    return {
      path: folderPath,
      StartupProfile: Startup_Profile,
      Advanced: Advanced,
      SuitHud: SuitHud,
      XmlProfile: XML_Profile
    }

  } catch (error) {
    throw error;
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
      await ini.saveIniFile(getIniFilePath(folderPath, 'Startup-Profile.ini'), themeINIs.StartupProfile),
      await ini.saveIniFile(getIniFilePath(folderPath, 'Advanced.ini'), themeINIs.Advanced),
      await ini.saveIniFile(getIniFilePath(folderPath, 'SuitHud.ini'), themeINIs.SuitHud),
      await ini.saveIniFile(getIniFilePath(folderPath, 'XML-Profile.ini'), themeINIs.XmlProfile),
    ]);
    return true;

  } catch (error) {
    console.error('Error at ThemeHelper/SaveThemeINIs():', error);
    throw error;
  }
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
            const foundValue = ini.findValueByKey(iniValues, element);
            /* foundValue: [
                 { key: 'x127', value: '0.063' },
                 { key: 'y127', value: '0.7011' },
                 { key: 'z127', value: '1' }
               ] 
                 or
               foundValue: 100.0 */

            if (foundValue != null && foundValue != undefined) {
              /*
              if (iniKey === 'x87') {
                console.log('Key: x87', 'Default:', defaultValue, 'Found:', foundValue);
              }*/

              if (Array.isArray(foundValue) && foundValue.length > 0) {
                
                const colorKeys = iniKey.split("|"); //<- iniKey === "x159|y159|z159" or "x159|y155|z153|w200"
                const colorComponents = colorKeys.map(key => {
                  const foundValueObj = foundValue.find(obj => obj.key === key);
                  return foundValueObj ? foundValueObj.value : undefined;
                }); //<- colorComponents: [ '0.063', '0.7011', '1' ]

                if (colorComponents != undefined && !colorComponents.includes(undefined)) {
                  const color = Util.reverseGammaCorrectedList(colorComponents); //<- color: { r: 81, g: 220, b: 255, a: 255 }
                  element.Value = Util.getColorDecimalValue(color);
                } else {                  
                  element.Value = parseFloat(defaultValue);
                  console.log('Color Conversion Error:', path.join(element.File, element.Section, element.Key), 'Val: ', element.Value);
                  //Log.Warning('Key Not Found:', path.join(element.File, element.Section, element.Key));
                }

              } else {
                // No hay Multi-Keys, foundValue: 100.0
                element.Value = parseFloat(foundValue ?? defaultValue)
                if (foundValue === null || foundValue === undefined) {
                  console.log('No Value?:', path.join(element.File, element.Section, element.Key), 'Val: ', element.Value);
                }
              }
            } else {
              element.Value = parseFloat(defaultValue);
              //console.log('Key Not Found:', path.join(element.File, element.Section, element.Key), 'Val: ', element.Value);
              //Log.Warning('Key Not Found:', path.join(element.File, element.Section, element.Key));
            }
          }
        }
      }
    }

    // Handle xml_profile (if it exists)
    if (template.xml_profile && iniValues.XmlProfile && iniValues.XmlProfile.constants) {
      for (const xmlProfileEntry of template.xml_profile) {
        if (iniValues.XmlProfile.constants[xmlProfileEntry.key]) {
          try {
            const iniValue = iniValues.XmlProfile.constants[xmlProfileEntry.key];
            if (!isNaN(parseFloat(iniValue))) {
              xmlProfileEntry.value = parseFloat(iniValue);
            }
          } catch (error) {
            console.error(`Error parsing value for xml_profile - ${xmlProfileEntry.key}:`, error);
          }
        }
      }
    }
  } catch (error) {
    throw error;
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

            stackTrace = path.join(element.File, element.Section, element.Key) + ' ';

            if (iniKey.includes('|')) {
              //Multi Key
              const keys = element.Key.split('|');  //<- iniKey === "x159|y159|z159" or "x159|y155|z153|w200"
              if (Array.isArray(keys) && keys.length > 2) {
                const RGBAcolor = Util.intToRGBA(element.Value); //<- color: { r: 81, g: 220, b: 255, a: 255 }
                const sRGBcolor = Util.GetGammaCorrected_RGBA(RGBAcolor);
                // Map keys to values 
                const values = [sRGBcolor.r, sRGBcolor.g, sRGBcolor.b, sRGBcolor.a];
                keys.forEach((key, index) => {
                  iniValues[fileName][section][key] = parseFloat(values[index]);
                });
              }
            } else {
              //Single Key:
              iniValues[fileName][section][iniKey] = parseFloat(element.Value);
            }
          }
        }
      }
    }

    // Handle xml_profile (if it exists)
    if (template.xml_profile && iniValues.XmlProfile && iniValues.XmlProfile.Constants) {
      console.log('We have XML');
      for (const element of template.xml_profile) {
        stackTrace = path.join('XmlProfile', 'Constants', element.key) + ' ';
        const key = element.key;
        iniValues.XmlProfile.Constants[key] = parseFloat(element.value);
      }
    }

  } catch (error) {
    throw new Error('At ThemeHelper.js/ApplyTemplateValuesToIni(): ' + stackTrace + error.message);
  }
  return iniValues;
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
    throw error;
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
    throw error;
  }
});

ipcMain.handle('get-themes', async (event, dirPath) => {
  try {
    const resolvedPath = fileHelper.resolveEnvVariables(dirPath);
    const files = await getThemes(resolvedPath);
    return files;
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('LoadTheme', async (event, dirPath) => {
  try {
    const resolvedPath = fileHelper.resolveEnvVariables(dirPath);
    const template = await LoadTheme(resolvedPath);
    return template;
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('LoadThemeINIs', async (event, folderPath) => {
  try {
    return LoadThemeINIs(folderPath);
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('SaveThemeINIs', async (event, folderPath, themeINIs) => {
  try {
    return SaveThemeINIs(folderPath, themeINIs);
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('reverseGammaCorrected', async (event, color, gammaValue) => {
  try {
    return Util.reverseGammaCorrected(color.r, color.g, color.b, color.a, gammaValue);
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('GetGammaCorrected_RGBA', async (event, color, gammaValue) => {
  try {
    return Util.GetGammaCorrected_RGBA(color, gammaValue);
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('intToRGBA', async (event, colorInt) => {
  try {
    return Util.intToRGBA(colorInt);
  } catch (error) {
    throw error;
  }
});


ipcMain.handle('apply-ini-values', async (event, template, iniValues) => {
  try {
    const updatedTemplate = await ApplyIniValuesToTemplate(template, iniValues);
    return updatedTemplate;
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('ApplyTemplateValuesToIni', async (event, template, iniValues) => {
  try {
    const updatedTemplate = await ApplyTemplateValuesToIni(template, iniValues);
    return updatedTemplate;
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('FavoriteTheme', async (event, theme) => {
  try {
    return FavoriteTheme(theme);
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('UnFavoriteTheme', async (event, theme) => {
  try {
    return UnFavoriteTheme(theme);
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('CreateNewTheme', async (event, credits) => {
  try {
    return CreateNewTheme(credits);
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('UpdateTheme', async (event, theme) => {
  try {
    return UpdateTheme(theme);
  } catch (error) {
    throw error;
  }
}); 
ipcMain.handle('SaveTheme', async (event, theme) => {
  try {
    return SaveTheme(theme);
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('ExportTheme', async (event, themeData) => {
  try {
    return ExportTheme(themeData);
  } catch (error) {
    throw error;
  }
});

ipcMain.handle('GetCurrentSettings', async (event, folderPath) => {
  try {
    return GetCurrentSettingsTheme(folderPath);
  } catch (error) {
    throw error;
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
  
};