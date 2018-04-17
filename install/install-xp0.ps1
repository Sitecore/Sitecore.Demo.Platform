Param(
    [string] $ConfigurationFile = "configuration-xp0.json"
)

#####################################################
# 
#  Install Sitecore
# 
#####################################################
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

if (!(Test-Path $ConfigurationFile)) {
    Write-Host "Configuration file '$($ConfigurationFile)' not found." -ForegroundColor Red
    Write-Host  "Please use 'set-installation...ps1' files to generate a configuration file." -ForegroundColor Red
    Exit 1
}
$config = Get-Content -Raw $ConfigurationFile |  ConvertFrom-Json
if (!$config) {
    throw "Error trying to load configuration!"
}

$carbon = Get-Module Carbon
if (-not $carbon) {
    Write-Host "Installing latest version of Carbon" -ForegroundColor Green
    Install-Module -Name Carbon -Repository PSGallery -AllowClobber -Verbose
    Import-Module Carbon
}



$site = $config.settings.site
$sql = $config.settings.sql
$xConnect = $config.settings.xConnect
$sitecore = $config.settings.sitecore
$solr = $config.settings.solr
$assets = $config.assets
$modules = $config.modules

Import-Module .\scripts\additional-tasks.psm1 -Force

Write-Host "*******************************************************" -ForegroundColor Green
Write-Host " Installing Sitecore $($assets.sitecoreVersion)" -ForegroundColor Green
Write-Host " Sitecore: $($site.hostName)" -ForegroundColor Green
Write-Host " xConnect: $($xConnect.siteName)" -ForegroundColor Green
Write-Host "*******************************************************" -ForegroundColor Green

function Install-Prerequisites {
    #Verify SQL version
    
    [reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo") | out-null
    $srv = New-Object "Microsoft.SqlServer.Management.Smo.Server" $sql.server
    $minVersion = New-Object System.Version($sql.minimumVersion)
    if ($srv.Version.CompareTo($minVersion) -lt 0) {
        throw "Invalid SQL version. Expected SQL 2016 SP1 ($($sql.minimumVersion)) or above."
    }

    #Verify Java version
    
    $minVersion = New-Object System.Version($assets.jreRequiredVersion)
    $foundVersion = $FALSE
   
    
    function getJavaVersions() {
        $versions = '', 'Wow6432Node\' |
            ForEach-Object {Get-ItemProperty -Path HKLM:\SOFTWARE\$($_)Microsoft\Windows\CurrentVersion\Uninstall\* |
                Where-Object {($_.DisplayName -like '*Java *') -and (-not $_.SystemComponent)} |
                Select-Object DisplayName, DisplayVersion, @{n = 'Architecture'; e = {If ($_.PSParentPath -like '*Wow6432Node*') {'x86'} Else {'x64'}}}}
        return $versions
    }
    function checkJavaversion($toVersion) {
        $versions_ = getJavaVersions
        foreach ($version_ in $versions_) {
            try {
                $version = New-Object System.Version($version_.DisplayVersion)
                
            }
            catch {
                continue
            }

            if ($version.CompareTo($toVersion) -ge 0) {
                return $TRUE
            }
        }

        return $false

    }
    
    $foundVersion = checkJavaversion($minversion)
    
    if (-not $foundVersion) {
        throw "Invalid Java version. Expected $minVersion or above."
    }

    # Verify Web Deploy
    $webDeployPath = ([IO.Path]::Combine($env:ProgramFiles, 'iis', 'Microsoft Web Deploy V3', 'msdeploy.exe'))
    if (!(Test-Path $webDeployPath)) {
        throw "Could not find WebDeploy in $webDeployPath"
    }   

    # Verify DAC Fx
    # Verify Microsoft.SqlServer.TransactSql.ScriptDom.dll
    try {
        $assembly = [reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.TransactSql.ScriptDom")
        if (-not $assembly) {
            throw "error"
        }
    }
    catch {
        throw "Could load the Microsoft.SqlServer.TransactSql.ScriptDom assembly. Please make sure it is installed and registered in the GAC"
    }
    
    #Enable Contained Databases
    Write-Host "Enable contained databases" -ForegroundColor Green
    try {
        Invoke-Sqlcmd -ServerInstance $sql.server `
            -Username $sql.adminUser `
            -Password $sql.adminPassword `
            -InputFile "$PSScriptRoot\database\containedauthentication.sql"
    }
    catch {
        write-host "Set Enable contained databases failed" -ForegroundColor Red
        throw
    }

    # Verify Solr
    Write-Host "Verifying Solr connection" -ForegroundColor Green
    if (-not $solr.url.ToLower().StartsWith("https")) {
        throw "Solr URL ($SolrUrl) must be secured with https"
    }
    Write-Host "Solr URL: $($solr.url)"
    $SolrRequest = [System.Net.WebRequest]::Create($solr.url)
    $SolrResponse = $SolrRequest.GetResponse()
    try {
        If ($SolrResponse.StatusCode -ne 200) {
            Write-Host "Could not contact Solr on '$($solr.url)'. Response status was '$SolrResponse.StatusCode'" -ForegroundColor Red
            
        }
    }
    finally {
        $SolrResponse.Close()
    }
    
    Write-Host "Verifying Solr directory" -ForegroundColor Green
    if (-not (Test-Path "$($solr.root)\server")) {
        throw "The Solr root path '$($solr.root)' appears invalid. A 'server' folder should be present in this path to be a valid Solr distributive."
    }

    Write-Host "Verifying Solr service" -ForegroundColor Green
    try {
        $null = Get-Service $solr.serviceName
    }
    catch {
        throw "The Solr service '$($solr.serviceName)' does not exist. Perhaps it's incorrect in settings.ps1?"
    }

    #Verify .NET framework
	
    $versionExists = Get-ChildItem "hklm:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" | Get-ItemPropertyValue -Name Release | ForEach-Object { $_ -ge $assets.dotnetMinimumVersionValue }
    if (-not $versionExists) {
        throw "Please install .NET Framework $($assets.dotnetMinimumVersion) or newer"
    }
}
function Install-Assets {
    #Register Assets PowerShell Repository
    if ((Get-PSRepository | Where-Object {$_.Name -eq $assets.psRepositoryName}).count -eq 0) {
        Register-PSRepository -Name $assets.psRepositoryName -SourceLocation $assets.psRepository -InstallationPolicy Trusted
    }

    #Sitecore Install Framework dependencies
    Import-Module WebAdministration

    #Install SIF
    $module = Get-Module -FullyQualifiedName @{ModuleName = "SitecoreInstallFramework"; ModuleVersion = $assets.installerVersion}
    if (-not $module) {
        write-host "Installing the Sitecore Install Framework, version $($assets.installerVersion)" -ForegroundColor Green
        Install-Module SitecoreInstallFramework -RequiredVersion $assets.installerVersion -Repository $assets.psRepositoryName -Scope CurrentUser 
        Import-Module SitecoreInstallFramework -RequiredVersion $assets.installerVersion
    }

    #Verify that manual assets are present
    if (!(Test-Path $assets.root)) {
        throw "$($assets.root) not found"
    }

    #Verify license file
    if (!(Test-Path $assets.licenseFilePath)) {
        throw "License file $($assets.licenseFilePath) not found"
    }
    
    #Verify Sitecore package
    if (!(Test-Path $sitecore.packagePath)) {
        throw "Sitecore package $($sitecore.packagePath) not found"
    }
    
    #Verify xConnect package
    if (!(Test-Path $xConnect.packagePath)) {
        throw "XConnect package $($xConnect.packagePath) not found"
    }
}

function Install-XConnect {
    #Install xConnect Solr
    try {
        Install-SitecoreConfiguration $xConnect.solrConfigurationPath `
            -SolrUrl $solr.url `
            -SolrRoot $solr.root `
            -SolrService $solr.serviceName `
            -CorePrefix $site.prefix
    }
    catch {
        write-host "XConnect SOLR Failed" -ForegroundColor Red
        throw
    }

    #Generate xConnect client certificate
    try {
        Write-Host $xConnect.certificateConfigurationPath
        Install-SitecoreConfiguration $xConnect.certificateConfigurationPath `
            -CertificateName $xConnect.certificateName `
            -CertPath $assets.certificatesPath
    }
    catch {
        write-host "XConnect Certificate Creation Failed" -ForegroundColor Red
        throw
    }
    
    #Install xConnect
    try {
        Install-SitecoreConfiguration $xConnect.ConfigurationPath `
            -Package $xConnect.PackagePath `
            -LicenseFile $assets.licenseFilePath `
            -SiteName $xConnect.siteName `
            -XConnectCert $xConnect.certificateName `
            -SqlDbPrefix $site.prefix `
            -SolrCorePrefix $site.prefix `
            -SqlAdminUser $sql.adminUser `
            -SqlAdminPassword $sql.adminPassword `
            -SqlServer $sql.server `
            -SqlCollectionUser $xConnect.sqlCollectionUser `
            -SqlCollectionPassword $xConnect.sqlCollectionPassword `
            -SolrUrl $solr.url `
            -WebRoot $site.webRoot
    }
    catch {
        write-host "XConnect Setup Failed" -ForegroundColor Red
        throw
    }
                             

    #Set rights on the xDB connection database
    Write-Host "Setting Collection User rights" -ForegroundColor Green
    try {
        $sqlVariables = "DatabasePrefix = $($site.prefix)", "UserName = $($xConnect.sqlCollectionUser)", "Password = $($xConnect.sqlCollectionPassword)"
        Invoke-Sqlcmd -ServerInstance $sql.server `
            -Username $sql.adminUser `
            -Password $sql.adminPassword `
            -InputFile "$PSScriptRoot\database\collectionusergrant.sql" `
            -Variable $sqlVariables
    }
    catch {
        write-host "Set Collection User rights failed" -ForegroundColor Red
        throw
    }
}

function Install-Sitecore {

    try {
        #Install Sitecore Solr
        Install-SitecoreConfiguration $sitecore.solrConfigurationPath `
            -SolrUrl $solr.url `
            -SolrRoot $solr.root `
            -SolrService $solr.serviceName `
            -CorePrefix $site.prefix
    }
    catch {
        write-host "Sitecore SOLR Failed" -ForegroundColor Red
        throw
    }

    try {
        #Install Sitecore
        Install-SitecoreConfiguration $sitecore.configurationPath `
            -Package $sitecore.packagePath `
            -LicenseFile $assets.licenseFilePath `
            -SiteName $site.hostName `
            -XConnectCert $xConnect.certificateName `
            -SqlDbPrefix $site.prefix `
            -SolrCorePrefix $site.prefix `
            -SqlAdminUser $sql.adminUser `
            -SqlAdminPassword $sql.adminPassword `
            -SqlServer $sql.server `
            -SolrUrl $solr.url `
            -XConnectCollectionService "https://$($xConnect.siteName)" `
            -XConnectReferenceDataService "https://$($xConnect.siteName)" `
            -MarketingAutomationOperationsService "https://$($xConnect.siteName)" `
            -MarketingAutomationReportingService "https://$($xConnect.siteName)"`
            -WebRoot $site.webRoot
    }
    catch {
        write-host "Sitecore Setup Failed" -ForegroundColor Red
        throw
    }

    try {
        #Set web certificate on Sitecore site
        Install-SitecoreConfiguration $sitecore.sslConfigurationPath `
            -SiteName $site.hostName `
            -WebRoot $site.WebRoot
    }
    catch {
        write-host "Sitecore SSL Binding Failed" -ForegroundColor Red
        throw
    }
    
}

function Add-AdditionalBindings {
    #Add-HabitatHomeBindingDetails $site.hostName $site.habitatHomeHostName
    try {
        
        Install-SitecoreConfiguration $xConnect.certificateConfigurationPath `
            -CertificateName $site.habitatHomeSslCertificateName `
            -CertPath $assets.certificatesPath
    }
    catch {
        write-host "$site.habitatHomeHostName Certificate Creation Failed" -ForegroundColor Red
        throw
    }

    try {
        Install-SitecoreConfiguration $site.habitatHomeConfigurationPath `
            -SSLCert $site.habitatHomeSslCertificateName `
            -SiteName $site.hostName `
            -HostHeader $site.habitatHomeHostName 
        
    }
    catch {
        write-host "Sitecore Setup Failed" -ForegroundColor Red
        throw
    }
}

function Copy-Tools {
    if (!(Test-Path $assets.installPackagePath)) {
        throw "$($assets.installPackagePath) not found"
    }

    try {
        Write-Host "Copying tools to webroot" -ForegroundColor Green
        Copy-Item $assets.installPackagePath -Destination $sitecore.siteRoot -Force
    }
    catch {
        write-host "Failed to copy InstallPackage.aspx to web root" -ForegroundColor Red
    }
}


function Copy-Package ($packagePath, $destination) {
   
    if (!(Test-Path $packagePath)) {
        throw "Package not found"
    }
    # Check destination
    if (! (Test-Path $destination)) { New-Item $destination -Type Directory }

    Write-Host $packageName
    Copy-Item $packagePath   $destination  -Verbose -Force
         
    
}
function Install-OptionalModules {
    #Copy InstallPackage.aspx to webroot
    
    $packageDestination = Join-Path $sitecore.siteRoot "\temp\Packages"
    foreach ($module in $modules | Where-Object {$_.install -eq $true}) {
        Write-Host "Copying $($module.name) to the $packageDestination"
        Copy-Package -packagePath $module.packagePath -destination "$packageDestination"
        $packageFileName = Split-Path $module.packagePath -Leaf

        $packageInstallerUrl = "https://$($site.habitatHomeHostName)/InstallPackage.aspx?package=/temp/Packages/"
        $url = $packageInstallerUrl + $packageFileName 
        $request = [system.net.WebRequest]::Create($url)
        $request.Timeout = 2400000
        Write-Host $url
        Write-Host "Installing Package : $($module.name)" -ForegroundColor Green
        $request.GetResponse()  
    }
}


Install-Prerequisites
Install-Assets
Install-XConnect
Install-Sitecore
Add-AdditionalBindings
Copy-Tools
Install-OptionalModules