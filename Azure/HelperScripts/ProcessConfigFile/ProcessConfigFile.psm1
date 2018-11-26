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
$configarray			= ProcessConfigFile -Config $ConfigurationFile
$config					= $configarray[0]
$assetconfig			= $configarray[1]
$azureuserconfig		= $configarray[2]
$assetconfigfile		= $configarray[3]
$azureuserconfigfile	= $configarray[4]
$topologypath	        = $configarray[5]
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
		}
		elseif ($config.Topology -eq "scaled")
		{
			[string] $topologyPath = $([io.path]::combine($config.ProjectFolder, 'Azure', 'XP'))
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
		
		$assetconfig = Get-Content -Raw $assetsConfigFile |  ConvertFrom-Json

		if (!$assetconfig)
		{
			throw "Error trying to load Assest File!"
		} 

		#########################################
		# Find and process azureuser-config.json
		#########################################
		
		[string] $azureuserconfigFile = $([io.path]::combine($topologyPath, 'azureuser-config.json'))

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

		return $config, $assetconfig, $azureuserconfig, $assetsConfigFile, $azureuserconfigFile, $topologyPath
}