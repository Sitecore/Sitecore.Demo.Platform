# used to maintain state of the CM files in a PVC
if (Test-Path c:\backup -PathType Container) {
    if ((Get-ChildItem c:\backup -Exclude .gitkeep | Measure-Object).Count -gt 0) {
        Write-Host "Backup folder found, restoring files"
        robocopy c:\backup c:\inetpub/wwwroot /s /njh /njs /np /ns /nfl
    }
    else {
        Write-Host "Nothing to restore from backup"
    }

    $BackupDirectoryParameters = @{
        Path               = 'c:\inetpub\wwwroot'
        Destination        = 'c:\backup'
        ExcludeDirectories = @('logs', 'mediacache', 'FrontEnd', 'packages', 'poststeps')
        ExcludeFiles       = @('DeviceDetectionDB*')
    }

    $backupDirectoryJobName = "Backup-Directory.ps1"

    Write-Host "$(Get-Date -Format $timeFormat): ENTRYPOINT: '$backupDirectoryJobName' starting..."

    Start-Job -Name $backupDirectoryJobName -ArgumentList $BackupDirectoryParameters -ScriptBlock {
        param([hashtable]$params)

        & "C:\tools\scripts\Backup-Directory.ps1" @params

    } | Out-Null
}

& "C:\LogMonitor\LogMonitor.exe" "C:\ServiceMonitor.exe" "w3svc"
