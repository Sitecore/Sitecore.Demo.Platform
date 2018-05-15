Param(
    [string] $configurationFile = "configuration-xp0.json"
)
Set-Location $PSScriptRoot
Write-Host "Setting Defaults and creating $configurationFile"

$json = Get-Content -Raw $configurationFile |  ConvertFrom-Json

# Assets and prerequisites

$assets = $json.assets
$assets.licenseFilePath = Join-Path $assets.root "license.xml"
$assets.certificatesPath = Join-Path $assets.root "Certificates"

$json.assets = $assets

# Settings

# Site Settings
$site = $json.settings.site
$site.hostName = $json.settings.site.prefix + "." + $json.settings.site.suffix
$json.settings.site = $site

#$sql = $json.settings.sql

# XConnect Parameters
$xConnect = $json.settings.xConnect

$xConnect.ConfigurationPath = Join-Path $assets.root "xconnect-xp0.json"
$xConnect.certificateConfigurationPath = Join-Path $assets.root "xconnect-createcert.json"
$xConnect.solrConfigurationPath = Join-Path $assets.root "xconnect-solr.json"
$xConnect.packagePath = Join-Path $assets.root $("Sitecore " + $assets.sitecoreVersion + " (OnPrem)_xp0xconnect.scwdp.zip")
$xConnect.siteName = $site.prefix + "_xconnect." + $site.suffix
$xConnect.certificateName = [string]::Join(".", @($site.prefix, $site.suffix, "xConnect.Client"))
$xConnect.siteRoot = Join-Path $site.webRoot -ChildPath $xConnect.siteName

$json.settings.xConnect = $xConnect

# Sitecore Parameters
$sitecore = $json.settings.sitecore

$sitecore.solrConfigurationPath = Join-Path $assets.root "sitecore-solr.json"
$sitecore.configurationPath = Join-Path $assets.root "sitecore-xp0.json"
$sitecore.sslConfigurationPath = "$PSScriptRoot\certificates\sitecore-ssl.json"
$sitecore.packagePath = Join-Path $assets.root $("Sitecore " + $assets.sitecoreVersion +" (OnPrem)_single.scwdp.zip")
$sitecore.siteRoot = Join-Path $site.webRoot -ChildPath $site.hostName
$json.settings.sitecore = $sitecore

Set-Content $configurationFile  (ConvertTo-Json -InputObject $json -Depth 3)