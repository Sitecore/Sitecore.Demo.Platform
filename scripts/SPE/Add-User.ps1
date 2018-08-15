param(
    [string]$instanceUrl,
    [string]$adminUsername,
    [string]$adminPassword,
    [string]$newUsername,
    [string]$newUserPassword,
    [string]$newUserEmail,
    [switch]$isAdministrator
)

$ErrorActionPreference = 'Stop'

Import-Module SPE
Write-Host ("Connecting to {0}" -f $instanceUrl)

$session = New-ScriptSession -Username $username -Password $password -ConnectionUri $("https://" + $instanceUrl)

Invoke-RemoteScript -Session $session -ScriptBlock { 
    New-User -Identity $newUsername -Enabled -Password $newUserPassword -Email $newUserEmail -FullName $newUsername
    if ($isAdministrator) {
        Set-User -Identity $newUsername -IsAdministrator $true
    }
}