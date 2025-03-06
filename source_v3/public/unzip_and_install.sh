#!/bin/bash

# Define variables
TARGET_DIR="$HOME/.local/share/"
APP_NEW_DIR="EDHM-UI-V3"
APP_EXE="edhm-ui-v3"
DESKTOP_FILE="$HOME/Desktop/edhm-ui-v3.desktop"
APPLICATIONS_FILE="$HOME/.local/share/applications/EDHM-UI-V3.desktop"
ICON_PATH="$TARGET_DIR/$APP_NEW_DIR/resources/images/icon.png" 

# Find the zip file
ZIP_FILE=$(find . -maxdepth 1 -name "edhm-ui-v3-linux-x64.zip")

# Check if the zip file was found
if [ -z "$ZIP_FILE" ]; then
  echo "Error: Zip file matching 'edhm-ui-v3-linux-x64-*.zip' not found in the current directory."
  exit 1
fi

# Create the target directory if it doesn't exist
mkdir -p "$TARGET_DIR"

# Unzip the file
unzip "$ZIP_FILE" -d "$TARGET_DIR"

# Find the unzipped directory
UNZIPPED_DIR=$(find "$TARGET_DIR" -maxdepth 1 -type d -name "edhm-ui-v3-linux-x64")

# Check if the unzipped directory was found
if [ -z "$UNZIPPED_DIR" ]; then
  echo "Error: Unzipped application directory not found."
  exit 1
fi

# Rename the unzipped directory
mv "$UNZIPPED_DIR" "$TARGET_DIR/$APP_NEW_DIR"

# Make the executable file executable
chmod +x "$TARGET_DIR/$APP_NEW_DIR/$APP_EXE"

# Create .desktop file content
DESKTOP_CONTENT="
[Desktop Entry]
Encoding=UTF-8
Name=EDHM-UI-V3
Exec=$TARGET_DIR/$APP_NEW_DIR/$APP_EXE
Icon=$ICON_PATH
Terminal=false
Type=Application
Comment=Mod for Elite Dangerous to customize the HUD of any ship.
StartupNotify=true
Categories=Utility;
"

# Create desktop shortcut
echo "$DESKTOP_CONTENT" > "$DESKTOP_FILE"
chmod +x "$DESKTOP_FILE"

# Create applications menu shortcut
echo "$DESKTOP_CONTENT" > "$APPLICATIONS_FILE"
chmod +x "$APPLICATIONS_FILE"

# Run the application
"$TARGET_DIR/$APP_NEW_DIR/$APP_EXE"

# Optional: Add a check for successful execution
if [ $? -eq 0 ]; then
  echo "Application started successfully."
else
  echo "Error: Application failed to start."
fi

echo "Desktop and Application Menu shortcuts created."
