Param(
    [bool]$UseLocal = $true
)
$rootQA = "\\fil1ca2\mars\QA\9.0\9.0.1"

$jsonLocalFileName = ".\assets\wdpUrls_OnPrem.json"
$WdpResourcesFeed = "http://nuget1dk1/nuget/9.0.1_master/"
$downloadFolder = ".\assets\"
$sxaPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/Sitecore%20Experience%20Accelerator%201.6%20rev.%20180103%20for%209.0.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$sxaPackageFileName = "Sitecore Experience Accelerator 1.6 rev. 180103 for 9.0.zip"
$spePackageUrl = "https://marketplace.sitecore.net/services/~/download/BA9304F736324923A4D034FF4D8D4F2D.ashx?data=Sitecore%20PowerShell%20Extensions-4.7%20for%20Sitecore%208&itemId=6aaea046-83af-4ef1-ab91-87f5f9c1aa57"
$spePackageFileName = "Sitecore PowerShell Extensions-4.7 for Sitecore 8.zip"
$DEFPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/Data%20Exchange%20Framework%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$DEFPackageFileName="Data Exchange Framework 2.0.1 rev. 180108.zip"
$DEFSqlProviderPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/SQL%20Provider%20for%20Data%20Exchange%20Framework%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$DEFSqlProviderPackageFileName="SQL Provider for Data Exchange Framework 2.0.1 rev. 180108.zip"
$DEFSitecoreProviderPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/Sitecore%20Provider%20for%20Data%20Exchange%20Framework%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$DEFSitecoreProviderPackageFileName="Sitecore Provider for Data Exchange Framework 2.0.1 rev. 180108.zip"
$DEFxConnectProviderPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/xConnect%20Provider%20for%20Data%20Exchange%20Framework%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$DEFxConnectProviderPackageFileName="xConnect Provider for Data Exchange Framework 2.0.1 rev. 180108.zip"
$DEFDynamicsProviderPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/Dynamics%20Provider%20for%20Data%20Exchange%20Framework%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$DEFDynamicsProviderPackageFileName="Dynamics Provider for Data Exchange Framework 2.0.1 rev. 180108.zip"
$DEFDynamicsConnectPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/Connect%20for%20Microsoft%20Dynamics%202.0.1%20rev.%20180108.zip?sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdlacup&se=2027-11-09T20%3A11%3A50Z&st=2017-11-09T12%3A11%3A50Z&spr=https&sig=naspk%2BQflDLjyuC6gfXw4OZKvhhxzTlTvDctfw%2FByj8%3D"
$DEFDynamicsConnectPackageFileName="Connect for Microsoft Dynamics 2.0.1 rev. 180108.zip"
$webPIPackageUrl = "https://download.microsoft.com/download/C/F/F/CFF3A0B8-99D4-41A2-AE1A-496C08BEB904/WebPlatformInstaller_amd64_en-US.msi"
$webPIPackageFileName = "WebPlatformInstaller_amd64_en-US.msi"


try {
    if ($useLocal -eq $false) {
        Write-Host "Trying to get latest Urls from $rootQA"
        $jsonFileName = (Get-ChildItem "$rootQA" -File -Recurse | Where-Object { $_.Name -eq "wdpUrls_OnPrem.json" } | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
    }
}
catch {
    Write-Host "Unable to get latest wdp url file. Using previously saved version" - -ForegroundColor Yellow
    $UseLocal = $true
}

if ($useLocal -eq $true) {
    Write-Host "Using Local: $jsonLocalFileName"
    $json = $(Get-Content $jsonLocalFileName -Raw | ConvertFrom-Json)
    Write-Host "Got Json"
    Write-Host $json
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

Write-Host "Saving $sitecorePackageUrl to $sitecoreDestination - if required" -ForegroundColor Green
if (!(Test-Path $sitecoreDestination)) {
    Start-BitsTransfer -Source $sitecorePackageUrl -Destination $sitecoreDestination
}

Write-Host "Saving $xConnectPackageUrl to $xConnectDestination - if required" -ForegroundColor Green
if (!(Test-Path $xConnectDestination)) {
    Start-BitsTransfer -Source $xConnectPackageUrl -Destination $xConnectDestination
}

Write-Host "Installing WPI, Url Rewrite and Web Deploy 3.6"
$wpiDestination = $([io.path]::combine($downloadFolder, $webPIPackageFileName))
if (!(Test-Path $wpiDestination)) {
    Start-BitsTransfer -Source $webPIPackageUrl -Destination $wpiDestination
    Start-Process -FilePath "assets\WebPlatformInstaller_amd64_en-US.msi" -Wait
}
set-alias wpi "$env:ProgramFiles\Microsoft\Web Platform Installer\WebpiCmd-x64.exe"
wpi /install /Products:"UrlRewrite2"  /AcceptEULA
wpi /install /Products:"WDeploy36NoSMO"  /AcceptEULA


$resources = $json.resources
$resourcesName = "Sitecore.WDP.Resources"
$resourcesVersion = $resources.Replace($resourcesName + ".", "")

if ($useLocal -eq $false) {
    Write-Host ("Installing Resource Version '{0}'" -f  $resourcesVersionJ)  -ForegroundColor Green
    nuget install $resourcesName -Version $resourcesVersion -Source $WdpResourcesFeed -OutputDirectory . -x -prerelease
}

New-Item -ItemType Directory -Force -Path $($downloadFolder + "\packages")

Write-Host "Downloading latest SPE and SXA`r`n" -ForegroundColor Green

$packagesFolder = (Join-Path $downloadFolder "packages")

if (!(Test-Path (Join-Path $packagesFolder $sxaPackageFileName))){
    Start-BitsTransfer -Source $sxaPackageUrl -Destination (Join-Path $packagesFolder $sxaPackageFileName)
}
 
Write-Host "Downloading Data Exchange Framework related packages`r`n" -ForegroundColor Green
if (!(Test-Path (Join-Path $packagesFolder $spePackageFileName))){
    Start-BitsTransfer -Source $spePackageUrl -Destination (Join-Path $packagesFolder $spePackageFileName)
}
if (!(Test-Path (Join-Path $packagesFolder $DEFPackageFileName))){
    Start-BitsTransfer -Source $DEFPackageUrl -Destination (Join-Path $packagesFolder $DEFPackageFileName)
}
if (!(Test-Path (Join-Path $packagesFolder $DEFSitecoreProviderPackageFileName))){
    Start-BitsTransfer -Source $DEFSitecoreProviderPackageUrl -Destination (Join-Path $packagesFolder $DEFSitecoreProviderPackageFileName)
}
if (!(Test-Path (Join-Path $packagesFolder $DEFxConnectProviderPackageFileName))){
    Start-BitsTransfer -Source $DEFxConnectProviderPackageUrl -Destination (Join-Path $packagesFolder $DEFxConnectProviderPackageFileName)
}
if (!(Test-Path (Join-Path $packagesFolder $DEFDynamicsProviderPackageFileName))){
    Start-BitsTransfer -Source $DEFDynamicsProviderPackageUrl -Destination (Join-Path $packagesFolder $DEFDynamicsProviderPackageFileName)
}
if (!(Test-Path (Join-Path $packagesFolder $DEFDynamicsConnectPackageFileName))){
    Start-BitsTransfer -Source $DEFDynamicsConnectPackageUrl -Destination (Join-Path $packagesFolder $DEFDynamicsConnectPackageFileName)
}
if (!(Test-Path (Join-Path $packagesFolder $DEFSqlProviderPackageFileName))){
    Start-BitsTransfer -Source $DEFSqlProviderPackageUrl -Destination (Join-Path $packagesFolder $DEFSqlProviderPackageFileName)
}

