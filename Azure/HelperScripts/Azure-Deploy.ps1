<#
	This script will deploy Sitecore to Azure based on the setting in the following files:

	cake-config.json 
	azuresuer-config.json
	azuredeploy.parameters.json

#>

Param(
	[parameter(Mandatory=$true)]
	[ValidateNotNullOrEmpty()]
    [string] $ConfigurationFile
)

###########################
# Find configuration files
###########################

Import-Module "$($PSScriptRoot)\ProcessConfigFile\ProcessConfigFile.psm1" -Force

$configarray     	= ProcessConfigFile -Config $ConfigurationFile
$config          	= $configarray[0]
$assetconfig	 	= $configarray[1]
$azureuserconfig 	= $configarray[2]
$topologyPath		= $configarray[5]
$assetsFolder		= $configarray[7]

#####################
# Fill in Parameters
#####################

$deploymentId = ($azureuserconfig.settings | Where-Object {$_.id -eq "AzureDeploymentID"}).value
$armTemplateFolder = Join-Path $assetsFolder 'ArmTemplates'
$ArmParametersPath = ("{0}\azuredeploy.parameters-{1}.json" -f $armTemplateFolder, $deploymentId)

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
		"templatelinkAccessToken"
		{
			$templatelinkAccessToken = $setting.value
		}
		"containerName"
		{
			$containerName = $setting.value
		}
		"storageAccountName"
		{
			$storageAccountName = $setting.value
		}
	}
}

$authCertificateBlob = [System.Convert]::ToBase64String([System.IO.File]::ReadAllBytes($certfilepath));

$setKeyValue = @{
    authCertificateBlob = $authCertificateBlob;
    templatelinkAccessToken = $templatelinkAccessToken;
}

#Point to the sitecore cloud tools on your local filesystem
Import-Module "$($assetsFolder)\Sitecore Azure Toolkit\tools\Sitecore.Cloud.Cmdlets.psm1"

Start-SitecoreAzureDeployment -Location $Location `
							  -Name $Name `
							  -ArmTemplateUrl $ArmTemplateUrl `
							  -ArmParametersPath $ArmParametersPath `
							  -LicenseXmlPath $LicenseXmlPath `
							  -SetKeyValue $setKeyValue `
							  -Verbose