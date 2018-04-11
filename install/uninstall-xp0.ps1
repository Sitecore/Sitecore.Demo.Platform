Param(
    [string] $ConfigurationFile = "configuration-xp0.json"
)

#####################################################
# 
#  Uninstall Sitecore
# 
#####################################################
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

if (!(Test-Path $ConfigurationFile)){
    Write-Host "Configuration file '$($ConfigurationFile)' not found." -ForegroundColor Red
    Write-Host  "Please use 'set-installation...ps1' files to generate a configuration file." -ForegroundColor Red
    Exit 1
}
$config =  Get-Content -Raw $ConfigurationFile -Force |  ConvertFrom-Json 
if (!$config){
    throw "Error trying to load configuration!"
}
$site = $config.settings.site
$sql  = $config.settings.sql
$xConnect = $config.settings.xConnect
$sitecore = $config.settings.sitecore
$solr = $config.settings.solr
$assets = $config.assets

Write-Host "*******************************************************" -ForegroundColor Green
Write-Host " UNInstalling Sitecore $($assets.sitecoreVersion)" -ForegroundColor Green
Write-Host " Sitecore: $($site.hostName)" -ForegroundColor Green
Write-Host " xConnect: $($xConnect.siteName)" -ForegroundColor Green
Write-Host "*******************************************************" -ForegroundColor Green

#if (Get-Module("uninstall")) {
#    Remove-Module "uninstall"
#}

$carbon = Get-Module Carbon
if (-not $carbon) {
    Write-Host "Installing latest version of Carbon" -ForegroundColor Green
    Install-Module -Name Carbon -Repository PSGallery -AllowClobber -Verbose
    Import-Module Carbon
}

Import-Module "$PSScriptRoot\uninstall\uninstall.psm1"

$database = Get-SitecoreDatabase -SqlServer $sql.server -SqlAdminUser $sql.adminUser -SqlAdminPassword $sql.adminPassword

# Unregister xconnect services
Remove-SitecoreWindowsService "$($xConnect.siteName)-MarketingAutomationService"
Remove-SitecoreWindowsService "$($xConnect.siteName)-IndexWorker"

# Delete xconnect site
Remove-SitecoreIisSite $xConnect.siteName

# Drop xconnect databases
Remove-SitecoreDatabase -Name "$($site.prefix)_Xdb.Collection.Shard0" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_Xdb.Collection.Shard1" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_Xdb.Collection.ShardMapManager" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_MarketingAutomation" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_Processing.Pools" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_Processing.Tasks" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_ReferenceData" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_Reporting" -Server $database

# Delete xconnect files
Remove-SitecoreFiles $xConnect.siteRoot

# Delete xconnect cores

Get-WmiObject win32_service  -Filter "name like '$($solr.serviceName)'" | Stop-Service 
Remove-SitecoreSolrCore "$($site.prefix)_xdb" -Root $solr.root
Remove-SitecoreSolrCore "$($site.prefix)_xdb_rebuild" -Root $solr.root
Get-WmiObject win32_service  -Filter "name like '$($solr.serviceName)'" | Start-Service 

# Delete xconnect server certificate
Remove-SitecoreCertificate $xConnect.siteName
# Delete xconnect client certificate
Remove-SitecoreCertificate $xConnect.certificateName
# Delete SSL certificate
Remove-SitecoreCertificate $site.habitatHomeSslCertificateName

# Delete sitecore site
Remove-SitecoreIisSite $site.hostName

# Drop sitecore databases
Remove-SitecoreDatabase -Name "$($site.prefix)_Core" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_ExperienceForms" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_Master" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_Web" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_EXM.Master" -Server $database
Remove-SitecoreDatabase -Name "$($site.prefix)_Messaging" -Server $database

# Delete sitecore files
Remove-SitecoreFiles $sitecore.siteRoot

# Delete sitecore cores
Get-WmiObject win32_service  -Filter "name like '$($solr.serviceName)'" | Stop-Service 
Remove-SitecoreSolrCore "$($site.prefix)_core_index" -Root $solr.root
Remove-SitecoreSolrCore "$($site.prefix)_master_index" -Root $solr.root
Remove-SitecoreSolrCore "$($site.prefix)_web_index" -Root $solr.root
Remove-SitecoreSolrCore "$($site.prefix)_marketingdefinitions_master" -Root $solr.root
Remove-SitecoreSolrCore "$($site.prefix)_marketingdefinitions_web" -Root $solr.root
Remove-SitecoreSolrCore "$($site.prefix)_marketing_asset_index_master" -Root $solr.root
Remove-SitecoreSolrCore "$($site.prefix)_marketing_asset_index_web" -Root $solr.root
Remove-SitecoreSolrCore "$($site.prefix)_testing_index" -Root $solr.root
Remove-SitecoreSolrCore "$($site.prefix)_suggested_test_index" -Root $solr.root
Remove-SitecoreSolrCore "$($site.prefix)_fxm_master_index" -Root $solr.root
Remove-SitecoreSolrCore "$($site.prefix)_fxm_web_index" -Root $solr.root
Get-WmiObject win32_service  -Filter "name like '$($solr.serviceName)'" | Start-Service 

# Delete sitecore certificate
Remove-SitecoreCertificate $site.hostName

# Drop the SQL Collectionuser login
Remove-SitecoreDatabaseLogin -Server $database, -Name $($xConnect.sqlCollectionUser)