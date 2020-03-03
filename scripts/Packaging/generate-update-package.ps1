Param(
  [string]$target,
  [string]$output
)
Write-Host "Installing Module"
Install-Module -Name Sitecore.Courier -Repository PSGallery -Force -Confirm:$False
Write-Host "Importing Module"
Import-Module Sitecore.Courier

$destination = Split-Path $output

New-Item $destination -ItemType Directory -ErrorAction SilentlyContinue
Write-Host "Creating Update Packages"

New-CourierPackage -Target $target -Output $output -SerializationProvider "Rainbow" -IncludeFiles $false -EnsureRevision $true
Write-Host "Created Update Package"

New-CourierSecurityPackage -items $target -output "$destination\security.dacpac"
Write-Host "Created Security Update Package"