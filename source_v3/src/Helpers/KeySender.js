import { execSync, spawnSync } from 'child_process';
import os from 'os';

//- Ctrl + F10  → "^{F10}"
//- Shift + F11 → "+{F11}"
//- Alt + F10   → "%{F10}"

// ["F11", "ctrl+F10", "alt+F4"];



/** Sends key inputs (simulates key presses) to a target process or window.
 * @param {Object} config - Configuration object.
 * @example
 * SendKey({
 *   targetProcess: "EliteDangerous64", // Executable name
 *   targetWindow: "Elite - Dangerous", // Window title
 *   keyBindings: ["F11", "ctrl+F10", "alt+F4"]   // Array of keys to send (neutral format)
 * }); */
function SendKey(config) {
    try {
        // Provide default config if none is given
        if (!config || typeof config !== "object") {
            config = {
                targetProcess: "EliteDangerous64",
                targetWindow: "Elite - Dangerous",
                keyBindings: ["F11"]
            };
        }

        console.log("--- KeySender: SendKey ------------------");

        // Check if target process is running
        if (isProcessRunning(config.targetProcess)) {
            console.log(`The program "${config.targetProcess}" is running.`);

            // Normalize key bindings for the current platform
            const normalizedConfig = {
                ...config,
                keyBindings: config.keyBindings.map(formatKeyForPlatform)
            };

            // Dispatch to the correct platform-specific sender
            const platform = os.platform();
            if (platform === "win32") {
                sendKeysWindows(normalizedConfig);
            } else if (platform === "linux") {
                sendKeysLinux(normalizedConfig);
            } else {
                console.error(`Unsupported platform: ${platform}`);
            }
        } else {
            console.log(`The program "${config.targetProcess}" is NOT running.`);
        }

    } catch (err) {
        console.error("Error in SendKey:", err.message);
    } finally {
        console.log("-----------------------------------------");
    }
}


/* ---------- WINDOWS VERSION ---------- */


/*function sendKeysWindows(config) {
    // Escape double quotes in the window name for PowerShell
    const windowTitle = String(config.targetWindow).replace(/"/g, '""');

    const lines = [
        'Add-Type -AssemblyName System.Windows.Forms',
        '$wshell = New-Object -ComObject wscript.shell',
        `if ($wshell.AppActivate("${windowTitle}")) {`,
        '  Start-Sleep -Milliseconds 300'
    ];

    for (const key of config.keyBindings) {
        const psKey = key.replace(/([A-Za-z0-9]+)/g, "{$1}");
        lines.push(`  [System.Windows.Forms.SendKeys]::SendWait("${psKey}")`);
        lines.push('  Start-Sleep -Milliseconds 100');
    }

    lines.push('} else { Write-Host "Could not activate the window." }');
    lines.push('Exit');

    // Join the script into a single line to pass as an argument
    const psScript = lines.join('; ');
    console.log(`Executing PowerShell script:\n${psScript}`);

    try {
        execSync(`powershell -ExecutionPolicy Bypass -Command "${psScript.replace(/"/g, '\\"')}"`);
        console.log(`Keybinding "${config.keyBindings}" sent using PowerShell.`);
    } catch (err) {
        console.error("Error executing PowerShell:", err.message);
    }
}*/

function sendKeysWindows(config) {
    const windowTitle = String(config.targetWindow).replace(/"/g, '""');

    const lines = [
        'Add-Type -AssemblyName System.Windows.Forms',
        '$wshell = New-Object -ComObject wscript.shell',
        `if ($wshell.AppActivate("${windowTitle}")) {`,
        '  Start-Sleep -Milliseconds 300'
    ];

    for (const rawKey of config.keyBindings) {
        const psKey = formatKeyForPlatform(rawKey);
        lines.push(`  [System.Windows.Forms.SendKeys]::SendWait("${psKey}")`);
        lines.push('  Start-Sleep -Milliseconds 100');
    }

    lines.push('} else { Write-Host "Could not activate the window." }');
    lines.push('Exit');

    const psScript = lines.join('; ');

    try {
        execSync(
            `powershell -ExecutionPolicy Bypass -Command "${psScript.replace(/"/g, '\\"')}"`,
            { stdio: 'inherit' }
        );
        console.log(`Keybinding "${config.keyBindings}" sent using PowerShell.`);
    } catch (err) {
        console.error("Error executing PowerShell:", err.message);
    }
}

// Verificar si el proceso está corriendo
function isProcessRunning(name) {
    const platform = os.platform();
    try {
        if (platform === "win32") {
            const output = execSync("tasklist").toString().toLowerCase();
            return output.includes(name.toLowerCase());

        } else if (platform === "linux") {
            const output = execSync("ps -A").toString().toLowerCase();
            return output.includes(name.toLowerCase());

        } else {
            console.error(" Plataforma no soportada:", platform);
            return false;
        }
    } catch (err) {
        console.error(" Error al verificar el proceso:", err.message);
        return false;
    }
}

/* ---------- LINUX VERSION ---------- */

/** Sends key presses to a target window on Linux using `xdotool`.
 * - Detects if the session is Wayland and verifies the window is X11/XWayland.
 * - Raises, focuses, and activates the window before typing.
 * - Types all keys in one go with a configurable delay between characters. */
function sendKeysLinux(config) {
    const sessionType = (process.env.XDG_SESSION_TYPE || "").toLowerCase();
    const winId = findWindowId(config.targetWindow);

    if (!winId) {
        console.error(`404 - No visible window found with title "${config.targetWindow}"`);
        return;
    }

    if (sessionType === 'wayland' && !isX11Window(winId)) {
        console.error("Target window is not X11/XWayland. Cannot send keys on Wayland.");
        return;
    }

    spawnSync('xdotool', ['windowraise', winId]);
    spawnSync('xdotool', ['windowfocus', winId]);
    spawnSync('xdotool', ['windowactivate', '--sync', winId]);

    for (const rawKey of config.keyBindings) {
        const combo = formatKeyForPlatform(rawKey);
        spawnSync('xdotool', ['key', '--delay', '50', combo]);
    }

    console.log(`Key combinations sent to window ID ${winId}`);
}


/** Check if a given window ID belongs to an X11/XWayland window.
 * Runs `xwininfo` which only works on X11/XWayland.
 * Returns true if the command succeeds, false otherwise. */
function isX11Window(winId) {
    if (!winId) return false;
    const res = spawnSync('xwininfo', ['-id', winId]);
    return res.status === 0;
}

function findWindowId(windowName) {
    const res = spawnSync(
        'xdotool',
        ['search', '--onlyvisible', '--limit', '1', '--name', windowName],
        { encoding: 'utf8' }
    );
    return res.status === 0 ? res.stdout.trim() : null;
}


/**
 * Format a key combination for the current platform.
 * - On Windows: wrap single words like F11 in {} for SendKeys, keep modifiers (^, +, %) as is.
 * - On Linux: remove {} if present (from Windows format), keep xdotool syntax. */
function formatKeyForPlatform(key) {
    if (process.platform === 'win32') {
        // If it's already in SendKeys format or starts with a modifier, leave it
        if (/^\{.*\}$/.test(key) || /^[\^\+\%]/.test(key)) {
            return key;
        }
        // Wrap plain words (e.g., F11) in {}
        return `{${key}}`;
    } else {
        // Remove surrounding {} if present (e.g., {F11} -> F11)
        return key.replace(/^\{(.+)\}$/, '$1');
    }
}

export default {
    SendKey,
    sendKeysWindows,
    sendKeysLinux
}