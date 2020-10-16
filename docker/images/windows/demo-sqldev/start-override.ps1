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

	# create new admin user if specified
	$adminUserName = $env:ADMIN_USER_NAME
	if ($null -ne $adminUserName -AND $adminUserName.ToLower() -ne "admin" ) {

		$userinfoAdmin = ./HashPassword.ps1 $env:SITECORE_ADMIN_PASSWORD

		$passwordParamAdmin = ("EncodedPassword='" + $userinfoAdmin.Password + "'")
		$saltParamAdmin = ("EncodedSalt='" + $userinfoAdmin.Salt + "'")
		$UserNameParamAdmin = ("UserName='" + $env:ADMIN_USER_NAME + "'")
		$EMailParamAdmin = ("EMail='noreply@sitecoredemo.com'")
		$paramsAdmin = $passwordParamAdmin, $saltParamAdmin, $UserNameParamAdmin, $EMailParamAdmin

		Invoke-Sqlcmd -InputFile "C:\sql\CreateSitecoreAdminUser.sql" -Variable $paramsAdmin
		Write-Verbose "$(Get-Date -Format $timeFormat): Invoke CreateSitecoreAdminUser.sql"
	}

	# disable admin user, if specified
	if ($env:DISABLE_DEFAULT_ADMIN -eq $TRUE ) {
		Invoke-Sqlcmd -InputFile "C:\sql\DisableSitecoreAdminUser.sql"
		Write-Verbose "$(Get-Date -Format $timeFormat): Invoke DisableSitecoreAdminUser.sql"
	}
	else {
		# reset OOB admin password, to match SHA512 *and* in case a new one was specified
		$userinfoAdmin = ./HashPassword.ps1 $env:SITECORE_ADMIN_PASSWORD

		$passwordParamAdmin = ("EncodedPassword='" + $userinfoAdmin.Password + "'")
		$saltParamAdmin = ("EncodedSalt='" + $userinfoAdmin.Salt + "'")
		$paramsAdmin = $passwordParamAdmin, $saltParamAdmin

		Invoke-Sqlcmd -InputFile "C:\sql\SetAdminPassword.sql" -Variable $paramsAdmin
		Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetAdminPassword.sql"
	}

	# Remove setting the admin password from the ootb StartStandalone.ps1 script as it uses SHA1
	(Get-Content .\StartStandalone.ps1) -replace ".\\SetSitecoreAdminPassword.ps1", "# .\SetSitecoreAdminPassword.ps1" | Set-Content .\StartStandalone.ps1

	# alter demo users, and set new password
	$userinfo = ./HashPassword.ps1 $env:USER_PASSWORD
	$passwordParam = ("EncodedPassword='" + $userinfo.Password + "'")
	$saltParam = ("EncodedSalt='" + $userinfo.Salt + "'")
	$paramsUser = $passwordParam, $saltParam

	Invoke-Sqlcmd -InputFile "C:\sql\ResetDemoUsers.sql" -Variable $paramsUser
	Write-Verbose "$(Get-Date -Format $timeFormat): Invoke ResetDemoUsers.sql"

	# set base URL for EXM root items - instance specific URL
	Write-Verbose "$(Get-Date -Format $timeFormat): Set EXM URL"
	# /sitecore/content/Demo SXA Sites/LighthouseLifestyle/LighthouseLifestyle Emails
	$itemParamExmRoot = ("ItemId='E0A6E451-FA94-4557-B101-4B1AD9E9BD93'")

	# /sitecore/templates/System/Email/Manager Root/Message Generation/Base URL
	$fieldParamExmRoot = ("FieldId='1B963507-6176-4336-A14D-D5070C3B0286'")
	$valueParamExmRoot = ("Value='" + $env:EXM_BASE_URL + "'")
	$paramsExmRoot = $itemParamExmRoot, $fieldParamExmRoot, $valueParamExmRoot

	Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsExmRoot
	Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

	# set DAM hostname if specified
	if ($env:DAM_URL) {
		Write-Verbose "$(Get-Date -Format $timeFormat): Set DAM URL"
		# /sitecore/system/Modules/DAM/Config/DAM connector
		$itemParamDAM = ("ItemId='15BE535E-1A49-4C91-A1CA-1DE14B35FF77'")

		# SearchPage field
		$fieldParamDAM = ("FieldId='0C74C58C-C855-43B7-A397-C6BB9EE4A792'")
		$valueParamDAM = ("Value='" + $env:DAM_URL + "/en-us/sitecore-dam-connect/approved-assets" + "'")
		$paramsDAM = $itemParamDAM, $fieldParamDAM, $valueParamDAM

		Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsDAM
		Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

		# DAMInstance field
		$fieldParamDAM = ("FieldId='803A50F3-C1F8-46A1-9FEC-D55B8BB9BE10'")
		$valueParamDAM = ("Value='" + $env:DAM_URL + "'")
		$paramsDAM = $itemParamDAM, $fieldParamDAM, $valueParamDAM

		Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsDAM
		Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"
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
.\StartStandalone.ps1 -ResourcesDirectory $env:RESOURCES_PATH -InstallDirectory $env:INSTALL_PATH -DataDirectory $env:DATA_PATH -sa_password $env:sa_password -sitecore_admin_password $env:sitecore_admin_password -ACCEPT_EULA $env:ACCEPT_EULA -attach_dbs $env:attach_dbs -sql_server $env:sql_server  -Verbose