[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$Build = 3
    ,
    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$Registry = "sitecoredemocontainers.azurecr.io/"
    ,
    [Parameter(Mandatory = $false)]
    [ValidateScript( { Test-Path $_ -PathType "Leaf" })]
    [string]$CmPublishPath = ( "..\Publish\Web")
    ,
    [Parameter(Mandatory = $false)]
    [ValidateScript( { Test-Path $_ -PathType "Leaf" })]
    [string]$CdPublishPath = ( "..\Publish\Web")
    ,
    [Parameter(Mandatory = $false)]
    [ValidateScript( { Test-Path $_ -PathType "Leaf" })]
    [string]$XConnectPath = ("..\Publish\xConnect")
    ,
    [Parameter(Mandatory = $false)]
    [ValidateScript( { Test-Path $_ -PathType "Leaf" })]
    [string]$DacpacPath = ("..\Publish\Data")
    ,
    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$BuildContext = "windows"
)
$ErrorActionPreference = "STOP"
$ProgressPreference = "SilentlyContinue"

$tags = @(
    # @{
    #   "context" = (Join-Path $PSScriptRoot "\images\windows\habitathome-xp-cd");
    #   "assets"     = $CdPublishPath;
    #   "tag"     = "habitathome-xp-cd:9.2.0-windowsservercore-1903";
    #   "options" = @("--build-arg BASE_IMAGE=$Registry`sitecore-xp-sxa-cd:9.2.0-windowsservercore-1903");
    # },
    @{
      "context" = (Join-Path $PSScriptRoot "\images\windows\habitathome-xp-standalone");
      "assets"     = $CmPublishPath;
      "tag"     = "habitathome-xp-standalone:9.2.0-windowsservercore-1903";
      "options" = @("--build-arg BASE_IMAGE=$Registry`sitecore-xp-sxa-standalone:9.2.0-windowsservercore-1903");
    },
    @{
      "context" = (Join-Path $PSScriptRoot "\images\windows\habitathome-xp-sqldev");
      "assets"     = $DacpacPath;
      "tag"     = "habitathome-xp-sqldev:9.2.0-windowsservercore-1903";
      "options" = @(
          "--memory 4GB",
          "--build-arg BASE_IMAGE=$Registry`sitecore-xp-sxa-sqldev:9.2.0-windowsservercore-1903"
      );
    },
    @{
      "context" = (Join-Path $PSScriptRoot "\images\windows\habitathome-xp-xconnect");
      "assets"     = $XConnectPath;
      "tag"     = "habitathome-xp-xconnect:9.2.0-windowsservercore-1903";
      "options" = @("--build-arg BASE_IMAGE=$Registry`sitecore-xp-xconnect:9.2.0-windowsservercore-1903");
    },
    @{
      "context" = (Join-Path $PSScriptRoot "\images\windows\habitathome-xp-xconnect-indexworker");
      "assets"     = $XConnectPath;
      "tag"     = "habitathome-xp-xconnect-indexworker:9.2.0-windowsservercore-1903";
      "options" = @("--build-arg BASE_IMAGE=$Registry`sitecore-xp-xconnect-indexworker:9.2.0-windowsservercore-1903");
    },
    @{
      "context" = (Join-Path $PSScriptRoot "\images\linux\habitathome-xp-sql");
      "assets"     = $DacpacPath;
      "tag"     = "habitathome-xp-sql:9.2.0-linux";
      "options" = @("--build-arg BASE_IMAGE=$Registry`sitecore-xp-sxa-sql:9.2.0-linux");
    }
)

$tags | Where-Object { $_.tag -like "*$BuildContext*" } | ForEach-Object {
  $context = (Get-Item $_.context).FullName
  $tag = $_.tag

  $remoteLatestTag = ("{0}{1}" -f $Registry, $tag)
  $remoteBuildTag = ("{0}{1}-v$Build" -f $Registry, $tag)
  $assets = $_.assets
  $options = New-Object System.Collections.Generic.List[System.Object]
  $options.Add("--tag '$tag'")
  $options.AddRange($_.options)

  Remove-Item -Path (Join-Path $context "temp") -Recurse -ErrorAction SilentlyContinue
  $source = Resolve-Path $assets
  Copy-Item -Path $source -Destination (Join-Path $context "temp") -Recurse -Force

  $dockerfile = "Dockerfile"

  if (-not ([string]::IsNullOrEmpty($_.dockerfile)))
  {
      $dockerfile = $_.dockerfile
  }

  if ("windows" -eq "$BuildContext" )
  {
      $options.Add("--isolation hyperv")
  }

  $command = "docker image build {0} -f {1} '{2}'" -f ($options -join " "), (Join-Path $context $dockerfile), $context

  Write-Verbose ("Invoking: {0} " -f $command) -Verbose

  & ([scriptblock]::create($command))

  $LASTEXITCODE -ne 0 | Where-Object { $_ } | ForEach-Object { throw ("Failed, exitcode was {0}" -f $LASTEXITCODE) }

  docker image tag $tag $remoteLatestTag
  docker image tag $tag $remoteBuildTag
  docker image push $remoteLatestTag
  docker image push $remoteBuildTag
}
