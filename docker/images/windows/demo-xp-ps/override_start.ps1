# Set Host
$publishingHost = ( -join ($PSScriptRoot, "\Sitecore.Framework.Publishing.Host.exe"))

Write-Host $publishingHost

# Set Conn strings
$coreArgs = "configuration setconnectionstring core" , """$env:CONN_STRING_CORE"""
$masterArgs = "configuration setconnectionstring master", """$env:CONN_STRING_MASTER"""
$webArgs = "configuration setconnectionstring web", """$env:CONN_STRING_WEB"""

# apply connectionstring
Start-Process $publishingHost -ArgumentList $coreArgs -NoNewWindow -Wait; 
Start-Process $publishingHost -ArgumentList $masterArgs -NoNewWindow -Wait;  
Start-Process $publishingHost -ArgumentList $webArgs -NoNewWindow -Wait; 
	
# Upgrade schema
Start-Process $publishingHost 'schema upgrade --force' -NoNewWindow -Wait; 

# Insert item into DB 
if (-not (Test-Path C:\app\startup)) {
    Write-Host "Insert publish item into the queue"
    Start-Process -FilePath powershell -ArgumentList .\publish.ps1 -NoNewWindow
    New-Item C:\app\startup -ItemType Directory
}
else {
    Write-Host "Skip publish all items, the item already exists"
}

# Start process
Start-Process $publishingHost -ArgumentList '--environment development' -NoNewWindow -Wait; 

