#!/bin/bash

# Set DEBUG mode: 1 for debugging, 0 for production
DEBUG=0

# Define variables
TARGET_DIR="$HOME/.local/share/EDHM-UI-V3"
APP_EXE="edhm-ui-v3"
DESKTOP_FILE="$HOME/Desktop/edhm-ui-v3.desktop"
APPLICATIONS_FILE="$HOME/.local/share/applications/EDHM-UI-V3.desktop"
ICON_PATH="$TARGET_DIR/resources/images/icon.png"

# Check if the process is running and attempt to kill it
echo "Checking for $APP_EXE process..."
if pgrep -x "$APP_EXE" > /dev/null; then
    pkill -x "$APP_EXE"
    echo "Info: $APP_EXE process terminated."
else
    echo "Info: $APP_EXE process not running."
fi

# DEBUG: Pause after killing the process
if [ $DEBUG -eq 1 ]; then
    read -p "Paused after killing the process. Press Enter to continue..."
fi

# Find the zip file
ZIP_FILE=$(find . -maxdepth 1 -name "edhm-ui-v3-linux-x64.zip")
if [ -z "$ZIP_FILE" ]; then
    echo "Error: Zip file not found."
    exit 1
fi

# Create the target directory and unzip
unzip "$ZIP_FILE"
UNZIPPED_DIR=$(find "." -maxdepth 1 -type d -iname "edhm-ui-v3-linux-x64" -print -quit)
if [ -z "$UNZIPPED_DIR" ]; then
    echo "Error: Unzipped directory not found."
    exit 1
fi

rm -r "$TARGET_DIR"

mv "$UNZIPPED_DIR" "$TARGET_DIR"

# Validate the case-sensitive executable name before changing permissions.
if [ ! -f "$TARGET_DIR/$APP_EXE" ]; then
    echo "Error: Application executable '$APP_EXE' was not found in the package."
    exit 1
fi

# Make the executable file executable
chmod +x "$TARGET_DIR/$APP_EXE"

# Create the desktop shortcut
DESKTOP_CONTENT="
[Desktop Entry]
Encoding=UTF-8
Name=EDHM-UI-V3
Exec=$TARGET_DIR/$APP_EXE %u
Icon=$ICON_PATH
Terminal=false
Type=Application
Comment=Mod for Elite Dangerous to customize the HUD of any ship.
StartupNotify=true
Categories=Utility;
MimeType=x-scheme-handler/edhm;
"
echo "$DESKTOP_CONTENT" > "$DESKTOP_FILE"
chmod +x "$DESKTOP_FILE"
echo "$DESKTOP_CONTENT" > "$APPLICATIONS_FILE"
chmod +x "$APPLICATIONS_FILE"

# Register the Frontier OAuth callback protocol for this desktop user.
if command -v xdg-mime >/dev/null 2>&1; then
    xdg-mime default "$(basename "$APPLICATIONS_FILE")" x-scheme-handler/edhm
fi
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database "$(dirname "$APPLICATIONS_FILE")"
fi

# DEBUG: Pause before running the application
if [ $DEBUG -eq 1 ]; then
    read -p "Paused before starting the application. Press Enter to continue..."
fi

# Run the application
"$TARGET_DIR/$APP_EXE"
if [ $? -eq 0 ]; then
    echo "Application started successfully."
else
    echo "Error: Application failed to start."
fi

echo "Desktop and Application Menu shortcuts created."

