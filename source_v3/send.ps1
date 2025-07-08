Add-Type -AssemblyName System.Windows.Forms
$wshell = New-Object -ComObject wscript.shell
if ($wshell.AppActivate("Elite - Dangerous")) {
  Start-Sleep -Milliseconds 300
  [System.Windows.Forms.SendKeys]::SendWait("{F11}")
  Start-Sleep -Milliseconds 100
} else { Write-Host " No se pudo activar la ventana." }
Exit