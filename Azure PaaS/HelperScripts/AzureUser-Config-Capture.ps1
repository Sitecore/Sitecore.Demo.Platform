<#

.SYNOPSIS
Gather use input pretaining to azure upload and deployment

.DESCRIPTION
This scripts enables a persisent azure session. This will allows future scripts to use this azure 
session without asking for crednetials again. This script will edit a azureuser-config.json bassed 
on user respones to prompts. The azureuser-config.json is intedned to be used by other scritps to 
help access their Azure env.

.PARAMETER ConfigurationFile
A cake-config.json file

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

$configarray         = ProcessConfigFile -Config $ConfigurationFile
$config              = $configarray[0]
$assetconfig         = $configarray[1]
$azureuserconfig     = $configarray[2]
$assetconfigFile     = $configarray[3]
$azureuserconfigFile = $configarray[4]

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

# Add the Azure Service Principal

$servicePrincipalConfiguration = $azureuserconfig.serviceprincipal;

if ([string]::IsNullOrEmpty($servicePrincipalConfiguration.azureSubscriptionName))
{
	$servicePrincipalConfiguration.azureSubscriptionName = Read-Host "Please Provide the Azure Subscription Name"
}

if ([string]::IsNullOrEmpty($servicePrincipalConfiguration.tenantId))
{
	$servicePrincipalConfiguration.tenantId = Read-Host "Please Provide the Azure Tenant ID"
}

if ([string]::IsNullOrEmpty($servicePrincipalConfiguration.applicationId))
{
	$servicePrincipalConfiguration.applicationId = Read-Host "Please Provide the Azure Application Id"
}

if ([string]::IsNullOrEmpty($servicePrincipalConfiguration.applicationPassword))
{
	$servicePrincipalConfiguration.applicationPassword = Read-Host "Please Provide the Azure Application Password"
}

$securePassword = ConvertTo-SecureString $servicePrincipalConfiguration.applicationPassword -AsPlainText -Force
$servicePrincipalCredentials = New-Object System.Management.Automation.PSCredential($servicePrincipalConfiguration.applicationId, $securePassword)
Login-AzureRmAccount -ServicePrincipal -Tenant $servicePrincipalConfiguration.tenantId -Credential $servicePrincipalCredentials
Set-AzureRmContext -SubscriptionName $servicePrincipalConfiguration.azureSubscriptionName -TenantId $servicePrincipalConfiguration.tenantId


###########################################
# Get User Input for azureuser-config.json
###########################################

Write-host "Please Enter Azure Settings"

foreach ($setting in $azureuserconfig.settings)
{
	if (-not ([string]::IsNullOrEmpty($setting.value)))
	{
		continue
	}

	switch ($setting.id)
	{		
		"ArmTemplateUrl"
		{
			continue
		}
		default
		{
			$setting.value = Read-Host "Please Provide the $($setting.description)"
		}
	}
}

$azureuserconfig | ConvertTo-Json  | set-content $azureuserconfigFile