Function Get-VariableValue {
  param (
    [Parameter(Mandatory)]
    $List,
    [Parameter(Mandatory)]
    $VariableName
  )

  foreach ($keypair in $List)
  {
    if ($keypair.Name -eq $VariableName) {
      return $($keypair.Value)
    }
  }
}

Write-Host "Pulling build libraries..." -ForegroundColor Green

$envFile = import-csv .\.env -Delimiter '=' -Header Name,Value
$registry = Get-VariableValue -List $envFile -VariableName "REGISTRY"
$coveoVersion = Get-VariableValue -List $envFile -VariableName "COVEO_VERSION"
$sitecoreVersion = Get-VariableValue -List $envFile -VariableName "SITECORE_VERSION"
$nanoserverVersion = Get-VariableValue -List $envFile -VariableName "NANOSERVER_VERSION"

$imageName = "${registry}community/modules/custom-coveo${coveoVersion}-assets:${sitecoreVersion}-${nanoserverVersion}"
$containerName = "coveo_assets"
$sourceFolder = "/module/cm/content/bin"
$destination = "./lib"
$isSuccess = $true

try {
  docker pull $imageName
  $LASTEXITCODE -ne 0 | Where-Object { $_ } | ForEach-Object { throw "Failed." }

  docker create -it --name $containerName $imageName
  $LASTEXITCODE -ne 0 | Where-Object { $_ } | ForEach-Object { throw "Failed." }

  docker cp ${containerName}:$sourceFolder/Coveo.AbstractLayer.dll $destination
  $LASTEXITCODE -ne 0 | Where-Object { $_ } | ForEach-Object { throw "Failed." }

  docker cp ${containerName}:$sourceFolder/Coveo.Framework.dll $destination
  $LASTEXITCODE -ne 0 | Where-Object { $_ } | ForEach-Object { throw "Failed." }

  docker cp ${containerName}:$sourceFolder/Coveo.SearchProvider.Rest.dll $destination
  $LASTEXITCODE -ne 0 | Where-Object { $_ } | ForEach-Object { throw "Failed." }

  docker cp ${containerName}:$sourceFolder/Coveo.SearchProviderBase.dll $destination
  $LASTEXITCODE -ne 0 | Where-Object { $_ } | ForEach-Object { throw "Failed." }
}
catch
{
  $isSuccess = $false
  Write-Error "An error occurred while pulling build librairies."
  Write-Error $_
}
finally
{
  docker rm -f $containerName
}

if ($isSuccess) {
  Write-Host "Build librairies are up to date." -ForegroundColor Green
}
