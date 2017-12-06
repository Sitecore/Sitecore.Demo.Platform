$rootQA = "\\mars\qa\9.0\9.0.1\nightly"
$xpJsonFileName = "wdpUrls_CMS_OnPrem.json"
$xConnectJsonFileName = "wdpUrls_xConnect_OnPrem.json"
$downloadFolder = ".\assets"
$sxaPackageUrl = "http://tcbuild-ext1ua1:8080/guestAuth/repository/download/Sxa90Build_Nightly/.lastSuccessful/Sitecore%20Experience%20Accelerator%20{build.number}.zip"
$spePackageUrl = "https://marketplace.sitecore.net/services/~/download/BA9304F736324923A4D034FF4D8D4F2D.ashx?data=Sitecore%20PowerShell%20Extensions-4.7%20for%20Sitecore%208&itemId=6aaea046-83af-4ef1-ab91-87f5f9c1aa57"
$currentVersionFolder = $(Get-ChildItem $rootQA -Filter "Sitecore*" | Sort LastWriteTime | Select -Last 1)

Function GetJson($path){

}

$xpUrlJson = $(Get-Content $(Join-Path $currentVersionFolder.Fullname $xpJsonFileName) -Raw | ConvertFrom-Json)

$sitecorePackageUrl = $xpUrlJson.xp0.single

$xConnectUrlJson = $(Get-Content $(Join-Path $currentVersionFolder.Fullname $xConnectJsonFileName) -Raw | ConvertFrom-Json)

$xConnectPackageUrl = $xConnectUrlJson.xp0.xconnect

$sitecorePackagePaths = $sitecorePackageUrl.Split("?")
$sitecorePackageFileName = $sitecorePackagePaths[0].substring($sitecorePackagePaths[0].LastIndexOf("/") + 1)

$xConnectPackagePaths = $xConnectPackageUrl.Split("?")
$xConnectPackageFileName = $xConnectPackagePaths[0].substring($xConnectPackagePaths[0].LastIndexOf("/") + 1)

$sitecoreDestination = $([io.path]::combine($downloadFolder, $sitecorePackageFileName)).ToString()
$xConnectDestination = $([io.path]::combine($downloadFolder, $xConnectPackageFileName)).ToString()

if (!(Test-Path $sitecoreDestination)) {
    Start-BitsTransfer -Source $sitecorePackageUrl -Destination $sitecoreDestination
}
if (!(Test-Path $xConnectDestination)) {
    Start-BitsTransfer -Source $xConnectPackageUrl -Destination $xConnectDestination
}
New-Item -ItemType Directory -Force -Path $($downloadFolder + "\packages")
Start-BitsTransfer -Source $sxaPackageUrl -Destination $([io.path]::combine($downloadFolder, "packages\sxa-nightly.zip")) 
Start-BitsTransfer -Source $spePackageUrl -Destination $([io.path]::combine($downloadFolder, "packages\spe-latest.zip"))