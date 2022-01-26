$JobParameters = @{
    Path               = 'c:\repository'
    Destination        = 'c:\inetpub\wwwroot'
}

$JobName = "Git-Sync.ps1"

if ([string]::IsNullOrEmpty($env:GIT_SYNC_REPO)) {
    Write-Host "$(Get-Date -Format $timeFormat): GIT_SYNC_REPO is not defined, $JobName will not be started"
}
else {
    Write-Host "$(Get-Date -Format $timeFormat): ENTRYPOINT: '$JobName' starting..."
    Start-Job -Name $JobName -ArgumentList $JobParameters -ScriptBlock {
        param([hashtable]$params)
        & "C:\tools\scripts\Git-Sync.ps1" @params
    }
}

& "C:\LogMonitor\LogMonitor.exe" "powershell" "C:\Run-W3SVCService.ps1" 
