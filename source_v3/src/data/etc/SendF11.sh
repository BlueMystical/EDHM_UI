#!/bin/bash

# Define the part of the window title you are looking for
window_title_part="Elite - Dangerous"

# --- Step 1: Check if xdotool is installed ---
# 'command -v xdotool' searches for the 'xdotool' command in the PATH
# '&> /dev/null' redirects both standard output and errors to /dev/null (silences the command)
# 'if ! ...' executes the block if the previous command fails (i.e., xdotool is not found)
if ! command -v xdotool &> /dev/null; then
    # Print error messages to standard error (>&2), which can be captured by the launching app
    echo "Error: 'xdotool' is not installed." >&2
    echo "This script requires 'xdotool' to interact with windows." >&2
    echo "Please install it using your distribution's package manager:" >&2
    echo "  On Debian/Ubuntu: sudo apt update && sudo apt install xdotool" >&2
    echo "  On Fedora: sudo dnf install xdotool" >&2
    echo "  On Arch Linux: sudo pacman -S xdotool" >&2
    echo "  On openSUSE: sudo zypper install xdotool" >&2
    exit 1 # Exit the script with error code 1 (indicating failure)
fi
# --- End of xdotool check ---

# --- Step 2: If xdotool is installed, proceed with finding the window and sending the key ---

# Search for the window ID by its title.
# Keep potential errors from xdotool search visible on stderr by not redirecting stderr here.
window_id=$(xdotool search --onlyvisible --limit 1 --name "$window_title_part")

# Check if any window was found
if [ -z "$window_id" ]; then
    # Print window not found error message to standard error (visible to launching app)
    echo "Error: No window found with title containing '$window_title_part'." >&2
    exit 1 # Exit the script with error code 1
else
    # Print success messages to /dev/null to hide them (invisible to launching app)
    echo "Window found (ID: $window_id). Sending key F11..." >/dev/null

    # Optional: Activate/focus the window before sending the key
    # Redirect output/errors from windowactivate to /dev/null if you uncomment this
    # xdotool windowactivate "$window_id" >/dev/null 2>&1
    # If you activate, sometimes it's good to wait a moment:
    # sleep 0.1

    # Send the F11 key to the found window.
    # Keep stderr visible in case xdotool key fails.
    xdotool key --window "$window_id" F11

    # Print success message to /dev/null to hide it (invisible to launching app)
    echo "Key F11 sent." >/dev/null
    exit 0 # Exit the script with success code 0
fi

