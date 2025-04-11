@ECHO OFF
SETLOCAL ENABLEEXTENSIONS
SETLOCAL ENABLEDELAYEDEXPANSION
TITLE EDHM MakeCert
COLOR 0A

ECHO =========================================================================
ECHO ***** CERTIFICADO DIGITAL PARA ESNAMBLADOS DE APLICACIONES **************
ECHO Este proceso utiliza la herramienta 'MakeCert' que viene con el SDK de 
ECHO Windows 10 para crear un Certificado Digital para firmar los Ensamblados.
ECHO Asegurese de tener el Windows SDK instalado.
ECHO ------------------------------------------------------------------- 
REM https://docs.microsoft.com/en-us/windows/win32/appxpkg/how-to-create-a-package-signing-certificate
ECHO Use esta herramienta cuando el Certificado haya Caducado o si necesita uno nuevo
ECHO Genera 3 archivos:
ECHO - .cer  El Certificado Digital
ECHO - .pvk  La Clave Privada del Certificado
ECHO - .pfx  Certificado Especial que contiene la Clave adentro.
ECHO ------------------------------------------------------------------- 
ECHO Autor: Jhollman Chacon @ CUTCSA - DPTO.INFORMATICO - 2022
ECHO ===================================================================
REM *****  Aqui Establecemos las Variables  *********************

rem Ruta donde estan los ejecutables 'makecert.exe' y 'pvk2pfx.exe'
SET WinSDKPath="C:\Program Files (x86)\Windows Kits\10\bin\10.0.17763.0\x64\"

rem Ruta donde se Copiaran los Certificados Generados
SET SavePath="G:\Soft TEST\ED UI mk2\EDHM_UI_mk2\Installer\"

rem Nombre de la Aplicacion
SET AppName="EDHM_UI"

rem Contraseña para el Certificado
SET CertPass="@Namllohj1975"

rem Fecha de Expiracion del Certificado
SET ExpireDate=12/31/2028
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

MakeCert /n "CN=%AppName%, O=%AppName%, C=US" /a SHA256 /r /h 0 /eku "1.3.6.1.5.5.7.3.3,1.3.6.1.4.1.311.10.3.13" /e %ExpireDate% /sv %AppName%.pvk %AppName%.cer
Pvk2Pfx /pvk %AppName%.pvk /pi %CertPass% /spc %AppName%.cer /pfx %AppName%.pfx /f

COPY %WinSDKPath%%AppName%.pvk %SavePath%%AppName%.pvk /Y
COPY %WinSDKPath%%AppName%.cer %SavePath%%AppName%.cer /Y
COPY %WinSDKPath%%AppName%.pfx %SavePath%%AppName%.pfx /Y

ECHO Certificado Creado en %WinSDKPath%
ECHO ===================================================================
PAUSE
:thehell
EXIT /B %ERRORLEVEL%
