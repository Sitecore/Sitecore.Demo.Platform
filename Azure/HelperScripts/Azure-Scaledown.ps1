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

# Get all Azure Web Roles in the configured resource group and for each one scale it down to Standard Small size

$serviceplans = Get-AzureRmAppServicePlan -ResourceGroupName $($azureuserconfig.settings[0].value)

foreach ($serviceplan in $serviceplans)
{
    Set-AzureRmAppServicePlan -ResourceGroupName $($azureuserconfig.settings[0].value) -Name $($serviceplan.AppServicePlanName) -Tier Standard -WorkerSize Small
}

# The commented-out section below is supposed to do scale down on the SQL databases, but currently times out because the operation takes too long

# Get all Azure SQL Servers in the configured resource group

<#$sqlServers = Get-AzureRmSqlServer -ResourceGroupName $($azureuserconfig.settings[0].value)

foreach ($sqlServer in $sqlServers)
{

    # Get all SQL databases in the configured SQL server and resource group and for each one scale it down to S0

    $sqlDatabases = Get-AzureRmSqlDatabase -ResourceGroupName $($azureuserconfig.settings[0].value) -ServerName $sqlServer.ServerName

    foreach ($sqlDatabase in $sqlDatabases)
    {
        Set-AzureRmSqlDatabase -ResourceGroupName $($azureuserconfig.settings[0].value) -DatabaseName $sqlDatabase.DatabaseName -ServerName $sqlServer.ServerName -Edition Standard -RequestedServiceObjectiveName S0
    }
}#>