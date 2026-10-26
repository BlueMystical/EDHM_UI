param([Parameter(Mandatory)][string]$Version)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$project = Join-Path $root '.github/installers/velopack'
$config = Get-Content -LiteralPath (Join-Path $project 'sdk.json') -Raw | ConvertFrom-Json
$package = Get-Content -LiteralPath (Join-Path $root 'source_v3/package.json') -Raw | ConvertFrom-Json
if ($Version -ne $package.version) { throw 'Launcher version must match package.json' }
$build = Join-Path $root 'source_v3/out/velopack-launcher'
$sdk = Join-Path $build 'sdk'
$archive = Join-Path $build "velopack_libc_$($config.version).zip"
New-Item -ItemType Directory -Force -Path $build | Out-Null
if (-not (Test-Path -LiteralPath $archive)) {
    Invoke-WebRequest "https://github.com/velopack/velopack/releases/download/$($config.version)/velopack_libc_$($config.version).zip" -OutFile $archive
}
if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $config.sha256) { throw 'Velopack SDK checksum mismatch' }
Expand-Archive -LiteralPath $archive -DestinationPath $sdk -Force
$cmake = Get-Command cmake -ErrorAction SilentlyContinue
if ($cmake) { $cmake = $cmake.Source } else {
    $vsRoot = & "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe" -latest -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
    $cmake = Join-Path $vsRoot 'Common7/IDE/CommonExtensions/Microsoft/CMake/CMake/bin/cmake.exe'
}
if (-not (Test-Path -LiteralPath $cmake)) { throw 'Install the Visual Studio C++ build tools with CMake' }
& $cmake -S $project -B (Join-Path $build 'build') -A x64 "-DVELOPACK_SDK_DIR=$sdk" "-DAPP_VERSION=$Version"
if ($LASTEXITCODE -ne 0) { throw 'Launcher configure failed' }
& $cmake --build (Join-Path $build 'build') --config Release
if ($LASTEXITCODE -ne 0) { throw 'Launcher build failed' }
& (Join-Path (Split-Path $cmake) 'ctest.exe') --test-dir (Join-Path $build 'build') -C Release --output-on-failure
if ($LASTEXITCODE -ne 0) { throw 'Launcher tests failed' }
$appDirectory = Join-Path $root 'source_v3/out/EDHM-UI-V3-win32-x64'
if (-not (Test-Path -LiteralPath (Join-Path $appDirectory 'EDHM-UI-V3.exe'))) { throw 'Package the Electron app first' }
foreach ($file in @('EDHM-UI-V3.Launcher.exe', 'velopack_libc.dll')) {
    Copy-Item -LiteralPath (Join-Path $build "build/Release/$file") -Destination $appDirectory
}
& (Join-Path $PSScriptRoot 'test-velopack-hooks.ps1') -Executable (Join-Path $appDirectory 'EDHM-UI-V3.Launcher.exe') -Version $Version
