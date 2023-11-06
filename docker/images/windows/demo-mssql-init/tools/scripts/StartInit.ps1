[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ -PathType Container })]
    [string]$ResourcesDirectory,

    [Parameter(Mandatory)]
    [string]$SqlServer,

    [Parameter(Mandatory)]
    [string]$SqlAdminUser,

    [Parameter(Mandatory)]
    [string]$SqlAdminPassword,

    [Parameter(Mandatory)]
    [string]$SitecoreAdminUsername,

    [Parameter(Mandatory)]
    [string]$SitecoreAdminPassword,

    [Parameter(Mandatory)]
    [string]$SitecoreUserPassword,

    [Parameter(Mandatory)]
    [string]$SqlDatabasePrefix,

    [string]$SqlCustomDatabasePrefixUpdateFrom,

    [string]$SqlElasticPoolName,
    [object[]]$DatabaseUsers,

    [string]$DatabasesToDeploy,

    [int]$PostDeploymentWaitPeriod
)

$deployDatabases = $true

if (-not $DatabasesToDeploy) {
    $serverDatabasesQuery = "SET NOCOUNT ON; SELECT name FROM sys.databases"
    $serverDatabases = Invoke-Expression "sqlcmd -S $SqlServer -U $SqlAdminUser -P $SqlAdminPassword -Q '$serverDatabasesQuery' -h -1 -W"

    $existingDatabases = Get-ChildItem $ResourcesDirectory -Filter *.dacpac -Recurse -Depth 1 | `
                            Where-Object { $serverDatabases.Contains($_.BaseName)}
    if ($existingDatabases.Count -gt 0) {
        Write-Information -MessageData "Sitecore databases are detected. Skipping deployment." -InformationAction Continue
        $deployDatabases = $false
    }
}

if ($deployDatabases) {
    .\DeployDatabases.ps1 -ResourcesDirectory $ResourcesDirectory -SqlServer:$SqlServer -SqlAdminUser:$SqlAdminUser -SqlAdminPassword:$SqlAdminPassword -EnableContainedDatabaseAuth -SkipStartingServer -SqlElasticPoolName $SqlElasticPoolName -DatabasesToDeploy $DatabasesToDeploy -SqlDatabasePrefix:$SqlDatabasePrefix -SqlCustomDatabasePrefixUpdateFrom:$SqlCustomDatabasePrefixUpdateFrom

    if(-not $DatabasesToDeploy) {
        if(Test-Path -Path (Join-Path $ResourcesDirectory "smm_azure.sql")) {
            .\InstallShards.ps1 -ResourcesDirectory $ResourcesDirectory -SqlElasticPoolName $SqlElasticPoolName -SqlServer $SqlServer -SqlAdminUser $SqlAdminUser -SqlAdminPassword $SqlAdminPassword -SqlDatabasePrefix:$SqlDatabasePrefix
        }

        .\SetDatabaseUsers.ps1 -ResourcesDirectory $ResourcesDirectory -SqlServer:$SqlServer -SqlAdminUser:$SqlAdminUser -SqlAdminPassword:$SqlAdminPassword `
            -DatabaseUsers $DatabaseUsers -SqlDatabasePrefix:$SqlDatabasePrefix -SqlCustomDatabasePrefixUpdateFrom:$SqlCustomDatabasePrefixUpdateFrom
        .\SetSitecoreAdminPassword.ps1 -ResourcesDirectory $ResourcesDirectory -SitecoreAdminPassword $SitecoreAdminPassword -SqlServer $SqlServer -SqlAdminUser $SqlAdminUser -SqlAdminPassword $SqlAdminPassword  -SqlDatabasePrefix:$SqlDatabasePrefix
    }

    Write-Host "Installing XGEN assets"
    .\DeployDatabases.ps1 -ResourcesDirectory C:\xgen_assets -SqlServer:$SqlServer -SqlAdminUser:$SqlAdminUser -SqlAdminPassword:$SqlAdminPassword -EnableContainedDatabaseAuth -SkipStartingServer -SqlElasticPoolName $SqlElasticPoolName -DatabasesToDeploy $DatabasesToDeploy -SqlDatabasePrefix:$SqlDatabasePrefix -SqlCustomDatabasePrefixUpdateFrom:$SqlCustomDatabasePrefixUpdateFrom

    Write-Host "Installing Solution"
    .\DeployDatabases.ps1 -ResourcesDirectory C:\solution_data\data -SqlServer:$SqlServer -SqlAdminUser:$SqlAdminUser -SqlAdminPassword:$SqlAdminPassword -EnableContainedDatabaseAuth -SkipStartingServer -SqlElasticPoolName $SqlElasticPoolName -DatabasesToDeploy $DatabasesToDeploy -SqlDatabasePrefix:$SqlDatabasePrefix -SqlCustomDatabasePrefixUpdateFrom:$SqlCustomDatabasePrefixUpdateFrom
    .\DeployDatabases.ps1 -ResourcesDirectory C:\solution_data\security -SqlServer:$SqlServer -SqlAdminUser:$SqlAdminUser -SqlAdminPassword:$SqlAdminPassword -EnableContainedDatabaseAuth -SkipStartingServer -SqlElasticPoolName $SqlElasticPoolName -DatabasesToDeploy $DatabasesToDeploy -SqlDatabasePrefix:$SqlDatabasePrefix -SqlCustomDatabasePrefixUpdateFrom:$SqlCustomDatabasePrefixUpdateFrom
}

$ready = Invoke-Sqlcmd -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -Query "select name from sys.databases where name = 'platform_init_ready'" -TrustServerCertificate
if (-not $ready) {

    # Disable sitecore\admin
    Invoke-Sqlcmd -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -InputFile "C:\sql\DisableSitecoreAdminUser.sql" -TrustServerCertificate -Verbose

    # Create custom admin user
    .\CreateSitecoreAdminUser.ps1 -SqlServer $SqlServer -SqlAdminUser $SqlAdminUser -SqlAdminPassword $SqlAdminPassword -SitecoreAdminUsername $SitecoreAdminUsername -SitecoreAdminPassword $SitecoreAdminPassword

    # Create custom backup admin user
    .\CreateSitecoreAdminUser.ps1 -SqlServer $SqlServer -SqlAdminUser $SqlAdminUser -SqlAdminPassword $SqlAdminPassword -SitecoreAdminUsername "sitecore\superadmin" -SitecoreAdminPassword $SitecoreAdminPassword

    # Alter demo users, and set new password
    .\ResetDemoUsers.ps1 -SqlServer $SqlServer -SqlAdminUser $SqlAdminUser -SqlAdminPassword $SqlAdminPassword -SitecoreUserPassword $SitecoreUserPassword

    # Set base URL for EXM root items - instance specific URL
    .\SetExmBaseUrl.ps1 -SqlServer $SqlServer -SqlAdminUser $SqlAdminUser -SqlAdminPassword $SqlAdminPassword

    # Create platform_init_ready database to indicate that init script is complete
    Invoke-Sqlcmd -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -Query "create database platform_init_ready" -TrustServerCertificate -Verbose
}

[System.Environment]::SetEnvironmentVariable("DatabasesDeploymentStatus", "Complete", "Machine")

Write-Host "Sleeping for 300 seconds to allow depends_on to proceed"

Start-Sleep -Seconds 300