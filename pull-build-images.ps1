Param (
  [Parameter(
    HelpMessage = "Topology. XP1 or XP0")]
    [ValidateSet("xp0", "xp1")]
  [string]$Topology = "xp0"
  ,
  [Parameter(HelpMessage = "Runs in the context of a CI pipeline.")]
  [switch]$CI
)

Write-Host "Pulling base images..." -ForegroundColor Green

$dockerComposeBaseCommand = "docker compose"
if ($CI) {
  $dockerComposeBaseCommand = "docker-compose"
}

$dockerComposeCommand = $dockerComposeBaseCommand

if ($Topology -eq "xp0"){
  $dockerComposeCommand += " -f docker-compose.yml -f docker-compose.build.yml -f docker-compose.build.solution.yml config"
}
elseif ($Topology -eq "xp1") {
  $dockerComposeCommand += " -f docker-compose-xp1.yml -f docker-compose-xp1.build.yml -f docker-compose.build.solution.yml config"
}
$configuration = & ([scriptblock]::create($dockerComposeCommand))
$images = @()

# Find images to pull in the Docker compose configuration
foreach ($line in $configuration) {
  if ($line -match "(BUILD_IMAGE|BASE_IMAGE|ASSETS_IMAGE|.*_ASSETS|HOTFIX_IMAGE):\s*([^\s]*)") {
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
