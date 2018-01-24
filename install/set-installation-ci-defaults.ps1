Param(
    [string] $ConfigurationFile = "configuration-xp0.json"
)
cd $PSScriptRoot
Write-Host "Setting Defaults and creating $ConfigurationFile"

$json = Get-Content -Raw .\install-settings.json |  ConvertFrom-Json

# Assets and prerequisites

$assets = $json.assets
$assets.psRepository = "https://sitecore.myget.org/F/sc-powershell/api/v2/"
$assets.psRepositoryName = "SitecoreGallery"
$assets.jreRequiredVersion = "1.8"
$assets.dotnetMinimumVersionValue = "394802"
$assets.dotnetMinimumVersion = "4.6.2"

$json.assets = $assets


# Settings

# Site Settings

$sql = $json.settings.sql
# SQL Settings
$sql.adminUser = "sa"
$sql.minimumVersion="13.0.4001"

$json.settings.sql = $sql

# XConnect Parameters
$xConnect = $json.settings.xConnect

$xConnect.sqlCollectionUser = "collectionuser"
$xConnect.sqlCollectionPassword = "Test12345"

$json.settings.xConnect = $xConnect

Set-Content $ConfigurationFile  (ConvertTo-Json -InputObject $json -Depth 3)