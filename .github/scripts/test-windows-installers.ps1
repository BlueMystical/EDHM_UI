# Installs/uninstalls real packages: deliberately restricted to disposable hosted CI.
$ErrorActionPreference = 'Stop'
if ($env:GITHUB_ACTIONS -ne 'true' -or $env:RUNNER_ENVIRONMENT -ne 'github-hosted') {
  throw 'Installer integration tests must run on a disposable GitHub-hosted Windows runner.'
}
$appRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../source_v3'))
$testRoot = Join-Path $appRoot 'out/installer-tests'
New-Item -ItemType Directory -Force -Path $testRoot | Out-Null
$free = Join-Path $appRoot 'out/release/free/edhm-ui-v3-windows-x64.exe'
$msi = Join-Path $appRoot 'out/release/free/EDHM-UI-V3.msi'
$manifest = Get-Content (Join-Path $appRoot 'out/release/free/payload-manifest.json') -Raw | ConvertFrom-Json
$version = (Get-Content (Join-Path $appRoot 'package.json') -Raw | ConvertFrom-Json).version
$installer = New-Object -ComObject WindowsInstaller.Installer
$upgradeCode = '{B446308B-0441-4301-84CF-79B7159894F5}'
function Get-Products { return @($installer.RelatedProducts($upgradeCode)) }
if ((Get-Products).Count -ne 0) { throw 'Runner already has EDHM-UI installed' }
function Invoke-Setup([string]$File, [string]$Arguments, [int[]]$Expected = @(0,3010)) {
  $process = Start-Process -FilePath $File -ArgumentList $Arguments -WindowStyle Hidden -PassThru
  if (-not $process.WaitForExit(300000)) { throw 'Installer timed out after five minutes' }
  if ($process.ExitCode -notin $Expected) { throw "Installer failed: $File exit $($process.ExitCode)" }
}
function Assert-Installed([string]$Directory) {
  $products = @(Get-Products)
  if ($products.Count -ne 1) { throw "Expected one registered product, found $($products.Count)" }
  if ($installer.ProductInfo($products[0], 'VersionString') -ne $version) { throw 'Incorrect installed version' }
  foreach ($entry in $manifest) {
    $path = Join-Path $Directory $entry.path
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing installed file: $($entry.path)" }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $entry.sha256) { throw "Incorrect installed payload: $($entry.path)" }
  }
}
function Uninstall-Current([string]$Name) {
  foreach ($product in Get-Products) {
    Invoke-Setup msiexec.exe "/x $product /qn /norestart /l*v `"$testRoot/$Name.log`""
  }
  if ((Get-Products).Count -ne 0) { throw 'Uninstall left a registered product' }
}
$customDirectory = Join-Path $env:LOCALAPPDATA 'EDHM CI Custom Folder'
$defaultDirectory = Join-Path $env:LOCALAPPDATA 'EDHM-UI-V3'
$userDataDirectory = Join-Path $env:APPDATA 'edhm-ui-v3'
New-Item -ItemType Directory -Force -Path $userDataDirectory | Out-Null
$sentinel = Join-Path $userDataDirectory 'installer-ci-preserve.txt'
[IO.File]::WriteAllText($sentinel, 'preserve my settings')
try {
  # Use an actual upstream release to check MSI-family interoperability.
  $upstream = Join-Path $testRoot 'upstream-3.0.70.exe'
  Invoke-WebRequest 'https://github.com/BlueMystical/EDHM_UI/releases/download/v3.0.70/edhm-ui-v3-windows-x64.exe' -OutFile $upstream
  if ((Get-FileHash $upstream -Algorithm SHA256).Hash -ne '8E3B683C36F67AD3BDC7103820A9882100212FDE033A3B87C95C767CD3F102E8') { throw 'Upstream test fixture checksum changed' }
  Invoke-Setup $upstream "/exenoui /qn /norestart APPDIR=`"$customDirectory`" RUNAPPLICATION=0 /l*v `"$testRoot/upstream-install.log`""
  if ((Get-Products).Count -ne 1) { throw 'Upstream fixture did not install' }
  [IO.File]::WriteAllText((Join-Path $customDirectory 'my-user-file.txt'), 'keep me')
  # Fault-inject a COPY of the MSI to verify that an interrupted upgrade restores
  # the old product. The distributable MSI and EXE are never modified.
  $failureMsi = Join-Path $testRoot 'rollback-test.msi'
  Copy-Item -LiteralPath $msi -Destination $failureMsi
  $database = $installer.OpenDatabase($failureMsi, 1)
  foreach ($sql in @(
    "INSERT INTO ``CustomAction`` (``Action``,``Type``,``Target``) VALUES ('InstallerTestFailure',19,'Intentional CI rollback test')",
    "INSERT INTO ``InstallExecuteSequence`` (``Action``,``Condition``,``Sequence``) VALUES ('InstallerTestFailure','1',4001)"
  )) {
    $view = $database.OpenView($sql)
    $view.Execute()
    $view.Close()
    [Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) | Out-Null
  }
  $database.Commit()
  [Runtime.InteropServices.Marshal]::FinalReleaseComObject($database) | Out-Null
  Invoke-Setup msiexec.exe "/i `"$failureMsi`" /qn /norestart /l*v `"$testRoot/rollback.log`"" @(1603)
  $restored = @(Get-Products)
  if ($restored.Count -ne 1 -or $installer.ProductInfo($restored[0], 'VersionString') -ne '3.0.70') { throw 'Failed upgrade did not restore upstream' }
  if (-not (Test-Path (Join-Path $customDirectory 'EDHM-UI-V3.exe'))) { throw 'Rollback did not restore the executable' }
  Invoke-Setup $free "/qn /norestart /l*v `"$testRoot/free-upgrade.log`""
  Assert-Installed $customDirectory
  # The pinned fixture is a downgrade only when building a newer app version.
  if ([version]$version -gt [version]'3.0.70') {
    Invoke-Setup $upstream "/exenoui /qn /norestart RUNAPPLICATION=0 /l*v `"$testRoot/downgrade.log`"" @(1603,1638)
  } else {
    Write-Output 'SKIP: upstream fixture downgrade check requires an app version newer than 3.0.70'
  }
  Assert-Installed $customDirectory
  Invoke-Setup msiexec.exe "/fa `"$msi`" /qn /norestart /l*v `"$testRoot/repair.log`""
  Assert-Installed $customDirectory
  # Rebuilt installers for the same app version must replace the cached MSI.
  $replacement = Join-Path $testRoot 'same-version-replacement.msi'
  Copy-Item -LiteralPath $msi -Destination $replacement
  $replacementCode = [guid]::NewGuid().ToString('B').ToUpperInvariant()
  $database = $installer.OpenDatabase($replacement, 1)
  $view = $database.OpenView("UPDATE ``Property`` SET ``Value``='$replacementCode' WHERE ``Property``='ProductCode'")
  $view.Execute(); $view.Close()
  [Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) | Out-Null
  $database.Commit()
  [Runtime.InteropServices.Marshal]::FinalReleaseComObject($database) | Out-Null
  # A real rebuild has a fresh PackageCode too. Reusing it makes MSI load the
  # installed package from cache instead of this replacement fixture.
  $summary = $installer.SummaryInformation([string]$replacement, 1)
  $summary.GetType().InvokeMember('Property', [Reflection.BindingFlags]::SetProperty, $null, $summary,
    [object[]]@([int]9, [string][guid]::NewGuid().ToString('B').ToUpperInvariant())) | Out-Null
  $summary.Persist()
  [Runtime.InteropServices.Marshal]::FinalReleaseComObject($summary) | Out-Null
  Invoke-Setup msiexec.exe "/i `"$replacement`" /qn /norestart /l*v `"$testRoot/same-version-upgrade.log`""
  Assert-Installed $customDirectory
  if (@(Get-Products)[0] -ne $replacementCode) { throw 'Same-version upgrade retained the old installer' }
  Uninstall-Current 'upgrade-uninstall'
  if (Test-Path (Join-Path $customDirectory 'EDHM-UI-V3.exe')) { throw 'Uninstall left the executable' }
  if ((Get-Content (Join-Path $customDirectory 'my-user-file.txt') -Raw) -ne 'keep me') { throw 'Uninstall removed user-owned files' }
  # Fresh install must default to upstream's location and create working shortcuts.
  Invoke-Setup $free "/qn /norestart /l*v `"$testRoot/fresh-install.log`""
  Assert-Installed $defaultDirectory
  foreach ($folder in @([Environment]::GetFolderPath('CommonDesktopDirectory'), [Environment]::GetFolderPath('CommonPrograms'))) {
    $link = Join-Path $folder 'EDHM-UI-V3.lnk'
    if (-not (Test-Path $link)) { throw "Missing shortcut: $link" }
    $shortcut = (New-Object -ComObject WScript.Shell).CreateShortcut($link)
    if ($shortcut.TargetPath -ne (Join-Path $defaultDirectory 'EDHM-UI-V3.exe')) { throw "Wrong shortcut target: $link" }
  }
  Uninstall-Current 'fresh-uninstall'
  if (Test-Path (Join-Path $defaultDirectory 'EDHM-UI-V3.exe')) { throw 'Uninstall left application files' }
  if ((Get-Content $sentinel -Raw) -ne 'preserve my settings') { throw 'User data was changed' }
  Invoke-Setup $free "/qn /norestart DESKTOPSHORTCUT=`"`" MENUSHORTCUT=`"`" /l*v `"$testRoot/no-shortcuts.log`""
  Assert-Installed $defaultDirectory
  foreach ($folder in @([Environment]::GetFolderPath('CommonDesktopDirectory'), [Environment]::GetFolderPath('CommonPrograms'))) {
    if (Test-Path (Join-Path $folder 'EDHM-UI-V3.lnk')) { throw 'A disabled shortcut was created or a previous shortcut was not removed' }
  }
  Uninstall-Current 'no-shortcuts-uninstall'
  Write-Output 'PASS: upstream and same-version upgrade, rollback, payload hashes, repair, fresh install, shortcut options, uninstall, user-data preservation'
} finally {
  Uninstall-Current 'cleanup'
}
