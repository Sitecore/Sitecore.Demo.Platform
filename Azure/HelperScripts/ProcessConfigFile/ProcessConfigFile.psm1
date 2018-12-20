function ProcessConfigFile {
    <#
.SYNOPSIS
Processes json configuration files

.DESCRIPTION
Converts cake-config.json, assets.json, and azureuser-config.json configs to powershell objects.
Specifices the file path of various folders and config files.
Specifices version number and topology name.
The script then returns all these as an array.

.PARAMETER ConfigurationFile
A cake-config.json file

.Example
$configarray			= ProcessConfigFile -Config $ConfigurationFile
$config					= $configarray[0]
$assetConfig			= $configarray[1]
$azureuserconfig		= $configarray[2]
$assetconfigfile		= $configarray[3]
$azureuserConfigFile	= $configarray[4]
$topologypath	        = $configarray[5]
$topologyName			= $configarray[6]
$assetsfolder			= $configarray[7]
$SCversion				= $configarray[8]
$buildFolder			= $configarray[9]
#>

    [CmdletBinding()]
    Param(
        [parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [Alias ("Config")]
        [string] $ConfigurationFile
    )
    ####################################
    # Find and process cake-config.json
    ####################################
    $configuration = @{}

    if (!(Test-Path $ConfigurationFile)) {
        Write-Host "Configuration file '$($ConfigurationFile)' not found." -ForegroundColor Red
        Write-Host  "Please ensure there is a cake-config.json configuration file at '$($ConfigurationFile)'" -ForegroundColor Red
        Exit 1
    }

    $configuration.cakeConfig = Get-Content -Raw $ConfigurationFile |  ConvertFrom-Json
		
    if (!$configuration.cakeConfig) {
        throw "Error trying to load configuration!"
    } 
    $cakeConfig = $configuration.cakeConfig

    # Note the selected topology and assign the correct project path

    if ($configuration.cakeConfig.Topology -eq "single") {
        [string] $topologyPath = $([io.path]::combine($cakeConfig.ProjectFolder, 'Azure', 'XPSingle'))
        $topologyName =	'XPSingle'

    }
    elseif ($cakeConfig.Topology -eq "scaled") {
        [string] $topologyPath = $([io.path]::combine($cakeConfig.ProjectFolder, 'Azure', 'XP'))
        $topologyName =	'XP'
    }
    else {
        Write-Host "Please select a topology - either 'single' or 'scaled'" -ForegroundColor DarkRed
    }
    $configuration.topologyName = $topologyName
    $configuration.topologyPath = $topologyPath

    ###############################
    # Find and process assets.json
    ###############################
		
    [string] $assetsConfigFile = $([io.path]::combine($topologyPath, 'assets.json'))

    if (!(Test-Path $assetsConfigFile)) {
        Write-Host "Assets file '$($assetsConfigFile)' not found." -ForegroundColor Red
        Write-Host  "Please ensure there is a assets.json file at '$($assetsConfigFile)'" -ForegroundColor Red
        Exit 1
    }
		
    $configuration.assetsConfigFile = $assetsConfigFile
    $configuration.assets = Get-Content -Raw $assetsConfigFile |  ConvertFrom-Json

    if (!$configuration.assets) {
        throw "Error trying to load Assest File!"
    } 

    #########################################
    # Find and process azureuser-config.json
    #########################################
    if ($cakeConfig.DeploymentTarget -eq "Azure") {

        [string] $azureuserConfigFile = $([io.path]::combine($topologyPath, 'azureuser-config.json'))

        if (!(Test-Path $azureuserConfigFile)) {
            Write-Host "azureuser-config file '$($azureuserConfigFile)' not found." -ForegroundColor Red
            Write-Host  "Please ensure there is a user-config.json configuration file at '$($azureuserConfigFile)'" -ForegroundColor Red
            Exit 1
        }

        $configuration.azureUserConfigFile = $azureuserConfigFile
        $configuration.azureUserConfig = Get-Content -Raw $azureuserConfigFile |  ConvertFrom-Json
	
        if (!$configuration.azureUserConfig) {
            throw "Error trying to load azureuser-config.json!"
        }
    }
    # Sitecore Version
    $SCversion = $config.version

    # Determine deployment target
    $deploymentTarget = $configuration.cakeConfig.DeploymentTarget
    if ($deploymentTarget -eq "Local" -or $deploymentTarget -eq "OnPrem") {
        $deploymentTarget = "OnPrem"
    }
    else {
        $deploymentTarget = "Cloud"
    }
    # Specifcy Asset Folder Location
    $assetsfolder = $([io.path]::combine($configuration.cakeConfig.DeployFolder, $configuration.cakeConfig.version, $deploymentTarget, $topologyName , 'assets'))
    $buildFolder = $([io.path]::combine($configuration.cakeConfig.DeployFolder, $configuration.cakeConfig.version, $deploymentTarget, $topologyName,  'Website'))
    $configuration.assetsFolder = $assetsfolder
    $configuration.buildFolder = $buildFolder


    ###########################################
    # Find and process habitathome-parameters.json
    ###########################################
		
    [string] $habitatHomeParamsConfigFile = $([io.path]::combine($topologyPath, 'habitathome-parameters.json'))
    if (!(Test-Path $habitatHomeParamsConfigFile)) {
        Write-Host "habitathome-parameters file '$($habitatHomeParamsConfigFile)' not found." -ForegroundColor Red
        Write-Host  "Please ensure there is a habitathome-parameters.json configuration file at '$($habitatHomeParamsConfigFile)'" -ForegroundColor Red
        Exit 1
    }
    $habitatHomeParamsConfig = Get-Content -Raw $habitatHomeParamsConfigFile |  ConvertFrom-Json
    if (!$habitatHomeParamsConfig) {
        throw "Error trying to load habitathome-parameters.json!"
    }
    [string] $habitatHomecdParamsConfigFile
    # Check if topology is scaled
    if ($configuration.cakeConfig.Topology -eq "scaled") {
        $habitatHomecdParamsConfigFile = $([io.path]::combine($topologyPath, 'habitathomecd-parameters.json'))
        if (!(Test-Path $habitatHomecdParamsConfigFile)) {
            Write-Host "habitatHomecd-parameters file '$($habitatHomecdParamsConfigFile)' not found." -ForegroundColor Red
            Write-Host  "Please ensure there is a habitatHomecd-parameters.json configuration file at '$($habitatHomecdParamsConfigFile)'" -ForegroundColor Red
            Exit 1
        }
        $habitatHomecdParamsConfig = Get-Content -Raw $habitatHomecdParamsConfigFile |  ConvertFrom-Json
        if (!$habitatHomecdParamsConfig) {
            throw "Error trying to load habitatHomecd-parameters.json!"
        }
    }

    $configuration.habitatHomeParamsConfig = $habitatHomeParamsConfig
    $configuration.habitatHomeParamsConfigFile = $habitatHomeParamsConfigFile
    $configuration.habitatHomeCDParamsConfigFile = $habitatHomeCDParamsConfigFile
    $configuration.habitatHomeCDParamsConfig = $habitatHomeCDParamsConfig
		




    return $configuration
}