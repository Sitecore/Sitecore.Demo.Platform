[CmdletBinding()]
param (
	[Parameter(Mandatory = $true)]
	[ValidateScript( { Test-Path $_ -PathType Container })]
	[string]$ResourcesDirectory,

	[Parameter(Mandatory = $true)]
	[string]$SqlServer,

	[Parameter(Mandatory = $true)]
	[string]$SqlAdminUser,

	[Parameter(Mandatory = $true)]
	[string]$SqlAdminPassword,

	[Parameter(Mandatory = $true)]
	[string]$SitecoreAdminPassword,

	[Parameter(Mandatory = $false)]
	[string]$SqlElasticPoolName,

	[Parameter(Mandatory = $false)]
	[object[]]$DatabaseUsers,

	[Parameter(Mandatory = $false)]
	[string]$InstallModulePath
)

.\DeployDatabases.ps1 -ResourcesDirectory $ResourcesDirectory -InstallModulePath $InstallModulePath -SqlServer:$SqlServer -SqlAdminUser:$SqlAdminUser -SqlAdminPassword:$SqlAdminPassword -EnableContainedDatabaseAuth -SkipStartingServer -SqlElasticPoolName $SqlElasticPoolName -Verbose

if (Test-Path -Path (Join-Path $ResourcesDirectory "smm_azure.sql")) {
	.\InstallShards.ps1 -ResourcesDirectory $ResourcesDirectory -SqlElasticPoolName $SqlElasticPoolName -SqlServer $SqlServer -SqlAdminUser $SqlAdminUser -SqlAdminPassword $SqlAdminPassword
}

.\SetDatabaseUsers.ps1 -ResourcesDirectory $ResourcesDirectory -SqlServer:$SqlServer -SqlAdminUser:$SqlAdminUser -SqlAdminPassword:$SqlAdminPassword `
	-DatabaseUsers $DatabaseUsers

Write-Host "set admin password"
# reset OOB admin password, to match SHA512 *and* in case a new one was specified
$userinfoAdmin = ./HashPassword.ps1 $SitecoreAdminPassword

$passwordParamAdmin = ("EncodedPassword='" + $userinfoAdmin.Password + "'")
$saltParamAdmin = ("EncodedSalt='" + $userinfoAdmin.Salt + "'")
$paramsAdmin = $passwordParamAdmin, $saltParamAdmin

Invoke-Sqlcmd -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -InputFile "C:\sql\SetAdminPassword.sql" -Variable $paramsAdmin
Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetAdminPassword.sql"

Write-Host "set user passwords"
# alter demo users, and set new password
$userinfo = ./HashPassword.ps1 "b"
$passwordParam = ("EncodedPassword='" + $userinfo.Password + "'")
$saltParam = ("EncodedSalt='" + $userinfo.Salt + "'")
$paramsUser = $passwordParam, $saltParam

Invoke-Sqlcmd -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -InputFile "C:\sql\ResetDemoUsers.sql" -Variable $paramsUser
Write-Verbose "$(Get-Date -Format $timeFormat): Invoke ResetDemoUsers.sql"

#Write-Host "set exm url"
# set base URL for EXM root items - instance specific URL
#Write-Verbose "$(Get-Date -Format $timeFormat): Set EXM URL"
# /sitecore/content/Demo SXA Sites/LighthouseLifestyle/LighthouseLifestyle Emails
# $itemParamExmRoot = ("ItemId='E0A6E451-FA94-4557-B101-4B1AD9E9BD93'")

# # /sitecore/templates/System/Email/Manager Root/Message Generation/Base URL
# $fieldParamExmRoot = ("FieldId='1B963507-6176-4336-A14D-D5070C3B0286'")
# $valueParamExmRoot = ("Value='" + $env:EXM_BASE_URL + "'")
# $paramsExmRoot = $itemParamExmRoot, $fieldParamExmRoot, $valueParamExmRoot

#Invoke-Sqlcmd -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsExmRoot
#Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"
