<#
	This script enables the upload of generated/downloaded WDP packages and accompanying Azure deployment files to an Azure storage account.
	It then captures the URLs for each Azure upload and populates the azuredeploy.parameters.json and azureuser-config.json files
#>

######################
# Mandatory parameters

Param(
    [parameter(Mandatory=$true)]
    [String] $configFile
)

# Find and process cake-config.json

if (!(Test-Path $configFile)) {

    Write-Host "Configuration file '$($configFile)' not found." -ForegroundColor Red
    Write-Host "Please ensure there is a cake-config.json configuration file at '$($configFile)'" -ForegroundColor Red
    Exit 1
    
}

$config = Get-Content -Raw $configFile |  ConvertFrom-Json
if (!$config) {

    throw "Error trying to load configuration!"
    
}

# Find and process assets.json
if($config.Topology -eq "single")
{
	[string] $AssetsFile = $([io.path]::combine($config.ProjectFolder, 'Azure Paas', 'Sitecore 9.0.2', 'XP0 Single', 'assets.json'))
}
else
{
	throw "Only XP0 Single Deployments are currently supported, please change the Topology parameter in the cake-config.json to single"
}

if (!(Test-Path $assetsFile)) {
    Write-Host "Assets file '$($assetsFile)' not found." -ForegroundColor Red
    Write-Host "Please ensure there is an assets.json file at '$($assetsFile)'" -ForegroundColor Red
    Exit 1
}

$assetsConfig = Get-Content -Raw $assetsFile |  ConvertFrom-Json
if (!$assetsConfig) {
    throw "Error trying to load Assest File!"
}

##########################
# Function for WDP uploads

Function UploadWDPs ([PSCustomObject] $cakeConfigFile, [PSCustomObject] $assetsConfigFile){

    # Upload Sitecore and xConnect main WDPs

    $sitecoreWDPFile = Get-Item -Path $([IO.Path]::Combine($config.DeployFolder, 'assets', 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_single.scwdp.zip'))
    $xconnectWDPFile = Get-Item -Path $([IO.Path]::Combine($config.DeployFolder, 'assets', 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_xp0xconnect.scwdp.zip'))
    try {
                        
        Get-AzureStorageBlob -Blob "wdps/$($sitecoreWDPFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($sitecoreWDPFile.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
        Set-AzureStorageBlobContent -File $sitecoreWDPFile.FullName -Blob "wdps/$($sitecoreWDPFile.Name)" -Container $containerName -Context $ctx -Force

    }

    try {
                        
        Get-AzureStorageBlob -Blob "wdps/$($xconnectWDPFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($xconnectWDPFile.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
        Set-AzureStorageBlobContent -File $xconnectWDPFile.FullName -Blob "wdps/$($xconnectWDPFile.Name)" -Container $containerName -Context $ctx -Force

    }

    # Upload Sitecore module WDPs

    $assetsFolder = (Join-Path $cakeConfigFile.DeployFolder assets)
    foreach ($asset in $assetsConfigFile.prerequisites){

        if(($asset.uploadToAzure -eq $True) -and ($asset.isWdp -eq $True)){

            if((Test-Path $(Join-Path $assetsFolder $asset.name)) -eq $True){
            
                $wdpSingleModuleFolder = ($(Join-Path $assetsFolder $asset.name))
                ForEach($blobFile in (Get-ChildItem -File -Recurse $wdpSingleModuleFolder)) { 
                        
                        try {
                        
                            Get-AzureStorageBlob -Blob "wdps/$($blobFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
                            Write-Host "Skipping... file $($blobFile.Name) already uploaded" -ForegroundColor Yellow
                        
                        } catch {
                        
                            Set-AzureStorageBlobContent -File $blobFile.FullName -Blob "wdps/$($blobFile.Name)" -Container $containerName -Context $ctx -Force
                        
                        }
                    
                }
                           
            }
        
        } elseif(($asset.uploadToAzure -eq $True) -and ($asset.isGroup -eq $True)){
        
            if((Test-Path $(Join-Path $assetsFolder $asset.name)) -eq $True){
                
                $wdpGroupModuleFolder = "$($(Join-Path $assetsFolder $asset.name))\convert to WDP\WDP"
                ForEach($blobFile in (Get-ChildItem -File -Recurse $wdpGroupModuleFolder)) { 
                        
                        try {
                        
                            Get-AzureStorageBlob -Blob "wdps/$($blobFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
                            Write-Host "Skipping... file $($blobFile.Name) already uploaded" -ForegroundColor Yellow
                        
                        } catch {
                        
                            Set-AzureStorageBlobContent -File $blobFile.FullName -Blob "wdps/$($blobFile.Name)" -Container $containerName -Context $ctx -Force
                        
                        }
                    
                }
                
            }
        
        }
    
    }

    # Upload Sitecore Bootloader WDP

    $bootloadWDPFile = Get-Item -Path $([IO.Path]::Combine($config.DeployFolder, 'assets', 'Sitecore Azure Toolkit', 'resources', '9.0.2', 'Addons', 'Sitecore.Cloud.Integration.Bootload.wdp.zip'))
    try {
                        
        Get-AzureStorageBlob -Blob "wdps/$($bootloadWDPFile.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($bootloadWDPFile.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
        Set-AzureStorageBlobContent -File $bootloadWDPFile.FullName -Blob "wdps/$($bootloadWDPFile.Name)" -Container $containerName -Context $ctx -Force

    }
    
}

##########################
# Function for file uploads

Function UploadFiles ([PSCustomObject] $cakeConfigFile){

    # Fetching all ARM templates' paths

    $azuredeployArmTemplate = Get-Item -Path $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'Sitecore 9.0.2', 'XP0 Single', 'azuredeploy.json'))
    $sxaArmTemplate = Get-Item -Path $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'Sitecore 9.0.2', 'ARM Templates', 'Modules', 'Sitecore Experience Accelerator', 'sxa_module.json'))
    $defArmTemplate = Get-Item -Path $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'Sitecore 9.0.2', 'ARM Templates', 'Modules', 'Data Exchange Framework', 'def_module.json'))
    $bootloadArmTemplate = Get-Item -Path $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'Sitecore 9.0.2', 'XP0 Single', 'addons', 'bootloader.json'))

    # Checking if the files are already uploaded and present in Azure and uploading

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($azuredeployArmTemplate.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($azuredeployArmTemplate.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
        Set-AzureStorageBlobContent -File $azuredeployArmTemplate.FullName -Blob "arm-templates/$($azuredeployArmTemplate.Name)" -Container $containerName -Context $ctx -Force
                        
    }

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($sxaArmTemplate.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($sxaArmTemplate.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
        Set-AzureStorageBlobContent -File $sxaArmTemplate.FullName -Blob "arm-templates/$($sxaArmTemplate.Name)" -Container $containerName -Context $ctx -Force
                        
    }

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($defArmTemplate.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($defArmTemplate.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
        Set-AzureStorageBlobContent -File $defArmTemplate.FullName -Blob "arm-templates/$($defArmTemplate.Name)" -Container $containerName -Context $ctx -Force
                        
    }

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($bootloadArmTemplate.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... file $($bootloadArmTemplate.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
        Set-AzureStorageBlobContent -File $bootloadArmTemplate.FullName -Blob "arm-templates/$($bootloadArmTemplate.Name)" -Container $containerName -Context $ctx -Force
                        
    }

}

###################################################
# Upload created WDPs and additional files in Azure

# Set variables for the container names
$containerName = "azure-toolkit"
$additionalContainerName = "temporary-toolkit"

# Check the Azure PowerShell Module's version
$AzureModule = Get-Module -ListAvailable AzureRM
if ($AzureModule -eq ""){

# If the Azure PowerShell module is not present, install the module
    Install-Module -Name AzureRM

}

# Import the module into the PowerShell session
Import-Module AzureRM

# Connect to Azure with an interactive dialog for sign-in
Connect-AzureRmAccount

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

# Try to write to the container - if failing, use a temporary one
try {

    "Verifying the existence of the current Azure container..."

	# Check if the container name already exists and if it does, upload the WDP modules and additional files to the container
    Get-AzureStorageContainer -Container $containerName -Context $ctx -ErrorAction Stop
    UploadWDPs -cakeConfigFile $config -assetsConfigFile $assetsConfig
    UploadFiles -cakeConfigFile $config

} catch {

    try {

        "Trying to create the container..."

        # Create the main container for the WDPs
        New-AzureStorageContainer -Name $containerName -Context $ctx -Permission blob -ErrorAction Stop

        # Upload the WDP modules to the blob container
        UploadWDPs -cakeConfigFile $config -assetsConfigFile $assetsConfig

        # Upload additional files to the blob container
        UploadFiles -cakeConfigFile $config

    } catch {
    
        "It seems like the container has been deleted very recently... creating a temporary container instead"

        # Create a temporary container
        New-AzureStorageContainer -Name $additionalContainerName -Context $ctx -Permission blob

        # Upload the WDP modules to the temporary blob container
        UploadWDPs -cakeConfigFile $config -assetsConfigFile $assetsConfig

        # Upload additional files to the temporary blob container
        UploadFiles -cakeConfigFile $config
    
    }
    
}

#########################################
# Get the URL for each WDP blob and record it

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

    } elseif($blob.Name -like "*bootloader.json"){
    
        $bootloaderTemplateLink = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri

    }

}


#######################################
# Construct azuredeploy.parameters.json

# Find and process azureuser-config.json

[String] $azureuserConfigFile = $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'Sitecore 9.0.2', 'XP0 Single', 'azureuser-config.json'))

if (!(Test-Path $azureuserConfigFile)) {
    Write-Host "Azure user config file '$($azureuserConfigFile)' not found." -ForegroundColor Red
    Write-Host "Please ensure there is an azureuser-config.json file at '$($azureuserConfigFile)'" -ForegroundColor Red
    Exit 1
}

$azureuserConfig = Get-Content -Raw $azureuserConfigFile |  ConvertFrom-Json
if (!$azureuserConfig) {
    throw "Error trying to load the Azure user config file!"
}

# Find and process the azuredeploy.parameters.json template

[String] $azuredeployConfigFile = $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'Sitecore 9.0.2', 'XP0 Single', 'azuredeploy.parameters.json'))

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

ForEach ($setting in $azureuserConfig.settings){

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
    $_.modules.value.items[1].templateLink = $defTemplateLink
    $_.modules.value.items[2].parameters.msDeployPackageUrl = $msDeployPackageUrl
    $_.modules.value.items[2].templateLink = $bootloaderTemplateLink

}

# Apply the azuredeploy.parameters JSON schema to the azuredeploy.parameters.json file

$azuredeployConfig | ConvertTo-Json -Depth 20 | Set-Content $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'Sitecore 9.0.2', 'XP0 Single', 'azuredeploy_TEST.parameters.json'))

# Populate the "azuredeploy.json" ARM template URL inside the azureuser-config JSON schema and apply the schema to the azureuser-config.json file

ForEach ($setting in $azureuserConfig.settings){

    # Check if an ARM template URL is already present inside the azureuser-config.json file

    if(($setting.id -eq "ArmTemplateUrl") -and ($setting.value -like "http*")){
    
        Continue

    } elseif(($setting.id -eq "ArmTemplateUrl") -and ($setting.value -eq "")){
    
        # Populate the value with the uploaded file's URL

        $setting.value = $azuredeployTemplateUrl
        $azureuserConfig | ConvertTo-Json -Depth 5 | Set-Content $([IO.Path]::Combine($config.ProjectFolder, 'Azure Paas', 'Sitecore 9.0.2', 'XP0 Single', 'azureuser-config_TEST.json'))

    }

}