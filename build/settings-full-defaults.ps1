
Write-Host "Setting Defaults"

$json = Get-Content -Raw .\settings.json |  ConvertFrom-Json

# Assets and prerequisites

$assets = $json.assets
$assets.root = "$PSScriptRoot\build\assets"
$assets.psRepository = "https://sitecore.myget.org/F/sc-powershell/api/v2/"
$assets.psRepositoryName = "SitecoreGallery"
$assets.licenseFilePath = Join-Path $assets.root "license.xml"
$assets.sitecoreVersion = "9.0.0 rev. 171002"
$assets.installerVersion = "1.0.2"
$assets.certificatesPath = Join-Path $assets.root "Certificates"

$json.assets = $assets


# Settings

# Site Settings
$site = $json.settings.site
$site.prefix.value = "habitat"
$site.suffix.value = "dev.local"
$site.webroot.value = "C:\inetpub\wwwroot"
$site.hostName.value = $json.settings.site.prefix.value + "." + $json.settings.site.suffix.value
$json.settings.site = $site

$sql = $json.settings.sql
# SQL Settings
$sql.server.value = "."
$sql.adminUser.value = "sa"
$sql.adminPassword.value = "12345"

$json.settings.sql = $sql

# XConnect Parameters
$xConnect = $json.settings.xConnect

$xConnect.ConfigurationPath.value = Join-Path $assets.root "xconnect-xp0.json"
$xConnect.certificateConfigurationPath.value = Join-Path $assets.root "xconnect-createcert.json"
$xConnect.solrConfigurationPath.value = Join-Path $assets.root "xconnect-solr.json"
$xConnect.packagePath.value = Join-Path $assets.root $("Sitecore " + $assets.sitecoreVersion + " (OnPrem)_xp0xconnect.scwdp.zip")
$xConnect.siteName.value = $site.prefix.value + "_xconnect." + $site.suffix.value
$xConnect.certificate.value = [string]::Join(".", @($site.prefix.value, $site.suffix.value, "xConnect.Client"))
$xConnect.siteRoot.value = Join-Path $site.webRoot.value -ChildPath $xConnect.siteName.value
$xConnect.sqlCollectionUser.value = "collectionuser"
$xConnect.sqlCollectionPassword.value = "Test12345"

$json.settings.xConnect = $xConnect

# Sitecore Parameters
$sitecore = $json.settings.sitecore

$sitecore.solrConfigurationPath.value = Join-Path $assets.root "sitecore-solr.json"
$sitecore.configurationPath.value = Join-Path $assets.root "sitecore-xp0.json"
$sitecore.sslConfigurationPath.value = "$PSScriptRoot\build\certificates\sitecore-ssl.json"
$sitecore.packagePath.value = Join-Path $assets.root $("Sitecore " + $assets.sitecoreVersion +" (OnPrem)_single.scwdp.zip")
$sitecore.siteName.value = [string]::Join(".", @($site.prefix.value, $site.suffix.value))
$sitecore.siteRoot.value = Join-Path $site.webRoot.value -ChildPath $sitecore.siteName.value
$json.settings.site = $sitecore
# Solr Parameters
$solr = $json.settings.solr
$solr.url = "https://localhost:8983/solr"
$solr.root = "c:\solr"
$solr.serviceName = "Solr"

Set-Content .\install-xp0.json  (ConvertTo-Json -InputObject $json -Depth 3)