# used to maintain state of the CM files in a PVC
if (Test-Path c:\backup -PathType Container) {
    if ((Get-ChildItem c:\backup -Exclude .gitkeep | Measure-Object).Count -gt 0) {
        Write-Host "Backup folder found, restoring files"
        robocopy c:\backup c:\inetpub/wwwroot /s
    }
    else {
        Write-Host "Nothing to restore from backup"
    }
    & c:\tools\entrypoints\iis\Development.ps1 -WatchDirectoryParameters @{ Path = 'c:\inetpub\wwwroot'; Destination = 'c:\backup'; ExcludeDirectories = @('logs', 'mediacache', 'FrontEnd', 'packages', 'poststeps'); ExcludeFiles = @('DeviceDetectionDB*') }
}
else {
    & "C:\LogMonitor\LogMonitor.exe" "C:\ServiceMonitor.exe" "w3svc"
}