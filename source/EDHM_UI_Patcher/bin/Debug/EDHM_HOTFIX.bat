@ECHO OFF
SETLOCAL ENABLEEXTENSIONS
SETLOCAL ENABLEDELAYEDEXPANSION

MODE CON: cols=80 lines=30
COLOR 70

SET MOD_Name=UPDATE7_HOTFIX
SET KEY_NAME="HKEY_CURRENT_USER\SOFTWARE\Elte Dangerous\Mods\EDHM"
SET VALUE_NAME=ED_Odissey

TITLE EDHM %MOD_Name%
ECHO ===================================================================
ECHO **************** EDHM %MOD_Name%  ***********************
ECHO ** This will fix shaders in your Odyssey Game **
ECHO -------------------------------------------------------------------
REM Gets UI path from Windows Registry:
FOR /F "usebackq skip=2 tokens=1,2*" %%A IN (`REG QUERY %KEY_NAME% /v %VALUE_NAME% 2^>nul`) DO (
    set ValueName=%%A
    set ValueType=%%B
    set GAME_PATH=%%C
)
IF DEFINED GAME_PATH (
  @echo Elite Dangerous Odyssey path: 
  @echo '%GAME_PATH%'
) else (
  @echo ERROR 404, '%KEY_NAME%\%VALUE_NAME%' not found.
  goto :thehell
)
ECHO -------------------------------------------------------------------
ECHO Applying fix..
CD %GAME_PATH%

IF EXIST "%GAME_PATH%\ShaderFixes\cf4b6740c7f9cd4b-ps.txt" (
 DEL "%GAME_PATH%\ShaderFixes\cf4b6740c7f9cd4b-ps.txt" 
) 
IF EXIST "%GAME_PATH%\ShaderFixes\05d9140e9c40fbd6-ps.txt" (
 DEL "%GAME_PATH%\ShaderFixes\05d9140e9c40fbd6-ps.txt" 
)
IF EXIST "%GAME_PATH%\ShaderFixes\18d0630271021aea-ps.txt" (
 DEL "%GAME_PATH%\ShaderFixes\18d0630271021aea-ps.txt" 
)
IF EXIST "%GAME_PATH%\ShaderFixes\19b261d1497c18fb-ps.txt" (
 DEL "%GAME_PATH%\ShaderFixes\19b261d1497c18fb-ps.txt" 
)
IF EXIST "%GAME_PATH%\ShaderFixes\745e27820ef3142d-ps.txt" (
 DEL "%GAME_PATH%\ShaderFixes\745e27820ef3142d-ps.txt" 
)
IF EXIST "%GAME_PATH%\ShaderFixes\4363dc4ff0e9ba7c-ps.txt" (
 DEL "%GAME_PATH%\ShaderFixes\4363dc4ff0e9ba7c-ps.txt" 
)
IF EXIST "%GAME_PATH%\ShaderFixes\ae8b226526f19ee0-ps.txt" (
 DEL "%GAME_PATH%\ShaderFixes\ae8b226526f19ee0-ps.txt" 
)
IF EXIST "%GAME_PATH%\ShaderFixes\e7acc5ec72327a3f-ps.txt" (
 DEL "%GAME_PATH%\ShaderFixes\e7acc5ec72327a3f-ps.txt" 
)
ECHO Done.

:thehell 
ECHO -------------------------------------------------------------------
EXIT %ERRORLEVEL%