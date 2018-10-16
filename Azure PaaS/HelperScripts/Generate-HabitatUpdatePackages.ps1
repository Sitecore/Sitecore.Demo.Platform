<#

.SYNOPSIS
This script generates a Sitecore update package out of the Habitat build output

.PARAMETER ConfigurationFile
A cake-config.json file

#>

Param(
	[parameter(Mandatory=$true)]
    [string] $ConfigurationFile
)

###########################
# Find configuration files
###########################

Import-Module "$($PSScriptRoot)\ProcessConfigFile\ProcessConfigFile.psm1" -Force

$configarray     = ProcessConfigFile -Config $ConfigurationFile
$config          = $configarray[0]
$assetconfig     = $configarray[1]
$azureuserconfig = $configarray[2]

################################################################
# Prepare folders for update package generation and triggers it
################################################################

Function Process-UpdatePackage([PSObject] $Configuration, [String] $FolderString){

    # Get the output folder path

    $targetFolderName = (Get-Item -Path $FolderString).Name
    $sourceFolder = (Get-Item -Path $FolderString).FullName

    # Create a target folder that will host the generated .update package file

    if(!(Test-Path -Path $([IO.Path]::Combine($Configuration.DeployFolder, 'assets', $targetFolderName))))
	{
        Write-Host "Creating" $([IO.Path]::Combine($Configuration.DeployFolder, 'assets', $targetFolderName))
        New-Item -ItemType Directory -Force -Path $([IO.Path]::Combine($Configuration.DeployFolder, 'assets', $targetFolderName))
            
    }

    $updateFile = Join-Path $([IO.Path]::Combine($Configuration.DeployFolder, 'assets', $targetFolderName)) "$($targetFolderName).update"
	$courierPath = $([IO.Path]::Combine($Configuration.DeployFolder, 'assets', 'Sitecore Courier'))
    GenerateUpdatePackage -configFile $Configuration -argSourcePackagingFolder $sourceFolder -argOutputPackageFile $updateFile

}

###############################
# Generate the Update packages
###############################

Function GenerateUpdatePackage(){

    Param(
		[parameter(Mandatory=$true)]
		[String] $configFile,
        [String] $argSourcePackagingFolder,
        [String] $argOutputPackageFile

    )

	Set-PSRepository -Name PSGallery -InstallationPolicy Trusted
	Install-Module -Name Sitecore.Courier

	New-CourierPackage -Target $($argSourcePackagingFolder) -Output $($argOutputPackageFile) -SerializationProvider "Rainbow" -IncludeFiles $true
	
}


###############################
# Clean up and prepare for packaging
###############################

Function Clean-Up([PSObject] $Configuration, [String] $FolderString){

    # Clean Assemblies

    $AssembliesToRemove = @("Sitecore.*.dll","Unicorn*.dll","Rainbow*.dll", "Kamsar*.dll")
    $AssembliesToKeep = @("Sitecore.HabitatHome.*")

    Get-ChildItem $FolderString -Include $AssembliesToRemove -Exclude $AssembliesToKeep -Recurse | foreach($_) { Remove-Item $_.FullName }

    # Clean Configs Configs

    $ConfigsToRemove = @("*.Serialization*.config", "Unicorn*.config*", "Rainbow*.config")  

    Get-ChildItem $FolderString -Include $ConfigsToRemove -Recurse | foreach($_) { Remove-Item $_.FullName }

    # Clean configurations in bin

    $BinFolder = $([IO.Path]::Combine($FolderString, "bin"))

    $BinConfigsToRemove = @("*.config", "*.xdt")  

    Get-ChildItem $BinFolder -Include $BinConfigsToRemove -Recurse | foreach($_) { Remove-Item $_.FullName }

    # Clean Empty Folders

    dir $FolderString -recurse | 

    Where { $_.PSIsContainer -and @(dir -Lit $_.Fullname -r | Where {!$_.PSIsContainer}).Length -eq 0 } |

    Remove-Item -recurse    
}


$rootFolder = Get-ChildItem (Join-Path $([IO.Path]::Combine($config.DeployFolder, 'Website')) *)

#Prepare Packages

ForEach($folder in $rootFolder){
    Clean-Up -Configuration $config -FolderString (Get-Item -Path $folder).FullName
    
    Write-Host $folder

    switch((Get-Item -Path $folder).Name){  

        "HabitatHome"
        {           
            Process-UpdatePackage -Configuration $config -FolderString $folder
        }
        "xconnect"
        {
            Process-UpdatePackage -Configuration $config -FolderString $folder
        }
    }
}