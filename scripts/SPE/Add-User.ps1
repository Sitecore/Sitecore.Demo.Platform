param(
    [string]$instanceUrl = "habitathome.dev.local",
    [string]$adminUsername,
    [string]$adminPassword,
    [string]$newUsername,
    [string]$newUserPassword,
    [string]$newUserEmail
)

$ErrorActionPreference = 'Stop'

Import-Module SPE
Write-Host ("Connecting to {0}" -f $instanceUrl)

$session = New-ScriptSession -Username $adminUsername -Password $adminPassword -ConnectionUri $("https://" + $instanceUrl)

Invoke-RemoteScript -Session $session -ScriptBlock { 
    New-User -Identity $using:newUsername -Enabled -Password $using:newUserPassword -Email $using:newUserEmail -FullName $using:newUsername
}