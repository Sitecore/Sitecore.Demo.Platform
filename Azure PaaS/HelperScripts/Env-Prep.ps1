<#
	This script will check the local Deploy folder defined in the cake-config.json file for an Assets folder, and create one if it doesn't exist.
	It will then check the folder for prerequisite files as defined by the assets.json. 
	The script will then download anything missing and extract tools so they can be used by later scripts.
#>

Param(
	[parameter(Mandatory=$true)]
	[ValidateNotNullOrEmpty()]
    [string] $ConfigurationFile,
	[Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
	[string] $Username,
	[Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
	[Security.SecureString] $Password
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

# Find and process assets.json
if($config.Topology -eq "single")
{
	[string] $AssetsFile = $([io.path]::combine($config.ProjectFolder, 'Azure Paas', 'XP0 Single', 'assets.json'))
}
else
{
	throw "Only XP0 Single Deployments are currently supported, please change the Topology parameter in the cake-config.json to single"
}

if (!(Test-Path $AssetsFile)) {
    Write-Host "Assets file '$($AssetsFile)' not found." -ForegroundColor Red
    Write-Host  "Please ensure there is a assets.json file at '$($AssetsFile)'" -ForegroundColor Red
    Exit 1
}

$assetconfig = Get-Content -Raw $AssetsFile |  ConvertFrom-Json
if (!$assetconfig) {
    throw "Error trying to load Assest File!"
} 

###################################
# Parameters
###################################

$foundfiles   = New-Object System.Collections.ArrayList
$downloadlist = New-Object System.Collections.ArrayList
$assetsfolder = (Join-Path $config.DeployFolder assets)
[string] $habitathomefilepath = $([io.path]::combine($config.DeployFolder, 'Website', 'HabitatHome'))
$credentials = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $Username, $Password 

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


###########################
# Download Required Files
###########################

 Import-Module "$($PSScriptRoot)\DownloadFileWithCredentials\DownloadFileWithCredentials.psm1" -Force

	Function Download-Asset {
    param(   [PSCustomObject]
        $assetfilename,
        $Credentials,
        $assetsfolder,
		$sourceuri
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
					}
				
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
					Download-Asset -assetfilename $module.fileName -Credentials $Credentials -assetsfolder $(Join-Path $assetsfolder $prereq.name) -sourceuri $module.url
				}
			}
		}
		elseif (($prereq.isWDP -eq $true))
		{
			if (!(Test-Path $(Join-Path $assetsfolder $prereq.name))) 
			{
				Write-Host $prereq.name "folder does not exist"
				Write-Host "Creating" $prereq.name "Folder"

				New-Item -ItemType Directory -Force -Path $(Join-Path $assetsfolder $prereq.name)
			}

			Download-Asset -assetfilename $prereq.fileName -Credentials $Credentials -assetsfolder $(Join-Path $assetsfolder $prereq.name) -sourceuri $prereq.url
			
		}
		elseif (($downloadlist -contains $prereq.fileName) -eq $false)
		{
			continue
		}
		else
		{
			Download-Asset -assetfilename $prereq.fileName -Credentials $Credentials -assetsfolder $assetsfolder -sourceuri $prereq.url
		}
	}
}

###########################
# Extract Files
###########################

Write-Host "Extracting Files"
$global:ProgressPreference = 'SilentlyContinue'

$localassets = Get-ChildItem -path $(Join-Path $assetsfolder *) -include *.zip -r

foreach ($_ in $assetconfig.prerequisites)
{
	if ((($localassets.name -contains $_.fileName) -eq $true) -and ($_.extract -eq $true))
	{
		Write-Host "Extracting" $_.filename -ForegroundColor Green
		Expand-Archive	-Path $(Join-path $assetsfolder $_.filename) -DestinationPath $(Join-path $assetsfolder $_.name) -force
	}
	elseif($_.isGroup -eq $true)
	{
		foreach($module in $_.modules)
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

if ($config.Topology -eq "single")
{
	foreach ($_ in $assetconfig.prerequisites)
	{
		if($_.id -eq "xp0")
		{
			if(!(Test-Path $([io.path]::combine($assetsfolder, 'Sitecore Experience Platform', 'sc', 'Content', 'Website', 'Web.config'))) -or `
				!(Test-Path $([io.path]::combine($assetsfolder, 'Sitecore Experience Platform', 'sc', 'Content', 'Website', 'App_Config', 'ConnectionStrings.config'))) -or `
				!(Test-Path $([io.path]::combine($assetsfolder, 'Sitecore Experience Platform', 'sc', 'Content', 'Website', 'App_Config', 'Security','Domains.config'))))
			{
				try
				{
					Write-Host "Extracting Sitecore 9.0.2 rev. 180604 (Cloud)_single.scwdp.zip"  -ForegroundColor Green
					Expand-Archive -Path $([io.path]::combine($assetsfolder, 'Sitecore Experience Platform', 'Sitecore 9.0.2 rev. 180604 (Cloud)_single.scwdp.zip')) `
					-Destination $([io.path]::combine($assetsfolder, 'Sitecore Experience Platform', 'sc')) -force
				}
				catch
				{
					throw "Extraction failed due to filename length. Please place the Deploy Folder in a shorter path"
				}
			}
			else
			{
				Write-host "Skipping Extration of Sitecore 9.0.2 rev. 180604 (Cloud)_single.scwdp.zip - Extrated files found"
			}
		}
	}
}
else
{
	throw "Only XP0 Single Deployments are currently supported, please change the Topology parameter in the cake-config.json to single"
}

#################################
# Move Assets to Correct Folders
#################################

Write-Host "Moving Files to correct folders"

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

if ($config.Topology -eq "single")
{
	if( (Test-Path $([io.path]::combine($habitathomefilepath, 'App_Config', 'Security', 'Domains.config'))) -and `
		(Test-Path $([io.path]::combine($habitathomefilepath, 'App_Config', 'ConnectionStrings.config'))) -and `
		(Test-Path $(Join-path $habitathomefilepath "Web.config")))
		{
			Write-host "Build Output folder preperation complete"
		}
		else
		{
			Write-host "Preparing Build Output Folder"

			if(!(Test-Path $habitathomefilepath))
			{
				Write-Host $habitathomefilepath "folder does not exist"
				Write-Host "Creating" $habitathomefilepath "Folder"
				New-Item -ItemType Directory -Force -Path $habitathomefilepath
			}
			if(!(Test-Path $(Join-Path $habitathomefilepath "App_Config")))
			{
				Write-Host $(Join-Path $habitathomefilepath "App_Config") "folder does not exist"
				Write-Host "Creating" $(Join-Path $habitathomefilepath "App_Config") "Folder"
				New-Item -ItemType Directory -Force -Path $(Join-Path $habitathomefilepath "App_Config")
			}

			if(!(Test-Path $([io.path]::combine($habitathomefilepath, 'App_Config', 'Security'))))
			{
				Write-Host $([io.path]::combine($habitathomefilepath, 'App_Config', 'Security')) "folder does not exist"
				Write-Host "Creating" $([io.path]::combine($habitathomefilepath, 'App_Config', 'Security')) "Folder"
				New-Item -ItemType Directory -Force -Path $([io.path]::combine($habitathomefilepath, 'App_Config', 'Security'))
			}

			$sitecoreconfigs = Get-ChildItem -path $(Join-Path $([io.path]::combine($assetsfolder, 'Sitecore Experience Platform', 'sc')) *) -include *.config -r

			$sitecoreconfigs.fullname -like "*\Web.config" | Copy-Item -destination $(Join-path $habitathomefilepath "Web.config") -force
			$sitecoreconfigs.fullname -like "*\ConnectionStrings.config" | Copy-Item -destination $([io.path]::combine($habitathomefilepath, 'App_Config', 'ConnectionStrings.config')) -force
			$sitecoreconfigs.fullname -like "*\Domains.config" | Copy-Item -destination $([io.path]::combine($habitathomefilepath, 'App_Config', 'Security', 'Domains.config')) -force

			Write-host "Build Output folder preperation complete"
		}
}
else
{
	throw "Only XP0 Single Deployments are currently supported, please change the Topology parameter in the cake-config.json to single"
}