param(
    [string]$instanceUrl = "habitathome.dev.local",
    [string]$adminUsername,
    [string]$adminPassword,
    [string]$username,
    [string]$newUserPassword
)

$ErrorActionPreference = 'Stop'

Import-Module SPE
Write-Host ("Connecting to {0}" -f $instanceUrl)

$session = New-ScriptSession -Username $adminUsername -Password $adminPassword -ConnectionUri $("https://" + $instanceUrl)

Invoke-RemoteScript -Session $session -ScriptBlock { 
    Set-UserPassword -Identity $using:username -Reset -NewPassword $using:newUserPassword
}