<#
    This script generates a Sitecore update package out of the Habitat build output
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

$rootFolder = Get-ChildItem (Join-Path $([IO.Path]::Combine($config.DeployFolder, 'Website')) *)

ForEach($folder in $rootFolder){

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