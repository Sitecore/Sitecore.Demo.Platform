Param (
  [Parameter(
    HelpMessage = "Topology. XP1 or XP0")]
    [ValidateSet("xp0", "xp1")]

  [string]$Topology = "xp0"
)

Write-Host "Pulling base images..." -ForegroundColor Green

if ($Topology -eq "xp0"){
  $configuration = docker-compose -f docker-compose.yml -f docker-compose.build.yml -f docker-compose.build.solution.yml config
}
elseif ($Topology -eq "xp1") {
  $configuration = docker-compose -f docker-compose-xp1.yml -f docker-compose-xp1.build.yml -f docker-compose.build.solution.yml config
}
$images = @()

# Find images to pull in the docker-compose configuration
foreach ($line in $configuration) {
  if ($line -match "(BUILD_IMAGE|BASE_IMAGE|ASSETS_IMAGE|HOTFIX_IMAGE|COVEO_ASSETS_IMAGE):\s*([^\s]*)") {
    $images += $Matches.2
  }
}

# Pull images
$images | Select-Object -Unique | ForEach-Object {
  $tag = $_
  Write-Host "Starting to pull $tag"
  docker image pull $tag
  $LASTEXITCODE -ne 0 | Where-Object { $_ } | ForEach-Object { throw "Failed." }
  Write-Host ("External image '{0}' is latest." -f $tag) -ForegroundColor Green
}

Write-Host "External images are up to date." -ForegroundColor Green
