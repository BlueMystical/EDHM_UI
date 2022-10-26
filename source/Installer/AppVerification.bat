@ECHO OFF
SETLOCAL ENABLEEXTENSIONS
SETLOCAL ENABLEDELAYEDEXPANSION
TITLE EDHM Windows App Certification Kit
COLOR 0A

ECHO ===================================================================
ECHO *********  VERIFICADOR DE APLICACIONES   **********************
ECHO Este proceso utiliza la herramienta 'appcert' que viene con el SDK de 
ECHO Windows 10 para verficar el Programa de Instalacion de la Aplicacion.
ECHO ------------------------------------------------------------------- 
ECHO - Compile primero el Programa. 
ECHO - Firme los ejecutables
ECHO - Cree el Setup
ECHO - Firme el Instalador
ECHO - Use esto para Verificar el Instalador
ECHO -------------------------------------------------------------------
ECHO Autor: Jhollman Chacon @ CUTCSA - DPTO.INFORMATICO - 2022
ECHO ===================================================================
SET WorkingPath="C:\Program Files (x86)\Windows Kits\10\App Certification Kit\"
SET InstallerPath="G:\Soft TEST\ED UI mk2\EDHM_UI_mk2\Installer"
REM -------------------------------------------------------------------
IF "%~1" == "-silent" GOTO :inicio
:choice
set /P c=Would you like to Proceed?:[Y/N]
if /I "%c%" EQU "Y" goto :inicio
if /I "%c%" EQU "N" goto :thehell
goto :choice
REM -------------------------------------------------------------------

:inicio
C:
CD %WorkingPath%
appcert.exe reset
appcert test -apptype desktop -setuppath %InstallerPath%\EDHM_UI_Setup.msi -appusage peruser -reportoutputpath %InstallerPath%\EDHM_UI_Report.xml
appcert.exe finalizereport -reportfilepath %InstallerPath%\EDHM_UI_Report.xml

ECHO ===================================================================
PAUSE
:thehell
EXIT /B %ERRORLEVEL%