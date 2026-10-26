# Installation tests must never run against the developer's machine.
$ErrorActionPreference = 'Stop'
if ($env:GITHUB_ACTIONS -ne 'true' -or $env:RUNNER_ENVIRONMENT -ne 'github-hosted' -or -not $env:RUNNER_TEMP) {
    throw 'Run only on a disposable GitHub-hosted Windows runner'
}
$appRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../source_v3'))
$buildInfo = Get-Content (Join-Path $appRoot 'out/release/velopack/build-info.json') -Raw | ConvertFrom-Json
$testRoot = Join-Path $env:RUNNER_TEMP 'Velopack lifecycle Ω'
$installDir = Join-Path $testRoot 'App'
if (Test-Path -LiteralPath $testRoot) { throw 'Expected a fresh test directory' }
$settingsPath = Join-Path $env:USERPROFILE 'EDHM_UI/Settings.json'
if (Test-Path -LiteralPath $settingsPath) { throw 'Expected a runner without application settings' }
New-Item -ItemType Directory -Path $testRoot | Out-Null
function Run-Checked([string]$File, [string]$Arguments) {
    $process = Start-Process -FilePath $File -ArgumentList $Arguments -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(120000)) { $process.Kill(); throw "Timed out: $File" }
    if ($process.ExitCode -ne 0) { throw "Failed ($($process.ExitCode)): $File $Arguments" }
}
function Check-Payload {
    $source = Join-Path $appRoot 'out/EDHM-UI-V3-win32-x64'
    foreach ($file in Get-ChildItem -LiteralPath $source -Recurse -File) {
        if ($file.Extension -eq '.pdb') { continue }
        $relative = [IO.Path]::GetRelativePath($source, $file.FullName)
        $installed = Join-Path $installDir "current/$relative"
        if (-not (Test-Path -LiteralPath $installed -PathType Leaf)) { throw "Missing installed payload: $relative" }
        if ((Get-FileHash -LiteralPath $installed).Hash -ne (Get-FileHash -LiteralPath $file.FullName).Hash) {
            throw "Installed payload mismatch: $relative"
        }
    }
}
$installer = Join-Path $appRoot 'out/release/edhm-ui-v3-windows-x64.exe'
Run-Checked $installer "--silent --installto `"$installDir`" --log `"$testRoot/install.log`""
Check-Payload
if (Test-Path -LiteralPath $settingsPath) { throw 'Silent installation started the application and created settings' }
# Exercise all four hooks through the real installed, non-elevating entry point.
& (Join-Path $PSScriptRoot 'test-velopack-hooks.ps1') `
    -Executable (Join-Path $installDir 'current/EDHM-UI-V3.Launcher.exe') -Version $buildInfo.appVersion
if (Test-Path -LiteralPath $settingsPath) { throw 'Installer hook started Electron and created settings' }
$registration = Get-ItemProperty -LiteralPath "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\$($buildInfo.packageId)"
if ($registration.DisplayVersion -ne $buildInfo.appVersion) { throw 'Incorrect registered app version' }
foreach ($folder in @([Environment]::GetFolderPath('Desktop'), [Environment]::GetFolderPath('Programs'))) {
    $shortcutPath = Join-Path $folder 'EDHM-UI-V3.lnk'
    if (-not (Test-Path -LiteralPath $shortcutPath)) { throw "Missing shortcut: $shortcutPath" }
    $shortcut = (New-Object -ComObject WScript.Shell).CreateShortcut($shortcutPath)
    if ([IO.Path]::GetFileName($shortcut.TargetPath) -ne 'EDHM-UI-V3.Launcher.exe') {
        throw 'Velopack shortcut bypasses the non-elevating launcher'
    }
}
# Full-installer updates remain the app's update mechanism; exercise a repeated install.
Run-Checked $installer "--silent --installto `"$installDir`" --log `"$testRoot/reinstall.log`""
Check-Payload
if (Test-Path -LiteralPath $settingsPath) { throw 'Silent reinstallation started the application' }
Run-Checked (Join-Path $installDir 'Update.exe') "uninstall --silent --log `"$testRoot/uninstall.log`""
if (Test-Path -LiteralPath (Join-Path $installDir 'current/EDHM-UI-V3.exe')) { throw 'Uninstall left the application executable' }
if (Test-Path -LiteralPath "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\$($buildInfo.packageId)") {
    throw 'Uninstall left the registration'
}
Write-Host 'Velopack payload, startup hook, registration, shortcuts, reinstall, and uninstall checks passed.'
