# DEMO TEAM CUSTOMIZATION - Add ability to skip building the containers.
[CmdletBinding(DefaultParameterSetName = "no-arguments")]
Param (
    [Parameter(HelpMessage = "Whether to skip building the Docker images.")]
    [switch]$SkipBuild,

    [Parameter(HelpMessage = "Whether to set up the environment with pre-release version of Sitecore products (internal only) .")]
    [switch]$PreRelease
)

$ErrorActionPreference = "Stop";

# Double check whether init has been run
$envCheckVariable = "REPORTING_API_KEY"
$envCheck = Get-Content .env -Encoding UTF8 | Where-Object { $_ -imatch "^$envCheckVariable=.+" }
if (-not $envCheck) {
    # DEMO TEAM CUSTOMIZATION - Auto run init.ps1 if not run.
    if (Test-Path "C:\License") {
        Write-Host "Initializing environment using default values" -ForegroundColor Yellow
        & .\init.ps1 -InitEnv -AdminPassword b -LicenseXmlPath C:\License\license.xml
        if ($PreRelease) {
            & .\init-ci.ps1 -PreRelease
        }
        else {
            & .\init-ci.ps1
        }
    }
    else {
        throw "$envCheckVariable does not have a value. Did you run 'init.ps1 -InitEnv'?"
    }
    # END CUSTOMIZATION
}

# DEMO TEAM CUSTOMIZATION - Add ability to skip building the containers.
if (-not $SkipBuild) {
    # Build all containers in the Sitecore instance, forcing a pull of latest base containers
    Write-Host "Building containers..." -ForegroundColor Green
    .\build-images.ps1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Container build failed, see errors above."
    }
}
# END CUSTOMIZATION

# Start the Sitecore instance
Write-Host "Starting Sitecore environment..." -ForegroundColor Green
docker compose up -d

# Wait for Traefik to expose CM route
Write-Host "Waiting for CM to become available..." -ForegroundColor Green
$startTime = Get-Date
do {
    Start-Sleep -Milliseconds 100
    try {
        $status = Invoke-RestMethod "http://localhost:8079/api/http/routers/cm-secure@docker"
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne "404") {
            throw
        }
    }
} while ($status.status -ne "enabled" -and $startTime.AddSeconds(15) -gt (Get-Date))
if (-not $status.status -eq "enabled") {
    $status
    Write-Error "Timeout waiting for Sitecore CM to become available via Traefik proxy. Check CM container logs."
}

try {
    # DEMO TEAM CUSTOMIZATION - Added restore command for computers without the Sitecore CLI already installed.
    Write-Host "Restoring Sitecore CLI..." -ForegroundColor Green
    dotnet tool restore
    # END CUSTOMIZATION
    # DEMO TEAM CUSTOMIZATION - Install the CLI plugins
    Write-Host "Installing Sitecore CLI Plugins..."
    dotnet sitecore --help | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Unexpected error installing Sitecore CLI Plugins"
    }
    # END CUSTOMIZATION

    # DEMO TEAM CUSTOMIZATION - Custom hostname
    dotnet sitecore login --cm https://cm.lighthouse.localhost/ --auth https://id.lighthouse.localhost/ --allow-write true
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Unable to log into Sitecore, did the Sitecore environment start correctly? See logs above."
    }

    # Populate Solr managed schemas to avoid errors during item deploy
    Write-Host "Populating Solr managed schema..." -ForegroundColor Green
    # DEMO TEAM CUSTOMIZATION - Populate Solr managed schemas using Sitecore CLI.
    dotnet sitecore index schema-populate
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Populating Solr managed schema failed, see errors above."
    }
    # END CUSTOMIZATION

    # DEMO TEAM CUSTOMIZATION - Removed initial JSS app items deployment and serialization. We are developing in Sitecore-first mode.
    # Push the serialized items
    Write-Host "Pushing items to Sitecore..." -ForegroundColor Green
    dotnet sitecore ser push
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Serialization push failed, see errors above."
    }
    # DEMO TEAM CUSTOMIZATION - Split pushing and publishing operations.
    dotnet sitecore publish
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Serialization publish failed, see errors above."
    }
    # END CUSTOMIZATION

    # DEMO TEAM CUSTOMIZATION - Rebuild indexes using Sitecore CLI.
    # Rebuild indexes
    Write-Host "Rebuilding indexes ..." -ForegroundColor Green
    dotnet sitecore index rebuild
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Rebuild indexes failed, see errors above."
    }
    # END CUSTOMIZATION
}
catch {
    Write-Error "An error occurred while attempting to log into Sitecore, populate the Solr managed schema, or pushing website items to Sitecore: $_"
}
finally {
    Pop-Location
}

Write-Host "Opening sites..." -ForegroundColor Green

Start-Process https://cm.lighthouse.localhost/sitecore
Start-Process https://cd.lighthouse.localhost
