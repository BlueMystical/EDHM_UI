#!/bin/bash

# Define the JSON filename
JSON_FILE="package.json"

# Check if the JSON file exists
if [ ! -f "$JSON_FILE" ]; then
  echo "Error: JSON file '$JSON_FILE' not found in the current directory."
  exit 1
fi

# Check if jq is installed
if ! command -v jq >/dev/null 2>&1; then
  echo "jq is not installed. Attempting to install it using apt..."
  # Attempt to update package lists and install jq
  if sudo apt update && sudo apt install -y jq; then
    echo "jq installed successfully."
  else
    echo "Error: Failed to install jq. Please ensure you have sudo privileges and internet access."
    echo "You can try installing it manually using: sudo apt update && sudo apt install jq"
    exit 1
  fi
fi

# Prompt the user for the new version number
read -p "Enter the new version number: " NEW_VERSION

# Check if the user provided a version number
if [ -z "$NEW_VERSION" ]; then
  echo "Error: Version number cannot be empty."
  exit 1
fi

# Use jq to update the version in the JSON file in-place
jq --arg new_version "$NEW_VERSION" '.version = $new_version' "$JSON_FILE" > temp.json && mv temp.json "$JSON_FILE"

echo "Version updated to: $NEW_VERSION in '$JSON_FILE'"

# Build the app using npm
echo "Building the app..."
npm run make -- --arch="x64" --targets="@electron-forge/maker-zip"

# Check the exit code of the npm command
if [ $? -eq 0 ]; then
  echo "App build completed successfully."
else
  echo "Error during app build."
fi

exit 0
