Param(
  [Parameter(Mandatory)]
  [string]$SqlServer,

  [Parameter(Mandatory)]
  [string]$SqlAdminUser,

  [Parameter(Mandatory)]
  [string]$SqlAdminPassword
)

$itemParamExmRoot = ("ItemId='E0A6E451-FA94-4557-B101-4B1AD9E9BD93'")

# /sitecore/templates/System/Email/Manager Root/Message Generation/Base URL
$fieldParamExmRoot = ("FieldId='1B963507-6176-4336-A14D-D5070C3B0286'")

# /sitecore/content/Demo SXA Sites/LighthouseLifestyle/LighthouseLifestyle Emails
$valueParamExmRoot = ("Value='" + $env:EXM_BASE_URL + "'")
$paramsExmRoot = $itemParamExmRoot, $fieldParamExmRoot, $valueParamExmRoot

Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsExmRoot -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -Verbose
Write-Host "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

# /sitecore/content/Demo SXA Sites/LighthouseFinancial/LighthouseFinancial Emails
$itemParamExmRootFinancial = ("ItemId='422A7377-107D-43BC-99DE-C6F14C3FBF0E'")
$paramsExmRootFinancial = $itemParamExmRootFinancial, $fieldParamExmRoot, $valueParamExmRoot

Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsExmRootFinancial -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -Verbose
Write-Host "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

# /sitecore/content/Demo SXA Sites/LighthouseHealthcare/LighthouseHealthcare Emails
$itemParamExmRootHealthcare = ("ItemId='F8BA8B00-F363-43FC-B902-D532DD385042'")
$paramsExmRootHealthcare = $itemParamExmRootHealthcare, $fieldParamExmRoot, $valueParamExmRoot

Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsExmRootHealthcare -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -Verbose
Write-Host "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"
