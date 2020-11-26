[CmdletBinding()]
param(
	[Parameter(Mandatory = $false)]
	[ValidateNotNullOrEmpty()]
	[string]$SqlHostname = $env:HOSTNAME,
	[string]$sql_scripts = "/sql",
	[string]$scripts = "/opt"
)

# NOTE: This file is a copy of the new Demo-Boot-Platform-Linux.ps1 file.
# It exists only for not breaking nightly deployments from the production portal while waiting for Demo-Boot-Platform-Linux.ps1 to be merged in the main branch.
# Please delete this file once Demo-Boot-Platform-Linux.ps1 is merged in the main branch.

Write-Host "$(Get-Date -Format $timeFormat): Starting demo team Platform boot override."

# create new admin user if specified
$adminUserName = $env:ADMIN_USER_NAME
if ($null -ne $adminUserName -AND $adminUserName.ToLower() -ne "admin" ) {

	$command = Join-Path $scripts  "HashPassword.ps1"
	$userinfoAdmin = & $command $env:SITECORE_ADMIN_PASSWORD

	$passwordParamAdmin = ("EncodedPassword='" + $userinfoAdmin.Password + "'")
	$saltParamAdmin = ("EncodedSalt='" + $userinfoAdmin.Salt + "'")
	$UserNameParamAdmin = ("UserName='" + $env:ADMIN_USER_NAME + "'")
	$EMailParamAdmin = ("EMail='noreply@sitecoredemo.com'")
	$paramsAdmin = $passwordParamAdmin, $saltParamAdmin, $UserNameParamAdmin, $EMailParamAdmin

	$command = Join-Path $sql_scripts "CreateSitecoreAdminUser.sql"
	Invoke-Sqlcmd -InputFile $command -Variable $paramsAdmin -HostName $SqlHostname -Username sa -Password $env:SA_PASSWORD
	Write-Verbose "$(Get-Date -Format $timeFormat): Invoke CreateSitecoreAdminUser.sql"
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

# /sitecore/content/Demo SXA Sites/LighthouseFinancial/zLighthouseFinancial Emails
$itemParamExmRootFinancial = ("ItemId='422A7377-107D-43BC-99DE-C6F14C3FBF0E'")
$paramsExmRootFinancial = $itemParamExmRootFinancial, $fieldParamExmRoot, $valueParamExmRoot

$command = Join-Path $sql_scripts "SetSharedFieldValue.sql"
Invoke-Sqlcmd -InputFile $command -Variable $paramsExmRootFinancial -HostName $SqlHostname -Username sa -Password $env:SA_PASSWORD
Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

Write-Host "$(Get-Date -Format $timeFormat): Demo team Platform boot override complete."
