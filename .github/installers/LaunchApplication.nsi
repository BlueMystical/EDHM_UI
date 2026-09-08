; MSI's direct EXE action cannot launch an app requiring UAC elevation (error 740).
; This unelevated helper delegates to ShellExecuteEx so the app's manifest decides
; whether Windows should prompt. Installation has already completed at this point.
Unicode true
RequestExecutionLevel user
SilentInstall silent
SetCompress off
Name "EDHM-UI-V3 launcher"
OutFile "${OutputExe}"
VIProductVersion "${AppVersion}.0"
VIAddVersionKey /LANG=1033 "ProductName" "EDHM-UI-V3"
VIAddVersionKey /LANG=1033 "FileDescription" "Launch EDHM-UI-V3 after setup"
VIAddVersionKey /LANG=1033 "LegalCopyright" "Blue Mystic"
VIAddVersionKey /LANG=1033 "FileVersion" "${AppVersion}"
VIAddVersionKey /LANG=1033 "ProductVersion" "${AppVersion}"
!include "FileFunc.nsh"

Section
  ${GetParameters} $0
  ClearErrors
  ${GetOptions} $0 "/app=" $1
  IfErrors invalid
  StrCmp $1 "" invalid
  ${GetParent} "$1" $2
  SetOutPath "$2"
  ; Windows owns the normal UAC prompt, cancellation, and any launch error UI.
  ExecShell /ALLOWERRORUI "open" "$1" "" SW_SHOWNORMAL
  IfErrors failed
  SetErrorLevel 0
  Quit
  invalid:
    SetErrorLevel 87
    Quit
  failed:
    SetErrorLevel 1
SectionEnd
