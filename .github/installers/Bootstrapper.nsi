; The EXE only transports the MSI. Windows Installer owns upgrade/repair/uninstall.
Unicode true
RequestExecutionLevel user
SilentInstall silent
; The embedded MSI cabinet is already compressed; avoid recompressing 250+ MB.
SetCompress off
Name "EDHM-UI-V3"
OutFile "${OutputExe}"
Icon "${AppIcon}"
VIProductVersion "${AppVersion}.0"
VIAddVersionKey /LANG=1033 "ProductName" "EDHM-UI-V3"
VIAddVersionKey /LANG=1033 "FileDescription" "EDHM-UI-V3 Setup"
VIAddVersionKey /LANG=1033 "CompanyName" "Blue Mystic"
VIAddVersionKey /LANG=1033 "LegalCopyright" "Blue Mystic"
VIAddVersionKey /LANG=1033 "FileVersion" "${AppVersion}"
VIAddVersionKey /LANG=1033 "ProductVersion" "${AppVersion}"
!include "FileFunc.nsh"
!include "LogicLib.nsh"

Section
  ; Keep the source MSI for Windows Installer maintenance and rollback.
  SetOutPath "$LOCALAPPDATA\Blue Mystic\EDHM-UI-V3\Installer\${AppVersion}\${ProductCode}"
  ClearErrors
  File /oname=EDHM-UI-V3.msi "${SourceMsi}"
  ${If} ${Errors}
    SetErrorLevel 1603
    Quit
  ${EndIf}
  ${GetParameters} $0
  ClearErrors
  ExecWait '"$SYSDIR\msiexec.exe" /i "$OUTDIR\EDHM-UI-V3.msi" $0' $1
  ${If} ${Errors}
    SetErrorLevel 1603
  ${Else}
    SetErrorLevel $1
  ${EndIf}
SectionEnd
