<#
	This script prepares Web Deploy Package (WDP) creation, by reading through configuration files and by looking for pre-existing mandatory files for the WDP creation process itself.
	It then generates a WDP to be used in Azure deployments. During the WDP generation process, a 3rd party zip library is used (Ionic Zip) to zip up and help generate the Sitecore
	Cargo Payload (SCCPL) packages.
#>

#######################
# Mandatory parameters
#######################

Param(
    [parameter(Mandatory=$true)]
    [String] $ConfigurationFile
)

###########################
# Find configuration files
###########################

if (!(Test-Path $ConfigurationFile)) {

        Write-Host "Configuration file '$($ConfigurationFile)' not found." -ForegroundColor Red
        Write-Host  "Please ensure there is a cake-config.json configuration file at '$($ConfigurationFile)'" -ForegroundColor Red
        Exit 1
    
    }

    $config = Get-Content -Raw $ConfigurationFile |  ConvertFrom-Json
    if (!$config) {

        throw "Error trying to load configuration!"
    
    }

    # Find and process assets.json

	if($config.Topology -eq "single")
	{
		[String] $assetsFile = $([IO.Path]::combine($config.ProjectFolder, 'Azure Paas', 'XP0 Single', 'assets.json'))
	}
	else
	{
		throw "Only XP0 Single Deployments are currently supported, please change the Topology parameter in the cake-config.json to single"
	}


    if (!(Test-Path $assetsFile)) {

        Write-Host "Assets file '$($assetsFile)' not found." -ForegroundColor Red
        Write-Host  "Please ensure there is a assets.json file at '$($assetsFile)'" -ForegroundColor Red
        Exit 1

    }

    $assetsConfig = Get-Content -Raw $assetsFile |  ConvertFrom-Json
    if (!$assetsConfig) {

        throw "Error trying to load Assest File!"

    } 

##################################################################
# 3rd Party Ionic Zip function - helping create the SCCPL package
##################################################################

Function Zip ([String] $FolderToZip, [String] $ZipFilePath, [String] $DotNetZipPath) {

  # load Ionic.Zip.dll 
  
  [System.Reflection.Assembly]::LoadFrom($DotNetZipPath)
  $Encoding = [System.Text.Encoding]::GetEncoding(65001)
  $ZipFile =  New-Object Ionic.Zip.ZipFile($Encoding)

  $ZipFile.AddDirectory($FolderToZip) | Out-Null

  If (!(Test-Path (Split-Path $ZipFilePath -Parent))) {

    mkdir (Split-Path $ZipFilePath -parent)

  }

  Write-Host "Saving zip file from $FolderToZip"
  $ZipFile.Save($ZipFilePath)
  $ZipFile.Dispose()
  Write-Host "Saved..."

}

################################
# Create the Web Deploy Package
################################

# Create-WDP function explained:

<#

 -RootFolder is the physical path on the filesystem to the source folder for WDP operations that will contain the WDP JSON configuration file, 
 the WDP XML parameters file and the folder with the module packages
 The typical structure that should be followed is:

    \RootFolder\module_name_module.json
    \RootFolder\module_name_parameters.xml
    \RootFolder\SourcePackage\module_installation_package.zip( or .update)

 -SitecoreCloudModulePath provides the path to the Sitecore.Cloud.Cmdlets.psm1 Azure Toolkit Powershell module (usually under \SAT\tools)

 -JsonConfigFilename is the name of your WDP JSON configuration file

 -XmlParameterFilename is the name of your XML parameter file (must match the name that is provided inside the JSON config)

 -SccplCargoFilename is the name of your Sitecore Cargo Payload package (must match the name that is provided inside the JSON config)

 -IonicZip is the path to Ionic's zipping library


 Examples:

 Create-WDP -RootFolder "C:\_deployment\website_packaged_test" `
            -SitecoreCloudModulePath "C:\Users\auzunov\Downloads\ARM_deploy\1_Sitecore Azure Toolkit\tools\Sitecore.Cloud.Cmdlets.psm1" `
            -JsonConfigFilename "website_config" `
            -XmlParameterFilename "website_parameters" `
            -SccplCargoFilename "website_cargo" `
            -IonicZip ".\Sitecore Azure Toolkit\tools\DotNetZip.dll"

 Create-WDP -RootFolder "C:\Users\auzunov\Downloads\ARM_deploy\Modules\DEF" `
            -SitecoreCloudModulePath "C:\Users\auzunov\Downloads\ARM_deploy\1_Sitecore Azure Toolkit\tools\Sitecore.Cloud.Cmdlets.psm1" `
            -JsonConfigFilename "def_config" `
            -XmlParameterFilename "def_parameters" `
            -SccplCargoFilename "def_cargo" `
            -IonicZip ".\Sitecore Azure Toolkit\tools\DotNetZip.dll"

#>

Function Create-WDP ([String] $RootFolder, [String] $SitecoreCloudModulePath, [String] $JsonConfigFilename, [String] $XmlParameterFilename, [String] $SccplCargoFilename, [String] $IonicZip){

    # Create empty folder structures for the WDP work

    [string] $DestinationFolderPath = New-Item -Path "$($RootFolder)\convert to WDP\WDP" -ItemType Directory -Force

    # WDP Components folder and sub-folders creation

    $ComponentsFolderPath = New-Item -Path "$($RootFolder)\convert to WDP\Components" -ItemType Directory -Force
    $CargoPayloadFolderPath = New-Item -Path "$($ComponentsFolderPath)\CargoPayloads" -ItemType Directory -Force
    $AdditionalWdpContentsFolderPath = New-Item -Path "$($ComponentsFolderPath)\AdditionalFiles" -ItemType Directory -Force
    $JsonConfigFolderPath = New-Item -Path "$($ComponentsFolderPath)\Configs" -ItemType Directory -Force
    $ParameterXmlFolderPath = New-Item -Path "$($ComponentsFolderPath)\MsDeployXmls" -ItemType Directory -Force

    # Provide the required files for WDP

    $JsonConfigFilenamePath = Get-ChildItem -Path $JsonConfigFilename

    [String] $ConfigFilePath = "$($JsonConfigFolderPath)\$($JsonConfigFilenamePath.Name)"
    [String] $CargoPayloadZipFilePath = "$($ComponentsFolderPath)\$($SccplCargoFilename).zip"
    [String] $CargoPayloadFilePath = "$($CargoPayloadFolderPath)\$($SccplCargoFilename).sccpl"

    # Copy the parameters.xml file over to the target ParameterXml folder

    Copy-Item -Path $XmlParameterFilename -Destination $ParameterXmlFolderPath.FullName -Force

    # Copy the config.json file over to the target Config folder

    Copy-Item -Path $JsonConfigFilename -Destination $ConfigFilePath -Force

    # Create folders for Sitecore Cargo Payload file

    $CopyToRootPath = New-Item -Path "$($CargoPayloadFolderPath)\CopyToRoot" -ItemType Directory -Force
    $CopyToWebsitePath = New-Item -Path "$($CargoPayloadFolderPath)\CopyToWebsite" -ItemType Directory -Force
    $IOActionsPath = New-Item -Path "$($CargoPayloadFolderPath)\IOActions" -ItemType Directory -Force
    $XdtsPath = New-Item -Path "$($CargoPayloadFolderPath)\Xdts" -ItemType Directory -Force

	# Copy over the configuration transform file structure from the Habitat project's build output to be inlcluded to the WDP package

	Copy-Item -Path $([IO.Path]::Combine($config.DeployFolder, 'assets', 'HabitatHome')) -Filter "*.xdt" -Destination $XdtsPath.FullName -Recurse -Container -force

    # Zip up all Cargo Payload folders using Ionic Zip

    Zip -FolderToZip $CargoPayloadFolderPath.FullName -ZipFilePath $CargoPayloadZipFilePath -DotNetZipPath $IonicZip

    # Move and rename the zipped file to .sccpl - create the Sitecore Cargo Payload file

    Move-Item -Path $CargoPayloadZipFilePath -Destination $CargoPayloadFilePath -Force

    # Clean up SCCPL folders

    Remove-Item -Path $CopyToRootPath.FullName -Recurse -Force
    Remove-Item -Path $CopyToWebsitePath.FullName -Recurse -Force
    Remove-Item -Path $IOActionsPath.FullName -Recurse -Force
    Remove-Item -Path $XdtsPath.FullName -Recurse -Force
    Remove-Item -Path $CargoPayloadZipFilePath -ErrorAction Ignore

    # Build the WDP file

    Import-Module $SitecoreCloudModulePath -Verbose
    Start-SitecoreAzureModulePackaging -SourceFolderPath $RootFolder `
                                        -DestinationFolderPath $DestinationFolderPath `
                                        -CargoPayloadFolderPath $CargoPayloadFolderPath.FullName `
                                        -AdditionalWdpContentsFolderPath $AdditionalWdpContentsFolderPath.FullName `
                                        -ParameterXmlFolderPath $ParameterXmlFolderPath.FullName `
                                        -ConfigFilePath $ConfigFilePath

    # Check for the _Data Exchange Framework 2.0.1 rev. 180108_single.scwdp.zip and rename that back (remove the underscore)

    $DEFWDPFile = "_Data Exchange Framework 2.0.1 rev. 180108_single.scwdp.zip"
    $DEFWDPFilePath = Join-Path $DestinationFolderPath $DEFWDPFile
                               
    If(Test-Path -Path $DEFWDPFilePath){

		try {
					
			Rename-Item -Path $DEFWDPFilePath -NewName "$($DestinationFolderPath)\$($DEFWDPFile.Substring(1))"
					
		} catch {
					
			Write-Host "Unable to rename the file... continuing" -ForegroundColor DarkYellow
			Continue

		}
                
    }

    # Check for the _Data Exchange Framework 2.0.1 rev. 180108.zip and rename that back (remove the underscore)

    $DEFZipFile = "_Data Exchange Framework 2.0.1 rev. 180108.zip"
    $DEFZipFilePath = Join-Path $RootFolder $DEFZipFile
                               
    If(Test-Path -Path $DEFZipFilePath){

		try {
					
			Rename-Item -Path $DEFZipFilePath -NewName "$($RootFolder)\$($DEFZipFile.Substring(1))"
					
		} catch {
					
			Write-Host "Unable to rename the file... continuing" -ForegroundColor DarkYellow
			Continue

		}
                
    }

}

########################################################################################
# WDP preparation function which sets up initial components required by the WDP process
########################################################################################

Function Prepare-WDP ($config, $assetsConfig) {

    # Assign values to required working folder paths
    
    [String] $assetsFolder = $([IO.Path]::combine($config.DeployFolder, 'assets'))
    [String] $ProjectModulesFolder = $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'WDP Components', 'Modules'))
	[String] $HabitatWDPFolder = $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'WDP Components', 'Habitat'))
    [String] $SitecoreCloudModule = $([IO.Path]::combine($assetsFolder, 'Sitecore Azure Toolkit', 'tools', 'Sitecore.Cloud.Cmdlets.psm1'))
    [String] $IonicZipPath = $([IO.Path]::combine($assetsFolder, 'Sitecore Azure Toolkit', 'tools', 'DotNetZip.dll'))

    # Go through the assets.json file and prepare files and paths for the conversion to WDP for all prerequisites for Habitat Home

    ForEach ($asset in $assetsConfig.prerequisites){

        If($asset.convertToWdp -eq $True){
            
			# Do a check if the WDP package already exists and if not, proceed with package generation

            [String] $ModuleFolder = $([IO.Path]::Combine($assetsFolder, $asset.name))
			[String] $ModuleWDPTarget = "$($ModuleFolder)\convert to WDP\WDP"
			If((Test-Path -Path $ModuleWDPTarget) -eq $False){
	
				Get-ChildItem -Path "$($ProjectModulesFolder)\$($asset.name)\*" -Include *.json | ForEach-Object { $WDPJsonFile = $_.FullName; $WDPJsonFileName = $_.BaseName }
				Get-ChildItem -Path "$($ProjectModulesFolder)\$($asset.name)\*" -Include *.xml | ForEach-Object { $WDPXMLFile = $_.FullName }
				$SccplCargoName = $WDPJsonFileName -replace "_config", "_cargo"

				# Special check - if dealing with DEF try to avoid limitations of the Cloud.Cmdlets script

				If($ModuleFolder -like "*Data Exchange Framework*")
				{
            
					# Check if the "Data Exchange Framework 2.0.1 rev. 180108.zip" file is present and rename it to "_Data Exchange Framework 2.0.1 rev. 180108.zip"

					$DEFZipFile = "Data Exchange Framework 2.0.1 rev. 180108.zip"
					$DEFZipFilePath = Join-Path $ModuleFolder $DEFZipFile
                                
					If(Test-Path -Path $DEFZipFilePath)
					{
                
						try 
						{
					
							Rename-Item -Path $DEFZipFilePath -NewName "$($ModuleFolder)\_$($DEFZipFile)"
					
						} 
						catch 
						{
					
							Write-Host "Unable to rename the file... continuing"
							Continue

						}
					}
				}

				# Call in the WDP creation function

				Create-WDP -RootFolder $ModuleFolder -SitecoreCloudModulePath $SitecoreCloudModule -JsonConfigFilename $WDPJsonFile -XmlParameterFilename $WDPXMLFile -SccplCargoFilename $SccplCargoName -IonicZip $IonicZipPath
      
			} else {
			
				Write-Host "Skipping WDP generation - there's already a WDP package, present at $($ModuleWDPTarget)" -ForegroundColor Yellow
			
			}

        }

    }   

	# Prepare the files and paths for the Habitat Home WDP creation

	ForEach($folder in (Get-ChildItem -Path "$($assetsFolder)")){

		switch($folder){
    
			"habitathome"
			{
				
				# Do a check if the WDP package already exists and if not, proceed with package generation

				[String] $HabitatWDPTarget = "$($folder.FullName)\convert to WDP\WDP"
				If((Test-Path -Path $HabitatWDPTarget) -eq $False){

					Get-ChildItem -Path "$($HabitatWDPFolder)\*" -Include *$($folder)*.json | ForEach-Object { $WDPJsonFile = $_.FullName }
					Get-ChildItem -Path "$($HabitatWDPFolder)\*" -Include *$($folder)*.xml | ForEach-Object { $WDPXMLFile = $_.FullName }
					[String] $SccplCargoName = -join ($folder.Name, "_cargo")
					Create-WDP -RootFolder $folder.FullName -SitecoreCloudModulePath $SitecoreCloudModule -JsonConfigFilename $WDPJsonFile -XmlParameterFilename $WDPXMLFile -SccplCargoFilename $SccplCargoName -IonicZip $IonicZipPath
					
				} else {
			
					Write-Host "Skipping WDP generation - there's already a WDP package, present at $($HabitatWDPTarget)" -ForegroundColor Yellow
			
				}

			}
			"xconnect"
			{
				
				# Do a check if the WDP package already exists and if not, proceed with package generation

				[String] $HabitatWDPTarget = "$($folder.FullName)\convert to WDP\WDP"
				If((Test-Path -Path $HabitatWDPTarget) -eq $False){

					Get-ChildItem -Path "$($HabitatWDPFolder)\*" -Include *$($folder)*.json | ForEach-Object { $WDPJsonFile = $_.FullName }
					Get-ChildItem -Path "$($HabitatWDPFolder)\*" -Include *$($folder)*.xml | ForEach-Object { $WDPXMLFile = $_.FullName }
					[String] $SccplCargoName = -join ($folder.Name, "_cargo")
					Create-WDP -RootFolder $folder.FullName -SitecoreCloudModulePath $SitecoreCloudModule -JsonConfigFilename $WDPJsonFile -XmlParameterFilename $WDPXMLFile -SccplCargoFilename $SccplCargoName -IonicZip $IonicZipPath
					
				} else {
			
					Write-Host "Skipping WDP generation - there's already a WDP package, present at $($HabitatWDPTarget)" -ForegroundColor Yellow
			
				}

			}

		}

	}
    
}

#######################################
# Call in the WDP preparation function
#######################################

Prepare-WDP -config $config -assetsConfig $assetsConfig