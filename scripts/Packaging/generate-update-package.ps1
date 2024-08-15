Param(
  [string]$target,
  [string]$output
)

Write-Host "Installing Module"
Install-Module -Name Sitecore.Courier -Repository PSGallery -Force -Confirm:$False  -RequiredVersion 1.4.3

Write-Host "Importing Module"
Import-Module Sitecore.Courier -Force -Verbose

Write-Host "Creating Update Packages"

New-Item -ItemType Directory -Path "$output/security"
New-CourierSecurityPackage -items $target -output "$output/security/Sitecore.Core.dacpac"
Write-Host "Created Security Update Package" -ForegroundColor Green
