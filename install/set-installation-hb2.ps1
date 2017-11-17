Param(
    [string] $configurationFile = "configuration-xp0.json"
)

Write-Host "Setting Local Overrides in $configurationFile"

$json = Get-Content -Raw $configurationFile |  ConvertFrom-Json


# Settings
$assets = $json.assets
$assets.licenseFilePath = Join-Path "c:\users\jfl\downloads" "license.xml"
$json.assets = $assets
# Site Settings
$site = $json.settings.site
$site.prefix = "hb2"
$site.suffix = "dev.local"
$site.webroot = "C:\inetpub\wwwroot"
$site.hostName = $json.settings.site.prefix + "." + $json.settings.site.suffix
$json.settings.site = $site

$sql = $json.settings.sql
# SQL Settings
$sql.server = "."
$sql.adminUser = "sa"
$sql.adminPassword = "Sitecore12!@"
$sql.minimumVersion="13.0.4001"

$json.settings.sql = $sql

# XConnect Parameters
$xConnect = $json.settings.xConnect
$xConnect.sqlCollectionUser = "collectionuser"
$xConnect.sqlCollectionPassword = "Test12345"

$json.settings.xConnect = $xConnect

# Sitecore Parameters
$sitecore = $json.settings.sitecore

$sitecore.siteName = [string]::Join(".", @($site.prefix, $site.suffix))
$sitecore.siteRoot = Join-Path $site.webRoot -ChildPath $sitecore.siteName
$json.settings.sitecore = $sitecore
# Solr Parameters
$solr = $json.settings.solr
$solr.url = "https://localhost:8983/solr"
$solr.root = "C:\solr\solr-6.6.2"
$solr.serviceName = "solr-6.6.2"

Set-Content $configurationFile  (ConvertTo-Json -InputObject $json -Depth 3)