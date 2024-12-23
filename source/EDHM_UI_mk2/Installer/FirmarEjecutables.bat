@ECHO OFF
SETLOCAL ENABLEEXTENSIONS
SETLOCAL ENABLEDELAYEDEXPANSION
TITLE EDHM SignTool
COLOR 0A

ECHO ===================================================================
ECHO *********  FIRMA DIGITAL DE ARCHIVOS EJECUTABLES   ****************
ECHO Este proceso usa la herramienta 'SignTool' que viene con el SDK de 
ECHO Windows 10 para firmar digitalmente los archivos que se van a distribuir.
ECHO Asegurese de tener el Windows SDK instalado.
ECHO -------------------------------------------------------------------
ECHO - Compile primero el Programa que quiere Firmar, 
ECHO - entonces usas esto para firmar los ejecutables y el Instalador.
ECHO -------------------------------------------------------------------
REM https://docs.microsoft.com/en-us/windows-hardware/drivers/devtest/signtool
ECHO Autor: Jhollman Chacon @ CUTCSA - DPTO.INFORMATICO - 2022
ECHO ===================================================================
REM *****  Aqui Establecemos las Variables  *********************

rem Ruta donde estan los ejecutables 'signtool.exe'
SET WinSDKPath="C:\Program Files (x86)\Windows Kits\10\bin\10.0.17763.0\x64\"

rem Ruta del Certificado a usar, .pfx es el Recomendado
SET CertificatePath="G:\Soft TEST\ED UI mk2\EDHM_UI_mk2\Installer\EDHM_UI.pfx"

rem URL del Servicio de Hora para establecer la marca 'TimeStamp'
SET TimeStampURL="http://timestamp.entrust.net/TSS/RFC3161sha2TS"

rem Contraseña para el Certificado
SET CertPass=@Namllohj1975

rem Ruta donde estan los archivos que queremos Firmar
SET ExecutablePath="G:\Soft TEST\ED UI mk2\EDHM_UI_mk2\bin\Debug"
REM -------------------------------------------------------------------
IF "%~1" == "-silent" GOTO :inicio
:choice
set /P c=Empezamos ya?:[S/N]
if /I "%c%" EQU "S" goto :inicio
if /I "%c%" EQU "N" goto :thehell
goto :choice
REM -------------------------------------------------------------------

:inicio

C:
CD %WinSDKPath%
signtool sign /f %CertificatePath% /p %CertPass% /fd SHA256 /tr %TimeStampURL% /td sha256 /v %ExecutablePath%\EDHM_UI_mk2.exe
signtool sign /f %CertificatePath% /p %CertPass% /fd SHA256 /tr %TimeStampURL% /td sha256 /v %ExecutablePath%\EDHM_UI_Patcher.exe
signtool sign /f %CertificatePath% /p %CertPass% /fd SHA256 /tr %TimeStampURL% /td sha256 /v %ExecutablePath%\EDHM_UI_Thumbnail_Maker.exe

REM signtool sign /a /fd SHA256 /v %InstallerPath%\EDHM_UI_Setup.msi

ECHO ===================================================================
PAUSE
:thehell
EXIT /B %ERRORLEVEL%