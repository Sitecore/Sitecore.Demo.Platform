Param(
  [Parameter(Mandatory)]
  [string]$SqlServer,

  [Parameter(Mandatory)]
  [string]$SqlAdminUser,

  [Parameter(Mandatory)]
  [string]$SqlAdminPassword
)

# CD URI stolen from EXM root field value
$valueParamHostnameBase = ([System.Uri]($env:EXM_BASE_URL)).Host
$valueParamHostname = ("Value='" + $valueParamHostnameBase + "'")

# HostName field ID
$fieldParamHostnameFieldId = ("FieldId='8E0DD914-9AFB-4D45-BF8B-7FF5D6E5337E'")

# Lighthouse Lifestyle
$itemSiteNodeLifestyle = ("ItemId='459C2D64-251D-4469-B364-1D53C165D60D'")
$paramsSiteNodeLifestyle = $itemSiteNodeLifestyle, $fieldParamHostnameFieldId, $valueParamHostname
Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsSiteNodeLifestyle -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -Verbose
Write-Host "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

# Lighthouse Financial
$itemSiteNodeFinancial = ("ItemId='3BDC06DE-022C-48B2-893B-6C8995DBB456'")
$paramsSiteNodeFinancial = $itemSiteNodeFinancial, $fieldParamHostnameFieldId, $valueParamHostname
Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsSiteNodeFinancial -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -Verbose
Write-Host "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

# Lighthouse Healthcare
$itemSiteNodeHealthcare = ("ItemId='53BAE432-0C58-415C-8081-EDAA5042A504'")
$paramsSiteNodeHealthcare = $itemSiteNodeHealthcare, $fieldParamHostnameFieldId, $valueParamHostname
Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsSiteNodeHealthcare -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -Verbose
Write-Host "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"

# Cumulus
$itemSiteNodeCumulus = ("ItemId='6D3DBEFE-E525-4FBA-8AE7-83796E8ABE3B'")
$paramsSiteNodeCumulus = $itemSiteNodeCumulus, $fieldParamHostnameFieldId, $valueParamHostname
Invoke-Sqlcmd -InputFile "C:\sql\SetSharedFieldValue.sql" -Variable $paramsSiteNodeCumulus -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -Verbose
Write-Host "$(Get-Date -Format $timeFormat): Invoke SetSharedFieldValue.sql"
