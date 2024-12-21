import { app, ipcMain } from 'electron';
import path from 'node:path';
import fs from 'fs';
import fileHelper from './FileHelper'; 

const LOG_TYPES = {
  ERROR: 'ERROR',
  INFO: 'INFO',
  WARNING: 'WARNING',
};

const logPath = path.join(app.getPath('appData'), 'edhm-ui', 'log.txt');
const logDirectory = path.dirname(logPath); 

/**
 * Logs an event with a specific type to both the console and a log file.
 *
 * @param {string} type - The type of the log event (ERROR, INFO, WARNING).
 * @param {...*} args - Arguments for the log message.
 *   - If a single string is provided, it's used as the message.
 *   - If an Error object is provided, the message and stack trace are extracted.
 *   - If a string and a stack trace string are provided, they are used as the message and stack trace.
 */
const logEvent = (type, ...args) => {

  fileHelper.ensureDirectoryExists(logDirectory); // Create the directory before logging

  let message;
  let stackTrace = '';

  if (args.length === 1 && typeof args[0] === 'string') {
    message = args[0];
  } else if (args.length === 1 && args[0] instanceof Error) {
    message = args[0].message;
    stackTrace = args[0].stack;
  } else if (args.length === 2 && typeof args[0] === 'string' && typeof args[1] === 'string') {
    message = args[0];
    stackTrace = args[1];
  } else {
    console.warn('Invalid arguments for logEvent. Expected string, Error object, or string and stackTrace.');
    return;
  }

  const date = new Date().toLocaleString();
  const logEntry = {
    type,
    date,
    message,
    stackTrace,
  };

  fs.appendFileSync(logPath, `${JSON.stringify(logEntry)}\n`, 'utf-8');

  // Log to console with type prefix
  console.log(`${date} - [${type}] - ${message}`);
};

// Helper methods for convenience:
const Info = (...args) => logEvent(LOG_TYPES.INFO, ...args);
const Error = (...args) => logEvent(LOG_TYPES.ERROR, ...args);
const Warning = (...args) => logEvent(LOG_TYPES.WARNING, ...args);

// Default export for easy access
export default {
  Error,
  Info,
  Warning,
};