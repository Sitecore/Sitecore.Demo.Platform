<#
	This script will deploy Sitecore to Azure PaaS based on the setting in the following files:

	cake-config.json 
	azuresuer-config.json
	azuredeploy.parameters.json

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

#Point to the sitecore cloud tools on your local filesystem
Import-Module "$($config.DeployFolder)\assets\Sitecore Azure Toolkit\tools\Sitecore.Cloud.Cmdlets.psm1"

#####################
# Fill in Parameters
#####################

$ArmParametersPath = "$($config.ProjectFolder)\Azure PaaS\Sitecore 9.0.2\XP0 Single\azuredeploy.parameters.json"


foreach($setting in $azureuserconfig.settings)
{
	switch($setting.id)
	{
		"AzureDeploymentID"
		{
			$Name = $setting.value
		}
		"AzureRegion"
		{
			$Location = $setting.value
		}
		"XConnectCertfilePath"
		{
			$certfilepath = $setting.value
		}
		"SitecoreLicenseXMLPath"
		{
			$LicenseXmlPath = $setting.value
		}
		"ArmTemplateUrl"
		{
			$ArmTemplateUrl = $setting.value
		}
	}
}


Start-SitecoreAzureDeployment -Location $Location `
							  -Name $Name `
							  -ArmTemplateUrl $ArmTemplateUrl `
							  -ArmParametersPath $ArmParametersPath `
							  -LicenseXmlPath $LicenseXmlPath `
							  -SetKeyValue @{"authCertificateBlob" = [System.Convert]::ToBase64String([System.IO.File]::ReadAllBytes($certfilepath))} `
							  -Verbose