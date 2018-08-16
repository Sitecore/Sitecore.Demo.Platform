param(
    [string]$instanceUrl = "habitathome.dev.local",
    [string]$adminUsername,
    [string]$adminPassword,
    [string]$disableUsername
)
$ErrorActionPreference = 'Stop'

Import-Module SPE
Write-Host ("Connecting to {0}" -f $instanceUrl)

$session = New-ScriptSession -Username $adminUsername -Password $adminPassword -ConnectionUri $("https://" + $instanceUrl)

Invoke-RemoteScript -Session $session -ScriptBlock { 
    Disable-User -Identity $using:disableUsername 
}