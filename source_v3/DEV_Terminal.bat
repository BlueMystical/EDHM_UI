@echo off
REM Simple Bat to setup a Terminal console for Development

rem 1. Go to the current BAT directory
cd /d "%~dp0"

rem 2. Open current folder in Explorer
start "" explorer .

rem 3. Open current folder in VS Code
start "EDHM_UI v3 Dev Console" code .

exit 0