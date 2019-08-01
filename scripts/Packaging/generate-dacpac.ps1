Param(
  [string]$SitecoreAzureToolkitPath,
  [string]$updatePackagePath,
  [string]$destinationPath
)
$wdpName = (Split-Path $updatePackagePath -Leaf).replace("update","scwdp.zip")

Import-Module (Join-Path $sitecoreAzureToolkitPath "tools\Sitecore.Cloud.Cmdlets.dll") -Force

# Generate a Sitecore scwdp
ConvertTo-SCModuleWebDeployPackage  $updatePackagePath  $destinationPath -Force

# We only really want to ge the .dacpacs out of this whole process, let's do that now by extracting them from the zip
$filter = "*.dacpac"

Add-Type -Assembly System.IO.Compression.FileSystem
$wdpPath = (Join-Path $destinationPath $wdpName)
$zip = [IO.Compression.ZipFile]::OpenRead($wdpPath)
$zip.Entries | Where-Object {$_.Name -like $filter} | ForEach-Object {
  [System.IO.Compression.ZipFileExtensions]::ExtractToFile($_, $destinationPath + "\" + $_, $true)}
$zip.Dispose()

# Cleanup
 Get-ChildItem $destinationPath -Include *.scwdp.zip -Recurse | Remove-Item
