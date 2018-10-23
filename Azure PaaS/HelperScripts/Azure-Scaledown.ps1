<#
.SYNOPSIS
Scale down azure resources post deployment

.DESCRIPTION
Scales down service plans in the resource group deonted 
by the deploymentID of the azure user config

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

$configarray     = ProcessConfigFile -Config $ConfigurationFile
$config          = $configarray[0]
$azureuserconfig = $configarray[2]
$topology		 = $configarray[5]

###########################
# Scale down service plans
###########################

$serviceplans = Get-AzureRmAppServicePlan -ResourceGroupName $($azureuserconfig.settings[0].value)

foreach ($serviceplan in $serviceplans)
{
    Set-AzureRmAppServicePlan -ResourceGroupName $($azureuserconfig.settings[0].value) -Name $($serviceplan.AppServicePlanName) -Tier Standard -WorkerSize Small
}