# escape=`

ARG PARENT_IMAGE
ARG SOLUTION_IMAGE

ARG XGEN_ASSETS

FROM ${XGEN_ASSETS} as xgen_assets
FROM ${SOLUTION_IMAGE} as solution
FROM ${PARENT_IMAGE}

ENV SQL_DATABASE_PREFIX=Sitecore

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN Install-PackageProvider -Name NuGet -Force; `
    Set-PackageSource -Name PSGallery -Trusted ; `
    Install-Module -Name SqlServer;

COPY --from=xgen_assets /module/db /xgen_assets

COPY --from=solution /solution/db /solution_data

COPY ./sql /sql
COPY ./tools/scripts\ .\

RUN .\InstallPrerequisites.ps1;

ENTRYPOINT ["powershell.exe", ".\\StartInit.ps1", "-ResourcesDirectory $env:RESOURCES_PATH", "-SqlServer $env:SQL_SERVER", "-SqlAdminUser $env:SQL_ADMIN_LOGIN", "-SqlDatabasePrefix $env:Sql_Database_Prefix", "-SqlAdminPassword $env:SQL_ADMIN_PASSWORD", "-SitecoreAdminUsername $env:SITECORE_ADMIN_USERNAME", "-SitecoreAdminPassword $env:sitecore_admin_password", "-SitecoreUserPassword $env:SITECORE_USER_PASSWORD", "-SqlElasticPoolName $env:SQL_ELASTIC_POOL_NAME", "-DatabasesToDeploy $env:DATABASES_TO_DEPLOY", "-PostDeploymentWaitPeriod $env:POST_DEPLOYMENT_WAIT_PERIOD", `
    "-DatabaseUsers @( `
    @{'databaseName' = 'Sitecore.Master'; 'username' = $env:Master_Database_Username; 'password' = $env:Master_Database_Password}, `
    @{'databaseName' = 'Sitecore.Core'; 'username' = $env:Core_Database_Username; 'password' = $env:Core_Database_Password}, `
    @{'databaseName' = 'Sitecore.Web'; 'username' = $env:Web_Database_Username; 'password' = $env:Web_Database_Password}, `
    @{'databaseName' = 'Sitecore.Experienceforms'; 'username' = $env:Forms_Database_Username; 'password' = $env:Forms_Database_Password}, `
    @{'databaseName' = 'Sitecore.Exm.master'; 'username' = $env:Exm_Master_Database_Username; 'password' = $env:Exm_Master_Database_Password}, `
    @{'databaseName' = 'Sitecore.Marketingautomation'; 'username' = $env:Marketing_Automation_Database_Username; 'password' = $env:Marketing_Automation_Database_Password}, `
    @{'databaseName' = 'Sitecore.Messaging'; 'username' = $env:Messaging_Database_Username; 'password' = $env:Messaging_Database_Password }, `
    @{'databaseName' = 'Sitecore.Reporting'; 'username' = $env:Reporting_Database_Username; 'password' = $env:Reporting_Database_Password }, `
    @{'databaseName' = 'Sitecore.Referencedata'; 'username' = $env:Reference_Data_Database_Username; 'password' = $env:Reference_Data_Database_Password }, `
    @{'databaseName' = 'Sitecore.Processing.Pools'; 'username' = $env:Processing_Pools_Database_Username; 'password' = $env:Processing_Pools_Database_Password }, `
    @{'databaseName' = 'Sitecore.Processing.Tasks'; 'username' = $env:Processing_Tasks_Database_Username; 'password' = $env:Processing_Tasks_Database_Password }, `
    @{'databaseName' = 'Sitecore.Processing.Engine.Tasks'; 'username' = $env:Processing_Engine_Tasks_Database_Username; 'password' = $env:Processing_Engine_Tasks_Database_Password }, `
    @{'databaseName' = 'Sitecore.Processing.Engine.Storage'; 'username' = $env:Processing_Engine_Storage_Database_Username; 'password' = $env:Processing_Engine_Storage_Database_Password }, `
    @{'databaseName' = 'Sitecore.Xdb.Collection.ShardMapManager'; 'username' = $env:Collection_ShardMapManager_Database_Username; 'password' = $env:Collection_ShardMapManager_Database_Password }, `
    @{'databaseName' = 'Sitecore.Xdb.Collection.Shard0'; 'username' = $env:Collection_ShardMapManager_Database_Username; 'password' = $env:Collection_ShardMapManager_Database_Password }, `
    @{'databaseName' = 'Sitecore.Xdb.Collection.Shard1'; 'username' = $env:Collection_ShardMapManager_Database_Username; 'password' = $env:Collection_ShardMapManager_Database_Password } `
    )"]