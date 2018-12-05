function ProcessConfigFile{
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
$configarray				= ProcessConfigFile -Config $ConfigurationFile
$config						= $configarray[0]
$assetConfig				= $configarray[1]
$azureuserconfig			= $configarray[2]
$assetconfigfile			= $configarray[3]
$azureuserConfigFile		= $configarray[4]
$topologypath				= $configarray[5]
$topologyName				= $configarray[6]
$assetsfolder				= $configarray[7]
$SCversion					= $configarray[8]
$buildFolder				= $configarray[9]
$habitatParamsConfig		= $configarray[10]
$habitatParamsConfigFile	= $configarray[11]
$habitatCdParamsConfig		= $configarray[12]
$habitatCdParamsConfigFile	= $configarray[13]
#>

[CmdletBinding()]
Param(
	[parameter(Mandatory=$true)]
	[ValidateNotNullOrEmpty()]
	[Alias ("Config")]
    [string] $ConfigurationFile
)
		####################################
		# Find and process cake-config.json
	    ####################################

		if (!(Test-Path $ConfigurationFile)) 
		{
			Write-Host "Configuration file '$($ConfigurationFile)' not found." -ForegroundColor Red
			Write-Host  "Please ensure there is a cake-config.json configuration file at '$($ConfigurationFile)'" -ForegroundColor Red
			Exit 1
		}

		$config = Get-Content -Raw $ConfigurationFile |  ConvertFrom-Json

		if (!$config) 
		{
			throw "Error trying to load configuration!"
		} 

		# Note the selected topology and assign the correct project path

		if ($config.Topology -eq "single")
		{
			[string] $topologyPath = $([io.path]::combine($config.ProjectFolder, 'Azure', 'XPSingle'))
			$topologyName =	'XPSingle'
		}
		elseif ($config.Topology -eq "scaled")
		{
			[string] $topologyPath = $([io.path]::combine($config.ProjectFolder, 'Azure', 'XP'))
			$topologyName =	'XP'
		}
		else 
		{
			Write-Host "Please select a topology - either 'single' or 'scaled'" -ForegroundColor DarkRed
		}

		###############################
		# Find and process assets.json
		###############################
		
		[string] $assetsConfigFile = $([io.path]::combine($topologyPath, 'assets.json'))

		if (!(Test-Path $assetsConfigFile)) 
		{
			Write-Host "Assets file '$($assetsConfigFile)' not found." -ForegroundColor Red
			Write-Host  "Please ensure there is a assets.json file at '$($assetsConfigFile)'" -ForegroundColor Red
			Exit 1
		}
		
		$assetConfig = Get-Content -Raw $assetsConfigFile |  ConvertFrom-Json

		if (!$assetConfig)
		{
			throw "Error trying to load Assest File!"
		} 

		#########################################
		# Find and process azureuser-config.json
		#########################################
		
		[string] $azureuserConfigFile = $([io.path]::combine($topologyPath, 'azureuser-config.json'))

		if (!(Test-Path $azureuserConfigFile)) 
		{
			Write-Host "azureuser-config file '$($azureuserConfigFile)' not found." -ForegroundColor Red
			Write-Host  "Please ensure there is an azureuser-config.json configuration file at '$($azureuserConfigFile)'" -ForegroundColor Red
			Exit 1
		}

		$azureuserconfig = Get-Content -Raw $azureuserConfigFile |  ConvertFrom-Json
		if (!$azureuserconfig) 
		{
			throw "Error trying to load azureuser-config.json!"
		}

		###########################################
		# Find and process habitat-parameters.json
		###########################################
		
		[string] $habitatParamsConfigFile = $([io.path]::combine($topologyPath, 'habitat-parameters.json'))

		if (!(Test-Path $habitatParamsConfigFile)) 
		{
			Write-Host "habitat-parameters file '$($habitatParamsConfigFile)' not found." -ForegroundColor Red
			Write-Host  "Please ensure there is a habitat-parameters.json configuration file at '$($habitatParamsConfigFile)'" -ForegroundColor Red
			Exit 1
		}

		$habitatParamsConfig = Get-Content -Raw $habitatParamsConfigFile |  ConvertFrom-Json
		if (!$habitatParamsConfig) 
		{
			throw "Error trying to load habitat-parameters.json!"
		}
		
		# Check if topology is scaled
		if ($config.topology -eq "scaled")
		{
			[string] $habitatCdParamsConfigFile = $([io.path]::combine($topologyPath, 'habitatcd-parameters.json'))

			if (!(Test-Path $habitatCdParamsConfigFile)) 
			{
				Write-Host "habitatcd-parameters file '$($habitatCdParamsConfigFile)' not found." -ForegroundColor Red
				Write-Host  "Please ensure there is a habitatcd-parameters.json configuration file at '$($habitatCdParamsConfigFile)'" -ForegroundColor Red
				Exit 1
			}

			$habitatCdParamsConfig = Get-Content -Raw $habitatCdParamsConfigFile |  ConvertFrom-Json
			if (!$habitatCdParamsConfig) 
			{
				throw "Error trying to load habitatcd-parameters.json!"
			}
		}

		# Sitecore Version
		$SCversion = $config.version

		# Specifcy Asset Folder Location
		$assetsfolder = $([io.path]::combine($config.DeployFolder, $SCversion, $topologyName, 'assets'))
		$buildFolder  = $([io.path]::combine($config.DeployFolder, $SCversion, $topologyName, 'Website'))

		return $config, $assetConfig, $azureuserConfig, $assetsConfigFile, $azureuserConfigFile, $topologyPath, $topologyName, $assetsfolder, $SCversion, $buildFolder, $habitatParamsConfig, $habitatParamsConfigFile, $habitatCdParamsConfig, $habitatCdParamsConfigFile
}