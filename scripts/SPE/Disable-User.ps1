param(
    [string]$instanceUrl,
    [string]$adminUsername,
    [string]$adminPassword,
    [string]$disableUsername
)
$ErrorActionPreference = 'Stop'

Import-Module SPE
Write-Host ("Connecting to {0}" -f $instanceUrl)

$session = New-ScriptSession -Username $username -Password $password -ConnectionUri $("https://" + $instanceUrl)

Invoke-RemoteScript -Session $session -ScriptBlock { 
    Disable-User -Identity $disableUsername 
}