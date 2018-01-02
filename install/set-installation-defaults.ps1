Param(
    [string] $ConfigurationFile = "configuration-xp0.json"
)

Write-Host "Setting Defaults and creating $ConfigurationFile"

$json = Get-Content -Raw .\install-settings.json -Encoding Ascii |  ConvertFrom-Json

# Assets and prerequisites

$assets = $json.assets
$assets.root = "$PSScriptRoot\assets"
$assets.psRepository = "https://sitecore.myget.org/F/sc-powershell/api/v2/"
$assets.psRepositoryName = "SitecoreGallery"
$assets.licenseFilePath = Join-Path $assets.root "license.xml"
$assets.sitecoreVersion = "9.0.1 rev. 171208"
$assets.installerVersion = "1.0.2"
$assets.certificatesPath = Join-Path $assets.root "Certificates"
$assets.jreRequiredVersion = "1.8"
$assets.dotnetMinimumVersionValue = "394802"
$assets.dotnetMinimumVersion = "4.6.2"
$assets.installPackagePath = Join-Path $assets.root "installpackage.aspx"

# Settings

# Site Settings
$site = $json.settings.site
$site.prefix = "habitat"
$site.suffix = "dev.local"
$site.webroot = "C:\inetpub\wwwroot"
$site.hostName = $json.settings.site.prefix + "." + $json.settings.site.suffix

$sql = $json.settings.sql
# SQL Settings
$sql.server = "."
$sql.adminUser = "sa"
$sql.adminPassword = "12345"
$sql.minimumVersion="13.0.4001"

# XConnect Parameters
$xConnect = $json.settings.xConnect

$xConnect.ConfigurationPath = (Get-ChildItem $pwd -filter "xconnect-xp0.json" -Recurse).FullName 
$xConnect.certificateConfigurationPath = (Get-ChildItem $assets.root -filter "xconnect-createcert.json" -Recurse).FullName
$xConnect.solrConfigurationPath = (Get-ChildItem $pwd -filter "xconnect-solr.json" -Recurse).FullName 
$xConnect.packagePath = Join-Path $assets.root $("Sitecore " + $assets.sitecoreVersion + " (OnPrem)_xp0xconnect.scwdp.zip")
$xConnect.siteName = $site.prefix + "_xconnect." + $site.suffix
$xConnect.certificateName = [string]::Join(".", @($site.prefix, $site.suffix, "xConnect.Client"))
$xConnect.siteRoot = Join-Path $site.webRoot -ChildPath $xConnect.siteName
$xConnect.sqlCollectionUser = $site.prefix + "collectionuser"
$xConnect.sqlCollectionPassword = "Test12345"


# Sitecore Parameters
$sitecore = $json.settings.sitecore

$sitecore.solrConfigurationPath =  (Get-ChildItem $pwd -filter "sitecore-solr.json" -Recurse).FullName 
$sitecore.configurationPath = (Get-ChildItem $pwd -filter "sitecore-xp0.json" -Recurse).FullName 
$sitecore.sslConfigurationPath = "$PSScriptRoot\certificates\sitecore-ssl.json"
$sitecore.packagePath = Join-Path $assets.root $("Sitecore " + $assets.sitecoreVersion +" (OnPrem)_single.scwdp.zip")
$sitecore.siteName = [string]::Join(".", @($site.prefix, $site.suffix))
$sitecore.siteRoot = Join-Path $site.webRoot -ChildPath $sitecore.siteName

# Solr Parameters
$solr = $json.settings.solr
$solr.url = "https://localhost:8983/solr"
$solr.root = "c:\solr"
$solr.serviceName = "Solr"
$modules = $json.modules

$spe = $modules | Where-Object { $_.id -eq "spe"}
$spe.packagePath = Join-Path $assets.root "packages\spe-latest.zip"
$spe.install = $true
$sxa = $modules | Where-Object { $_.id -eq "sxa"}
$sxa.packagePath = Join-Path $assets.root "packages\Sitecore Experience Accelerator 1.6 rev. 171222 for 9.0.zip"
$sxa.install = $true

Set-Content $ConfigurationFile  (ConvertTo-Json -InputObject $json -Depth 3 )