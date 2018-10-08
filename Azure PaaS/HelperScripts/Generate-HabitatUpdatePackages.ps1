<#
    This script generates a Sitecore update package out of the Habitat build output
#>

###############################################################
# Prepare folders for update package generation and triggers it

Function Process-UpdatePackage([PSObject] $Configuration, [String] $FolderString){

    # Get the output folder path

    $targetFolderName = (Get-Item -Path $FolderString).Name
    $sourceFolder = (Get-Item -Path $FolderString).FullName

    # Create a target folder that will host the generated .update package file

    if(!(Test-Path -Path $([IO.Path]::Combine($Configuration.DeployFolder, 'assets', $targetFolderName)))){
            
        $targetFolder = New-Item -ItemType Directory -Force -Path $([IO.Path]::Combine($Configuration.DeployFolder, 'assets', $targetFolderName))
            
    } else {
            
        Write-Host "The target folder at $((Get-Item -Path $FolderString).FullName) already exists"
        $confirmation = Read-Host "Do you want to overwrite it? Press 'Y' for yes or 'N' for no"
        if($confirmation -eq "Y"){
                
            $targetFolder = New-Item -ItemType Directory -Force -Path $([IO.Path]::Combine($Configuration.DeployFolder, 'assets', $targetFolderName))
                
        } else {
                
            Write-Host "Cancelled ... Stopping the script"
            Exit 1
                
        }
            
    }

    $updateFile = Join-Path $targetFolder.FullName "$($targetFolder.Name).update"
	$courierPath = $([IO.Path]::Combine($Configuration.DeployFolder, 'assets', 'Sitecore Courier')
    GenerateUpdatePackage -pathToCourier "$($courierPath)\Sitecore.Courier.Runner.exe" -argSourcePackagingFolder $sourceFolder -argOutputPackageFile $updateFile

}

###############################
# Generate the Update packages

Function GenerateUpdatePackage(){

    Param(
        [parameter(Mandatory=$true)]
        [String] $pathToCourier,
		[parameter(Mandatory=$true)]
		[String] $configFile,
        [String] $argSourcePackagingFolder,
        [String] $argOutputPackageFile

    )

    $packagingProcess = Start-Process -FilePath $pathToCourier -ArgumentList "-t $($argSourcePackagingFolder)", "-o $($argOutputPackageFile)", "-r", "-f"
    $packagingProcess.HasExited
    $packagingProcess.ExitCode

}

# Find and process cake-config.json

if (!(Test-Path $configFile)) {

    Write-Host "Configuration file '$($configFile)' not found." -ForegroundColor Red
    Write-Host  "Please ensure there is a cake-config.json configuration file at '$($configFile)'" -ForegroundColor Red
    Exit 1
    
}

$config = Get-Content -Raw $configFile | ConvertFrom-Json
if (!$config) {

    throw "Error trying to load configuration!"
    
}

$rootFolder = Get-ChildItem (Join-Path $config.DeployFolder *)

ForEach($folder in $rootFolder){

    switch((Get-Item -Path $folder).Name){
    
        "website"
        {

            Process-UpdatePackage -Configuration $config -FolderString $folder

        }
        "xconnect"
        {

            Process-UpdatePackage -Configuration $config -FolderString $folder

        }

    }

}