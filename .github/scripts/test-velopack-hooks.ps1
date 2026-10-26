param([Parameter(Mandatory)][string]$Executable, [Parameter(Mandatory)][string]$Version)
$ErrorActionPreference = 'Stop'
# Safe without installing anything: fast hooks must exit before launching the UI.
# Use CreateProcess, exactly like Velopack. A highestAvailable entry point fails
# with error 740 when this check runs from an unelevated administrator session.
$principal = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
Write-Host "Hook test elevated: $($principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))"
foreach ($hook in @('--veloapp-install', '--veloapp-updated', '--veloapp-obsolete', '--veloapp-uninstall')) {
    $start = [Diagnostics.ProcessStartInfo]::new([IO.Path]::GetFullPath($Executable))
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.ArgumentList.Add($hook)
    $start.ArgumentList.Add($Version)
    $process = [Diagnostics.Process]::Start($start)
    try {
        if (-not $process.WaitForExit(10000)) { $process.Kill($true); throw "$hook did not exit before app launch" }
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        if ($process.ExitCode -ne 0 -or -not [string]::IsNullOrWhiteSpace($stdout) -or -not [string]::IsNullOrWhiteSpace($stderr)) {
            throw "$hook failed (exit $($process.ExitCode)): $stdout $stderr"
        }
        Write-Host "$hook passed"
    } finally { $process.Dispose() }
}
