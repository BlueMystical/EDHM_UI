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
    if (Array.isArray(template.ui_groups) && template.ui_groups.length > 0) {
      for (const group of template.ui_groups) {
        if (group.Elements != null) {
          for (const element of group.Elements) {
            const iniFile = element.File;
            const iniSection = element.Section;
            const iniKey = element.Key;
  
            // Map iniFile to the corresponding object in iniValues
            const iniFileObject = iniValues[iniFile.replace(/-/g, '')];
  
            if (iniFileObject && iniFileObject.constants && iniKey) {
              try {
                if (element.ValueType === "Color") {
                  const colorKeys = iniKey.split("|"); 
                  const colorComponents = colorKeys.map(key => iniFileObject.constants[key]); 
                  if (colorComponents.every(component => !isNaN(parseFloat(component)))) {
                    const color = reverseGammaCorrected(colorComponents); 
                    element.Value = getColorDecimalValue(color); 
                  } 
                } else {
                  const iniValue = iniFileObject.constants[iniKey];
                  if (!isNaN(parseFloat(iniValue))) {
                    element.Value = parseFloat(iniValue); 
                  }
                }
              } catch (error) {
                console.error(`Error parsing value for ${iniFile} - ${iniSection} - ${iniKey}:`, error);
              }
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
  
  // ... rest of the code (getColorDecimalValue, reverseGammaCorrected) ...


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
function getColorDecimalValue(color) {
    // Convert RGB to a single decimal value (e.g., for use with some UI libraries)
    // This is a simple example, you might need to adjust based on your specific needs
    return (color.r << 16) + (color.g << 8) + color.b;
};

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
        result.r = Math.round(linearSrgb.r * 255);
        result.g = Math.round(linearSrgb.g * 255);
        result.b = Math.round(linearSrgb.b * 255);

        // Handle alpha (if provided)
        if (gammaA !== undefined) {
            result.a = Math.round(gammaA * 255);
        }
    } catch (error) {
        console.error("Error converting gamma corrected values to color:", error);
    }

    return result;
};

function reverseGammaCorrectedList(gammaComponents, gammaValue = 2.4) {
    if (!Array.isArray(gammaComponents) || gammaComponents.length < 3) {
        throw new Error("Invalid gamma components list (requires at least 3 elements)");
    }

    const [gammaR, gammaG, gammaB, ...remaining] = gammaComponents;
    const gammaA = remaining.length > 0 ? remaining[0] : 1.0;

    return reverseGammaCorrected(gammaR, gammaG, gammaB, gammaA, gammaValue);
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

ipcMain.handle('apply-ini-values', async (event, template, iniValues) => {
    const updatedTemplate = await ApplyIniValuesToTemplate(template, iniValues);
    return updatedTemplate;
});


export default { getThemes, LoadThemeINIs, ApplyIniValuesToTemplate };