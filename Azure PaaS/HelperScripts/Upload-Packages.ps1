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
$azureuserconfigfile = $configarray[4]
$topology		     = $configarray[5]

###########################
# Initialize Params
###########################

# Set variables for the container names
[String] $originalContainerName = "azure-toolkit"
[String] $additionalContainerName = "temporary-toolkit"

# Get the current storage account
$sa = Get-AzureRmStorageAccount -Name $storageAccountName -ResourceGroupName $resourceGroupName

# Obtain the storage account context
$ctx = $sa.Context

$containerName = $originalContainerName

##########################
# Function for WDP uploads
##########################

Function UploadWDPs ([PSCustomObject] $cakeJsonConfig, [PSCustomObject] $assetsJsonConfig){

    $assetsFolder = (Join-Path $cakeJsonConfig.DeployFolder assets)
    $sitecoreWDPpathArray = New-Object System.Collections.ArrayList

    # Add Sitecore and habitat WDPs to upload list
    if($cakeJsonConfig.Topology -eq "single")
    {
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_single.scwdp.zip'))))
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_xp0xconnect.scwdp.zip'))))
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'habitathome', 'WDPWorkFolder', 'WDP', 'habitathome_single.scwdp.zip'))))
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'xconnect', 'WDPWorkFolder', 'WDP', 'xconnect_single.scwdp.zip'))))
    }
    elseif($cakeJsonConfig.Topology -eq "scaled")
    {
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_cm.scwdp.zip'))))
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_cd.scwdp.zip'))))
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_prc.scwdp.zip'))))
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_rep.scwdp.zip'))))
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_xp1referencedata.scwdp.zip'))))
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_xp1collection.scwdp.zip'))))
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_xp1collectionsearch.scwdp.zip'))))
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_xp1marketingautomation.scwdp.zip'))))
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_xp1marketingautomationreporting.scwdp.zip'))))  
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'habitathome', 'WDPWorkFolder', 'WDP', 'habitathome_cd.scwdp.zip'))))  
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'habitathome', 'WDPWorkFolder', 'WDP', 'habitathome_single.scwdp.zip'))))  
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'xconnect', 'WDPWorkFolder', 'WDP', 'xconnect_single.scwdp.zip'))))  
    }

        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.DeployFolder, 'assets', 'Sitecore Azure Toolkit', 'resources', '9.0.2', 'Addons', 'Sitecore.Cloud.Integration.Bootload.wdp.zip'))))

    # Add Module WDPs to upload list
    foreach ($asset in $assetsJsonConfig.prerequisites)
    {
        if(($asset.uploadToAzure -eq $True) -and ($asset.isWdp -eq $True))
        {
            if((Test-Path $(Join-Path $assetsFolder $asset.name)) -eq $True)
            {
                $wdpSingleModuleFolder = ($(Join-Path $assetsFolder $asset.name))
                ForEach($blobFile in (Get-ChildItem -File -Recurse $wdpSingleModuleFolder)) 
                {    
                    $sitecoreWDPpathArray.Add($(Get-Item -Path $blobFile.FullName))
                }            
            }
        } 
        elseif(($asset.uploadToAzure -eq $True) -and ($asset.isGroup -eq $True))
        {
        
            if((Test-Path $(Join-Path $assetsFolder $asset.name)) -eq $True)
            { 
                $wdpGroupModuleFolder = "$($(Join-Path $assetsFolder $asset.name))\WDPWorkFolder\WDP"
                ForEach($blobFile in (Get-ChildItem -File -Recurse $wdpGroupModuleFolder)) 
                {           
					$sitecoreWDPpathArray.Add($(Get-Item -Path $blobFile.FullName))
                }
            }
        }
    }

    # Perform Upload
    foreach ($_ in $sitecoreWDPpathArray)
    {
        try 
        {             
            Get-AzureStorageBlob -Blob "wdps/$($_.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
            Write-Host "Skipping... file $($_.Name) already uploaded" -ForegroundColor Yellow              
        } 
        catch 
        {
            Write-Host "Starting file upload for $($_.Name)" -ForegroundColor Green
            Set-AzureStorageBlobContent -File $_.FullName -Blob "wdps/$($_.Name)" -Container $containerName -Context $ctx -Force
            Write-Host "Upload of $($_.Name) completed" -ForegroundColor Green
        }
    }
    
}

###########################
# Function for file uploads
###########################

Function UploadFiles ([PSCustomObject] $cakeJsonConfig){

    $sitecoreARMpathArray = New-Object System.Collections.ArrayList
    
    # Fetching all ARM templates' paths
    $sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'azuredeploy.json'))))
    $sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'ARM Templates', 'Habitat', 'habitathome.json'))))
    $sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'ARM Templates', 'Habitat', 'xconnect.json'))))
    $sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'addons', 'bootloader.json'))))
    $sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'ARM Templates','Sitecore Experience Accelerator', 'sxa_module.json'))))
    $sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'ARM Templates','Data Exchange Framework', 'def_module.json'))))

    # Checking if the files are already uploaded and present in Azure and uploading

    foreach($_ in $sitecoreARMpathArray)
    {
        try {
                        
            Get-AzureStorageBlob -Blob "arm-templates/$($_.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
            Write-Host "Skipping... file $($_.Name) already uploaded" -ForegroundColor Yellow
                            
        } catch {
                            
            Write-Host "Starting file upload for $($_.Name)" -ForegroundColor Green
            Set-AzureStorageBlobContent -File $_.FullName -Blob "arm-templates/$($_.Name)" -Container $containerName -Context $ctx -Force
            Write-Host "Upload of $($_.Name) completed" -ForegroundColor Green
                            
        }
    }

    $nestedArmTemplate = Get-Item -Path $([IO.Path]::Combine($topology, 'nested'))

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

ForEach ($setting in $azureuserconfig.settings) {

    if($setting.id -eq "AzureDeploymentID") {

        # Assign a name to the resource group based on the Deployment ID
        [String] $resourceGroupName = $setting.value

        # Generate a random name for the storage account by taking into account the 24 character limits imposed by Azure
        $seed = (24 - $resourceGroupName.Length)
        if ($seed -gt 1) {

            $resourceGroupNameSeed = $resourceGroupName -replace '-',''
            [String] $storageAccountName = "$($resourceGroupNameSeed)$([System.Math]::Round($(Get-Random -Minimum ([System.Math]::Pow(10 , $seed - 1)) -Maximum ([System.Math]::Pow(10 , $seed) - 1))))"

        }

    }

    if($setting.id -eq "AzureRegion") {

		$region = $setting.value

		# Trying to create a resource group for deployments to Azure, based on the selected region

		try {
        
            # Check if the resource group is already there
            Get-AzureRmResourceGroup -Name $resourceGroupName -ErrorAction Stop
			if($null -ne (Get-AzureRmResourceGroup -Name $resourceGroupName)) {
							   
				Write-Host "A resource group named $($resourceGroupName) already exists" -ForegroundColor Yellow

			}
		
		} catch {
					
            # Create the resource group if the attempt to get the group fails
            Write-Host "Creating a new resource group named $($resourceGroupName)..." -ForegroundColor Green
            New-AzureRmResourceGroup -Name $resourceGroupName -Location $region
				
		}

		try {

            $storageAccountsList = Get-AzureRmStorageAccount -ResourceGroupName $resourceGroupName
            if ($null -ne $storageAccountsList) {

                foreach ($storageAccount in $storageAccountsList) {

                    # Check if a previously generated storage account already exists
                    if ($storageAccount.StorageAccountName -like "*$($resourceGroupNameSeed)*") {
    
                        Write-Host "A generated storage account named $($storageAccountName) already exists... Skipping storage account creation" -ForegroundColor Yellow
                        #Get-AzureRmStorageAccount -Name $storageAccountName -ResourceGroupName $resourceGroupName -ErrorAction Stop
    
                    }
    
                }
                
            } else {

                # Try to create the storage account
                Write-Host "Creating a new storage account named $($storageAccountName)..." -ForegroundColor Green
                New-AzureRmStorageAccount -Name $storageAccountName -ResourceGroupName $resourceGroupName -Location $region -SkuName Standard_GRS -Kind BlobStorage -AccessTier Hot

            }
				
		} catch {
					
            # Create the storage account if one does not exist
            Write-Host "Creating a new storage account named $($storageAccountName)..."
            New-AzureRmStorageAccount -Name $storageAccountName -ResourceGroupName $resourceGroupName -Location $region -SkuName Standard_GRS -Kind BlobStorage -AccessTier Hot
				
		}

	}
    
}

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

ForEach($blob in $blobsList)
{
    Switch($blob.Name)
    {
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


if($config.Topology -eq "single")
{
    ForEach($blob in $blobsList)
    {
        Switch($blob.Name)
        {
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
}
elseif($config.Topology -eq "scaled")
{
    ForEach($blob in $blobsList)
    {

        Switch($blob.Name)
        {
        "wdps/Sitecore Experience Accelerator 1.7.1 rev. 180604 for 9.0 CD.scwdp.zip"
        {
            $sxaCDMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Data Exchange Framework 2.0.1 rev. 180108_single.scwdp.zip"
        {
            $defCDdeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Sitecore Provider for Data Exchange Framework 2.0.1 rev. 180108_scaled.scwdp.zip"
        {
            $defSitecoreCDdeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/SQL Provider for Data Exchange Framework 2.0.1 rev. 180108_scaled.scwdp.zip"
        {
            $defSqlCDdeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/xConnect Provider for Data Exchange Framework 2.0.1 rev. 180108_scaled.scwdp.zip"
        {
            $defxConnectCDdeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Dynamics Provider for Data Exchange Framework 2.0.1 rev. 180108_scaled.scwdp.zip"
        {
            $defDynamicsCDdeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Connect for Microsoft Dynamics 2.0.1 rev. 180108_scaled.scwdp.zip"
        {
            $defDynamicsConnectCDdeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Salesforce Provider for Data Exchange Framework 2.0.1 rev. 180108_scaled.scwdp.zip"
        {
            $defSalesforceCDdeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Connect for Salesforce 2.0.1 rev. 180108_scaled.scwdp.zip"
        {
            $defSalesforceConnectCDdeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Sitecore.Cloud.Integration.Bootload.wdp.zip"
        {
            $msDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Sitecore 9.0.2 rev. 180604 (Cloud)_cm.scwdp.zip.zip"
        {
            $cmMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Sitecore 9.0.2 rev. 180604 (Cloud)_cd.scwdp.zip"
        {
            $cdMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Sitecore 9.0.2 rev. 180604 (Cloud)_prc.scwdp.zip"
        {
            $prcMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Sitecore 9.0.2 rev. 180604 (Cloud)_rep.scwdp.zip"
        {
            $repMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Sitecore 9.0.2 rev. 180604 (Cloud)_xp1referencedata.scwdp.zip"
        {
            $xcRefDataMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Sitecore 9.0.2 rev. 180604 (Cloud)_xp1collection.scwdp.zip"
        {
            $xcCollectMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Sitecore 9.0.2 rev. 180604 (Cloud)_xp1collectionsearch.scwdp.zip"
        {
            $xcSearchMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Sitecore 9.0.2 rev. 180604 (Cloud)_xp1marketingautomation.scwdp.zip"
        {
            $maOpsMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/Sitecore 9.0.2 rev. 180604 (Cloud)_xp1marketingautomationreporting.scwdp.zip"
        {
            $maRepMsDeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
        "wdps/habitathome_cd.scwdp.zip"
        {
            $habitatWebsiteCDdeployPackageUrl = (Get-AzureStorageBlob -Blob $blob.Name -Container $containerName -Context $ctx).ICloudBlob.uri.AbsoluteUri
        }
    }
}



#############################################################
# Get the URL of each required additional file and record it
#############################################################

ForEach ($blob in $blobsList)
{

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


########################################
# Construct azuredeploy.parameters.json
########################################

# Find and process the azuredeploy.parameters.json template

[String] $azuredeployConfigFile = $([IO.Path]::Combine($topology, 'azuredeploy.parameters.json'))

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

if($config.Topology -eq "single")
{
    $azuredeployConfig.parameters | ForEach-Object 
    {
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
}
elseif($config.Topology -eq "scaled")
{
    $azuredeployConfig.parameters | ForEach-Object 
    {
        $_.deploymentId.value                   = $deploymentId
        $_.location.value                       = $location
        $_.sitecoreAdminPassword.value          = $sitecoreAdminPassword
        $_.licenseXml.value                     = $licenseXml
        $_.sqlServerLogin.value                 = $sqlServerLogin
        $_.sqlServerPassword.value              = $sqlServerPassword
        $_.authCertificatePassword.value        = $authCertificatePassword
        $_.cmMsDeployPackageUrl.value           = $cmMsDeployPackageUrl
        $_.cdMsDeployPackageUrl.value           = $cdMsDeployPackageUrl
        $_.cdMsDeployPackageUrl.value           = $cdMsDeployPackageUrl
        $_.prcMsDeployPackageUrl.value          = $prcMsDeployPackageUrl
        $_.repMsDeployPackageUrl.value          = $repMsDeployPackageUrl
        $_.xcRefDataMsDeployPackageUrl.value    = $xcRefDataMsDeployPackageUrl
        $_.xcCollectMsDeployPackageUrl.value    = $xcCollectMsDeployPackageUrl
        $_.xcSearchMsDeployPackageUrl.value     = $xcSearchMsDeployPackageUrl
        $_.maOpsMsDeployPackageUrl.value        = $maOpsMsDeployPackageUrl
        $_.maRepMsDeployPackageUrl.value        = $maRepMsDeployPackageUrl
        $_.modules.value.items[0].parameters.cdSxaMsDeployPackageUrl                    = $sxaCDMsDeployPackageUrl
        $_.modules.value.items[0].parameters.cmSxaMsDeployPackageUrl                    = $speMsDeployPackageUrl
        $_.modules.value.items[0].parameters.speMsDeployPackageUrl                      = $speMsDeployPackageUrl
        $_.modules.value.items[0].templateLink                                          = $sxaTemplateLink
        $_.modules.value.items[1].parameters.defDeployPackageUrl                        = $defDeployPackageUrl
        $_.modules.value.items[1].parameters.defSitecoreDeployPackageUrl                = $defSitecoreDeployPackageUrl
        $_.modules.value.items[1].parameters.defSqlDeployPackageUrl                     = $defSqlDeployPackageUrl
        $_.modules.value.items[1].parameters.defxConnectDeployPackageUrl                = $defxConnectDeployPackageUrl
        $_.modules.value.items[1].parameters.defDynamicsDeployPackageUrl                = $defDynamicsDeployPackageUrl
        $_.modules.value.items[1].parameters.defDynamicsConnectDeployPackageUrl         = $defDynamicsConnectDeployPackageUrl
        $_.modules.value.items[1].parameters.defSalesforceDeployPackageUrl              = $defSalesforceDeployPackageUrl
        $_.modules.value.items[1].parameters.defSalesforceConnectDeployPackageUrl       = $defSalesforceConnectDeployPackageUrl
        $_.modules.value.items[1].parameters.def-cdDeployPackageUrl                     = $defCDdeployPackageUrl
        $_.modules.value.items[1].parameters.defSitecore-cdDeployPackageUrl             = $defSitecoreCDdeployPackageUrl
        $_.modules.value.items[1].parameters.defSql-cdDeployPackageUrl                  = $defSqlCDdeployPackageUrl
        $_.modules.value.items[1].parameters.defxConnect-cdDeployPackageUrl             = $defxConnectCDdeployPackageUrl
        $_.modules.value.items[1].parameters.defDynamics-cdDeployPackageUrl             = $defDynamicsCDdeployPackageUrl
        $_.modules.value.items[1].parameters.defDynamicsConnect-cdDeployPackageUrl      = $defDynamicsConnectCDdeployPackageUrl
        $_.modules.value.items[1].parameters.defSalesforce-cdDeployPackageUrl           = $defSalesforceCDdeployPackageUrl
        $_.modules.value.items[1].parameters.defSalesforceConnect-cdDeployPackageUrl    = $defSalesforceConnectCDdeployPackageUrl
        $_.modules.value.items[1].templateLink                                          = $defTemplateLink
        $_.modules.value.items[2].parameters.habitatWebsite-cdMsDeployPackageUrl        = $habitatWebsiteCDdeployPackageUrl
        $_.modules.value.items[2].parameters.habitatWebsite-cmMsDeployPackageUrl        = $habitatWebsiteDeployPackageUrl
        $_.modules.value.items[2].templateLink                                          = $habitatWebsiteTemplateLink
        $_.modules.value.items[3].parameters.habitatXconnectDeployPackageUrl            = $habitatXconnectDeployPackageUrl
        $_.modules.value.items[3].templateLink                                          = $habitatXconnectTemplateLink
        $_.modules.value.items[4].parameters.msDeployPackageUrl                         = $msDeployPackageUrl
        $_.modules.value.items[4].templateLink                                          = $bootloaderTemplateLink
    }
}

# Apply the azuredeploy.parameters JSON schema to the azuredeploy.parameters.json file

$azuredeployConfig | ConvertTo-Json -Depth 20 | Set-Content $([IO.Path]::Combine($topology, 'azuredeploy.parameters.json'))

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