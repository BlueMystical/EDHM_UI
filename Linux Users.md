# Linux Users

EDHM_UI runs as a native Linux x64 Electron application and can manage an Elite Dangerous installation running through Proton or Wine. This guide is limited to installing, configuring, building, and troubleshooting EDHM_UI, including Frontier fleet authorization.

## Install the released build

1. Download both `edhm-ui-v3-linux-x64.zip` and `linux_installer.sh` from the [latest EDHM_UI release](https://github.com/BlueMystical/EDHM_UI/releases/latest).
2. Place both files in the same directory.
3. Open a terminal in that directory and run:

   ```bash
   chmod +x linux_installer.sh
   ./linux_installer.sh
   ```

The installer extracts EDHM_UI to `~/.local/share/EDHM-UI-V3`, makes the application executable, creates desktop and application-menu shortcuts, and registers EDHM-UI to handle Frontier's `edhm://frontier-auth` callback. The `unzip`, `pgrep`, and `pkill` commands must be available.

Do not run EDHM_UI or its installer as root. Running it as another user changes its settings and credential-storage locations.

## Configure EDHM-UI paths

Use the EDHM_UI Settings menu to select the Elite Dangerous installation and Player Journal directories. A path selected by the user always takes priority.

For a new Linux configuration, the Player Journal fallback is the standard Steam/Proton location:

```text
~/.local/share/Steam/steamapps/compatdata/359320/pfx/drive_c/users/steamuser/Saved Games/Frontier Developments/Elite Dangerous
```

Steam libraries on other drives and installations managed by Lutris, Heroic, or a custom Wine prefix may store the journal elsewhere. Select the actual `Elite Dangerous` journal directory in Settings; EDHM_UI preserves custom paths instead of replacing them with the fallback.

## Frontier account authorization

The Shipyard can import the Commander's fleet from Frontier. Open the Shipyard and select **Connect Frontier** to begin.

Linux requires an installed and unlocked system credential store, such as GNOME Keyring/libsecret or KDE Wallet. EDHM_UI encrypts the Frontier tokens through Electron's secure storage and deliberately refuses the insecure `basic_text` fallback. If secure storage is unavailable, unlock or configure the desktop keyring and restart EDHM_UI.

Authorization works as follows:

1. EDHM_UI registers itself as the handler for the `edhm://` protocol.
2. The default browser opens Frontier's authorization page.
3. After approval, Frontier returns the browser to `edhm://frontier-auth`.
4. The desktop opens the callback in the running EDHM-UI instance, which validates the request, securely stores the tokens, and downloads the fleet.

No localhost web server or self-signed certificate is used. The browser may ask for confirmation before opening an external application; approve opening EDHM-UI. There should be no unsafe-certificate warning or Advanced bypass.

If the authorization page is closed before approval, select **Cancel Connection** in the Shipyard to clear the pending request before trying again.

Frontier access and refresh tokens never enter the renderer or `Shipyard_v3.json`. They are encrypted in the Electron user-data directory, normally under `~/.config/EDHM-UI-V3`. Disconnecting the Frontier account removes the stored authorization without deleting Shipyard themes.

## Build from source

Build on a Linux x64 system with Git, a current Node.js LTS release, npm, `jq`, and the normal Electron build dependencies for the distribution.

```bash
git clone https://github.com/BlueMystical/EDHM_UI.git
cd EDHM_UI/source_v3
npm ci
npm test
npm start
```

After verifying the application locally, create the release ZIP with:

```bash
chmod +x linux-build.sh
./linux-build.sh
```

The script asks for the application version and runs the Electron Forge ZIP maker. The resulting archive is written below:

```text
source_v3/out/make/zip/linux/x64/
```

Rename the release archive to `edhm-ui-v3-linux-x64.zip` before distributing it with `linux_installer.sh`.

## Sandbox packaging

The published ZIP installation is not sandboxed. Flatpak or other sandboxed packages need explicit access to the system browser, the desktop keyring, custom-protocol activation, the selected Steam or Wine prefix, the Player Journal directory, and the Elite Dangerous installation. A package that does not grant those permissions may launch successfully but still be unable to authorize Frontier or manage EDHM files.

## Troubleshooting checklist

- Launch EDHM_UI as the desktop user, not as root.
- Confirm the configured game and Player Journal directories in Settings.
- Confirm the system keyring is installed, unlocked, and available to Electron.
- Allow the browser to open EDHM-UI when it receives the `edhm://frontier-auth` callback.
- If the browser reports that no application can handle `edhm://`, rerun `linux_installer.sh` as the desktop user to restore the protocol registration.
- For Frontier errors, disconnect the account, reconnect it, and complete the browser flow again.
- If a selected directory is inside another Steam library or Wine prefix, confirm that the EDHM_UI process can read and write that location.

Implementation and security details are documented in [Frontier OAuth and Shipyard Fleet Sync](source_v3/FRONTIER_OAUTH.md).
