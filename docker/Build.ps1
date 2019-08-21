[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$Build = 3
    ,
    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$Registry = "sitecoreakshack.azurecr.io/"
    ,
    [Parameter(Mandatory = $false)]
    [ValidateScript( { Test-Path $_ -PathType "Leaf" })] 
    [string]$CmWdpPath = (Join-Path $PSScriptRoot "\*-CM-*.scwdp.zip")
    ,
    [Parameter(Mandatory = $false)]
    [ValidateScript( { Test-Path $_ -PathType "Leaf" })] 
    [string]$CdWdpPath = (Join-Path $PSScriptRoot "\*-CD-*.scwdp.zip")
)

$ErrorActionPreference = "STOP"
$ProgressPreference = "SilentlyContinue"

$tags = @(
    @{ 
        "context" = (Join-Path $PSScriptRoot "\images\windows\habitathome-xp-cd");
        "wdp"     = $CdWdpPath;
        "tag"     = "habitathome-xp-cd:9.2.0-windowsservercore-ltsc2019"; 
        "options" = @("--build-arg BASE_IMAGE=sitecore-xp-sxa-1.9.0-cd:9.2.0-windowsservercore-ltsc2019");
    }, 
    @{ 
        "context" = (Join-Path $PSScriptRoot "\images\windows\habitathome-xp-standalone");
        "wdp"     = $CmWdpPath;
        "tag"     = "habitathome-xp-standalone:9.2.0-windowsservercore-ltsc2019"; 
        "options" = @("--build-arg BASE_IMAGE=sitecore-xp-sxa-1.9.0-standalone:9.2.0-windowsservercore-ltsc2019");
    },
    @{ 
        "context" = (Join-Path $PSScriptRoot "\images\windows\habitathome-xp-sqldev");
        "wdp"     = $CmWdpPath;
        "tag"     = "habitathome-xp-sqldev:9.2.0-windowsservercore-ltsc2019"; 
        "options" = @(
            "--memory 4GB",
            "--build-arg BASE_IMAGE=sitecore-xp-sxa-1.9.0-sqldev:9.2.0-windowsservercore-ltsc2019"
        );
    }, 
    @{ 
        "context" = (Join-Path $PSScriptRoot "\images\windows\habitathome-xp-xconnect");
        "wdp"     = $CmWdpPath;
        "tag"     = "habitathome-xp-xconnect:9.2.0-windowsservercore-ltsc2019"; 
        "options" = @("--build-arg BASE_IMAGE=sitecore-xp-xconnect:9.2.0-windowsservercore-ltsc2019");
    }, 
    @{ 
        "context" = (Join-Path $PSScriptRoot "\images\windows\habitathome-xp-xconnect-indexworker");
        "wdp"     = $CmWdpPath;
        "tag"     = "habitathome-xp-xconnect-indexworker:9.2.0-windowsservercore-ltsc2019"; 
        "options" = @("--build-arg BASE_IMAGE=sitecore-xp-xconnect-indexworker:9.2.0-windowsservercore-ltsc2019");
    },
    @{ 
        "context" = (Join-Path $PSScriptRoot "\images\linux\habitathome-xp-sql");
        "wdp"     = $CmWdpPath;
        "tag"     = "habitathome-xp-sql:9.2.0-linux"; 
        "options" = @("--build-arg BASE_IMAGE=sitecore-xp-sxa-1.9.0-sql:9.2.0-linux");
    }
)

$tags | ForEach-Object {
    $context = $_.context
    $platform = (Get-Item -Path $context).Parent.Name

    if ($platform -eq "windows")
    {
        & (Join-Path $env:ProgramFiles "\Docker\Docker\DockerCli.exe") -SwitchWindowsEngine
    }
    elseif ($platform -eq "linux")
    {
        & (Join-Path $env:ProgramFiles "\Docker\Docker\DockerCli.exe") -SwitchLinuxEngine
    }
    else
    {
        throw "Unknown platform '$platform'."
    }

    $tag = $_.tag
    $remoteLatestTag = ("{0}{1}" -f $Registry, $tag)
    $remoteBuildTag = ("{0}{1}-v$Build" -f $Registry, $tag)
    $wdp = $_.wdp
    $options = New-Object System.Collections.Generic.List[System.Object]
    $options.Add("--tag '$tag'")
    $options.AddRange($_.options)
    
    Copy-Item -Path $wdp -Destination $context
    
    $command = "docker image build {0} '{1}'" -f ($options -join " "), $context
    
    Write-Verbose ("Invoking: {0} " -f $command) -Verbose
    
    & ([scriptblock]::create($command))
    
    $LASTEXITCODE -ne 0 | Where-Object { $_ } | ForEach-Object { throw ("Failed, exitcode was {0}" -f $LASTEXITCODE) }
        
    docker image tag $tag $remoteLatestTag
    docker image tag $tag $remoteBuildTag
    docker image push $remoteLatestTag
    docker image push $remoteBuildTag
}
