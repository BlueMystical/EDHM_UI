@echo off
title EDHM Installer Script
echo Starting Installer Script..

REM Set mode: DEBUG=1 for debugging, DEBUG=0 for production
set DEBUG=0

REM Attempt to kill running instances of the application
echo Searching for EDHM-UI-V3.exe process...
tasklist /FI "IMAGENAME eq EDHM-UI-V3.exe" 2>NUL | find /I "EDHM-UI-V3.exe" >NUL
if errorlevel 1 (
    echo Info: EDHM-UI-V3.exe process not found.
) else (
    echo Terminating EDHM-UI-V3.exe...
    taskkill /F /IM EDHM-UI-V3.exe >NUL
    echo Info: EDHM-UI-V3.exe process terminated.
)

REM Set the installer EXE name
set INSTALLER_EXE="%~dp0edhm-ui-v3-windows-x64.exe"

REM Check if the installer EXE exists in the same directory
if not exist "%INSTALLER_EXE%" (
    echo Error: Installer EXE "%INSTALLER_EXE%" not found.
    exit /b 1
)

echo Found installer: %INSTALLER_EXE%

REM Launch the installer
echo Starting installer: %INSTALLER_EXE%
start "" "%INSTALLER_EXE%"
echo Installer started.

REM Pause for debugging only if DEBUG=1
if "%DEBUG%"=="1" (
    PAUSE
)

echo ---- YOU CAN CLOSE THIS WINDOW NOW ----
exit /b 0
