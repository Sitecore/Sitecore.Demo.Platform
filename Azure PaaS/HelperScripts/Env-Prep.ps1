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

$configarray     = ProcessConfigFile -Config $ConfigurationFile
$config          = $configarray[0]
$assetconfig     = $configarray[1]
$azureuserconfig = $configarray[2]
$azureuserconfigFile = $configarray[4]

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
$assetsfolder = (Join-Path $config.DeployFolder assets)
[string] $habitathomefilepath = $([io.path]::combine($config.DeployFolder, 'Website', 'HabitatHome'))
$credentials = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $sitecoreAccountConfiguration.username, $securePassword

##################################################
# Check for existing Files in Deploy\Assets Folder
##################################################

Write-Host "Checking for prerequisite files"
Write-Host "Checking for files in" $assetsfolder

if (!(Test-Path $assetsfolder)) 
{
  Write-Host "Assets Folder does not exist"
  Write-Host "Creating Assets Folder"

  New-Item -ItemType Directory -Force -Path $assetsfolder
}

$localassets = Get-ChildItem -path $(Join-Path $assetsfolder *) -include *.zip -r


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
    param(   [PSCustomObject]
        $assetfilename,
        $Credentials,
        $assetsfolder,
		$sourceuri,
		$sourceType
    )

        if (!(Test-Path $assetsfolder)) {

			Write-Host "Assets Folder does not exist"
			Write-Host "Creating Assets Folder"

            New-Item -ItemType Directory -Force -Path $assetsfolder
        }

        Write-Host "Downloading" $assetfilename -ForegroundColor Green

			$params = @{
                    Source      = $sourceuri
                    Destination = $assetsfolder
					Credentials = $Credentials
					Assetfilename   = $assetfilename
					TypeSource = $sourceType
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
			
			if (!(Test-Path $(Join-Path $assetsfolder $prereq.name))) 
			{
				Write-Host $prereq.name "folder does not exist"
				Write-Host "Creating" $prereq.name "Folder"

				New-Item -ItemType Directory -Force -Path $(Join-Path $assetsfolder $prereq.name)
			}
			
			foreach ($module in $prereq.modules)
			{
				if(($downloadlist -contains $module.fileName) -eq $false)
				{
					continue
				}
				else
				{
					Download-Asset -assetfilename $module.fileName -Credentials $Credentials -assetsfolder $(Join-Path $assetsfolder $prereq.name) -sourceuri $module.url -sourceType $module.source
				}
			}
		}
		elseif (($prereq.isWDP -eq $true) -and ($downloadlist -contains $prereq.fileName))
		{
			if (!(Test-Path $(Join-Path $assetsfolder $prereq.name))) 
			{
				Write-Host $prereq.name "folder does not exist"
				Write-Host "Creating" $prereq.name "Folder"

				New-Item -ItemType Directory -Force -Path $(Join-Path $assetsfolder $prereq.name)
			}

			Download-Asset -assetfilename $prereq.fileName -Credentials $Credentials -assetsfolder $(Join-Path $assetsfolder $prereq.name) -sourceuri $prereq.url -sourceType $prereq.source
			
		}
		elseif (($downloadlist -contains $prereq.fileName) -eq $false)
		{
			continue
		}
		else
		{
			Download-Asset -assetfilename $prereq.fileName -Credentials $Credentials -assetsfolder $assetsfolder -sourceuri $prereq.url -sourceType $prereq.source
		}
	}
}

###########################
# Extract Files
###########################

$global:ProgressPreference = 'SilentlyContinue'

$localassets = Get-ChildItem -path $(Join-Path $assetsfolder *) -include *.zip -r

foreach ($_ in $assetconfig.prerequisites)
{
	if ((($localassets.name -contains $_.fileName) -eq $true) -and ($_.extract -eq $true))
	{
		# This is a bug fix due to the DotNetZip.dll getting locked if the build is ran multiple times
		if (($_.name -eq "Sitecore Azure Toolkit") -and $(Test-Path $([io.path]::combine($assetsfolder, $_.name, 'tools', 'DotNetZip.dll'))))
		{
			Write-Host $_.name "found, skipping extraction"
			continue
		} 
		elseif ($_.name -eq "Sitecore Experience Platform")
		{
			if (($config.Topology -eq "single") -and $(Test-Path -Path "$($assetsfolder)\$($_.name)\*xp0*"))
			{
				Write-Host $_.name "found, skipping extraction"
				continue
			}
			elseif (($config.Topology -eq "scaled") -and $(Test-Path -Path "$($assetsfolder)\$($_.name)\*xp1*"))
			{
				Write-Host $_.name "found, skipping extraction"
				continue
			}
		}

		Write-Host "Extracting" $_.filename -ForegroundColor Green

		Expand-Archive	-Path $(Join-path $assetsfolder $_.filename) -DestinationPath $(Join-path $assetsfolder $_.name) -force
	}
	elseif ($_.isGroup -eq $true)
	{
		foreach ($module in $_.modules)
		{
			if ((($localassets.name -contains $module.fileName) -eq $true) -and ($module.extract -eq $true))
			{
				
				if (!(Test-Path $(Join-Path $assetsfolder $_.name))) 
				{
					Write-Host $_.name "folder does not exist"
					Write-Host "Creating" $_.name "Folder"

					New-Item -ItemType Directory -Force -Path $(Join-Path $assetsfolder $_.name)
				}

				Write-Host "Extracting" $module.filename -ForegroundColor Green
				Expand-Archive	-Path $(Join-path $assetsfolder $module.filename) -DestinationPath $(Join-path $assetsfolder $_.name) -force
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
				if((Test-Path $(Join-path $assetsfolder $(Join-Path $prereq.name $module.filename))))
				{
					continue
				}
				else
				{
					Write-host "Moving" $module.fileName "to" $(Join-path $assetsfolder $prereq.name)
					$localassets.fullname -like "*\$($module.filename)" | Move-Item -destination $(Join-path $assetsfolder $(Join-Path $prereq.name $module.fileName)) -force
				}
			}
		}
	}
}