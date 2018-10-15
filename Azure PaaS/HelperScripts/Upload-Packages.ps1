<#
.SYNOPSIS
Upload necessary files for Sitecore 9 and Habitat home Azure PaaS deployment

.DESCRIPTION
This script enables the upload of generated/downloaded WDP packages and accompanying Azure deployment files to an Azure storage account.
It then captures the URLs for each Azure upload and populates the azuredeploy.parameters.json and azureuser-config.json files

.PARAMETER ConfigurationFile
A cake-config.json file

#>

Param(
    [parameter(Mandatory=$true)]
	[ValidateNotNullOrEmpty()]
    [String] $ConfigurationFile
)

###########################
# Find configuration files
###########################

Import-Module "$($PSScriptRoot)\ProcessConfigFile\ProcessConfigFile.psm1" -Force

$configarray         = ProcessConfigFile -Config $ConfigurationFile
$config              = $configarray[0]
$assetconfig         = $configarray[1]
$azureuserconfig     = $configarray[2]
$assetconfigfile     = $configarray[3]
$azureuserconfigfile = $configarray[4]

##########################
# Function for WDP uploads
##########################

Function UploadWDPs ([PSCustomObject] $cakeJsonConfig, [PSCustomObject] $assetsJsonConfig){

    # Upload Sitecore and xConnect main WDPs

    $sitecoreWDPFile = Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.DeployFolder, 'assets', 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_single.scwdp.zip'))
    $xconnectWDPFile = Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.DeployFolder, 'assets', 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_xp0xconnect.scwdp.zip'))
    try {
                        
        Get-AzureStorageBlob -Blob "wdps/$($sitecoreWDPFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($sitecoreWDPFile.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {

		Write-Host "Starting file upload for $($sitecoreWDPFile.Name)" -ForegroundColor Green
        Set-AzureStorageBlobContent -File $sitecoreWDPFile.FullName -Blob "wdps/$($sitecoreWDPFile.Name)" -Container $containerName -Context $ctx -Force
		Write-Host "Upload of $($sitecoreWDPFile.Name) completed" -ForegroundColor Green

    }

    try {
                        
        Get-AzureStorageBlob -Blob "wdps/$($xconnectWDPFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($xconnectWDPFile.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                  
		Write-Host "Starting file upload for $($xconnectWDPFile.Name)" -ForegroundColor Green		
        Set-AzureStorageBlobContent -File $xconnectWDPFile.FullName -Blob "wdps/$($xconnectWDPFile.Name)" -Container $containerName -Context $ctx -Force
		Write-Host "Upload of $($xconnectWDPFile.Name) completed" -ForegroundColor Green

    }

    # Upload Sitecore module WDPs

    $assetsFolder = (Join-Path $cakeJsonConfig.DeployFolder assets)
    foreach ($asset in $assetsJsonConfig.prerequisites){

        if(($asset.uploadToAzure -eq $True) -and ($asset.isWdp -eq $True)){

            if((Test-Path $(Join-Path $assetsFolder $asset.name)) -eq $True){
            
                $wdpSingleModuleFolder = ($(Join-Path $assetsFolder $asset.name))
                ForEach($blobFile in (Get-ChildItem -File -Recurse $wdpSingleModuleFolder)) { 
                        
                        try {
                        
                            Get-AzureStorageBlob -Blob "wdps/$($blobFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
                            Write-Host "Skipping... file $($blobFile.Name) already uploaded" -ForegroundColor Yellow
                        
                        } catch {
                        
							Write-Host "Starting file upload for $($blobFile.Name)" -ForegroundColor Green
                            Set-AzureStorageBlobContent -File $blobFile.FullName -Blob "wdps/$($blobFile.Name)" -Container $containerName -Context $ctx -Force
							Write-Host "Upload of $($blobFile.Name) completed" -ForegroundColor Green
                        
                        }
                    
                }
                           
            }
        
        } elseif(($asset.uploadToAzure -eq $True) -and ($asset.isGroup -eq $True)){
        
            if((Test-Path $(Join-Path $assetsFolder $asset.name)) -eq $True){
                
                $wdpGroupModuleFolder = "$($(Join-Path $assetsFolder $asset.name))\WDPWorkFolder\WDP"
                ForEach($blobFile in (Get-ChildItem -File -Recurse $wdpGroupModuleFolder)) { 
                        
                        try {
                        
                            Get-AzureStorageBlob -Blob "wdps/$($blobFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
                            Write-Host "Skipping... file $($blobFile.Name) already uploaded" -ForegroundColor Yellow
                        
                        } catch {
                        
							Write-Host "Starting file upload for $($blobFile.Name)" -ForegroundColor Green
                            Set-AzureStorageBlobContent -File $blobFile.FullName -Blob "wdps/$($blobFile.Name)" -Container $containerName -Context $ctx -Force
							Write-Host "Upload of $($blobFile.Name) completed" -ForegroundColor Green
                        
                        }
                    
                }
                
            }
        
        }
    
    }

    # Upload Sitecore Bootloader WDP

    $bootloadWDPFile = Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.DeployFolder, 'assets', 'Sitecore Azure Toolkit', 'resources', '9.0.2', 'Addons', 'Sitecore.Cloud.Integration.Bootload.wdp.zip'))
    try {
                        
        Get-AzureStorageBlob -Blob "wdps/$($bootloadWDPFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($bootloadWDPFile.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                   
		Write-Host "Starting file upload for $($bootloadWDPFile.Name)" -ForegroundColor Green
        Set-AzureStorageBlobContent -File $bootloadWDPFile.FullName -Blob "wdps/$($bootloadWDPFile.Name)" -Container $containerName -Context $ctx -Force
		Write-Host "Upload of $($bootloadWDPFile.Name) completed" -ForegroundColor Green

    }

	# Upload Habitat WDPs

	$habitatWebsiteWDPPath = [IO.Path]::Combine($assetsFolder, 'habitathome', 'WDPWorkFolder', 'WDP', 'habitathome_single.scwdp.zip')
    if((Test-Path $habitatWebsiteWDPPath) -eq $True){
	
		$habitatWebsiteWDPFile = Get-Item -Path $habitatWebsiteWDPPath
		try {
                        
			Get-AzureStorageBlob -Blob "wdps/$($habitatWebsiteWDPFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
			Write-Host "Skipping... file $($habitatWebsiteWDPFile.Name) already uploaded" -ForegroundColor Yellow
                        
		} catch {
                 
			Write-Host "Starting file upload for $($habitatWebsiteWDPFile.Name)" -ForegroundColor Green
			Set-AzureStorageBlobContent -File $habitatWebsiteWDPFile.FullName -Blob "wdps/$($habitatWebsiteWDPFile.Name)" -Container $containerName -Context $ctx -Force
			Write-Host "Upload of $($habitatWebsiteWDPFile.Name) completed" -ForegroundColor Green

		}

	}
	
	$habitatXconnectWDPPath = [IO.Path]::Combine($assetsFolder, 'xconnect', 'WDPWorkFolder', 'WDP', 'xconnect_single.scwdp.zip')
	if((Test-Path $habitatXconnectWDPPath) -eq $True){
	
		$habitatXconnectWDPFile = Get-Item -Path $habitatXconnectWDPPath
		try {
                        
			Get-AzureStorageBlob -Blob "wdps/$($habitatXconnectWDPFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
			Write-Host "Skipping... file $($habitatXconnectWDPFile.Name) already uploaded" -ForegroundColor Yellow
                        
		} catch {
                
			Write-Host "Starting file upload for $($habitatXconnectWDPFile.Name)" -ForegroundColor Green
			Set-AzureStorageBlobContent -File $habitatXconnectWDPFile.FullName -Blob "wdps/$($habitatXconnectWDPFile.Name)" -Container $containerName -Context $ctx -Force
			Write-Host "Upload of $($habitatXconnectWDPFile.Name) completed" -ForegroundColor Green

		}	
	
	}
    
}

###########################
# Function for file uploads
###########################

Function UploadFiles ([PSCustomObject] $cakeJsonConfig){

    # Fetching all ARM templates' paths

    $azuredeployArmTemplate = Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.ProjectFolder, 'Azure Paas', 'XP0 Single', 'azuredeploy.json'))
    $sxaArmTemplate = Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.ProjectFolder, 'Azure Paas', 'ARM Templates', 'Modules', 'Sitecore Experience Accelerator', 'sxa_module.json'))
    $defArmTemplate = Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.ProjectFolder, 'Azure Paas', 'ARM Templates', 'Modules', 'Data Exchange Framework', 'def_module.json'))
    $habitatWebsiteArmTemplate = Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.ProjectFolder, 'Azure Paas', 'ARM Templates', 'Habitat', 'habitathome.json'))
    $habitatXconnectArmTemplate = Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.ProjectFolder, 'Azure Paas', 'ARM Templates', 'Habitat', 'xconnect.json'))
    $bootloadArmTemplate = Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.ProjectFolder, 'Azure Paas', 'XP0 Single', 'addons', 'bootloader.json'))
	$nestedArmTemplates =  Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.ProjectFolder, 'Azure Paas', 'XP0 Single', 'nested'))

    # Checking if the files are already uploaded and present in Azure and uploading

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($azuredeployArmTemplate.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($azuredeployArmTemplate.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
		Write-Host "Starting file upload for $($azuredeployArmTemplate.Name)" -ForegroundColor Green
        Set-AzureStorageBlobContent -File $azuredeployArmTemplate.FullName -Blob "arm-templates/$($azuredeployArmTemplate.Name)" -Container $containerName -Context $ctx -Force
		Write-Host "Upload of $($azuredeployArmTemplate.Name) completed" -ForegroundColor Green
                        
    }

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($sxaArmTemplate.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($sxaArmTemplate.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                   
		Write-Host "Starting file upload for $($sxaArmTemplate.Name)" -ForegroundColor Green
        Set-AzureStorageBlobContent -File $sxaArmTemplate.FullName -Blob "arm-templates/$($sxaArmTemplate.Name)" -Container $containerName -Context $ctx -Force
		Write-Host "Upload of $($sxaArmTemplate.Name) completed" -ForegroundColor Green
                        
    }

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($defArmTemplate.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($defArmTemplate.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
		Write-Host "Starting file upload for $($defArmTemplate.Name)" -ForegroundColor Green
        Set-AzureStorageBlobContent -File $defArmTemplate.FullName -Blob "arm-templates/$($defArmTemplate.Name)" -Container $containerName -Context $ctx -Force
		Write-Host "Upload of $($defArmTemplate.Name) completed" -ForegroundColor Green
                        
    }

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($habitatWebsiteArmTemplate.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($habitatWebsiteArmTemplate.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
		Write-Host "Starting file upload for $($habitatWebsiteArmTemplate.Name)" -ForegroundColor Green
        Set-AzureStorageBlobContent -File $habitatWebsiteArmTemplate.FullName -Blob "arm-templates/$($habitatWebsiteArmTemplate.Name)" -Container $containerName -Context $ctx -Force
		Write-Host "Upload of $($habitatWebsiteArmTemplate.Name) completed" -ForegroundColor Green
                        
    }

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($habitatXconnectArmTemplate.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($habitatXconnectArmTemplate.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                    
		Write-Host "Starting file upload for $($habitatXconnectArmTemplate.Name)" -ForegroundColor Green
        Set-AzureStorageBlobContent -File $habitatXconnectArmTemplate.FullName -Blob "arm-templates/$($habitatXconnectArmTemplate.Name)" -Container $containerName -Context $ctx -Force
		Write-Host "Upload of $($habitatXconnectArmTemplate.Name) completed" -ForegroundColor Green
                        
    }

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($bootloadArmTemplate.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($bootloadArmTemplate.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
		Write-Host "Starting file upload for $($bootloadArmTemplate.Name)" -ForegroundColor Green
        Set-AzureStorageBlobContent -File $bootloadArmTemplate.FullName -Blob "arm-templates/$($bootloadArmTemplate.Name)" -Container $containerName -Context $ctx -Force
		Write-Host "Upload of $($bootloadArmTemplate.Name) completed" -ForegroundColor Green
                        
    }

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($nestedArmTemplates.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... folder $($nestedArmTemplates.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
		Get-ChildItem -File -Recurse -Path $nestedArmTemplates.FullName | ForEach { 

			Write-Host "Starting file upload for $($_.Name)" -ForegroundColor Green
			Set-AzureStorageBlobContent -File $_.FullName -Blob "arm-templates/$($nestedArmTemplates.Name)/$($_.Name)" -Container $containerName -Context $ctx -Force
			Write-Host "Upload of $($_.Name) completed" -ForegroundColor Green
		
		}
                        
    }

}

###################################################
# Upload created WDPs and additional files in Azure
###################################################

# Set variables for the container names
$originalContainerName = "azure-toolkit"
$additionalContainerName = "temporary-toolkit"

# Get the current storage account
$sa = Get-AzureRmStorageAccount

# Obtain the storage account context
$ctx = $sa.Context

try {

    # Remove the temporary container if it exists

    "Trying to remove any temporary containers, created on previous runs of the script..."
    Remove-AzureStorageContainer -Name "$additionalContainerName" -Context $ctx -Force -ErrorAction Stop
    
}
catch {
    
    "...no temporary container found"

}

$containerName = $originalContainerName

# Try to write to the container - if failing, use a temporary one
try {

    "Verifying the existence of the current Azure container..."
	   
	# Check if the container name already exists and if it does, upload the WDP modules and additional files to the container
    Get-AzureStorageContainer -Container $containerName -Context $ctx -ErrorAction Stop

} catch {

    try {

        "Trying to create the container..."

        # Create the main container for the WDPs
        New-AzureStorageContainer -Name $containerName -Context $ctx -Permission blob -ErrorAction Stop

    } catch {
    
        "It seems like the container has been deleted very recently... creating a temporary container instead"

		$containerName = $additionalContainerName
        # Create a temporary container
        New-AzureStorageContainer -Name $containerName -Context $ctx -Permission blob

    }
    
}

UploadWDPs -cakeJsonConfig $config -assetsJsonConfig $assetconfig
UploadFiles -cakeJsonConfig $config

##############################################
# Get the URL for each WDP blob and record it
##############################################

$blobsList = Get-AzureStorageBlob -Container $containerName -Context $ctx

ForEach($blob in $blobsList){

    Switch($blob.Name){
    
        "wdps/Sitecore Experience Accelerator 1.7.1 rev. 180604 for 9.0.scwdp.zip"
        {
        
            $sxaMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/Sitecore PowerShell Extensions-4.7.2 for Sitecore 8.scwdp.zip"
        {
        
            $speMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/Data Exchange Framework 2.0.1 rev. 180108_single.scwdp.zip"
        {
        
            $defDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/Sitecore Provider for Data Exchange Framework 2.0.1 rev. 180108_single.scwdp.zip"
        {
        
            $defSitecoreDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/SQL Provider for Data Exchange Framework 2.0.1 rev. 180108_single.scwdp.zip"
        {
        
            $defSqlDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/xConnect Provider for Data Exchange Framework 2.0.1 rev. 180108_single.scwdp.zip"
        {
        
            $defxConnectDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/Dynamics Provider for Data Exchange Framework 2.0.1 rev. 180108_single.scwdp.zip"
        {
        
            $defDynamicsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/Connect for Microsoft Dynamics 2.0.1 rev. 180108_single.scwdp.zip"
        {
        
            $defDynamicsConnectDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
		"wdps/Salesforce Provider for Data Exchange Framework 2.0.1 rev. 180108_single.scwdp.zip"
        {
        
            $defSalesforceDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/Connect for Salesforce 2.0.1 rev. 180108_single.scwdp.zip"
        {
        
            $defSalesforceConnectDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/Sitecore.Cloud.Integration.Bootload.wdp.zip"
        {
        
            $msDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/Sitecore 9.0.2 rev. 180604 (Cloud)_single.scwdp.zip"
        {
        
            $singleMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/Sitecore 9.0.2 rev. 180604 (Cloud)_xp0xconnect.scwdp.zip"
        {
        
            $xcSingleMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/habitathome_single.scwdp.zip"
        {
        
            $habitatWebsiteDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
        "wdps/xconnect_single.scwdp.zip"
        {
        
            $habitatXconnectDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        
        }
    
    }
    
}

############################################################
# Get the URL of each required additional file and record it

ForEach ($blob in $blobsList){

    if($blob.Name -like "*azuredeploy.json"){
    
        $azuredeployTemplateUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
    
    } elseif($blob.Name -like "*sxa*.json"){
    
        $sxaTemplateLink = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri

    } elseif($blob.Name -like "*def*.json"){
    
        $defTemplateLink = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri

    } elseif($blob.Name -like "*habitathome.json"){
    
        $habitatWebsiteTemplateLink = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri

    } elseif($blob.Name -like "*xconnect.json"){
    
        $habitatXconnectTemplateLink = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri

    } elseif($blob.Name -like "*bootloader.json"){
    
        $bootloaderTemplateLink = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri

    }

}


#######################################
# Construct azuredeploy.parameters.json

# Find and process the azuredeploy.parameters.json template

[String] $azuredeployConfigFile = $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'XP0 Single', 'azuredeploy.parameters.json'))

if (!(Test-Path $azuredeployConfigFile)) {
    Write-Host "Azuredeploy parameters file '$($azuredeployConfigFile)' not found." -ForegroundColor Red
    Write-Host "Please ensure there is an azuredeploy.parameters.json file at '$($azuredeployConfigFile)'" -ForegroundColor Red
    Exit 1
}

$azuredeployConfig = Get-Content -Raw $azuredeployConfigFile |  ConvertFrom-Json
if (!$azuredeployConfig) {
    throw "Error trying to load the Azuredeploy parameters file!"
}

# Get all user-defined settings from the azureuser-config.json files and assign them to variables

ForEach ($setting in $azureuserconfig.settings){

    Switch($setting.id){
    
        "AzureDeploymentID"
        {
        
            $deploymentId = $setting.value

        }
        "AzureRegion"
        {
        
            $location = $setting.value
        
        }
        "XConnectCertFilePath"
        {
        
            $authCertificateFilePath = $setting.value
        
        }
        "XConnectCertificatePassword"
        {
        
            $authCertificatePassword = $setting.value

        }
        "SitecoreLoginAdminPassword"
        {
        
            $sitecoreAdminPassword = $setting.value
        
        }
        "SitecoreLicenseXMLPath"
        {
        
            $licenseXml = $setting.value
        
        }
        "SqlServerLoginAdminAccount"
        {
        
            $sqlServerLogin = $setting.value
        
        }
        "SqlServerLoginAdminPassword"
        {
        
            $sqlServerPassword = $setting.value
        
        }
        "ArmTemplateUrl"
        {
        
            $armTemplateURL = $setting.value
        
        }
    
    }

}

# Populate parameters inside the azuredeploy.parameters JSON schema with values from previously prepared variables

$azuredeployConfig.parameters | ForEach-Object {

    $_.deploymentId.value = $deploymentId
    $_.location.value = $location
    $_.sitecoreAdminPassword.value = $sitecoreAdminPassword
    $_.licenseXml.value = $licenseXml
    $_.sqlServerLogin.value = $sqlServerLogin
    $_.sqlServerPassword.value = $sqlServerPassword
    $_.authCertificatePassword.value = $authCertificatePassword
    $_.singleMsDeployPackageUrl.value = $singleMsDeployPackageUrl
    $_.xcSingleMsDeployPackageUrl.value = $xcSingleMsDeployPackageUrl
    $_.modules.value.items[0].parameters.sxaMsDeployPackageUrl = $sxaMsDeployPackageUrl
    $_.modules.value.items[0].parameters.speMsDeployPackageUrl = $speMsDeployPackageUrl
    $_.modules.value.items[0].templateLink = $sxaTemplateLink
    $_.modules.value.items[1].parameters.defDeployPackageUrl = $defDeployPackageUrl
    $_.modules.value.items[1].parameters.defSitecoreDeployPackageUrl = $defSitecoreDeployPackageUrl
    $_.modules.value.items[1].parameters.defSqlDeployPackageUrl = $defSqlDeployPackageUrl
    $_.modules.value.items[1].parameters.defxConnectDeployPackageUrl = $defxConnectDeployPackageUrl
    $_.modules.value.items[1].parameters.defDynamicsDeployPackageUrl = $defDynamicsDeployPackageUrl
    $_.modules.value.items[1].parameters.defDynamicsConnectDeployPackageUrl = $defDynamicsConnectDeployPackageUrl
    $_.modules.value.items[1].parameters.defSalesforceDeployPackageUrl = $defSalesforceDeployPackageUrl
    $_.modules.value.items[1].parameters.defSalesforceConnectDeployPackageUrl = $defSalesforceConnectDeployPackageUrl
    $_.modules.value.items[1].templateLink = $defTemplateLink
    $_.modules.value.items[2].parameters.habitatWebsiteDeployPackageUrl = $habitatWebsiteDeployPackageUrl
    $_.modules.value.items[2].templateLink = $habitatWebsiteTemplateLink
	$_.modules.value.items[3].parameters.habitatXconnectDeployPackageUrl = $habitatXconnectDeployPackageUrl
    $_.modules.value.items[3].templateLink = $habitatXconnectTemplateLink
    $_.modules.value.items[4].parameters.msDeployPackageUrl = $msDeployPackageUrl
    $_.modules.value.items[4].templateLink = $bootloaderTemplateLink

}

# Apply the azuredeploy.parameters JSON schema to the azuredeploy.parameters.json file

$azuredeployConfig | ConvertTo-Json -Depth 20 | Set-Content $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'XP0 Single', 'azuredeploy.parameters.json'))

# Populate the "azuredeploy.json" ARM template URL inside the azureuser-config JSON schema and apply the schema to the azureuser-config.json file

ForEach ($setting in $azureuserconfig.settings){

    # Check if an ARM template URL is already present inside the azureuser-config.json file

    if(($setting.id -eq "ArmTemplateUrl") -and ($setting.value -like "http*")){
    
        Continue

    } elseif(($setting.id -eq "ArmTemplateUrl") -and ($setting.value -eq "")){
    
        # Populate the value with the uploaded file's URL

        $setting.value = $azuredeployTemplateUrl
        $azureuserconfig | ConvertTo-Json -Depth 5 | Set-Content $azureuserconfigfile

    }

}