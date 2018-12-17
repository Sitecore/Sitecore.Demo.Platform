
<#
.SYNOPSIS
Prepare the environment for building and packaging Habitat Home WDP

.DESCRIPTION
This script prepares Web Deploy Package (WDP) creation, by reading through configuration files and by looking for 
pre-existing mandatory files for the WDP creation process. It then generates a WDP to be used in Azure deployments. 
During the WDP generation process, a 3rd party zip library is used (Ionic Zip) to zip up and help generate the Sitecore
Cargo Payload (SCCPL) packages.

.PARAMETER ConfigurationFile
A cake-config.json file

#>

#######################
# Mandatory parameters
#######################

Param(
    [parameter(Mandatory = $true)]
    [String] $ConfigurationFile,
    [string] $devSitecoreUserName,
    [string] $devSitecorePassword
)

###########################
# Find configuration files
###########################

Import-Module "$($PSScriptRoot)\ProcessConfigFile\ProcessConfigFile.psm1" -Force

$configuration = ProcessConfigFile -Config $ConfigurationFile
$assetconfig = $configuration.assets
$assetsFolder = $configuration.assetsFolder

Function Download-Asset {
    param(
        [PSCustomObject]$assetfilename,
        $Credentials,
        $assetsFolder,
        $sourceuri,
        $sourceType
    )

    if (!(Test-Path $assetsFolder)) {

        Write-Host "Assets Folder does not exist"
        Write-Host "Creating Assets Folder"

        New-Item -ItemType Directory -Force -Path $assetsFolder
    }

    Write-Host "Downloading" $assetfilename -ForegroundColor Green

    $params = @{
        Source        = $sourceuri
        Destination   = $assetsFolder
        Credentials   = $Credentials
        Assetfilename = $assetfilename
        TypeSource    = $sourceType
    }
    Import-Module "$($PSScriptRoot)\DownloadFileWithCredentials\DownloadFileWithCredentials.psm1" -Force

    Invoke-DownloadFileWithCredentialsTask  @params  
		
}
Function Install-SitecoreAzureToolkit {

    # Download Sitecore Azure Toolkit (used for converting modules)
    $package = $assetconfig.prerequisites | Where-Object {$_.id -eq "sat"}
    $destination = Join-Path $assetsFolder $package.fileName
    Set-Alias sz 'C:\Program Files\7-Zip\7z.exe'
    if (!(Test-Path $destination)) {
        $securePassword = ConvertTo-SecureString $devSitecorePassword -AsPlainText -Force
        $credentials = New-Object System.Management.Automation.PSCredential($devSitecoreUserName, $securePassword)

        Download-Asset -assetfilename $package.fileName -Credentials $credentials -assetsFolder $assetsFolder -sourceuri $package.url -sourceType "sitecore"
    }

    if ((Test-Path $destination) -and ( $package.install -eq $true)) {
        if (Test-Path $([io.path]::combine($assetsFolder, $package.name, 'tools', 'DotNetZip.dll'))) {
            Write-Host $package.name "found, skipping extraction"
            continue
        }
        else {
            sz x -o"$([io.path]::combine($assetsFolder,$package.name))" $destination  -y -aoa
        }
    }
}

Install-SitecoreAzureToolkit