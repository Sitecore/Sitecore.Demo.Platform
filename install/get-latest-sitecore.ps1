Param(
    [bool]$UseLocal = $true
)
$rootQA = "\\fil1ca2\mars\QA\9.0\9.0.1"

$jsonLocalFileName = ".\assets\wdpUrls_OnPrem.json"
$WdpResourcesFeed = "http://nuget1dk1/nuget/9.0.1_master/"
$downloadFolder = ".\assets\"
$sxaPackageUrl = "https://v9assets.blob.core.windows.net/v9-onprem-assets/Sitecore%20Experience%20Accelerator%201.6%20rev.%20180103%20for%209.0.zip"
$spePackageUrl = "https://marketplace.sitecore.net/services/~/download/BA9304F736324923A4D034FF4D8D4F2D.ashx?data=Sitecore%20PowerShell%20Extensions-4.7%20for%20Sitecore%208&itemId=6aaea046-83af-4ef1-ab91-87f5f9c1aa57"


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


$resources = $json.resources
$resourcesName = "Sitecore.WDP.Resources"
$resourcesVersion = $resources.Replace($resourcesName + ".", "")

if ($useLocal -eq $false) {
    Write-Host "Installing Resource Version $resourcesVersion" -ForegroundColor Green
    nuget install $resourcesName -Version $resourcesVersion -Source $WdpResourcesFeed -OutputDirectory . -x -prerelease
}

New-Item -ItemType Directory -Force -Path $($downloadFolder + "\packages")
if ($useLocal -eq $false) {
    Write-Host "Downloading latest SPE and SXA" -ForegroundColor Green

    Start-BitsTransfer -Source $sxaPackageUrl -Destination $([io.path]::combine($downloadFolder, "packages\Sitecore Experience Accelerator 1.6 rev. 180103 for 9.0.zip")) 
    Start-BitsTransfer -Source $spePackageUrl -Destination $([io.path]::combine($downloadFolder, "packages\Sitecore PowerShell Extensions-4.7 for Sitecore 8.zip"))
}