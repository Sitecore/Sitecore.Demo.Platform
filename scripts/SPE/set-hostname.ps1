param(
    [string]$instanceUrl,
    [string]$username,
    [string]$password)
$ErrorActionPreference = 'Stop'

$ScriptPath = Split-Path $MyInvocation.MyCommand.Path

# This is an example PowerShell script that will remotely execute a Unicorn sync using the new CHAP authentication system.

Import-Module SPE
Write-Host ("Connecting to {0}" -f $instanceUrl)

$session = New-ScriptSession -Username $username -Password $password -ConnectionUri $("https://" + $instanceUrl)

Invoke-RemoteScript -Session $session -ScriptBlock { Get-Item -Path "master:/content/Habitat Sites/Habitat Home/Settings/Site Grouping/Habitat Home" | ForEach-Object {$_.HostName = "$instanceUrl"}}