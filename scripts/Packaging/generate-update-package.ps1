Param(
  [string]$target,
  [string]$output
)
Install-Module -Name Sitecore.Courier
Import-Module Sitecore.Courier
$destination = Split-Path $output

New-Item $destination -ItemType Directory -ErrorAction SilentlyContinue

New-CourierPackage -Target $target -Output $output -SerializationProvider "Rainbow" -IncludeFiles $false