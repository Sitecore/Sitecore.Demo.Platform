<#
	This script will prompt the user to enter their azure credentials; it then enables a persisent azure session.
	This will allows future scripts to use this azure session without asking for crednetials again
	This script will edit a azureuser-config.json bassed on user respones to prompts.
	The azureuser-config.json is intedned to be used by other scritps to help access the Azure env
#>

Param(
	[parameter(Mandatory=$true)]
    [string] $ConfigurationFile
)

###########################
# Find configuration files
###########################

# Find and process cake-config.json
if (!(Test-Path $ConfigurationFile)) {
    Write-Host "Configuration file '$($ConfigurationFile)' not found." -ForegroundColor Red
    Write-Host  "Please ensure there is a cake-config.json configuration file at '$($ConfigurationFile)'" -ForegroundColor Red
    Exit 1
}

$config = Get-Content -Raw $ConfigurationFile |  ConvertFrom-Json
if (!$config) {
    throw "Error trying to load configuration!"
}

# Find and process azureuser-config.json
if($config.Topology -eq "single")
{
	[string] $azureuserconfigFile = $([io.path]::combine($config.ProjectFolder, 'Azure Paas', 'Sitecore 9.0.2', 'XP0 Single', 'azureuser-config.json'))
}
else
{
	throw "Only XP0 Single Deployments are currently supported, please change the Topology parameter in the cake-config.json to single"
}

if (!(Test-Path $azureuserconfigFile)) {
    Write-Host "azureuser-config file '$($azureuserconfigFile)' not found." -ForegroundColor Red
    Write-Host  "Please ensure there is a user-config.json configuration file at '$($azureuserconfigFile)'" -ForegroundColor Red
    Exit 1
}

$azureuserconfig = Get-Content -Raw $azureuserconfigFile |  ConvertFrom-Json
if (!$azureuserconfig) {
    throw "Error trying to load azureuser-config.json!"
}

########################
# Get Azure Credentials
########################

Write-Host "Importing and Installing AzureRm Module"

$AzureModule = Get-Module -ListAvailable AzureRM
if ($AzureModule -eq ""){
    Install-Module -Name AzureRM
}

Import-Module AzureRM

# Add Persisent Azure Session
Enable-AzureRmContextAutosave

Add-AzureRmAccount

###########################################
# Get User Input for azureuser-config.json
###########################################

foreach ($setting in $azureuserconfig.settings)
{
	switch ($setting.id)
	{
		"SitecoreLoginAdminPassword"
		{
			$setting.value = Read-Host "Please Provide the $($setting.id) (8 Character Minimum)"
		}
		"SqlServerLoginAdminAccount"
		{
			$setting.value = Read-Host "Please Provide the $($setting.id) (SA is not a valid admin name for Azure SQL)"
		}
		"ArmTemplateUrl"
		{
			continue
		}
		default
		{
			$setting.value = Read-Host "Please Provide the $($setting.id)"
		}
	}
}

$azureuserconfig | ConvertTo-Json  | set-content $azureuserconfigFile