# The script sets the sa password and start the SQL Service
# Also it attaches additional database from the disk
# The format for attach_dbs

param(
	[Parameter(Mandatory = $false)]
	[string]$sa_password,

	[Parameter(Mandatory = $false)]
	[string]$ACCEPT_EULA,

	[Parameter(Mandatory = $false)]
	[string]$attach_dbs
)

if ($ACCEPT_EULA -ne "Y" -And $ACCEPT_EULA -ne "y") {
	Write-Verbose "ERROR: You must accept the End User License Agreement before this container can start."
	Write-Verbose "Set the environment variable ACCEPT_EULA to 'Y' if you accept the agreement."

	exit 1
}

# start the service
Write-Verbose "Starting SQL Server"
start-service MSSQLSERVER

if ($sa_password -eq "_") {
	if (Test-Path $env:sa_password_path) {
		$sa_password = Get-Content -Raw $secretPath
	}
	else {
		Write-Verbose "WARN: Using default SA password, secret file not found at: $secretPath"
	}
}

if ($sa_password -ne "_") {
	Write-Verbose "Changing SA login credentials"
	$sqlcmd = "ALTER LOGIN sa with password=" + "'" + $sa_password + "'" + ";ALTER LOGIN sa ENABLE;"
	& sqlcmd -Q $sqlcmd
}

# override

Write-Host "$(Get-Date -Format $timeFormat): Starting demo team boot override."

$ready = Invoke-Sqlcmd -Query "select name from sys.databases where name = 'platform_init_ready'"
if (-not $ready) {
	Invoke-Sqlcmd -Query "create database platform_init_ready"

	$env:START_OVERRIDE_SCRIPTS.Split(";") | ForEach-Object {
		# Get script path
		$bootOverrideScript = Join-Path ".\" $_
		# Call script
		& $bootOverrideScript;
	}
}

# set datafolder
.\SetSqlServerDataFolder.ps1 -DataDirectory $env:DATA_PATH -Verbose

# set acl
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("NT AUTHORITY\NETWORKSERVICE","FullControl","Allow")
$acl = Get-Acl -Path $env:DATA_PATH
$acl.SetAccessRule($accessRule)
$acl | Set-Acl -Path $env:DATA_PATH

Write-Host "$(Get-Date -Format $timeFormat): Demo team boot override complete, calling StartStandalone.ps1!"

# start
.\StartStandalone.ps1 -ResourcesDirectory $env:RESOURCES_PATH -InstallDirectory $env:INSTALL_PATH -DataDirectory $env:DATA_PATH -sa_password $sa_password -sitecore_admin_password $env:sitecore_admin_password -ACCEPT_EULA $ACCEPT_EULA -attach_dbs $attach_dbs -sql_server $env:sql_server  -Verbose
