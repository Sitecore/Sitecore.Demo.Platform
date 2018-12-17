<#

.SYNOPSIS
This script generates a Sitecore update package out of the Habitat Home build output

.PARAMETER ConfigurationFile
A cake-config.json file

#>

Param(
    [parameter(Mandatory = $true)]
    [string] $ConfigurationFile
)

###########################
# Find configuration files
###########################

Import-Module "$($PSScriptRoot)\ProcessConfigFile\ProcessConfigFile.psm1" -Force

$configuration = ProcessConfigFile -Config $ConfigurationFile
$config = $configuration.cakeConfig
$azureuserconfig = $configuration.azureUserConfig
$assetsFolder = $configuration.assetsFolder
$buildFolder = $configuration.buildFolder

################################################################
# Prepare folders for update package generation and triggers it
################################################################

Function Process-UpdatePackage([PSObject] $Configuration, [String] $FolderString, $assetsfolder) {

    # Get the output folder path

    $targetFolderName = (Get-Item -Path $FolderString).Name
    $sourceFolder = (Get-Item -Path $FolderString).FullName

    # Create a target folder that will host the generated .update package file

    if (!(Test-Path -Path $([IO.Path]::Combine($assetsfolder, $targetFolderName)))) {
        Write-Host "Creating" $([IO.Path]::Combine($assetsfolder, $targetFolderName))
        New-Item -ItemType Directory -Force -Path $([IO.Path]::Combine($assetsfolder, $targetFolderName))        
            
    }

    $updateFile = Join-Path $([IO.Path]::Combine($assetsfolder, $targetFolderName)) "$($targetFolderName).update"
    GenerateUpdatePackage -configFile $Configuration -argSourcePackagingFolder $sourceFolder -argOutputPackageFile $updateFile

    # Check if scaled configuration is in use and generate an additional CD package

    if (($config.Topology -eq "scaled") -and ($targetFolderName -eq "HabitatHome")) {

        # Copy the contents of the HabitatHome build output to a new folder, but exclude the YML files inside the serialization folder

        $sourceFolderCD = $sourceFolder + "CD"
        
        Copy-Item -Path $sourceFolder -Destination $sourceFolderCD -Exclude *.yml -Recurse -Force

        # Remove App_Data folder from CD
        $exclusionFolder = "$($sourceFolderCD)\App_Data\"
        if (Test-Path $exclusionFolder) {
            Remove-Item $exclusionFolder -Recurse -Force
        }

        # Create a separate folder that will host the scaled CD package 
        
        $targetFolderNameCD = $targetFolderName + "CD"
        if (!(Test-Path -Path $([IO.Path]::Combine($assetsfolder, $targetFolderNameCD)))) {
            Write-Host "Creating" $([IO.Path]::Combine($assetsfolder, $targetFolderNameCD))
            New-Item -ItemType Directory -Force -Path $([IO.Path]::Combine($assetsfolder, $targetFolderNameCD))        
                
        }

        # Update the filename for the package and generate the CD update package in a separate folder
        
        $updateFile = Join-Path $([IO.Path]::Combine($assetsfolder, $targetFolderNameCD)) "$($targetFolderName).update"
        GenerateUpdatePackage -configFile $Configuration -argSourcePackagingFolder $sourceFolderCD -argOutputPackageFile $updateFile

    }

}

###############################
# Generate the Update packages
###############################

Function GenerateUpdatePackage() {

    Param(
        [parameter(Mandatory = $true)]
        [String] $configFile,
        [String] $argSourcePackagingFolder,
        [String] $argOutputPackageFile

    )

    Set-PSRepository -Name PSGallery -InstallationPolicy Trusted
    Install-Module -Name Sitecore.Courier

    New-CourierPackage -Target $($argSourcePackagingFolder) -Output $($argOutputPackageFile) -SerializationProvider "Rainbow" -IncludeFiles $true
	
}

###############################
# Setup CDN
###############################

Function SetupCDN([PSObject] $Configuration, [String] $FolderString) {
      
    # Web.config.xdt
    $webConfigFilePath = $([IO.Path]::Combine($FolderString, "web.config.xdt"))   

    if (Test-Path -Path $webConfigFilePath) {   
        [xml]$webConfigXml = Get-Content -Path $webConfigFilePath  
        $integrationsNode = $webConfigXml.SelectSingleNode("//add[@key='integrations:define']")
 
        if ($Configuration.CDN -eq "true") {   
            if ($integrationsNode) {
                if ($integrationsNode.Value -eq "None") {
                    $integrationsNode.Value = "CDN"
                }
                else {
                    $integrationsNode.Value = $integrationsNode.Value + ",CDN"
                }
            }
        }
        else {
    
            if ($integrationsNode) {       

                if ($integrationsNode.Value -eq "CDN") {
                    $integrationsNode.Value = "None" 
                }
                else {
                    if ($integrationsNode.Value -match "CDN") {
                        $integrationsNode.Value = $integrationsNode.Value -replace ",CDN,", ","
                        $integrationsNode.Value = $integrationsNode.Value -replace ",CDN", ""
                        $integrationsNode.Value = $integrationsNode.Value -replace "CDN,", ","
                    }
                }
            }
        }

        $webConfigXml.Save($webConfigFilePath)
    }
    
    ## Modify Foundation CDN Config

    $foundationCdnConfigFilePath = $([IO.Path]::Combine($FolderString, "App_Config\Include\Foundation\Foundation.CDN.config"))   

    if (Test-Path -Path $foundationCdnConfigFilePath) {   
        [xml]$foundationCDNConfig = Get-Content -Path $foundationCdnConfigFilePath
        $namespace = New-Object System.Xml.XmlNamespaceManager($foundationCDNConfig.NameTable)
        $namespace.AddNamespace("patch", "http://www.sitecore.net/xmlconfig/")

        $mediaLinkServerUrlPatchNode = $foundationCDNConfig.SelectSingleNode("//setting[@name='Media.MediaLinkServerUrl']/patch:attribute", $namespace)
 
        if ($Configuration.CDN -eq "true") { 
            $config = $azureuserconfig.settings | Where-Object {$_.id -eq "AzureDeploymentID"}
            $AzureDeploymentID = $config.value
                
            if ($mediaLinkServerUrlPatchNode) {                    
        
                $mediaLinkServerUrlPatchNode.InnerText = $AzureDeploymentID + "-endpoint.azureedge.net";
            }

            $foundationCDNConfig.Save($foundationCdnConfigFilePath)
        }
	
    }
}



#####################################
# Clean up and prepare for packaging
#####################################

Function Clean-Up([PSObject] $Configuration, [String] $FolderString) {

    # Clean Assemblies

    $AssembliesToRemove = @("Sitecore.*.dll", "Unicorn*.dll", "Rainbow*.dll", "Kamsar*.dll", "Microsoft.*.dll", "HtmlAgilityPack.dll", "ICSharpCode.SharpZipLib.dll", "Lucene.Net.*", "Mvp.Xml.dll", "Newtonsoft.Json.dll", "Owin.dll", "Remotion.Linq.dll", "System.*.dll")
    $AssembliesToKeep = @("Sitecore.HabitatHome.*", "Sitecore.DataExchange.*", "Microsoft.Owin.Security.Facebook.dll", "Microsoft.Owin.Security.MicrosoftAccount.dll", "Microsoft.Owin.Security.OpenIdConnect.dll")

    Get-ChildItem $FolderString -Include $AssembliesToRemove -Exclude $AssembliesToKeep -Recurse | ForEach-Object($_) { Remove-Item $_.FullName }

    # Clean Configs Configs

    $ConfigsToRemove = @("*.Serialization*.config", "Unicorn*.config*", "Rainbow*.config")  

    Get-ChildItem $FolderString -Include $ConfigsToRemove -Recurse | ForEach-Object($_) { Remove-Item $_.FullName }

    # Clean configurations in bin

    $BinFolder = $([IO.Path]::Combine($FolderString, "bin"))

    $BinConfigsToRemove = @("*.config", "*.xdt")  
       

    Get-ChildItem $BinFolder -Include $BinConfigsToRemove -Recurse | ForEach-Object($_) { Remove-Item $_.FullName }

    # Clean Empty Folders

    Get-ChildItem $FolderString -recurse | 

    Where-Object { $_.PSIsContainer -and @(Get-ChildItem -Lit $_.Fullname -r | Where-Object {!$_.PSIsContainer}).Length -eq 0 } |

    Remove-Item -recurse    
}


$rootFolder = Get-ChildItem (Join-Path $buildFolder *)

#Prepare Packages

ForEach ($folder in $rootFolder) {
    Clean-Up -Configuration $config -FolderString (Get-Item -Path $folder).FullName
    SetupCDN -Configuration $config -FolderString (Get-Item -Path $folder).FullName

    Write-Host $folder

    switch ((Get-Item -Path $folder).Name) {  

        "HabitatHome" {           
            Process-UpdatePackage -Configuration $config -FolderString $folder -assetsfolder $assetsfolder
        }
        "habitatHome_xConnect" {
            Process-UpdatePackage -Configuration $config -FolderString $folder -assetsfolder $assetsfolder
        }
    }
}