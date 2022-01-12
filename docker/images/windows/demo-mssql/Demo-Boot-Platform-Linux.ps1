[CmdletBinding()]
param(
	[Parameter(Mandatory = $false)]
	[ValidateNotNullOrEmpty()]
	[string]$SqlHostname = $env:HOSTNAME,
	[string]$sql_scripts = "/sql",
	[string]$scripts = "/opt"
)

Write-Host "$(Get-Date -Format $timeFormat): Starting demo team Platform boot override."

function CreateAdminUser
{
	Param(
		[string] $username,
		[string] $password
	)

	if ([string]::IsNullOrEmpty($username) -or [string]::IsNullOrEmpty($password))
	{
		Write-Warning "There was no username or password provided. Not creating admin user."
		return $null
	}

	$command = Join-Path $scripts  "HashPassword.ps1"
	$userinfoAdmin = & $command $password

	$passwordParamAdmin = ("EncodedPassword='" + $userinfoAdmin.Password + "'")
	$saltParamAdmin = ("EncodedSalt='" + $userinfoAdmin.Salt + "'")
	$UserNameParamAdmin = ("UserName='" + $username + "'")
	$EMailParamAdmin = ("EMail='noreply@sitecoredemo.com'")
	$paramsAdmin = $passwordParamAdmin, $saltParamAdmin, $UserNameParamAdmin, $EMailParamAdmin

	$command = Join-Path $sql_scripts "CreateSitecoreAdminUser.sql"
	Invoke-Sqlcmd -InputFile $command -Variable $paramsAdmin -HostName $SqlHostname -Username sa -Password $env:SA_PASSWORD
	Write-Verbose "$(Get-Date -Format $timeFormat): Invoke CreateSitecoreAdminUser.sql for $username"
}

# create new admin user if specified
$adminUserName = $env:ADMIN_USER_NAME
if ($null -ne $adminUserName -AND $adminUserName.ToLower() -ne "admin" ) {
	CreateAdminUser $adminUserName $env:SITECORE_ADMIN_PASSWORD
}

# create new Coveo admin user if specified
$coveoAdminUserName = $env:COVEO_ADMIN_USER_NAME
if ($null -ne $coveoAdminUserName -AND $coveoAdminUserName.ToLower() -ne "admin" -AND $coveoAdminUserName.ToLower() -ne $adminUserName ) {
	CreateAdminUser $coveoAdminUserName $env:SITECORE_ADMIN_PASSWORD
}

# disable admin user, if specified
if ($env:DISABLE_DEFAULT_ADMIN -eq $TRUE ) {
	$command = Join-Path $sql_scripts "DisableSitecoreAdminUser.sql"
	Invoke-Sqlcmd -InputFile $command -HostName $SqlHostname -Username sa -Password $env:SA_PASSWORD
	Write-Verbose "$(Get-Date -Format $timeFormat): Invoke DisableSitecoreAdminUser.sql"
}
else {
	# reset OOB admin password, to match SHA512 *and* in case a new one was specified
	$userinfoAdmin = Join-Path $scripts "HashPassword.ps1" $env:SITECORE_ADMIN_PASSWORD

	$passwordParamAdmin = ("EncodedPassword='" + $userinfoAdmin.Password + "'")
	$saltParamAdmin = ("EncodedSalt='" + $userinfoAdmin.Salt + "'")
	$paramsAdmin = $passwordParamAdmin, $saltParamAdmin

	$command = Join-Path $sql_scripts "SetAdminPassword.sql"
	Invoke-Sqlcmd -InputFile $command -Variable $paramsAdmin -HostName $SqlHostname -Username sa -Password $env:SA_PASSWORD
	Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetAdminPassword.sql"
}

# alter demo users, and set new password
$command = Join-Path $scripts "HashPassword.ps1"
$userinfo = & $command $env:USER_PASSWORD
$passwordParam = ("EncodedPassword='" + $userinfo.Password + "'")
$saltParam = ("EncodedSalt='" + $userinfo.Salt + "'")
$paramsUser = $passwordParam, $saltParam

$command = Join-Path $sql_scripts "ResetDemoUsers.sql"
Invoke-Sqlcmd -InputFile $command -Variable $paramsUser -HostName $SqlHostname -Username sa -Password $env:SA_PASSWORD
Write-Verbose "$(Get-Date -Format $timeFormat): Invoke ResetDemoUsers.sql"

# set base URL for EXM root items - instance specific URL

# /sitecore/content/Demo SXA Sites/LighthouseLifestyle/LighthouseLifestyle Emails
$itemParamExmRoot = ("ItemId='E0A6E451-FA94-4557-B101-4B1AD9E9BD93'")

# /sitecore/templates/System/Email/Manager Root/Message Generation/Base URL
$fieldParamExmRoot = ("FieldId='1B963507-6176-4336-A14D-D5070C3B0286'")
$valueParamExmRoot = ("Value='" + $env:EXM_BASE_URL + "'")
$paramsExmRoot = $itemParamExmRoot, $fieldParamExmRoot, $valueParamExmRoot

$command = Join-Path $sql_scripts "SetSharedFieldValue.sql"
Invoke-Sqlcmd -InputFile $command -Variable $paramsExmRoot -HostName $SqlHostname -Username sa -Password $env:SA_PASSWORD
Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

# /sitecore/content/Demo SXA Sites/LighthouseFinancial/LighthouseFinancial Emails
$itemParamExmRootFinancial = ("ItemId='422A7377-107D-43BC-99DE-C6F14C3FBF0E'")
$paramsExmRootFinancial = $itemParamExmRootFinancial, $fieldParamExmRoot, $valueParamExmRoot

$command = Join-Path $sql_scripts "SetSharedFieldValue.sql"
Invoke-Sqlcmd -InputFile $command -Variable $paramsExmRootFinancial -HostName $SqlHostname -Username sa -Password $env:SA_PASSWORD
Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

# /sitecore/content/Demo SXA Sites/LighthouseHealthcare/LighthouseHealthcare Emails
$itemParamExmRootHealthcare = ("ItemId='F8BA8B00-F363-43FC-B902-D532DD385042'")
$paramsExmRootHealthcare = $itemParamExmRootHealthcare, $fieldParamExmRoot, $valueParamExmRoot

$command = Join-Path $sql_scripts "SetSharedFieldValue.sql"
Invoke-Sqlcmd -InputFile $command -Variable $paramsExmRootHealthcare -HostName $SqlHostname -Username sa -Password $env:SA_PASSWORD
Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

Write-Host "$(Get-Date -Format $timeFormat): Demo team Platform boot override complete."
