[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$Build = 1
    ,
    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$Registry = "sitecoreakshack.azurecr.io/"
)

$ErrorActionPreference = "STOP"
$ProgressPreference = "SilentlyContinue"

$namespace = "sitecore-habitathome-platform"

kubectl set image deployment/cm cm=$registry`habitathome-xp-standalone:9.2.0-windowsservercore-ltsc2019-v$build --namespace $namespace
kubectl set image deployment/cd cd=$registry`habitathome-xp-cd:9.2.0-windowsservercore-ltsc2019-v$build --namespace $namespace
kubectl set image deployment/sql sql=$registry`habitathome-xp-sqldev:9.2.0-windowsservercore-ltsc2019-v$build --namespace $namespace
kubectl set image deployment/xconnect xconnect=$registry`habitathome-xp-xconnect:9.2.0-windowsservercore-ltsc2019-v$build --namespace $namespace
kubectl set image deployment/indexworker indexworker=$registry`habitathome-xp-xconnect-indexworker:9.2.0-windowsservercore-ltsc2019-v$build --namespace $namespace