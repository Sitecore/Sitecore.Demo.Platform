Param(
    [bool]$UseLocal = $true
)
$rootQA = "\\fil1ca2\mars\QA\9.0\9.0.1"

$jsonLocalFileName = ".\assets\wdpUrls_OnPrem.json"
$WdpResourcesFeed = "http://nuget1dk1/nuget/9.0.1_master/"
$downloadFolder = ".\assets\"
$sxaPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/Sitecore%20Experience%20Accelerator%201.6%20rev.%20180103%20for%209.0.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$spePackageUrl = "https://marketplace.sitecore.net/services/~/download/BA9304F736324923A4D034FF4D8D4F2D.ashx?data=Sitecore%20PowerShell%20Extensions-4.7%20for%20Sitecore%208&itemId=6aaea046-83af-4ef1-ab91-87f5f9c1aa57"
$DEFPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/Data%20Exchange%20Framework%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$DEFSqlProviderPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/SQL%20Provider%20for%20Data%20Exchange%20Framework%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$DEFSitecoreProviderPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/Sitecore%20Provider%20for%20Data%20Exchange%20Framework%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$DEFxConnectProviderPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/xConnect%20Provider%20for%20Data%20Exchange%20Framework%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$DEFDynamicsProviderPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/Dynamics%20Provider%20for%20Data%20Exchange%20Framework%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$DEFDynamicsConnectPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/Connect%20for%20Microsoft%20Dynamics%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"

Write-Host ""

try {
    if ($useLocal -eq $false) {
        Write-Host "Trying to get latest Urls from $rootQA" -ForegroundColor Yellow
        $jsonFileName = (Get-ChildItem "$rootQA" -File -Recurse | Where-Object { $_.Name -eq "wdpUrls_OnPrem.json" } | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
    }
}
catch {
    Write-Host "Unable to get latest wdp url file. Using previously saved version" - -ForegroundColor Yellow
    $UseLocal = $true
}

if ($useLocal -eq $true) {
    Write-Host "Using Local: $jsonLocalFileName" -ForegroundColor Green
    Write-Host ""
    $json = $(Get-Content $jsonLocalFileName -Raw | ConvertFrom-Json)
    
}
else {
    $json = $(Get-Content $jsonFileName -Raw | ConvertFrom-Json)
    Set-Content $jsonLocalFileName   (ConvertTo-Json -InputObject $json -Depth 3 )
}

$sitecorePackageUrl = $json.xp0.single
$xConnectPackageUrl = $json.xp0.xconnect

$sitecorePackagePaths = $sitecorePackageUrl.Split("?")
$sitecorePackageFileName = $sitecorePackagePaths[0].substring($sitecorePackagePaths[0].LastIndexOf("/") + 1)

$xConnectPackagePaths = $xConnectPackageUrl.Split("?")
$xConnectPackageFileName = $xConnectPackagePaths[0].substring($xConnectPackagePaths[0].LastIndexOf("/") + 1)

$sitecoreDestination = $([io.path]::combine($downloadFolder, $sitecorePackageFileName)).ToString()
$xConnectDestination = $([io.path]::combine($downloadFolder, $xConnectPackageFileName)).ToString()

Write-Host ("Saving '{0}' to '{1}' - if required" -f $sitecorePackageFileName, $sitecoreDestination) -ForegroundColor Green
if (!(Test-Path $sitecoreDestination)) {
    Start-BitsTransfer -Source $sitecorePackageUrl -Destination $sitecoreDestination
}

Write-Host ("Saving '{0}' to '{1}' - if required" -f $xConnectPackageFileName, $xConnectDestination)  -ForegroundColor Green
if (!(Test-Path $xConnectDestination)) {
    Start-BitsTransfer -Source $xConnectPackageUrl -Destination $xConnectDestination
}


$resources = $json.resources
$resourcesName = "Sitecore.WDP.Resources"
$resourcesVersion = $resources.Replace($resourcesName + ".", "")

if ($useLocal -eq $false) {
    Write-Host "Installing Resource Version $resourcesVersion" -ForegroundColor Green
    nuget install $resourcesName -Version $resourcesVersion -Source $WdpResourcesFeed -OutputDirectory . -x -prerelease
}

New-Item -ItemType Directory -Force -Path $($downloadFolder + "\packages")

Write-Host "Downloading latest SPE and SXA - if required`r`n" -ForegroundColor Green

$sxaDestination = $([io.path]::combine($downloadFolder, "packages\Sitecore Experience Accelerator 1.6 rev. 180103 for 9.0.zip"))
    
if (!(Test-Path $sxaDestination)) {
    Start-BitsTransfer -Source $sxaPackageUrl -Destination $sxaDestination
}
$speDestination = $([io.path]::combine($downloadFolder, "packages\Sitecore PowerShell Extensions-4.7 for Sitecore 8.zip"))
    
if (!(Test-Path $speDestination)) {
    Start-BitsTransfer -Source $spePackageUrl -Destination $speDestination
}
Write-Host "Downloading Data Exchange Framework packages - if required" -ForegroundColor Green
    
$DEFDestination = $([io.path]::combine($downloadFolder, "packages\Data Exchange Framework 2.0.1 rev. 180108.zip"))
if (!(Test-Path $DEFDestination)) {
    Start-BitsTransfer -Source $DEFPackageUrl -Destination $DEFDestination
}

$DEFSqlProviderDestination = $([io.path]::combine($downloadFolder, "packages\SQL Provider for Data Exchange Framework 2.0.1 rev. 180108.zip"))
if (!(Test-Path $DEFSqlProviderDestination)) {
    Start-BitsTransfer -Source $DEFSqlProviderPackageUrl -Destination $DEFSqlProviderDestination
}

$DEFSitecoreProviderDestination = $([io.path]::combine($downloadFolder, "packages\Sitecore Provider for Data Exchange Framework 2.0.1 rev. 180108.zip"))
if (!(Test-Path $DEFSitecoreProviderDestination)) {
    Start-BitsTransfer -Source $DEFSitecoreProviderPackageUrl -Destination $DEFSitecoreProviderDestination
}
$DEFxConnectProviderDestination = $([io.path]::combine($downloadFolder, "packages\xConnect Provider for Data Exchange Framework 2.0.1 rev. 180108.zip"))
if (!(Test-Path $DEFxConnectProviderDestination)) {
    Start-BitsTransfer -Source $DEFxConnectProviderPackageUrl -Destination $DEFxConnectProviderDestination
}   
$DEFDynamicsProviderDestination = $([io.path]::combine($downloadFolder, "packages\Dynamics Provider for Data Exchange Framework 2.0.1 rev. 180108.zip"))
if (!(Test-Path $DEFDynamicsProviderDestination)) {
    Start-BitsTransfer -Source $DEFDynamicsProviderPackageUrl -Destination $DEFDynamicsProviderDestination
}   
$DEFDynamicsConnectDestination = $([io.path]::combine($downloadFolder, "packages\Connect for Microsoft Dynamics 2.0.1 rev. 180108.zip"))
if (!(Test-Path $DEFDynamicsConnectDestination)) {
    Start-BitsTransfer -Source $DEFDynamicsConnectPackageUrl -Destination $DEFDynamicsConnectDestination
}