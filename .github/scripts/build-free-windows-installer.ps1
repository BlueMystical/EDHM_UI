param(
  [Parameter(Mandatory)][string]$Version,
  [string]$AppDirectory,
  [string]$OutputDirectory,
  [string]$Wix = 'wix',
  [string]$MakeNSIS = 'makensis',
  [switch]$MsiOnly
)
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$appRoot = Join-Path $repoRoot 'source_v3'
$package = Get-Content (Join-Path $appRoot 'package.json') -Raw | ConvertFrom-Json
if ($Version -ne $package.version) { throw 'Requested version must match package.json' }
if (-not $AppDirectory) { $AppDirectory = Join-Path $appRoot 'out/EDHM-UI-V3-win32-x64' }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $appRoot 'out/release/free' }
$AppDirectory = [IO.Path]::GetFullPath($AppDirectory)
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
foreach ($required in @('EDHM-UI-V3.exe', 'resources/app.asar', 'resources/settings_window/settings.html')) {
  if (-not (Test-Path -LiteralPath (Join-Path $AppDirectory $required) -PathType Leaf)) { throw "Missing packaged file: $required" }
}
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

# Keep the installed-product identity tied to the source installer, not to this tool.
[xml]$original = Get-Content (Join-Path $appRoot 'out/Installer/EDHM-UI-V3.aip') -Raw
$upgradeCode = $original.SelectSingleNode('//COMPONENT[@cid="caphyon.advinst.msicomp.MsiPropsComponent"]/ROW[@Property="UpgradeCode"]').Value
if (-not $upgradeCode) { throw 'Original installer UpgradeCode is missing' }
function Get-StableId([string]$Text) {
  $hash = [Security.Cryptography.SHA256]::Create()
  try { return [Convert]::ToHexString($hash.ComputeHash([Text.Encoding]::UTF8.GetBytes($Text))).Substring(0, 32) }
  finally { $hash.Dispose() }
}
function Get-StableGuid([string]$Text) { return ([guid](Get-StableId $Text)).ToString().ToUpperInvariant() }
function Escape-Xml([string]$Text) { return [Security.SecurityElement]::Escape($Text) }
# Each package build must replace a previously installed package, even when only
# installer behavior changed and the application version stays the same.
$productCode = [guid]::NewGuid().ToString().ToUpperInvariant()
$icon = Join-Path $appRoot 'src/images/Icon_v3_a0.ico'
$fileXml = [Text.StringBuilder]::new()
[void]$fileXml.AppendLine('<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs"><Fragment><DirectoryRef Id="APPDIR">')
$components = [Collections.Generic.List[string]]::new()
$manifest = [Collections.Generic.List[object]]::new()
function Add-Folder([string]$Folder) {
  foreach ($file in Get-ChildItem -LiteralPath $Folder -File | Sort-Object Name) {
    $relative = [IO.Path]::GetRelativePath($AppDirectory, $file.FullName)
    $id = 'F' + (Get-StableId $relative.ToLowerInvariant())
    $fileId = if ($relative -eq 'EDHM-UI-V3.exe') { 'MainExecutable' } else { $id }
    $component = 'C' + $id
    $guid = Get-StableGuid "edhm-ui/free-msi/file/$($relative.ToLowerInvariant())"
    [void]$fileXml.AppendLine("<Component Id=`"$component`" Guid=`"$guid`"><File Id=`"$fileId`" Source=`"$(Escape-Xml $file.FullName)`" KeyPath=`"yes`" /></Component>")
    $components.Add($component)
    $manifest.Add(@{ path = $relative; sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash })
  }
  foreach ($directory in Get-ChildItem -LiteralPath $Folder -Directory | Sort-Object Name) {
    if ($directory.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Unexpected directory link: $directory" }
    $relative = [IO.Path]::GetRelativePath($AppDirectory, $directory.FullName)
    $id = 'D' + (Get-StableId $relative.ToLowerInvariant())
    [void]$fileXml.AppendLine("<Directory Id=`"$id`" Name=`"$(Escape-Xml $directory.Name)`">")
    Add-Folder $directory.FullName
    [void]$fileXml.AppendLine('</Directory>')
  }
}
Add-Folder $AppDirectory
[void]$fileXml.AppendLine('</DirectoryRef><ComponentGroup Id="AppFiles">')
foreach ($component in $components) { [void]$fileXml.AppendLine("<ComponentRef Id=`"$component`" />") }
[void]$fileXml.AppendLine('</ComponentGroup></Fragment></Wix>')
$generated = Join-Path $OutputDirectory 'Files.wxs'
[IO.File]::WriteAllText($generated, $fileXml.ToString())
$manifest | ConvertTo-Json -Depth 3 | Set-Content (Join-Path $OutputDirectory 'payload-manifest.json')
$msi = Join-Path $OutputDirectory 'EDHM-UI-V3.msi'
$launcher = Join-Path $OutputDirectory 'LaunchApplication.exe'
& $MakeNSIS /V2 "/DAppVersion=$Version" "/DOutputExe=$launcher" (Join-Path $repoRoot '.github/installers/LaunchApplication.nsi')
if ($LASTEXITCODE -ne 0) { throw "Launch helper build failed: $LASTEXITCODE" }
$arguments = @('build', '-arch', 'x64', '-ext', 'WixToolset.UI.wixext',
  '-d', "AppVersion=$Version", '-d', "ProductCode=$productCode", '-d', "UpgradeCode=$upgradeCode", '-d', "AppIcon=$icon", '-d', "LauncherExe=$launcher",
  (Join-Path $repoRoot '.github/installers/Product.wxs'), (Join-Path $repoRoot '.github/installers/InstallerUI.wxs'), $generated, '-o', $msi)
& $Wix @arguments
if ($LASTEXITCODE -ne 0) { throw "WiX build failed: $LASTEXITCODE" }
if (-not $MsiOnly) {
  $exe = Join-Path $OutputDirectory 'edhm-ui-v3-windows-x64.exe'
  & $MakeNSIS /V2 "/DAppVersion=$Version" "/DProductCode=$productCode" "/DSourceMsi=$msi" "/DOutputExe=$exe" "/DAppIcon=$icon" (Join-Path $repoRoot '.github/installers/Bootstrapper.nsi')
  if ($LASTEXITCODE -ne 0) { throw "NSIS build failed: $LASTEXITCODE" }
  $info = (Get-Item -LiteralPath $exe).VersionInfo
  if ($info.FileVersion -ne $Version -or $info.ProductVersion -ne $Version) {
    throw 'Installer EXE version metadata does not match package.json'
  }
}
Write-Output "Built free MSI installer $Version ($productCode)"
