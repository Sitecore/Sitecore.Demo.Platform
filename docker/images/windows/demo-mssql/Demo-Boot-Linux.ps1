[CmdletBinding()]
param(
	[Parameter(Mandatory = $false)]
	[ValidateNotNullOrEmpty()]
	[string]$SqlHostname = $env:HOSTNAME,
	[string]$sql_scripts = "/sql",
	[string]$scripts = "/opt"
)

$timeFormat = "HH:mm:ss:fff"

$ready = Invoke-Sqlcmd -Query "select name from sys.databases where name = 'platform_init_ready'" -HostName $SqlHostname -Username sa -Password $env:SA_PASSWORD
if ($ready) {
	Write-Host "$(Get-Date -Format $timeFormat): Demo team boot override has already been executed, it will not execute this time."
	return
}

Write-Host "$(Get-Date -Format $timeFormat): Starting demo team boot override."

$env:BOOT_OVERRIDE_SCRIPTS.Split(";") | ForEach-Object {
	# Get script path
	$bootOverrideScript = Join-Path $scripts $_
	# Call script
	& $bootOverrideScript -SqlHostname $SqlHostname -sql_scripts $sql_scripts -scripts $scripts;
}

Invoke-Sqlcmd -Query "create database platform_init_ready" -HostName $SqlHostname -Username sa -Password $env:SA_PASSWORD

Write-Host "$(Get-Date -Format $timeFormat): Demo team boot override complete, databases ready!"
