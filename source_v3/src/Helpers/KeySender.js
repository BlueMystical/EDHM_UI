const fs = require("fs");
const os = require("os");
const path = require("path");
const { execSync } = require("child_process");

function SendKey(config) {
    try {
        // Validar la configuración
        if (!config || typeof config !== "object") {
            config = {
                targetProcess: "EliteDangerous64",
                targetWindow: "Elite - Dangerous",
                keyBindings: ["F11"]
            }
        }

        //console.log(" Config Loaded:", config);

        //  Flujo principal
        if (isProcessRunning(config.targetProcess)) {
            console.log(`The Program "${config.targetProcess}" is Running.`);

            const platform = os.platform();
            if (platform === "win32") {
                sendKeysWindows(config);

            } else if (platform === "linux") {
                for (const combo of config.keyBindings) {
                    sendKeysLinux(combo);
                }
            }

            process.exit(0);
        } else {
            console.log(`The Program "${config.targetProcess}" is NOT Running.`);
        }

    } catch (err) {
        console.error(err.message);
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
            console.error("❌ Plataforma no soportada:", platform);
            return false;
        }
    } catch (err) {
        console.error("⚠️ Error al verificar el proceso:", err.message);
        return false;
    }
}

function buildPowerShellScript(config) {
    const lines = [
        'Add-Type -AssemblyName System.Windows.Forms',
        '$wshell = New-Object -ComObject wscript.shell',
        `if ($wshell.AppActivate("${config.targetWindow}")) {`,
        '  Start-Sleep -Milliseconds 300'
    ];

    for (const key of config.keyBindings) {
        const psKey = key.replace(/([A-Za-z0-9]+)/g, "{$1}");
        lines.push(`  [System.Windows.Forms.SendKeys]::SendWait("${psKey}")`);
        lines.push('  Start-Sleep -Milliseconds 100');
    }

    lines.push('} else { Write-Host " No se pudo activar la ventana." }');
    lines.push('Exit');

    const scriptPath = path.join(process.cwd(), "send.ps1");
    fs.writeFileSync(scriptPath, lines.join('\n'), "utf-8");

    return scriptPath;
}

// Ejecutar el script en Windows
function sendKeysWindows(config) {
    const scriptPath = buildPowerShellScript(config);
    try {
        execSync(`powershell -ExecutionPolicy Bypass -File "${scriptPath}"`);
        console.log(" Keybindings send using PowerShell.");
    } catch (err) {
        console.error(" Error executing PowerShell:", err.message);
    }
}

// Ejecutar comandos en Linux
function sendKeysLinux(combo) {
    const { execSync } = require("child_process");

    const sessionType = process.env.XDG_SESSION_TYPE || "";
    const isWayland = sessionType.toLowerCase() === "wayland";

    if (isWayland) {
        console.error(" This system uses Wayland, which prevents simulating keypresses with xdotool.");
        console.info(" To use this feature, please log into an X11 session (e.g., GNOME on X11).");
        return;
    }

    try {
        const winId = execSync(`xdotool search --onlyvisible --limit 1 --name "${config.targetWindow}"`).toString().trim();

        if (!winId) {
            console.error(` 404 - No visible window found with title "${config.targetWindow}"`);
            return;
        }

        execSync(`xdotool windowraise ${winId}`);
        execSync(`xdotool windowfocus ${winId}`);
        execSync(`xdotool windowactivate --sync ${winId}`);
        execSync(`sleep 0.2`);

        for (const key of combo) {
            execSync(`xdotool type "${key}"`);
            execSync(`sleep 0.05`);
        }

        console.log(` Keybindings sent to window ID ${winId}`);
    } catch (err) {
        console.error(" Error simulating input:", err.message);
    }
}

export default {
    SendKey,
    isProcessRunning,
    sendKeysWindows,
    sendKeysLinux
}