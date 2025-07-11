import { execSync } from 'child_process';
import os from 'os';

/** Sends key inputs (simulates key presses) to a target process or window. 
 * @param {Object} config - Configuration object. * 
 * @example
 * SendKey({
 *   targetProcess: "EliteDangerous64", //<- Executable name
 *   targetWindow: "Elite - Dangerous", //<- Window title
 *   keyBindings: ["F11"]   //<- Array of keys to send
 * });
 */
function SendKey(config) {
    try {
        if (!config || typeof config !== "object") {
            config = {
                targetProcess: "EliteDangerous64", //<- Executable name
                targetWindow: "Elite - Dangerous", //<- Window title
                keyBindings: ["F11"]    //<- Array of keys to send
            }
        }

        console.log("--- KeySender: SendKey ------------------");

        //  Check if target process is running:
        if (isProcessRunning(config.targetProcess)) {
            console.log(`The Program "${config.targetProcess}" is Running.`);

            const platform = os.platform();
            if (platform === "win32") {
                sendKeysWindows(config);

            } else if (platform === "linux") {
                sendKeysLinux(config);
            }
        } else {
            console.log(`The Program "${config.targetProcess}" is NOT Running.`);
        }

    } catch (err) {
        console.error(err.message);
    } finally {
        console.log("-----------------------------------------");
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

function sendKeysWindows(config) {
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

    try {
        execSync(`powershell -ExecutionPolicy Bypass -Command "${psScript.replace(/"/g, '\\"')}"`);
        console.log(`Keybinding "${config.keyBindings}" sent using PowerShell.`);
    } catch (err) {
        console.error("Error executing PowerShell:", err.message);
    }
}

// Checks if the window is X11/XWayland (not native Wayland)
function isX11Window(windowName) {
    try {
        // Find the window and get its ID
        const winId = execSync(`xdotool search --onlyvisible --limit 1 --name "${windowName}"`).toString().trim();
        if (!winId) return false;
        // Try to get info with xwininfo (only works on X11/XWayland)
        execSync(`xwininfo -id ${winId}`);
        return true;
    } catch (err) {
        return false;
    }
}

function sendKeysLinux(config) {
    const sessionType = process.env.XDG_SESSION_TYPE || "";
    const isWayland = sessionType.toLowerCase() === "wayland";

    if (isWayland) {
        // Only try if the window is X11/XWayland
        if (!isX11Window(config.targetWindow)) {
            console.error("Target window is not X11/XWayland. Cannot send keys on Wayland.");
            return;
        }
    }

    try {
        const winId = execSync(`xdotool search --onlyvisible --limit 1 --name "${config.targetWindow}"`).toString().trim();
        if (!winId) {
            console.error(`404 - No visible window found with title "${config.targetWindow}"`);
            return;
        }
        execSync(`xdotool windowraise ${winId}`);
        execSync(`xdotool windowfocus ${winId}`);
        execSync(`xdotool windowactivate --sync ${winId}`);
        execSync(`sleep 0.2`);

        for (const key of config.keyBindings) {
            execSync(`xdotool type "${key}"`);
            execSync(`sleep 0.05`);
        }
        console.log(`Keybindings sent to window ID ${winId}`);
    } catch (err) {
        console.error("Error simulating input:", err.message);
    }
}

export default {
    SendKey,
    sendKeysWindows,
    sendKeysLinux
}