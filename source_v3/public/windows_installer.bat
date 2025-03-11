@echo off
echo Starting Windows Installer Script

REM Kill running instances of the application
taskkill /F /IM EDHM-UI-V3.exe
if %errorlevel% neq 0 (
    echo Warning: EDHM-UI-V3.exe process not found or could not be terminated.
) else (
    echo Taskkill finished.
)

REM Set the installer EXE name
set INSTALLER_EXE="edhm-ui-v3-windows-x64.exe"

REM Check if the installer EXE exists in the same directory
if not exist "%INSTALLER_EXE%" (
    echo Error: Installer EXE "%INSTALLER_EXE%" not found.
    exit /b 1
)

echo Found installer: "%INSTALLER_EXE%"

REM Launch the installer
start "" "%INSTALLER_EXE%"
echo Installer started.

exit /b 0