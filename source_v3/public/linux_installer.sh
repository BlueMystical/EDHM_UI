#!/bin/bash
set -euo pipefail

# Set DEBUG mode: 1 for debugging, 0 for production
DEBUG=0

# Define variables
TARGET_DIR="$HOME/.local/share/"
APP_NEW_DIR="EDHM-UI-V3"
APP_EXE="edhm-ui-v3"
DESKTOP_FILE="$HOME/Desktop/edhm-ui-v3.desktop"
APPLICATIONS_FILE="$HOME/.local/share/applications/EDHM-UI-V3.desktop"
ICON_PATH="$TARGET_DIR/$APP_NEW_DIR/resources/images/icon.png"

# Check if the process is running and attempt to kill it
echo "Checking for $APP_EXE process..."
if pgrep -x "$APP_EXE" > /dev/null; then
    pkill -x "$APP_EXE"
    echo "Info: $APP_EXE process terminated."
else
    echo "Info: $APP_EXE process not running."
fi

# Delete any folders starting with EDHM or edhm in the target directory
echo "Deleting old EDHM/edhm folders..."
for dir in "$TARGET_DIR"/EDHM* "$TARGET_DIR"/edhm*; do
    if [ -d "$dir" ]; then
        rm -rf "$dir"
        echo "Deleted: $dir"
    fi
done

# DEBUG: Pause after killing the process and deleting old folders
if [ $DEBUG -eq 1 ]; then
    read -p "Paused after killing the process and deleting old folders. Press Enter to continue..."
fi

# Find the zip file
ZIP_FILE=$(find . -maxdepth 1 -name "edhm-ui-v3-linux-x64.zip")
if [ -z "$ZIP_FILE" ]; then
    echo "Error: Zip file not found."
    exit 1
fi

# Create the target directory and unzip
mkdir -p "$TARGET_DIR"
unzip "$ZIP_FILE" -d "$TARGET_DIR" || { echo "Error: Unzip failed."; exit 1; }

#move unzipped folder
mv "$TARGET_DIR/edhm-ui-v3-linux-x64" "$TARGET_DIR/$APP_NEW_DIR" || { echo "Error: moving unzipped folder failed"; exit 1;}

# Make the executable file executable
chmod +x "$TARGET_DIR/$APP_NEW_DIR/$APP_EXE"

# Create the desktop shortcut
DESKTOP_CONTENT="
[Desktop Entry]
Encoding=UTF-8
Name=EDHM-UI-V3
Exec=$TARGET_DIR/$APP_NEW_DIR/$APP_EXE
"

if [ -f "$ICON_PATH" ]; then
    DESKTOP_CONTENT+="Icon=$ICON_PATH\n"
else
    echo "Warning: Icon file not found."
fi

DESKTOP_CONTENT+="Terminal=false
Type=Application
Comment=Mod for Elite Dangerous to customize the HUD of any ship.
StartupNotify=true
Categories=Utility;
"
echo "$DESKTOP_CONTENT" > "$DESKTOP_FILE"
echo "$DESKTOP_CONTENT" > "$APPLICATIONS_FILE"
chmod +x "$DESKTOP_FILE"
chmod +x "$APPLICATIONS_FILE"

# DEBUG: Pause before running the application
if [ $DEBUG -eq 1 ]; then
    read -p "Press Enter to continue..."
fi

# Run the application
"$TARGET_DIR/$APP_NEW_DIR/$APP_EXE"
if [ $? -eq 0 ]; then
    echo "Application started successfully."
else
    echo "Error: Application failed to start."
fi

echo "Desktop and Application Menu shortcuts created."
