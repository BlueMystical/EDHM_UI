param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $repoRoot 'source_v3/out/release/free' }
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
$installer = New-Object -ComObject WindowsInstaller.Installer
$database = $installer.OpenDatabase((Join-Path $OutputDirectory 'EDHM-UI-V3.msi'), 0)
$view = $database.OpenView("SELECT ``Type``,``Source``,``Target`` FROM ``CustomAction`` WHERE ``Action``='LaunchApp'")
$view.Execute()
$action = $view.Fetch()
# Type 2 is the embedded, unelevated shell helper. Type 18 directly starts the
# elevated application, which fails with 740 from the installer's UI process.
if (($action.IntegerData(1) -band 63) -ne 2 -or $action.StringData(2) -ne 'LaunchApplication') {
  throw 'LaunchApp still directly starts the UAC-requiring app instead of the shell helper'
}
if ($action.StringData(3) -ne '/app="[#MainExecutable]"') { throw 'Launch action does not quote the installed application path' }
$view.Close()
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) | Out-Null
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($database) | Out-Null

# Exercise the actual compiled MSI action with a harmless app, including spaces
# and Unicode. A fresh product identity keeps this read-only session independent
# of any real installation on the developer's machine.
# No installation, registry changes, real EDHM launch, or UAC consent is needed.
$probeDirectory = Join-Path $OutputDirectory ('launch-tests/path with spaces ' + [char]0x00e9)
New-Item -ItemType Directory -Path $probeDirectory -Force | Out-Null
$probe = Join-Path $probeDirectory 'EDHM-UI-V3.exe'
$result = Join-Path $probeDirectory 'launch-result.txt'
if (Test-Path -LiteralPath $result) { Remove-Item -LiteralPath $result }
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $compiler /nologo /target:winexe "/out:$probe" (Join-Path $repoRoot '.github/installers/tests/LaunchProbe.cs')
if ($LASTEXITCODE -ne 0) { throw 'Launch probe compilation failed' }
$testMsi = Join-Path $OutputDirectory 'launch-tests/launch-action-test.msi'
Copy-Item -LiteralPath (Join-Path $OutputDirectory 'EDHM-UI-V3.msi') -Destination $testMsi -Force
$database = $installer.OpenDatabase([string]$testMsi, 1)
$productCode = [guid]::NewGuid().ToString('B')
$view = $database.OpenView("UPDATE ``Property`` SET ``Value``='$productCode' WHERE ``Property``='ProductCode'")
$view.Execute()
$view.Close()
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($view) | Out-Null
$database.Commit()
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($database) | Out-Null
$summary = $installer.SummaryInformation([string]$testMsi, 1)
$summary.GetType().InvokeMember('Property', [Reflection.BindingFlags]::SetProperty, $null, $summary,
  [object[]]@([int]9, [string][guid]::NewGuid().ToString('B').ToUpperInvariant())) | Out-Null
$summary.Persist()
[Runtime.InteropServices.Marshal]::FinalReleaseComObject($summary) | Out-Null
$installer.UILevel = 2
$session = $installer.OpenPackage([string]$testMsi, 0)
try {
  $session.GetType().InvokeMember('Property', [Reflection.BindingFlags]::SetProperty, $null, $session,
    [object[]]@([string]'APPDIR', [string]$probeDirectory)) | Out-Null
  foreach ($actionName in @('CostInitialize','FileCost','CostFinalize')) {
    if ($session.DoAction($actionName) -ne 1) { throw "MSI $actionName failed" }
  }
  if ($session.Property('APPDIR').TrimEnd('\') -ne $probeDirectory.TrimEnd('\')) { throw 'MSI probe directory was overridden' }
  # Only costing and the UI launch action run. Never run InstallInitialize,
  # InstallFiles, or any other installation/uninstallation action in this test.
  if ($session.DoAction('LaunchApp') -ne 1) { throw 'MSI LaunchApp action did not execute' }
} finally {
  [Runtime.InteropServices.Marshal]::FinalReleaseComObject($session) | Out-Null
}
$deadline = [DateTime]::UtcNow.AddSeconds(10)
while (-not (Test-Path -LiteralPath $result) -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 100 }
if (-not (Test-Path -LiteralPath $result)) { throw 'Finish launch helper did not start the target application' }
if ((Get-Content -LiteralPath $result -Raw).TrimEnd('\') -ne $probeDirectory.TrimEnd('\')) {
  throw 'Application launched in the wrong working directory'
}
Write-Output 'PASS: actual MSI Finish action opens a spaced/Unicode app path in its installation folder through the shell helper'
