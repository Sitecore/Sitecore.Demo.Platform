function ProcessConfigFile{
<#
.SYNOPSIS
Processes json configuration files

.DESCRIPTION
Converts cake-config.json, assets.json, and azureuser-config.json configs to powershell objects.
The script then returns these 3 objects as an array in the above order.

.PARAMETER ConfigurationFile
A cake-config.json file

.Example
$configarray     = ProcessConfigFile -Config $ConfigurationFile
$config          = $configarray[0]
$assetconfig     = $configarray[1]
$azureuserconfig = $configarray[2]

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

		###############################
		# Find and process assets.json
		###############################
		
		if($config.Topology -eq "single")
		{
			[string] $AssetsFile = $([io.path]::combine($config.ProjectFolder, 'Azure Paas', 'XP0 Single', 'assets.json'))
		}
		else
		{
			throw "Only XP0 Single Deployments are currently supported, please change the Topology parameter in the cake-config.json to single"
		}

		if (!(Test-Path $AssetsFile)) 
		{
			Write-Host "Assets file '$($AssetsFile)' not found." -ForegroundColor Red
			Write-Host  "Please ensure there is a assets.json file at '$($AssetsFile)'" -ForegroundColor Red
			Exit 1
		}
		
		$assetconfig = Get-Content -Raw $AssetsFile |  ConvertFrom-Json

		if (!$assetconfig)
		{
			throw "Error trying to load Assest File!"
		} 

		###############################
		# Find and process assets.json
		###############################
		
		if($config.Topology -eq "single")
		{
			[string] $azureuserconfigFile = $([io.path]::combine($config.ProjectFolder, 'Azure Paas', 'XP0 Single', 'azureuser-config.json'))
		}
		else
		{
			throw "Only XP0 Single Deployments are currently supported, please change the Topology parameter in the cake-config.json to single"
		}

		if (!(Test-Path $azureuserconfigFile)) 
		{
			Write-Host "azureuser-config file '$($azureuserconfigFile)' not found." -ForegroundColor Red
			Write-Host  "Please ensure there is a user-config.json configuration file at '$($azureuserconfigFile)'" -ForegroundColor Red
			Exit 1
		}

		$azureuserconfig = Get-Content -Raw $azureuserconfigFile |  ConvertFrom-Json
		if (!$azureuserconfig) 
		{
			throw "Error trying to load azureuser-config.json!"
		}

		return $config, $assetconfig, $azureuserconfig
}