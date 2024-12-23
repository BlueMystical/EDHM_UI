import { app, ipcMain } from 'electron';
import path from 'node:path';
import fs from 'fs';

import ini from './IniHelper.js';
import fileHelper from './FileHelper';
import Log from './LoggingHelper.js';
import settingsHelper from './SettingsHelper.js';
import eventBus from '../EventBus';

/**
 * Retrieves themes from a specified directory path.
 * @async
 * @param {string} dirPath - The directory path to search for themes.
 * @returns {Promise<Array>} A promise that resolves to an array of themes.
 */
const getThemes = async (dirPath) => {
    const subfolders = await fs.promises.readdir(dirPath, { withFileTypes: true });
    const files = [];

    for (const dirent of subfolders) {
        if (dirent.isDirectory()) {
            const subfolderPath = path.join(dirPath, dirent.name);
            //console.log('Processing subfolder:', subfolderPath);

            const previewFilePath = path.join(subfolderPath, 'PREVIEW.jpg');
            let creditsFilePath = null;

            try {
                // Check for PREVIEW.jpg
                const previewExists = await fs.promises.access(previewFilePath, fs.constants.F_OK).then(() => true).catch(() => false);

                // Find any .credits file
                const allFiles = await fs.promises.readdir(subfolderPath);
                const creditsFile = allFiles.find(file => file.endsWith('.credits'));

                if (creditsFile) {
                    creditsFilePath = path.join(subfolderPath, creditsFile);
                }

                //console.log('Preview exists:', previewExists, 'Credits file found:', !!creditsFilePath);

                if (previewExists) {
                    let creditsJson = { name: dirent.name }; // Default to folder name if credits file is missing
                    if (creditsFilePath) {
                        const creditsData = await fs.promises.readFile(creditsFilePath, 'utf-8');
                        try {
                            creditsJson = JSON.parse(creditsData);
                        } catch {
                            creditsJson = {
                                "theme": dirent.name,
                                "author": "Unknown",
                                "description": "** .Credits file missing or Invalid **",
                                "preview": "",
                                "color": []
                            };
                        }
                    }
                    files.push({
                        path: subfolderPath,
                        thumbnail: 'PREVIEW.jpg', //previewFilePath,
                        credits: creditsJson
                    });
                    //console.log('Added file:', { preview: previewFilePath, credits: creditsJson });
                }

            } catch (error) {
                Log.Error(error.message, error.stack);
            }
        }
    }

    //console.log('All files:', files);
    return files;
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

        console.log('Loading Inis from: ', folderPath);
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
        Log.Error(error);
        return null;
    }
};

const ApplyIniValuesToTemplate = (template, iniValues) => {
  console.log('ApplyIniValuesToTemplate..');
    if (Array.isArray(template.ui_groups) && template.ui_groups.length > 0) {
      for (const group of template.ui_groups) {
        if (group.Elements != null) {
          for (const element of group.Elements) {
            const iniKey = element.Key;
            const foundValue = ini.findValueByKey(iniValues, element);            
         /* foundValue: [
              { key: 'x127', value: '0.063' },
              { key: 'y127', value: '0.7011' },
              { key: 'z127', value: '1' }
            ],
            foundValue: 100.0 */

            if (foundValue != null) {
              if (Array.isArray(foundValue) && foundValue.length > 2) { 
                const colorKeys = iniKey.split("|"); //<- iniKey === "x159|y159|z159"
                const colorComponents = colorKeys.map(key => {
                  const foundValueObj = foundValue.find(obj => obj.key === key); 
                  return foundValueObj ? foundValueObj.value : undefined; 
                }); //<- colorComponents: [ '0.063', '0.7011', '1' ]

                if (colorComponents != undefined && !colorComponents.includes(undefined)) {
                  const color = reverseGammaCorrectedList(colorComponents); //<- color: { r: 81, g: 220, b: 255, a: 255 }
                  element.Value = getColorDecimalValue(color); 
                  //console.log("Value:", element.Value);
                } else {
                  Log.Warning('Key Not Found:', path.join(element.File, element.Section, element.Key));
                }

              } else {
                element.Value = parseFloat(foundValue);
              }
            } else {
              Log.Warning('Key Not Found:', path.join(element.File, element.Section, element.Key));
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
  
    return template;
  };
  


  

function deepCopy(obj) {
    if (typeof obj !== 'object' || obj === null) {
        return obj;
    }

    if (Array.isArray(obj)) {
        return obj.map(item => deepCopy(item));
    }

    const copy = {};
    for (const key in obj) {
        if (obj.hasOwnProperty(key)) {
            copy[key] = deepCopy(obj[key]);
        }
    }
    return copy;
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
    // This is a simple example, you might need to adjust based on your specific needs
    return (color.r << 16) + (color.g << 8) + color.b;
};

function intToHex(int) {
    // Ensure unsigned 32-bit
    int >>>= 0;
    
    // Convert each component to a two-digit hex string
    const r = ((int >> 16) & 0xFF).toString(16).padStart(2, '0');
    const g = ((int >> 8) & 0xFF).toString(16).padStart(2, '0');
    const b = (int & 0xFF).toString(16).padStart(2, '0');
    
    return `#${r}${g}${b}`;
  };

  function intToRGB(int) {
    int >>>= 0; // Ensure unsigned 32-bit
    const r = (int >> 16) & 0xFF;
    const g = (int >> 8) & 0xFF;
    const b = int & 0xFF;
    return { r, g, b };
  };

  function rgbToHex(r, g, b) {
    // Convert each component to a two-digit hex string
    const red = r.toString(16).padStart(2, '0');
    const green = g.toString(16).padStart(2, '0');
    const blue = b.toString(16).padStart(2, '0');
  
    // Concatenate the components with '#' to form the HEX color string
    return `#${red}${green}${blue}`;
  };
  

  

//  const colorInt = -23296;
  //const rgb = intToRGB(colorInt);
//  console.log(`RGB: (${rgb.r}, ${rgb.g}, ${rgb.b})`);

function getGammaCorrectedRgba(color, gammaValue = 2.4) {
    // No ColorManagment library assumed in JavaScript
    // Basic conversion based on sRGB to linear approximation

    const result = [0, 0, 0, color.alpha / 255]; // Initialize result with alpha

    try {
        // Convert to approximate linear sRGB (assuming sRGB space)
        const linearSrgb = {
            r: color.r / 255,
            g: color.g / 255,
            b: color.b / 255,
        };

        // Apply gamma correction (assuming power function)
        result[0] = Math.round(Math.pow(linearSrgb.r, gammaValue), 4);
        result[1] = Math.round(Math.pow(linearSrgb.g, gammaValue), 4);
        result[2] = Math.round(Math.pow(linearSrgb.b, gammaValue), 4);
    } catch (error) {
        console.error("Error converting color to gamma corrected RGBA:", error);
    }

    return result;
};

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
        console.error("Error converting gamma corrected values to color:", error);
    }

    return result;
};

function reverseGammaCorrectedList(gammaComponents, gammaValue = 2.4) {
    if (!Array.isArray(gammaComponents) || gammaComponents.length < 3) {
      console.log(gammaComponents);
        throw new Error("Invalid gamma components list (requires at least 3 elements)");
    }

    const [gammaR, gammaG, gammaB, ...remaining] = gammaComponents;
    const gammaA = remaining.length > 0 ? remaining[0] : 1.0;

    return reverseGammaCorrected(gammaR, gammaG, gammaB, gammaA, gammaValue);
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

function safeRound(value) {
    return isNaN(value) ? 0 : Math.round(value);
  };


/*--------- Expose Methods via IPC Handlers: ---------------------*/
ipcMain.handle('get-themes', async (event, dirPath) => {
    try {
        const resolvedPath = fileHelper.resolveEnvVariables(dirPath);
        const files = await getThemes(resolvedPath);
        return files;
    } catch (error) {
        Log.Error(error);
    }
});

ipcMain.handle('LoadThemeINIs', async (event, folderPath) => {
    return LoadThemeINIs(folderPath);
});

ipcMain.handle('reverseGammaCorrected', async (event, gammaR, gammaG, gammaB) => {
    return reverseGammaCorrected(gammaR, gammaG, gammaB);
});

ipcMain.handle('apply-ini-values', async (event, template, iniValues) => {
    const updatedTemplate = await ApplyIniValuesToTemplate(template, iniValues);
    return updatedTemplate;
});


export default { getThemes, LoadThemeINIs, ApplyIniValuesToTemplate };