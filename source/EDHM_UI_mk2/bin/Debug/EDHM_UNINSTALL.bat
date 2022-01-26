@ECHO OFF
SETLOCAL ENABLEEXTENSIONS
SETLOCAL ENABLEDELAYEDEXPANSION

COLOR 70

SET MOD_Name=UNINSTALLER
SET KEY_NAME="HKEY_CURRENT_USER\SOFTWARE\Elte Dangerous\Mods\EDHM"
SET VALUE_NAME=ED_Odissey

TITLE EDHM %MOD_Name%
ECHO ===================================================================
ECHO ******************** EDHM %MOD_Name%  **************************
ECHO ** This will un-install EDHM files from your system **
ECHO -------------------------------------------------------------------
REM Gets the game paths from Windows Registry:
FOR /F "usebackq skip=2 tokens=1,2*" %%A IN (`REG QUERY %KEY_NAME% /v ED_Odissey 2^>nul`) DO (
    set ValueName=%%A
    set ValueType=%%B
    set ODYS_PATH=%%C
)
FOR /F "usebackq skip=2 tokens=1,2*" %%A IN (`REG QUERY %KEY_NAME% /v ED_Horizons 2^>nul`) DO (
    set ValueName=%%A
    set ValueType=%%B
    set HORIS_PATH=%%C
)
IF DEFINED ODYS_PATH (
  @echo Elite Dangerous Odissey path: 
  @echo '%ODYS_PATH%'
) else (
  REM Not everyone has Odyssey, so this is ok, but still will inform it:
  @echo ERROR 404, '%KEY_NAME%\ED_Odissey' not found.
)
IF DEFINED HORIS_PATH (
  @echo Elite Dangerous Horizons path: 
  @echo '%HORIS_PATH%'
) else (
  REM If Horizons path doesnt exits then we dont have nothing to do
  @echo ERROR 404, '%KEY_NAME%\ED_Horizons' not found.
  goto :thehell
)
ECHO -------------------------------------------------------------------
ECHO UN-INSTALLING HORIZONS MOD..
IF DEFINED HORIS_PATH (
 ECHO %HORIS_PATH%
 CD %HORIS_PATH%

 DEL "%HORIS_PATH%\d3d11.dll"
 DEL "%HORIS_PATH%\d3d11_log.txt"
 DEL "%HORIS_PATH%\d3d11_profile_log.txt"

 DEL "%HORIS_PATH%\d3dcompiler_46.dll"
 DEL "%HORIS_PATH%\d3dcompiler_46_log.txt"

 DEL "%HORIS_PATH%\nvapi64.dll"
 DEL "%HORIS_PATH%\nvapi_log.txt"

 DEL "%HORIS_PATH%\d3dx.ini"
 DEL "%HORIS_PATH%\ShaderUsage.txt"
 DEL "%HORIS_PATH%\EDHM-Uninstall.bat"

 DEL "%HORIS_PATH%\EDHM-v1.5-Catalogue.pdf"
 DEL "%HORIS_PATH%\EDHM-v1.51-Manual.pdf"
 DEL "%HORIS_PATH%\EDHM-v1.51-Profile-Guide.pdf"
 DEL "%HORIS_PATH%\EDHM-Keybinds-Essential.bat"
 DEL "%HORIS_PATH%\EDHM-Keybinds-Full.bat"
 DEL "%HORIS_PATH%\EDHM-RemoveDemos.bat"

 RMDIR /s /q "%HORIS_PATH%\ShaderFixes"
 RMDIR /s /q "%HORIS_PATH%\EDHM-ini"
 RMDIR /s /q "%HORIS_PATH%\ShaderCache"
)
ECHO -------------------------------------------------------------------
ECHO UN-INSTALLING ODISSEY MOD..
IF DEFINED ODYS_PATH (
 CD %ODYS_PATH%

 DEL "%ODYS_PATH%\d3d11.dll"
 DEL "%ODYS_PATH%\d3d11_log.txt"
 DEL "%ODYS_PATH%\d3d11_profile_log.txt"

 DEL "%ODYS_PATH%\d3dcompiler_46.dll"
 DEL "%ODYS_PATH%\d3dcompiler_46_log.txt"

 DEL "%ODYS_PATH%\nvapi64.dll"
 DEL "%ODYS_PATH%\nvapi_log.txt"

 DEL "%ODYS_PATH%\d3dx.ini"
 DEL "%ODYS_PATH%\ShaderUsage.txt"
 DEL "%ODYS_PATH%\EDHM-Uninstall.bat"

 RMDIR /s /q "%ODYS_PATH%\ShaderFixes"
 RMDIR /s /q "%ODYS_PATH%\EDHM-ini"
 RMDIR /s /q "%ODYS_PATH%\ShaderCache"
)
ECHO -------------------------------------------------------------------
ECHO.
ECHO      -`                                                                                              `- 
ECHO      oo:`                                                                                          `:oo 
ECHO      oooo/.                                                                                      ./oooo 
ECHO      oooooo/.                                                                                  ./oooooo 
ECHO      :ooooooo+-                                 Uninstalled                                 -+ooooooo: 
ECHO       -oooooooo+-`                  Elite Dangerous HUD Mod (EDHM)                        `-+oooooooo-  
ECHO        `:+ooooooo+:`                                                                    `:+oooooooo:`   
ECHO          `:+oooooooo:`                                                                `:oooooooo+:`     
ECHO            `-+oooooooo/.                                                            ./oooooooo+-`       
ECHO             :::+oooooooo/.`.                                                    .`./oooooooo+:::        
ECHO             /oo/:oooooooo+-o/.                                                ./o-+oooooooo:/oo/        
ECHO             :ooooooooooooo.ooo/.                                            ./ooo.ooooooooooooo:        
ECHO             -ooooooooooooo-+oooo+-`                                      `-+oooo+-ooooooooooooo-        
ECHO              :oooooooooooo+-oooooo+-`                                  `-+oooooo-+oooooooooooo:         
ECHO               `:ooooooooooo/-ooooooo+-`                              `-+ooooooo-/ooooooooooo:`          
ECHO                 `:oooooooooo/.:oooooooo:`           -  -           `:oooooooo:./oooooooooo:`            
ECHO                   `:oooooooo+:+::oooooooo:`        `+  +`        `:oooooooo::+:+oooooooo:`              
ECHO                    +::oooooo+:ooo::oooooooo/`      .o  o.      `:oooooooo::ooo:+oooooo::+               
ECHO                    +oo:oooooo.ooooo:/oooooooo      -o  o-      oooooooo/:ooooo.oooooo:oo+               
ECHO                    /oooooooooo::oooo.oooooooo      :o//o:      oooooooo.oooo::oooooooooo/               
ECHO                    `/ooooooooooo::+/:oooooo/`      .oooo.      `/oooooo:/+::ooooooooooo/`               
ECHO                      .+oooooooooo+`/oooooo:      `--:oo:--`      :oooooo/`+oooooooooo+.                 
ECHO                        -+oooooooo.oooooooo:     `-:oo::oo:-`     :oooooooo.oooooooo+-                   
ECHO                          .+oooooo-:ooooooooo-  -:::/oooo/:::-  -ooooooooo:-oooooo+.                     
ECHO                            ./ooooo+:/oooooooooooo:` `::` `:oooooooooooo/:+ooooo/.                       
ECHO                              ./oooooo::+ooooooooooo`/++/`oooooooooooo::oooooo/.                         
ECHO                              :+:/oooooo/:+ooooooooo/-oo-/ooooooooo+:/oooooo/:+:                         
ECHO                              -oo+:/oooooo/:/oooooooo`oo`oooooooo/:/oooooo/:+oo-                         
ECHO                               /ooo+::oooooo+:/oooooo-//-oooooo/:+oooooo::+ooo/                          
ECHO                                ./ooo+::+ooooo+::oooo+..+oooo::+ooooo+::+ooo/.                           
ECHO                                  `/ooo+:/oooooo+::+oo``oo+::+oooooo/:+ooo/`                             
ECHO                                    `:ooo-ooooooooo:-/::/-:ooooooooo-ooo:`                               
ECHO                                      `:o-ooooooooooo/``/ooooooooooo-o:`                                 
ECHO                                        ``+oooooooooo/--/oooooooooo+``                                   
ECHO                                          `-+ooooooo+-oo-/ooooooo+-`                                     
ECHO                                            `-+oooo+.oooo.+oooo+-`                                       
ECHO                                               -+oo.+o..o+.oo+-                                          
ECHO                                                 ../o///:o/..                                            
ECHO                                                  .oo.oo.oo.                                             
ECHO                                                   .:-oo-:.                                              
ECHO                                                     /oo/                                                
ECHO                                                     oooo                                                
ECHO                                                    .oooo.                                               
ECHO                                                    `+oo+`                                               
ECHO                                                      --                                                 
ECHO.
ECHO.
ECHO.
ECHO  Thanks for trying our mod! Good hunting CMDR o7
:thehell 
ECHO -------------------------------------------------------------------
REM IF YOU WANT TO RUN THIS BATCH MANUALLY THEN UNCOMMENT THE LINE BELOW:
REM PAUSE
EXIT %ERRORLEVEL%