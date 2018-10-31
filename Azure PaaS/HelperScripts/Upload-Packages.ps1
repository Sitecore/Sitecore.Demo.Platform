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

##########################
# Function for WDP uploads
##########################

Function UploadWDPs ([PSCustomObject] $cakeJsonConfig, [PSCustomObject] $assetsJsonConfig){

    $assetsFolder = (Join-Path $cakeJsonConfig.DeployFolder "assets")
    $sitecoreWDPpathArray = New-Object System.Collections.ArrayList

    # Add Sitecore and habitat WDPs to upload list
    if($cakeJsonConfig.Topology -eq "single")
    {
        $sitecorepackages = Get-ChildItem -path $(Join-Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform')) *) -include *.zip -Exclude *xp1*,*cd*,*cm*,*prc*,*rep*

        foreach ($seppackage in $sitecorepackages)
        {
            $sitecoreWDPpathArray.Add($seppackage) | out-null
        }

        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'habitathome', 'WDPWorkFolder', 'WDP', 'habitathome_single.scwdp.zip')))) | out-null
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'xconnect', 'WDPWorkFolder', 'WDP', 'xconnect_single.scwdp.zip')))) | out-null
    }
    elseif($cakeJsonConfig.Topology -eq "scaled")
    {
        $sitecorepackages = Get-ChildItem -path $(Join-Path $([IO.Path]::Combine($assetsFolder, 'Sitecore Experience Platform')) *) -include *.zip -Exclude *xp0*,*single*

        foreach ($seppackage in $sitecorepackages)
        {
            $sitecoreWDPpathArray.Add($seppackage) | out-null
        }
        
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'habitathomeCD', 'WDPWorkFolder', 'WDP', 'habitathome_cd.scwdp.zip')))) | out-null
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'habitathome', 'WDPWorkFolder', 'WDP', 'habitathome_single.scwdp.zip')))) | out-null   
        $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($assetsFolder, 'xconnect', 'WDPWorkFolder', 'WDP', 'xconnect_single.scwdp.zip')))) | out-null
    }

    # Add sitecore azure toolkit module bootloader 
    foreach($asset in $assetsJsonConfig.prerequisites)
    {
        if($asset.name -eq "Sitecore Experience Platform")
        {
            $sepversion = $($asset.FileName -replace ' rev.*','')
            $sepversion = $($sepversion -replace '^Sitecore ','')
        }
    }

    $sitecoreWDPpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($cakeJsonConfig.DeployFolder, 'assets', 'Sitecore Azure Toolkit', 'resources', $sepversion, 'Addons', 'Sitecore.Cloud.Integration.Bootload.wdp.zip')))) | out-null

    # Add Module WDPs to upload list
    foreach ($asset in $assetsJsonConfig.prerequisites)
    {
        if(($asset.uploadToAzure -eq $True) -and ($asset.isWdp -eq $True) -and ($asset.install -eq $True))
        {
            if((Test-Path $(Join-Path $assetsFolder $asset.name)) -eq $True)
            {
                $wdpSingleModuleFolder = ($(Join-Path $assetsFolder $asset.name))
                ForEach($blobFile in (Get-ChildItem -File -Recurse $wdpSingleModuleFolder)) 
                {    
                    $sitecoreWDPpathArray.Add($(Get-Item -Path $blobFile.FullName)) | out-null
                }            
            }
        } 
        elseif(($asset.uploadToAzure -eq $True) -and ($($asset.isGroup -eq $True) -or $($asset.convertToWdp -eq $True)) -and ($asset.install -eq $True))
        {
        
            if((Test-Path $(Join-Path $assetsFolder $asset.name)) -eq $True)
            { 
                $wdpGroupModuleFolder = "$($(Join-Path $assetsFolder $asset.name))\WDPWorkFolder\WDP"
                ForEach($blobFile in (Get-ChildItem -File -Recurse $wdpGroupModuleFolder)) 
                {           
					$sitecoreWDPpathArray.Add($(Get-Item -Path $blobFile.FullName)) | out-null
                }
            }
        }
    }

    # Perform Upload
    foreach ($scwdpinarray in $sitecoreWDPpathArray)
    {
        try 
        {             
            Get-AzureStorageBlob -Blob "wdps/$($scwdpinarray.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
            Write-Host "Skipping... file $($scwdpinarray.Name) already uploaded" -ForegroundColor Yellow              
        } 
        catch 
        {
            Write-Host "Starting file upload for $($scwdpinarray.Name)" -ForegroundColor Green
            Set-AzureStorageBlobContent -File $scwdpinarray.FullName -Blob "wdps/$($scwdpinarray.Name)" -Container $containerName -Context $ctx -Force
            Write-Host "Upload of $($scwdpinarray.Name) completed" -ForegroundColor Green
        }
    }
    
}

###########################
# Function for file uploads
###########################

Function UploadFiles ([PSCustomObject] $cakeJsonConfig, [PSCustomObject] $assetsJsonConfig){

    $sitecoreARMpathArray = New-Object System.Collections.ArrayList
    
    # Fetching all ARM templates' paths
    $sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'azuredeploy.json')))) | out-null
    $sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'ARM Templates', 'Habitat', 'habitathome.json')))) | out-null
    $sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'ARM Templates', 'Habitat', 'xconnect.json')))) | out-null
    $sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'addons', 'bootloader.json')))) | out-null
    $sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'ARM Templates','Sitecore Experience Accelerator', 'sxa_module.json')))) | out-null

	foreach ($asset in $assetsJsonConfig.prerequisites)
	{
		if(($asset.uploadToAzure -eq "Data Exchange Framework") -and ($asset.install -eq $true))
		{
			$sitecoreARMpathArray.Add($(Get-Item -Path $([IO.Path]::Combine($topology, 'ARM Templates','Data Exchange Framework', 'def_module.json')))) | out-null
        }
	}

    # Checking if the files are already uploaded and present in Azure and uploading

    foreach($scARMsInArray in $sitecoreARMpathArray)
    {
        try {
                        
            Get-AzureStorageBlob -Blob "arm-templates/$($scARMsInArray.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
            Write-Host "Skipping... file $($scARMsInArray.Name) already uploaded" -ForegroundColor Yellow
                            
        } catch {
                            
            Write-Host "Starting file upload for $($scARMsInArray.Name)" -ForegroundColor Green
            Set-AzureStorageBlobContent -File $scARMsInArray.FullName -Blob "arm-templates/$($scARMsInArray.Name)" -Container $containerName -Context $ctx -Force
            Write-Host "Upload of $($scARMsInArray.Name) completed" -ForegroundColor Green
                            
        }
    }

    $nestedArmTemplates = Get-Item -Path $([IO.Path]::Combine($topology, 'nested'))

    try {
                        
        Get-AzureStorageBlob -Blob "arm-templates/$($nestedArmTemplates.Name)" -Container $containerName -Context $ctx -ErrorAction Stop
        Write-Host "Skipping... folder $($nestedArmTemplates.Name) already uploaded" -ForegroundColor Yellow
                        
    } catch {
                        
		Get-ChildItem -File -Path $nestedArmTemplates.FullName | ForEach-Object { 

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
        $seed = Get-Random -Maximum 99999
        $resourceGroupNameSeed = $resourceGroupName -replace '-',''
     
        if($resourceGroupNameSeed.length -gt 19)
        {
            $resourceGroupNameSeed = $resourceGroupNameSeed.substring(0, 19)
        }
        $storageAccountName = $resourceGroupNameSeed+$seed
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
            if ($null -ne $storageAccountsList) 
            {
                foreach ($storageAccount in $storageAccountsList) 
                {
                    # Check if a previously generated storage account already exists
                    if ($storageAccount.StorageAccountName -contains $storageAccountName) 
                    {
                        Write-Host "A generated storage account named $($storageAccountName) already exists... Skipping storage account creation" -ForegroundColor Yellow
                        $createstorageaccount = $false
                        break
                    }
                    else 
                    {
                        $createstorageaccount = $true
                    }
                }
            } 
            else 
            {
                $createstorageaccount = $true
            }
           
            if($createstorageaccount)
            {
                # Try to create the storage account
                Write-Host "Creating a new storage account named $($storageAccountName)..." -ForegroundColor Green
                New-AzureRmStorageAccount -Name $storageAccountName -ResourceGroupName $resourceGroupName -Location $region -SkuName Standard_GRS -Kind BlobStorage -AccessTier Hot
            }
        } 
        catch 
        {		
            # Create the storage account if one does not exist
            Write-Host "Creating a new storage account named $($storageAccountName)..."
            New-AzureRmStorageAccount -Name $storageAccountName -ResourceGroupName $resourceGroupName -Location $region -SkuName Standard_GRS -Kind BlobStorage -AccessTier Hot	
		}

	}
    
}

# Set variables for the container names
[String] $additionalContainerName = "temporary-toolkit"
[String] $containerName = "azure-toolkit"

# Get the current storage account
$sa = Get-AzureRmStorageAccount -Name $storageAccountName -ResourceGroupName $resourceGroupName

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

} catch {

    try {

        "Trying to create the container..."

        # Create the main container for the WDPs
        New-AzureStorageContainer -Name $containerName -Context $ctx -Permission off -ErrorAction Stop

    } catch {
    
        "It seems like the container has been deleted very recently... creating a temporary container instead"

		$containerName = $additionalContainerName
        # Create a temporary container
        New-AzureStorageContainer -Name $containerName -Context $ctx -Permission off

    }
    
}

UploadWDPs -cakeJsonConfig $config -assetsJsonConfig $assetconfig
UploadFiles -cakeJsonConfig $config

##############################################
# Get the URL for each WDP blob and record it
##############################################

$blobsList = Get-AzureStorageBlob -Container $containerName -Context $ctx
$assetsFolder = (Join-Path $config.DeployFolder "assets")

foreach($asset in $assetconfig.prerequisites)
{
    if(($asset.name -eq "Data Exchange Framework") -and ($asset.install -eq $true))
    {
        $definstall = $true

        foreach ($module in $asset.modules)
        {
            if($module.Name -eq "Data Exchange Framework")
            {
                $defversion = $($module.FileName -replace '\.zip$','')
                $defversion = $($defversion -replace '^Data Exchange Framework ','')
            }
        }
    }
    elseif(($asset.name -eq "Data Exchange Framework CD") -and ($asset.install -eq $true))
    {
        foreach ($module in $asset.modules)
        {
            if($module.Name -eq "Data Exchange Framework CD")
            {
                $defCDversion = $($module.FileName -replace '\.zip$','')
                $defCDversion = $($defCDversion -replace '^Data Exchange Framework CD Server ','')
            }
        }
    }
    elseif(($asset.name -eq "Data Exchange Framework") -and ($asset.install -eq $false))
    {
        $definstall = $false
    }
}

foreach($asset in $assetconfig.prerequisites)
{
    foreach($blob in $blobsList)
    {
        if($asset.filename -eq $($blob.Name -replace '^wdps\/',''))
        {
            Switch($asset.Name)
            {
                "Sitecore Experience Accelerator"
                {
                    $sxaMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
                }
                "Sitecore PowerShell Extensions"
                {
                    $speMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
                }
            }
        }
    }
}


ForEach($blob in $blobsList)
{
    Switch($blob.Name)
    {
        "wdps/Sitecore.Cloud.Integration.Bootload.wdp.zip"
        {
            $msDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                -Blob $blob.Name `
                                                                -Permission rwd `
                                                                -StartTime (Get-Date) `
                                                                -ExpiryTime (Get-Date).AddHours(3) `
                                                                -Context $ctx `
                                                                -FullUri
        }
        "wdps/habitathome_single.scwdp.zip"
        {
            $habitatWebsiteDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
        }
        "wdps/xconnect_single.scwdp.zip"
        {
            $habitatXconnectDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
        }
    }

    if($definstall -eq $true)
    {
        Switch($blob.Name)
        {
            $("wdps/Data Exchange Framework "+$defversion+"_single.scwdp.zip")
            {
               $defDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                    -Blob $blob.Name `
                                                                    -Permission rwd `
                                                                    -StartTime (Get-Date) `
                                                                    -ExpiryTime (Get-Date).AddHours(3) `
                                                                    -Context $ctx `
                                                                    -FullUri
            }
            $("wdps/Sitecore Provider for Data Exchange Framework "+$defversion+"_single.scwdp.zip")
            {
                $defSitecoreDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
            }
           $("wdps/SQL Provider for Data Exchange Framework "+$defversion+"_single.scwdp.zip")
            {
                $defSqlDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                        -Blob $blob.Name `
                                                                        -Permission rwd `
                                                                        -StartTime (Get-Date) `
                                                                        -ExpiryTime (Get-Date).AddHours(3) `
                                                                        -Context $ctx `
                                                                        -FullUri
            }
            $("wdps/xConnect Provider for Data Exchange Framework "+$defversion+"_single.scwdp.zip")
            {
                $defxConnectDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
            }
            $("wdps/Dynamics Provider for Data Exchange Framework "+$defversion+"_single.scwdp.zip")
            {
                $defDynamicsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
            }
            $("wdps/Connect for Microsoft Dynamics "+$defversion+"_single.scwdp.zip")
            {
                $defDynamicsConnectDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                    -Blob $blob.Name `
                                                                                    -Permission rwd `
                                                                                    -StartTime (Get-Date) `
                                                                                    -ExpiryTime (Get-Date).AddHours(3) `
                                                                                    -Context $ctx `
                                                                                    -FullUri
            }
            $("wdps/Salesforce Provider for Data Exchange Framework "+$defversion+"_single.scwdp.zip")
            {
                $defSalesforceDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                -Blob $blob.Name `
                                                                                -Permission rwd `
                                                                                -StartTime (Get-Date) `
                                                                                -ExpiryTime (Get-Date).AddHours(3) `
                                                                                -Context $ctx `
                                                                                -FullUri
            }
            $("wdps/Connect for Salesforce "+$defversion+"_single.scwdp.zip")
            {
                $defSalesforceConnectDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                        -Blob $blob.Name `
                                                                                        -Permission rwd `
                                                                                        -StartTime (Get-Date) `
                                                                                        -ExpiryTime (Get-Date).AddHours(3) `
                                                                                        -Context $ctx `
                                                                                        -FullUri
            }
        }
    }
}

if($config.Topology -eq "single")
{

    $localSitecoreassets = Get-ChildItem -path $(Join-Path $assetsfolder "Sitecore Experience Platform\*") -include *.zip -Exclude *xp1*,*cd*,*cm*,*prc*,*rep*

    foreach($localSCfile in $localSitecoreassets)
    {
        foreach($blob in $blobsList)
        {
            if($localSCfile.Name -eq $($blob.Name -replace '^wdps\/',''))
            {
                if($($localSCfile.Name -like "*_single.scwdp.zip"))
                {
                    $singleMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                -Blob $blob.Name `
                                                                                -Permission rwd `
                                                                                -StartTime (Get-Date) `
                                                                                -ExpiryTime (Get-Date).AddHours(3) `
                                                                                -Context $ctx `
                                                                                -FullUri
                }
                elseif($($localSCfile.Name -like "*_xp0xconnect.scwdp.zip"))
                {
                    $xcSingleMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                -Blob $blob.Name `
                                                                                -Permission rwd `
                                                                                -StartTime (Get-Date) `
                                                                                -ExpiryTime (Get-Date).AddHours(3) `
                                                                                -Context $ctx `
                                                                                -FullUri
                }
            }
        }
    }
}
elseif($config.Topology -eq "scaled")
{

    $localSitecoreassets = Get-ChildItem -path $(Join-Path $assetsfolder "Sitecore Experience Platform\*") -include *.zip -Exclude *xp0*,*single*

    foreach($localSCfile in $localSitecoreassets)
    {
        foreach($blob in $blobsList)
        {
            if($localSCfile.Name -eq $($blob.Name -replace '^wdps\/',''))
            {
                if($($localSCfile.Name -like "*_cm.scwdp.zip"))
                {
                    $cmMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
                }
                elseif($($localSCfile.Name -like "*_cd.scwdp.zip"))
                {
                    $cdMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
                }
                elseif($($localSCfile.Name -like "*_prc.scwdp.zip"))
                {
                    $prcMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
                }
                elseif($($localSCfile.Name -like "*_rep.scwdp.zip"))
                {
                    $repMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
                }
                elseif($($localSCfile.Name -like "*_xp1referencedata.scwdp.zip"))
                {
                    $xcRefDataMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                -Blob $blob.Name `
                                                                                -Permission rwd `
                                                                                -StartTime (Get-Date) `
                                                                                -ExpiryTime (Get-Date).AddHours(3) `
                                                                                -Context $ctx `
                                                                                -FullUri
                }
                elseif($($localSCfile.Name -like "*_xp1collection.scwdp.zip"))
                {
                    $xcCollectMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                -Blob $blob.Name `
                                                                                -Permission rwd `
                                                                                -StartTime (Get-Date) `
                                                                                -ExpiryTime (Get-Date).AddHours(3) `
                                                                                -Context $ctx `
                                                                                -FullUri
                }
                elseif($($localSCfile.Name -like "*_xp1collectionsearch.scwdp.zip"))
                {
                    $xcSearchMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                -Blob $blob.Name `
                                                                                -Permission rwd `
                                                                                -StartTime (Get-Date) `
                                                                                -ExpiryTime (Get-Date).AddHours(3) `
                                                                                -Context $ctx `
                                                                                -FullUri
                }
                elseif($($localSCfile.Name -like "*_xp1marketingautomation.scwdp.zip"))
                {
                    $maOpsMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
                }
                elseif($($localSCfile.Name -like "*_xp1marketingautomationreporting.scwdp.zip"))
                {
                    $maRepMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
                }
            }
        }
    }

    foreach($asset in $assetconfig.prerequisites)
    {
        foreach($blob in $blobsList)
        {
            if($asset.filename -eq $($blob.Name -replace '^wdps\/',''))
            {
                Switch($asset.Name)
                {
                    "Sitecore Experience Accelerator CD"
                    {
                        $sxaCDMsDeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                -Blob $blob.Name `
                                                                                -Permission rwd `
                                                                                -StartTime (Get-Date) `
                                                                                -ExpiryTime (Get-Date).AddHours(3) `
                                                                                -Context $ctx `
                                                                                -FullUri
                    }
                }
            }
        }
    }

    ForEach($blob in $blobsList)
    {
        Switch($blob.Name)
        {
            "wdps/habitathome_cd.scwdp.zip"
            {
                $habitatWebsiteCDdeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                    -Blob $blob.Name `
                                                                                    -Permission rwd `
                                                                                    -StartTime (Get-Date) `
                                                                                    -ExpiryTime (Get-Date).AddHours(3) `
                                                                                    -Context $ctx `
                                                                                    -FullUri
            }
        }

        if($definstall -eq $true)
        {
            Switch($blob.Name)
            {
                $("wdps/Data Exchange Framework CD Server "+$defCDversion+"_scaled.scwdp.zip")
                {
                    $defCDdeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                            -Blob $blob.Name `
                                                                            -Permission rwd `
                                                                            -StartTime (Get-Date) `
                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                            -Context $ctx `
                                                                            -FullUri
                }
                $("wdps/Sitecore Provider for Data Exchange Framework CD Server "+$defCDversion+"_scaled.scwdp.zip")
                {
                    $defSitecoreCDdeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                    -Blob $blob.Name `
                                                                                    -Permission rwd `
                                                                                    -StartTime (Get-Date) `
                                                                                    -ExpiryTime (Get-Date).AddHours(3) `
                                                                                    -Context $ctx `
                                                                                    -FullUri
                }
                $("wdps/SQL Provider for Data Exchange Framework CD Server "+$defCDversion+"_scaled.scwdp.zip")
                {
                    $defSqlCDdeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                -Blob $blob.Name `
                                                                                -Permission rwd `
                                                                                -StartTime (Get-Date) `
                                                                                -ExpiryTime (Get-Date).AddHours(3) `
                                                                                -Context $ctx `
                                                                                -FullUri
                }
                $("wdps/xConnect Provider for Data Exchange Framework CD Server "+$defCDversion+"_scaled.scwdp.zip")
                {
                    $defxConnectCDdeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                    -Blob $blob.Name `
                                                                                    -Permission rwd `
                                                                                    -StartTime (Get-Date) `
                                                                                    -ExpiryTime (Get-Date).AddHours(3) `
                                                                                    -Context $ctx `
                                                                                    -FullUri
                }
                $("wdps/Dynamics Provider for Data Exchange Framework CD Server "+$defCDversion+"_scaled.scwdp.zip")
                {
                    $defDynamicsCDdeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                    -Blob $blob.Name `
                                                                                    -Permission rwd `
                                                                                    -StartTime (Get-Date) `
                                                                                    -ExpiryTime (Get-Date).AddHours(3) `
                                                                                    -Context $ctx `
                                                                                    -FullUri
                }
                $("wdps/Connect for Microsoft Dynamics CD Server "+$defCDversion+"_scaled.scwdp.zip")
                {
                    $defDynamicsConnectCDdeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                            -Blob $blob.Name `
                                                                                            -Permission rwd `
                                                                                            -StartTime (Get-Date) `
                                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                                            -Context $ctx `
                                                                                            -FullUri
                }
                $("wdps/Salesforce Provider for Data Exchange Framework CD Server "+$defCDversion+"_scaled.scwdp.zip")
                {
                    $defSalesforceCDdeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                    -Blob $blob.Name `
                                                                                    -Permission rwd `
                                                                                    -StartTime (Get-Date) `
                                                                                    -ExpiryTime (Get-Date).AddHours(3) `
                                                                                    -Context $ctx `
                                                                                    -FullUri
                }
                $("wdps/Connect for Salesforce CD Server "+$defCDversion+"_scaled.scwdp.zip")
                {
                    $defSalesforceConnectCDdeployPackageUrl = New-AzureStorageBlobSASToken -Container $containerName `
                                                                                            -Blob $blob.Name `
                                                                                            -Permission rwd `
                                                                                            -StartTime (Get-Date) `
                                                                                            -ExpiryTime (Get-Date).AddHours(3) `
                                                                                            -Context $ctx `
                                                                                            -FullUri
                }
            }
        }
    }
}


#############################################################
# Get the URL of each required additional file and record it
#############################################################

ForEach ($blob in $blobsList)
{
    if($blob.Name -like "*sxa*.json")
    {
        $sxaTemplateLink = New-AzureStorageBlobSASToken -Container $containerName `
                                                        -Blob $blob.Name `
                                                        -Permission rwd `
                                                        -StartTime (Get-Date) `
                                                        -ExpiryTime (Get-Date).AddHours(3) `
                                                        -Context $ctx `
                                                        -FullUri

    } elseif($blob.Name -like "*def*.json")
    {
        $defTemplateLink = New-AzureStorageBlobSASToken -Container $containerName `
                                                        -Blob $blob.Name `
                                                        -Permission rwd `
                                                        -StartTime (Get-Date) `
                                                        -ExpiryTime (Get-Date).AddHours(3) `
                                                        -Context $ctx `
                                                        -FullUri

    } elseif($blob.Name -like "*habitathome.json")
    {
        $habitatWebsiteTemplateLink = New-AzureStorageBlobSASToken -Container $containerName `
                                                                    -Blob $blob.Name `
                                                                    -Permission rwd `
                                                                    -StartTime (Get-Date) `
                                                                    -ExpiryTime (Get-Date).AddHours(3) `
                                                                    -Context $ctx `
                                                                    -FullUri

    } elseif($blob.Name -like "*xconnect.json")
    {
        $habitatXconnectTemplateLink = New-AzureStorageBlobSASToken -Container $containerName `
                                                                    -Blob $blob.Name `
                                                                    -Permission rwd `
                                                                    -StartTime (Get-Date) `
                                                                    -ExpiryTime (Get-Date).AddHours(3) `
                                                                    -Context $ctx `
                                                                    -FullUri
    } elseif($blob.Name -like "*bootloader.json")
    {
        $bootloaderTemplateLink = New-AzureStorageBlobSASToken -Container $containerName `
                                                                -Blob $blob.Name `
                                                                -Permission rwd `
                                                                -StartTime (Get-Date) `
                                                                -ExpiryTime (Get-Date).AddHours(3) `
                                                                -Context $ctx `
                                                                -FullUri
    }
}


########################################
# Construct azuredeploy.parameters.json
########################################

# Find and process the azuredeploy.parameters.json template

[String] $azuredeployConfigFile = $null

if($definstall -eq $true)
{
    $azuredeployConfigFile = $([IO.Path]::Combine($topology, 'azuredeploy.parameters.json'))
}
elseif($definstall -eq $false)
{
    $azuredeployConfigFile = $([IO.Path]::Combine($topology, 'azuredeploy.parametersWOdef.json'))
}

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
    }
}

# Populate parameters inside the azuredeploy.parameters JSON schema with values from previously prepared variables
if($definstall -eq $true)
{
    if($config.Topology -eq "single")
    {
        $azuredeployConfig.parameters | ForEach-Object {
            $_.deploymentId.value               = $deploymentId
            $_.location.value                   = $location
            $_.sitecoreAdminPassword.value      = $sitecoreAdminPassword
            $_.licenseXml.value                 = $licenseXml
            $_.sqlServerLogin.value             = $sqlServerLogin
            $_.sqlServerPassword.value          = $sqlServerPassword
            $_.authCertificatePassword.value    = $authCertificatePassword
            $_.singleMsDeployPackageUrl.value   = $singleMsDeployPackageUrl
            $_.xcSingleMsDeployPackageUrl.value = $xcSingleMsDeployPackageUrl
            $_.modules.value.items[0].parameters.sxaMsDeployPackageUrl                  = $sxaMsDeployPackageUrl
            $_.modules.value.items[0].parameters.speMsDeployPackageUrl                  = $speMsDeployPackageUrl
            $_.modules.value.items[0].templateLink                                      = $sxaTemplateLink
            $_.modules.value.items[1].parameters.defDeployPackageUrl                    = $defDeployPackageUrl
            $_.modules.value.items[1].parameters.defSitecoreDeployPackageUrl            = $defSitecoreDeployPackageUrl
            $_.modules.value.items[1].parameters.defSqlDeployPackageUrl                 = $defSqlDeployPackageUrl
            $_.modules.value.items[1].parameters.defxConnectDeployPackageUrl            = $defxConnectDeployPackageUrl
            $_.modules.value.items[1].parameters.defDynamicsDeployPackageUrl            = $defDynamicsDeployPackageUrl
            $_.modules.value.items[1].parameters.defDynamicsConnectDeployPackageUrl     = $defDynamicsConnectDeployPackageUrl
            $_.modules.value.items[1].parameters.defSalesforceDeployPackageUrl          = $defSalesforceDeployPackageUrl
            $_.modules.value.items[1].parameters.defSalesforceConnectDeployPackageUrl   = $defSalesforceConnectDeployPackageUrl
            $_.modules.value.items[1].templateLink                                      = $defTemplateLink
            $_.modules.value.items[2].parameters.habitatWebsiteDeployPackageUrl         = $habitatWebsiteDeployPackageUrl
            $_.modules.value.items[2].templateLink                                      = $habitatWebsiteTemplateLink
            $_.modules.value.items[3].parameters.habitatXconnectDeployPackageUrl        = $habitatXconnectDeployPackageUrl
            $_.modules.value.items[3].templateLink                                      = $habitatXconnectTemplateLink
            $_.modules.value.items[4].parameters.msDeployPackageUrl                     = $msDeployPackageUrl
            $_.modules.value.items[4].templateLink                                      = $bootloaderTemplateLink
        }
    }
    elseif($config.Topology -eq "scaled")
    {
        $azuredeployConfig.parameters | ForEach-Object {
            $_.deploymentId.value                   = $deploymentId
            $_.location.value                       = $location
            $_.sitecoreAdminPassword.value          = $sitecoreAdminPassword
            $_.licenseXml.value                     = $licenseXml
            $_.repAuthenticationApiKey.value        = $(New-Guid)
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
            $_.modules.value.items[0].parameters.cmSxaMsDeployPackageUrl                    = $sxaMsDeployPackageUrl
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
            $_.modules.value.items[1].parameters.defCdDeployPackageUrl                      = $defCDdeployPackageUrl
            $_.modules.value.items[1].parameters.defSitecoreCdDeployPackageUrl              = $defSitecoreCDdeployPackageUrl
            $_.modules.value.items[1].parameters.defSqlCdDeployPackageUrl                   = $defSqlCDdeployPackageUrl
            $_.modules.value.items[1].parameters.defxConnectCdDeployPackageUrl              = $defxConnectCDdeployPackageUrl
            $_.modules.value.items[1].parameters.defDynamicsCdDeployPackageUrl              = $defDynamicsCDdeployPackageUrl
            $_.modules.value.items[1].parameters.defDynamicsConnectCdDeployPackageUrl       = $defDynamicsConnectCDdeployPackageUrl
            $_.modules.value.items[1].parameters.defSalesforceCdDeployPackageUrl            = $defSalesforceCDdeployPackageUrl
            $_.modules.value.items[1].parameters.defSalesforceConnectCdDeployPackageUrl     = $defSalesforceConnectCDdeployPackageUrl
            $_.modules.value.items[1].templateLink                                          = $defTemplateLink
            $_.modules.value.items[2].parameters.habitatWebsiteCdMsDeployPackageUrl         = $habitatWebsiteCDdeployPackageUrl
            $_.modules.value.items[2].parameters.habitatWebsiteCmMsDeployPackageUrl         = $habitatWebsiteDeployPackageUrl
            $_.modules.value.items[2].templateLink                                          = $habitatWebsiteTemplateLink
            $_.modules.value.items[3].parameters.habitatXconnectDeployPackageUrl            = $habitatXconnectDeployPackageUrl
            $_.modules.value.items[3].templateLink                                          = $habitatXconnectTemplateLink
            $_.modules.value.items[4].parameters.msDeployPackageUrl                         = $msDeployPackageUrl
            $_.modules.value.items[4].templateLink                                          = $bootloaderTemplateLink
        }
    }
}
elseif($definstall -eq $false)
{
    if($config.Topology -eq "single")
    {
        $azuredeployConfig.parameters | ForEach-Object {
            $_.deploymentId.value               = $deploymentId
            $_.location.value                   = $location
            $_.sitecoreAdminPassword.value      = $sitecoreAdminPassword
            $_.licenseXml.value                 = $licenseXml
            $_.sqlServerLogin.value             = $sqlServerLogin
            $_.sqlServerPassword.value          = $sqlServerPassword
            $_.authCertificatePassword.value    = $authCertificatePassword
            $_.singleMsDeployPackageUrl.value   = $singleMsDeployPackageUrl
            $_.xcSingleMsDeployPackageUrl.value = $xcSingleMsDeployPackageUrl
            $_.modules.value.items[0].parameters.sxaMsDeployPackageUrl                  = $sxaMsDeployPackageUrl
            $_.modules.value.items[0].parameters.speMsDeployPackageUrl                  = $speMsDeployPackageUrl
            $_.modules.value.items[0].templateLink                                      = $sxaTemplateLink
            $_.modules.value.items[1].parameters.habitatWebsiteDeployPackageUrl         = $habitatWebsiteDeployPackageUrl
            $_.modules.value.items[1].templateLink                                      = $habitatWebsiteTemplateLink
            $_.modules.value.items[2].parameters.habitatXconnectDeployPackageUrl        = $habitatXconnectDeployPackageUrl
            $_.modules.value.items[2].templateLink                                      = $habitatXconnectTemplateLink
            $_.modules.value.items[3].parameters.msDeployPackageUrl                     = $msDeployPackageUrl
            $_.modules.value.items[3].templateLink                                      = $bootloaderTemplateLink
        }
    }
    elseif($config.Topology -eq "scaled")
    {
        $azuredeployConfig.parameters | ForEach-Object {
            $_.deploymentId.value                   = $deploymentId
            $_.location.value                       = $location
            $_.sitecoreAdminPassword.value          = $sitecoreAdminPassword
            $_.licenseXml.value                     = $licenseXml
            $_.repAuthenticationApiKey.value        = $(New-Guid)
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
            $_.modules.value.items[0].parameters.cmSxaMsDeployPackageUrl                    = $sxaMsDeployPackageUrl
            $_.modules.value.items[0].parameters.speMsDeployPackageUrl                      = $speMsDeployPackageUrl
            $_.modules.value.items[0].templateLink                                          = $sxaTemplateLink
            $_.modules.value.items[1].parameters.habitatWebsiteCdMsDeployPackageUrl         = $habitatWebsiteCDdeployPackageUrl
            $_.modules.value.items[1].parameters.habitatWebsiteCmMsDeployPackageUrl         = $habitatWebsiteDeployPackageUrl
            $_.modules.value.items[1].templateLink                                          = $habitatWebsiteTemplateLink
            $_.modules.value.items[2].parameters.habitatXconnectDeployPackageUrl            = $habitatXconnectDeployPackageUrl
            $_.modules.value.items[2].templateLink                                          = $habitatXconnectTemplateLink
            $_.modules.value.items[3].parameters.msDeployPackageUrl                         = $msDeployPackageUrl
            $_.modules.value.items[3].templateLink                                          = $bootloaderTemplateLink
        }
    }  
}

# Apply the azuredeploy.parameters JSON schema to the azuredeploy.parameters.json file

if($definstall -eq $true)
{
    $azuredeployConfig | ConvertTo-Json -Depth 20 | Set-Content $([IO.Path]::Combine($topology, 'azuredeploy.parameters.json'))
}
elseif($definstall -eq $false)
{
    $azuredeployConfig | ConvertTo-Json -Depth 20 | Set-Content $([IO.Path]::Combine($topology, 'azuredeploy.parametersWOdef.json'))
}

# Populate the "azuredeploy.json" ARM template URL inside the azureuser-config JSON schema and apply the schema to the azureuser-config.json file

ForEach ($setting in $azureuserconfig.settings)
{

    # Check if an ARM template URL is already present inside the azureuser-config.json file

    switch($setting.id)
    {
        "containerName"
        {
            $setting.value = $containerName
        }
        "storageAccountName"
        {
            $setting.value = $storageAccountName
        }
    }

}

$azureuserconfig | ConvertTo-Json -Depth 5 | Set-Content $azureuserconfigfile