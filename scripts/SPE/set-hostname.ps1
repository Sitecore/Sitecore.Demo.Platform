param(
    [string]$instanceUrl = "habitathome.dev.local",
    [string]$hostname,
    [string]$adminUsername,
    [string]$adminPassword)
$ErrorActionPreference = 'Stop'

Import-Module SPE
Write-Host ("Connecting to {0}" -f $instanceUrl)

$session = New-ScriptSession -Username "$adminUsername" -Password "$adminPassword" -ConnectionUri $("https://" + $instanceUrl)

Invoke-RemoteScript -Session $session -ScriptBlock { 
    (Get-Item -Path "master:/content/Habitat Sites/Habitat Home/Settings/Site Grouping/Habitat Home").HostName = ("{0}" -f $using:hostname)
}