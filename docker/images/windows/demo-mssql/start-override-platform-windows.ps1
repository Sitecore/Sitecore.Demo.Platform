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

	$userinfoAdmin = ./HashPassword.ps1 $password

	$passwordParamAdmin = ("EncodedPassword='" + $userinfoAdmin.Password + "'")
	$saltParamAdmin = ("EncodedSalt='" + $userinfoAdmin.Salt + "'")
	$UserNameParamAdmin = ("UserName='" + $username + "'")
	$EMailParamAdmin = ("EMail='noreply@sitecoredemo.com'")
	$paramsAdmin = $passwordParamAdmin, $saltParamAdmin, $UserNameParamAdmin, $EMailParamAdmin

	Invoke-Sqlcmd -InputFile "C:\sql\CreateSitecoreAdminUser.sql" -Variable $paramsAdmin
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
Write-Verbose "$(Get-Date -Format $timeFormat): Set EXM URLs"
# /sitecore/content/Demo SXA Sites/LighthouseLifestyle/LighthouseLifestyle Emails
$itemParamExmRoot = ("ItemId='E0A6E451-FA94-4557-B101-4B1AD9E9BD93'")

# /sitecore/templates/System/Email/Manager Root/Message Generation/Base URL
$fieldParamExmRoot = ("FieldId='1B963507-6176-4336-A14D-D5070C3B0286'")
$valueParamExmRoot = ("Value='" + $env:EXM_BASE_URL + "'")
$paramsExmRoot = $itemParamExmRoot, $fieldParamExmRoot, $valueParamExmRoot

Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsExmRoot
Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

# /sitecore/content/Demo SXA Sites/LighthouseFinancial/zLighthouseFinancial Emails
$itemParamExmRootFinancial = ("ItemId='422A7377-107D-43BC-99DE-C6F14C3FBF0E'")
$paramsExmRootFinancial = $itemParamExmRootFinancial, $fieldParamExmRoot, $valueParamExmRoot

Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsExmRootFinancial
Write-Verbose "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

Write-Host "$(Get-Date -Format $timeFormat): Demo team Platform boot override complete."
