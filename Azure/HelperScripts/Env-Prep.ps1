<#
.SYNOPSIS
Prepare Local environnment for habitat home scwdp package creation

.DESCRIPTION
This script will check the local Deploy folder defined in the $ConfigurationFile file 
for an Assets folder, and create one if it doesn't exist. It will then check the folder 
for prerequisite files as defined by the assets.json. The script will then download anything missing 
and extract tools and files so they can be used by later scripts.

.PARAMETER ConfigurationFile
A cake-config.json file
.PARAMETER SitecoreDownloadUsername
dev.sitecore.com username.
.PARAMETER SitecoreDownloadPassword
dev.sitecore.com password.

#>

[CmdletBinding()]
Param(
	[parameter(Mandatory=$true, HelpMessage="Please Enter your cake-config.json")]
	[ValidateNotNullOrEmpty()]
    [string] $ConfigurationFile
)

###########################
# Find configuration files
###########################

Import-Module "$($PSScriptRoot)\ProcessConfigFile\ProcessConfigFile.psm1" -Force

$configarray     		= ProcessConfigFile -Config $ConfigurationFile
$config          		= $configarray[0]
$assetconfig     		= $configarray[1]
$azureuserconfig 		= $configarray[2]
$azureuserconfigFile 	= $configarray[4]
$topologyPath	        = $configarray[5]
$topologyName			= $configarray[6]
$assetsFolder			= $configarray[7]
$SCversion				= $configarray[8]
$buildFolder			= $configarray[9]

############################
# Get Sitecore Credentials
############################

$sitecoreAccountConfiguration = $azureuserconfig.sitecoreAccount;

if ([string]::IsNullOrEmpty($sitecoreAccountConfiguration.username))
{
	$sitecoreAccountConfiguration.username = Read-Host "Please provide your dev.sitecore.com username"
}

if ([string]::IsNullOrEmpty($sitecoreAccountConfiguration.password))
{
	$sitecoreAccountConfiguration.password = Read-Host "Please provide your dev.sitecore.com password"
}

$azureuserconfig | ConvertTo-Json | set-content $azureuserconfigFile

$securePassword = ConvertTo-SecureString $sitecoreAccountConfiguration.password -AsPlainText -Force

###################################
# Parameters
###################################

$foundfiles   = New-Object System.Collections.ArrayList
$downloadlist = New-Object System.Collections.ArrayList
[string] $habitathomefilepath = $([io.path]::combine($buildFolder, 'HabitatHome'))
$credentials = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $sitecoreAccountConfiguration.username, $securePassword

##################################################
# Check for existing Files in Deploy\Assets Folder
##################################################

Write-Host "Checking for prerequisite files"
Write-Host "Checking for files in" $assetsFolder

if (!(Test-Path $assetsFolder)) 
{
  Write-Host "Assets Folder does not exist"
  Write-Host "Creating Assets Folder"

  New-Item -ItemType Directory -Force -Path $assetsFolder
}

$localassets = Get-ChildItem -path $(Join-Path $assetsFolder *) -include *.zip -r


foreach($_ in $localassets)
{
  $foundfiles.Add($_.name) | out-null
}

if($foundfiles)
{	
	Write-Host "Files found:"

	foreach ($_ in $assetconfig.prerequisites)
	{
		if($_.install -eq $true)
		{
			if (($foundfiles -contains $_.fileName) -eq $true)
			{
				Write-Host `t $_.filename
				continue
			}
			elseif ($_.isGroup -eq "true")
			{
				foreach ($module in $_.modules)
				{
					if (($foundfiles -contains $module.fileName) -eq $true)
					{
						Write-Host `t $module.filename
						continue
					}
					else
					{
						$downloadlist.Add($module.fileName) | out-null
					}
				}
			}
			else
			{
				$downloadlist.Add($_.fileName) | out-null
			}
		}
	}
	
	if($downloadlist)
	{
		Write-Host "Files Missing:"
	
		foreach ($_ in $downloadlist)
		{
			Write-Host `t $_
		}

	}
	else
	{
		Write-Host "All Local required files found"
	}
}
else
{
    Write-Host "No Local files have been found"

	foreach ($_ in $assetconfig.prerequisites)
	{
		if($_.install -eq $true)
		{
			if ($_.isGroup -eq "true")
			{
				foreach ($module in $_.modules)
				{
					$downloadlist.Add($module.fileName) | out-null
				}
			}
			else
			{
				$downloadlist.Add($_.fileName) | out-null
			}
		}
	}

}


###########################
# Download Required Files
###########################

	Function Download-Asset {
    param(
		[PSCustomObject]$assetfilename,
        $Credentials,
        $assetsFolder,
		$sourceuri,
		$sourceType
    )

        if (!(Test-Path $assetsFolder)) {

			Write-Host "Assets Folder does not exist"
			Write-Host "Creating Assets Folder"

            New-Item -ItemType Directory -Force -Path $assetsFolder
        }

        Write-Host "Downloading" $assetfilename -ForegroundColor Green

			$params = @{
                    Source      	= $sourceuri
                    Destination 	= $assetsFolder
					Credentials 	= $Credentials
					Assetfilename   = $assetfilename
					TypeSource 		= $sourceType
					}
			Import-Module "$($PSScriptRoot)\DownloadFileWithCredentials\DownloadFileWithCredentials.psm1" -Force

            Invoke-DownloadFileWithCredentialsTask  @params  
		
	}

if($downloadlist)
{
	Write-Host "Downloading necessary files"

	foreach ($prereq in $assetconfig.prerequisites)
	{
		if($prereq.isGroup -eq $true)
		{
			
			if (!(Test-Path $(Join-Path $assetsFolder $prereq.name))) 
			{
				Write-Host $prereq.name "folder does not exist"
				Write-Host "Creating" $prereq.name "Folder"

				New-Item -ItemType Directory -Force -Path $(Join-Path $assetsFolder $prereq.name)
			}
			
			foreach ($module in $prereq.modules)
			{
				if(($downloadlist -contains $module.fileName) -eq $false)
				{
					continue
				}
				else
				{
					Download-Asset -assetfilename $module.fileName -Credentials $Credentials -assetsfolder $(Join-Path $assetsFolder $prereq.name) -sourceuri $module.url -sourceType $module.source
				}
			}
		}
		elseif ((($prereq.isWDP -eq $true) -or ($prereq.convertToWdp -eq $true)) -and ($downloadlist -contains $prereq.fileName))
		{
			if (!(Test-Path $(Join-Path $assetsFolder $prereq.name))) 
			{
				Write-Host $prereq.name "folder does not exist"
				Write-Host "Creating" $prereq.name "Folder"

				New-Item -ItemType Directory -Force -Path $(Join-Path $assetsFolder $prereq.name)
			}

			Download-Asset -assetfilename $prereq.fileName -Credentials $Credentials -assetsfolder $(Join-Path $assetsFolder $prereq.name) -sourceuri $prereq.url -sourceType $prereq.source
			
		}
		elseif (($downloadlist -contains $prereq.fileName) -eq $false)
		{
			continue
		}
		else
		{
			Download-Asset -assetfilename $prereq.fileName -Credentials $Credentials -assetsfolder $assetsFolder -sourceuri $prereq.url -sourceType $prereq.source
		}
	}
}

# Download ArmTemplates

function DownloadFilesFromRepo {
	Param(
		[string]$Owner,
		[string]$Repository,
		[string]$Path,
		[string]$DestinationPath
	)

		[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
		$ProgressPreference = 'SilentlyContinue'

	$baseUri = "https://api.github.com/"
	$arguments = "repos/$Owner/$Repository/contents/$Path"
	$wr = Invoke-WebRequest -Uri $($baseuri+$arguments)
	$objects = $wr.Content | ConvertFrom-Json
	$files = $objects | where {$_.type -eq "file"} | Select -exp download_url
	$directories = $objects | where {$_.type -eq "dir"}
	
	$directories | ForEach-Object { 
		DownloadFilesFromRepo -Owner $Owner -Repository $Repository -Path $_.path -DestinationPath $($DestinationPath+$_.name)
	}

	
	if (-not (Test-Path $DestinationPath)) {
		try {
			New-Item -Path $DestinationPath -ItemType Directory -ErrorAction Stop
		} catch {
			throw "Could not create path '$DestinationPath'!"
		}
	}

	foreach ($file in $files) {
		$fileDestination = Join-Path $DestinationPath (Split-Path $file -Leaf)
		try {
			Invoke-WebRequest -Uri $file -OutFile $fileDestination -ErrorAction Stop -Verbose
			"Grabbed '$($file)' to '$fileDestination'"
		} catch {
			throw "Unable to download '$($file.path)'"
		}
	}
}

if (!(Test-Path $(Join-Path $assetsFolder 'ArmTemplates'))) 
{
	Write-Host "Assets Folder does not exist"
	Write-Host "Creating Assets Folder"

	New-Item -ItemType Directory -Force -Path $assetsFolder

	Write-Host "Downloading ARM Templates" -ForegroundColor Green
	DownloadFilesFromRepo Sitecore Sitecore-Azure-Quickstart-Templates "Sitecore%20$SCversion/$topologyName" $(Join-Path $assetsFolder 'ArmTemplates\')
}
elseif (!(Test-Path $([io.path]::combine($assetsFolder, 'ArmTemplates', '*'))))
{
	Write-Host "Downloading ARM Templates" -ForegroundColor Green
	DownloadFilesFromRepo Sitecore Sitecore-Azure-Quickstart-Templates "Sitecore%20$SCversion/$topologyName" $(Join-Path $assetsFolder 'ArmTemplates\')
}

# Copy over the infrastructure-cdn.json file to the appropriate nested folder

$sourceCdnPath = $([IO.Path]::Combine($topologyPath, 'ARM Templates', 'Habitat', 'infrastructure-cdn.json'))
$destinationCdnPath = $([IO.Path]::Combine($assetsFolder, 'ArmTemplates', 'nested'))

if ((Test-Path $sourceCdnPath) -and (Test-Path $destinationCdnPath))
{
	Copy-Item -Path $sourceCdnPath -Destination $destinationCdnPath -Force
}

###########################
# Extract Files
###########################

$global:ProgressPreference = 'SilentlyContinue'

$localassets = Get-ChildItem -path $(Join-Path $assetsFolder *) -include *.zip -r

foreach ($_ in $assetconfig.prerequisites)
{
	if ((($localassets.name -contains $_.fileName) -eq $true) -and ($_.extract -eq $true))
	{
		# This is a bug fix due to the DotNetZip.dll getting locked if the build is ran multiple times
		if (($_.name -eq "Sitecore Azure Toolkit") -and $(Test-Path $([io.path]::combine($assetsFolder, $_.name, 'tools', 'DotNetZip.dll'))))
		{
			Write-Host $_.name "found, skipping extraction"
			continue
		} 
		elseif ($_.name -eq "Sitecore Experience Platform")
		{
			if (($config.Topology -eq "single") -and $(Test-Path -Path "$($assetsFolder)\$($_.name)\*xp0*"))
			{
				Write-Host $_.name "found, skipping extraction"
				continue
			}
			elseif (($config.Topology -eq "scaled") -and $(Test-Path -Path "$($assetsFolder)\$($_.name)\*xp1*"))
			{
				Write-Host $_.name "found, skipping extraction"
				continue
			}
		}

		Write-Host "Extracting" $_.filename -ForegroundColor Green

		Expand-Archive	-Path $(Join-path $assetsFolder $_.filename) -DestinationPath $(Join-path $assetsFolder $_.name) -force
	}
	elseif ($_.isGroup -eq $true)
	{
		foreach ($module in $_.modules)
		{
			if ((($localassets.name -contains $module.fileName) -eq $true) -and ($module.extract -eq $true))
			{
				
				if (!(Test-Path $(Join-Path $assetsFolder $_.name))) 
				{
					Write-Host $_.name "folder does not exist"
					Write-Host "Creating" $_.name "Folder"

					New-Item -ItemType Directory -Force -Path $(Join-Path $assetsFolder $_.name)
				}

				Write-Host "Extracting" $module.filename -ForegroundColor Green
				Expand-Archive	-Path $(Join-path $assetsFolder $module.filename) -DestinationPath $(Join-path $assetsFolder $_.name) -force
			}
		}
	}
}

#################################
# Move Assets to Correct Folders
#################################

foreach ($prereq in $assetconfig.prerequisites)
{
	if($prereq.isGroup -eq $true)
	{
		foreach($module in $prereq.modules)
		{
			if(($localassets.name -contains $module.fileName) -eq $true)
			{
				if((Test-Path $(Join-path $assetsFolder $(Join-Path $prereq.name $module.filename))))
				{
					continue
				}
				else
				{
					Write-host "Moving" $module.fileName "to" $(Join-path $assetsFolder $prereq.name)
					$localassets.fullname -like "*\$($module.filename)" | Move-Item -destination $(Join-path $assetsFolder $(Join-Path $prereq.name $module.fileName)) -force
				}
			}
		}
	}
}

#######################
# Modify ARM Templates
#######################

[System.Reflection.Assembly]::LoadWithPartialName("System.Web.Extensions") | Out-Null
$oJsSerializer = New-Object System.Web.Script.Serialization.JavaScriptSerializer

# Allow Self Signed Certs in Azure params
$allowSelfSigned =  @"
{
"allowInvalidClientCertificates": {
	"value": true
  }
}
"@

$allowSelfSigned = ConvertFrom-Json $allowSelfSigned

$azureParametersFile = Get-Content $([io.path]::combine($assetsFolder, 'ArmTemplates', 'azuredeploy.parameters.json'))
$azureParametersFile = $oJsSerializer.DeserializeObject($azureParametersFile)
if(!($azureParametersFile.parameters.allowInvalidClientCertificates))
{
	$azureParametersFile.parameters.add("allowInvalidClientCertificates",$allowSelfSigned.allowInvalidClientCertificates)
	$azureParametersFile  | ConvertTo-Json -Depth 50 | Set-Content $([io.path]::combine($assetsFolder, 'ArmTemplates', 'azuredeploy.parameters.json')) -Encoding Ascii
}

# Scale up App Services
if($config.Topology -eq "single")
{
	$azureInfrastructureFile = Get-Content $([io.path]::combine($assetsFolder, 'ArmTemplates', 'nested', 'infrastructure.json'))
	$azureInfrastructureFile = $oJsSerializer.DeserializeObject($azureInfrastructureFile)

	if($azureInfrastructureFile.parameters.singleHostingPlanSkuName.defaultValue -ne "P3v2")
	{
		$azureInfrastructureFile.parameters.singleHostingPlanSkuName.defaultValue = "P3v2"
		$azureInfrastructureFile  | ConvertTo-Json -Depth 50 | Set-Content $([io.path]::combine($assetsFolder, 'ArmTemplates', 'nested', 'infrastructure.json')) -Encoding Ascii
	}
}
elseif($config.Topology -eq "scaled")
{
	$azureInfrastructureFile = Get-Content $([io.path]::combine($assetsFolder, 'ArmTemplates', 'nested', 'infrastructure.json'))
	$azureInfrastructureFile = $oJsSerializer.DeserializeObject($azureInfrastructureFile)
	if($azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".cmHostingPlan.SkuName -ne "P3v2")
	{
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".cmHostingPlan.SkuName = "P3v2"
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".cdHostingPlan.SkuName = "P3v2"
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".prcHostingPlan.SkuName = "P3v2"
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".repHostingPlan.SkuName = "P3v2"
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".coreSqlDatabase.ServiceObjectiveLevel = "S3"
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".masterSqlDatabase.ServiceObjectiveLevel = "S3"
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".webSqlDatabase.ServiceObjectiveLevel = "S3"
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".reportingSqlDatabase.ServiceObjectiveLevel = "S3"
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".poolsSqlDatabase.ServiceObjectiveLevel = "S3"
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".tasksSqlDatabase.ServiceObjectiveLevel = "S3"
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".formsSqlDatabase.ServiceObjectiveLevel = "S3"
		$azureInfrastructureFile.parameters.skuMap.defaultValue."Extra Small".exmMasterSqlDatabase.ServiceObjectiveLevel = "S3"
		$azureInfrastructureFile  | ConvertTo-Json -Depth 50 | Set-Content $([io.path]::combine($assetsFolder, 'ArmTemplates', 'nested', 'infrastructure.json')) -Encoding Ascii
	}
}