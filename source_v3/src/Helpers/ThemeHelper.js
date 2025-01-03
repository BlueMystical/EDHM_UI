import { ipcMain } from 'electron';
import path from 'node:path';
import fs from 'fs';

import ini from './IniHelper.js';
import fileHelper from './FileHelper';
import Log from './LoggingHelper.js';
import { writeFile } from 'node:fs/promises';


/**
 * Retrieves themes from a specified directory path.
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
            const template = await LoadTheme(subfolderPath);

            files.push({
              path: subfolderPath,
              thumbnail: 'PREVIEW.jpg',
              credits: template.credits,
              theme: template
            });

            // Theme Migration Tool:
            const ThemeCleansing = false; //<- Swap to 'true' to save the json and cleanse old files
            if (ThemeCleansing) {
              try {
                // Writes the JSON in the theme folder:
  
                const JsonString = JSON.stringify(template, null, 4);
                await writeFile(
                  path.join(subfolderPath, 'ThemeSettings.json'), 
                  JsonString, 
                  { encoding: "utf8", flag: 'w' }
                );
  
                // Sanitization:
                fileHelper.deleteFilesByType(subfolderPath, '.ini');
                fileHelper.deleteFilesByType(subfolderPath, '.credits');
                //fileHelper.deleteFilesByType(subfolderPath, '.json');
  
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
}

const LoadTheme = async (themeFolder) => {
  let template = {};
  try {
    if (fs.existsSync(path.join(themeFolder, 'ThemeSettings.json'))) {
      // New v3 Format for Themes, single File:
      template = fileHelper.loadJsonFile( path.join(themeFolder, 'ThemeSettings.json') );
      template.path = themeFolder;

    } else {
      // Old fashion format for Themes:
      const defaultThemePath = fileHelper.getAssetPath('./data/ED_Odissey_ThemeTemplate.json');  
      const ThemeINIs = await LoadThemeINIs(themeFolder); 
      template = fileHelper.loadJsonFile(defaultThemePath);
      template = await ApplyIniValuesToTemplate(template, ThemeINIs);
      template.credits = await GetCreditsFile(themeFolder);
      template.path = themeFolder;
    }
  } catch (error) {
    throw error;
  }
  return template;
};

const getIniFilePath = (basePath, fileName) => {
  const joinedPath = path.join(basePath, fileName);
  return fileHelper.resolveEnvVariables(joinedPath);
};

/**
 * Retrieve the INI files asociated to a Theme
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

/**
 * Save the modified INI files back to their original location
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
                  const color = reverseGammaCorrectedList(colorComponents); //<- color: { r: 81, g: 220, b: 255, a: 255 }
                  element.Value = getColorDecimalValue(color);
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
              console.log('Key Not Found:', path.join(element.File, element.Section, element.Key), 'Val: ', element.Value);
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
                const RGBAcolor = intToRGBA(element.Value); //<- color: { r: 81, g: 220, b: 255, a: 255 }
                const sRGBcolor = GetGammaCorrected_RGBA(RGBAcolor);
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


/*--------- Color Conversion Methods ---------------------*/
function RGBAtoColor(colorComponents) {
  if (colorComponents.length === 3) {
    const [r, g, b] = colorComponents.map(parseFloat);
    return `rgb(${r}, ${g}, ${b})`;
  } else if (colorComponents.length === 4) {
    const [r, g, b, a] = colorComponents.map(parseFloat);
    return `rgba(${r}, ${g}, ${b}, ${a})`;
  } else {
    throw new Error("Invalid number of color components. Expected 3 or 4.");
  }
};

function getColorDecimalValue(color) {
  // Convert RGB to a single decimal value (e.g., for use with some UI libraries)
  return (color.r << 16) + (color.g << 8) + color.b;
};

function intToHex(int) {
  try {
    // Ensure unsigned 32-bit
    int >>>= 0;

    // Convert each component to a two-digit hex string
    const r = ((int >> 16) & 0xFF).toString(16).padStart(2, '0');
    const g = ((int >> 8) & 0xFF).toString(16).padStart(2, '0');
    const b = (int & 0xFF).toString(16).padStart(2, '0');

    return `#${r}${g}${b}`;
  } catch (error) {
    throw error;
  }
};

function rgbToHex(r, g, b) {
  // Convert each component to a two-digit hex string
  const red = r.toString(16).padStart(2, '0');
  const green = g.toString(16).padStart(2, '0');
  const blue = b.toString(16).padStart(2, '0');

  // Concatenate the components with '#' to form the HEX color string
  return `#${red}${green}${blue}`;
};

// Function to convert an integer value to RGBA components
function intToRGBA(colorInt) {
  const r = (colorInt >> 16) & 0xFF;
  const g = (colorInt >> 8) & 0xFF;
  const b = colorInt & 0xFF;
  const a = ((colorInt >> 24) & 0xFF) || 255; // Default alpha to 255 if not present

  return { r, g, b, a };
}

// Function to convert sRGB to Linear RGB using gamma correction
function Convert_sRGB_ToLinear(thesRGBValue, gammaValue = 2.4) {
  return thesRGBValue <= 0.04045
    ? thesRGBValue / 12.92
    : Math.pow((thesRGBValue + 0.055) / 1.055, gammaValue);
}

// Function to get gamma corrected RGBA
function GetGammaCorrected_RGBA(color, gammaValue = 2.4) {
  const normalize = value => value / 255;

  const gammaCorrected = {
    r: Math.round(Convert_sRGB_ToLinear(normalize(color.r), gammaValue) * 10000) / 10000,
    g: Math.round(Convert_sRGB_ToLinear(normalize(color.g), gammaValue) * 10000) / 10000,
    b: Math.round(Convert_sRGB_ToLinear(normalize(color.b), gammaValue) * 10000) / 10000,
    a: Math.round(normalize(color.a) * 10000) / 10000 // Alpha remains linear
  };

  return gammaCorrected;
}

// Function to reverse the gamma correction for comparison
function reverseGammaCorrected(gammaR, gammaG, gammaB, gammaA = 1.0, gammaValue = 2.4) {
  const result = { r: 255, g: 255, b: 255, a: 255 }; // Initialize with white and full alpha

  try {
    // Undo gamma correction (assuming power function)
    const invR = Math.pow(gammaR, 1 / gammaValue);
    const invG = Math.pow(gammaG, 1 / gammaValue);
    const invB = Math.pow(gammaB, 1 / gammaValue);

    // Approximate linear sRGB (assuming conversion to sRGB space)
    const linearSrgb = { r: invR, g: invG, b: invB };

    // Convert to RGB (assuming 0-255 range)
    result.r = safeRound(linearSrgb.r * 255);
    result.g = safeRound(linearSrgb.g * 255);
    result.b = safeRound(linearSrgb.b * 255);

    // Handle alpha (if provided)
    if (gammaA !== undefined) {
      result.a = safeRound(gammaA * 255);
    }
  } catch (error) {
    throw error;
  }

  return result;
}

// Helper function for safe rounding
function safeRound(value) {
  return isNaN(value) ? 0 : Math.round(value);
}




function reverseGammaCorrectedList(gammaComponents, gammaValue = 2.4) {
  try {
    if (!Array.isArray(gammaComponents) || gammaComponents.length < 3) {
      console.log(gammaComponents);
      throw new Error("Invalid gamma components list (requires at least 3 elements)");
    }

    const [gammaR, gammaG, gammaB, ...remaining] = gammaComponents;
    const gammaA = remaining.length > 0 ? remaining[0] : 1.0;

    return reverseGammaCorrected(gammaR, gammaG, gammaB, gammaA, gammaValue);
  } catch (error) {
    throw error;
  }
};

function convert_sRGB_FromLinear(theLinearValue, _GammaValue = 2.4) {
  return theLinearValue <= 0.0031308
    ? theLinearValue * 12.92
    : Math.pow(theLinearValue, 1.0 / _GammaValue) * 1.055 - 0.055;
};

function convert_sRGB_ToLinear(thesRGBValue, _GammaValue = 2.4) {
  return thesRGBValue <= 0.04045
    ? thesRGBValue / 12.92
    : Math.pow((thesRGBValue + 0.055) / 1.055, _GammaValue);
};




/*--------- Expose Methods via IPC Handlers: ---------------------*/
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
    return reverseGammaCorrected(color.r, color.g, color.b, color.a, gammaValue);
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('GetGammaCorrected_RGBA', async (event, color, gammaValue) => {
  try {
    return GetGammaCorrected_RGBA(color, gammaValue);
  } catch (error) {
    throw error;
  }
});
ipcMain.handle('intToRGBA', async (event, colorInt) => {
  try {
    return intToRGBA(colorInt);
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


export default { getThemes, LoadThemeINIs, SaveThemeINIs, ApplyIniValuesToTemplate, ApplyTemplateValuesToIni };