
if (-not ([string]::IsNullOrEmpty($env:INTERNAL_NUGET_SOURCE) -or [string]::IsNullOrEmpty($env:INTERNAL_NUGET_SOURCE_USERNAME) -or [string]::IsNullOrEmpty($env:INTERNAL_NUGET_SOURCE_PASSWORD))) {
    Write-Host "Adding internal nuget source"
    dotnet nuget add source "$($env:INTERNAL_NUGET_SOURCE)" --name "sc-internal-package-feed" --username "$($env:INTERNAL_NUGET_SOURCE_USERNAME)" --password "$($env:INTERNAL_NUGET_SOURCE_PASSWORD)" --configfile "nuget.config" --store-password-in-clear-text
}