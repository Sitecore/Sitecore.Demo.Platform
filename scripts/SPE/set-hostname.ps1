param(
    [string]$instanceUrl = "habitathome.dev.local",
    [string]$hostname,
    [string]$username,
    [string]$password)
$ErrorActionPreference = 'Stop'

Import-Module SPE
Write-Host ("Connecting to {0}" -f $instanceUrl)

$session = New-ScriptSession -Username "$username" -Password "$password" -ConnectionUri $("https://" + $instanceUrl)

Invoke-RemoteScript -Session $session -ScriptBlock { 
    (Get-Item -Path "master:/content/Habitat Sites/Habitat Home/Settings/Site Grouping/Habitat Home").HostName = ("{0}" -f $using:hostname)
}