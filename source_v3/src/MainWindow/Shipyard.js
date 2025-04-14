import { ipcMain } from 'electron'; // Import BrowserWindow
import fs from 'node:fs/promises';
import path from 'node:path';
import chokidar from 'chokidar';
import settingsHelper from '../Helpers/SettingsHelper.js';
import fileHelper from '../Helpers/FileHelper.js';

// Configuration
const LOG_DIRECTORY = fileHelper.resolveEnvVariables(
    settingsHelper.readSetting('PlayerJournal',
        '%USERPROFILE%\\Saved Games\\Frontier Developments\\Elite Dangerous')
);
let currentlyMonitoredFile = null;
let fileWatcher = null;
let directoryWatcher = null;
let mainWindowInstance = null; // To hold the mainWindow instance

export const setMainWindow = (mainWindow) => {
    mainWindowInstance = mainWindow;
};

function OnShipyardEvent(event) {
    try {
        
    } catch (error) {
        console.error('Error in OnShipyard_Event:', error);        
    }
}

async function getLatestLogFile() {
    try {
        const files = await fs.readdir(LOG_DIRECTORY);
        const logFiles = files.filter(file => file.endsWith('.log') || file.endsWith('.txt')); // Adjust filter as needed

        if (logFiles.length === 0) {
            return null;
        }

        const filesWithStats = await Promise.all(
            logFiles.map(async (file) => {
                const filePath = path.join(LOG_DIRECTORY, file);
                const stats = await fs.stat(filePath);
                return { path: filePath, created: stats.birthtimeMs };
            })
        );

        filesWithStats.sort((a, b) => b.created - a.created); // Sort by creation time (newest first)
        return filesWithStats[0].path;
    } catch (error) {
        console.error('Error getting latest log file:', error);
        return null;
    }
}

async function analyzeLogFile(filePath) {
    try {
        const content = await fs.readFile(filePath, 'utf-8');
        const lines = content.trim().split('\n');
        const parsedLines = lines.map(line => {
            try {
                return JSON.parse(line);
            } catch (error) {
                console.warn('Error parsing JSON line:', line, error);
                return null;
            }
        }).filter(line => line !== null);

        // Perform your analysis here on the parsedLines array
        console.log(`Analyzed ${parsedLines.length} lines from:`, filePath);
        if (mainWindowInstance && mainWindowInstance.webContents) {
            mainWindowInstance.webContents.send('log-analysis-update', parsedLines); // Send data to renderer
        } else {
            console.warn('mainWindowInstance not available to send log analysis update.');
        }
        OnShipyardEvent(parsedLines[length - 1]); // Call the event handler with the last parsed line
        return parsedLines; // You can still return it if needed for internal logic
    } catch (error) {
        console.error('Error analyzing log file:', error);
    }
}

async function startMonitoringFile(filePath) {
    if (filePath === currentlyMonitoredFile) {
        return; // Already monitoring this file
    }

    console.log('Starting to monitor:', filePath);
    currentlyMonitoredFile = filePath;

    // Stop previous watcher if any
    if (fileWatcher) {
        fileWatcher.close();
    }

    // Initial analysis
    await analyzeLogFile(filePath);

    // Watch for changes in the file
    fileWatcher = chokidar.watch(filePath, {
        persistent: true,
        ignoreInitial: true,
    }).on('change', async () => {
        console.log('File changed:', filePath);
        await analyzeLogFile(filePath); // Re-analyze and send updates
    }).on('error', error => console.error('Error watching file:', error));
}

async function checkAndSwitchLogFile() {
    const latestFile = await getLatestLogFile();
    if (latestFile && latestFile !== currentlyMonitoredFile) {
        startMonitoringFile(latestFile);
    }
}

function startDirectoryMonitoring() {
    console.log('Starting directory monitoring...');
    // Ensure the log directory exists
    fs.mkdir(LOG_DIRECTORY, { recursive: true }).catch(console.error);

    // Initial check for the latest file
    checkAndSwitchLogFile();

    // Watch the directory for new files
    directoryWatcher = chokidar.watch(LOG_DIRECTORY, {
        persistent: true,
        ignoreInitial: true,
    }).on('add', async (filePath) => {
        console.log('New file created:', filePath);
        checkAndSwitchLogFile();
    }).on('unlink', async (filePath) => {
        console.log('File removed:', filePath);
        checkAndSwitchLogFile(); // Re-evaluate in case the monitored file was removed
    }).on('error', error => console.error('Error watching directory:', error));
}

ipcMain.handle('start-log-monitoring', async (event) => {
    try {
        return startDirectoryMonitoring();
    } catch (error) {
        console.error('Error reading XML file:', error);
        return null;
    }
});

export default { 
    getLatestLogFile, analyzeLogFile, 
    startMonitoringFile, checkAndSwitchLogFile, 
    startDirectoryMonitoring, 
    setMainWindow,
    OnShipyardEvent
};