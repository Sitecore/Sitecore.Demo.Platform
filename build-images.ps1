Param (
  [Parameter(HelpMessage = "Topology. XP1 or XP0")]
  [ValidateSet("xp0", "xp1")]
  [string]$Topology = "xp0"
  ,
  [Parameter(HelpMessage = "Set Docker Compose services to build. Passed to the docker compose build command.")]
  [string[]]$Services
  ,
  [Parameter(HelpMessage = "Set whether to build images in parallel ")]
  [switch]$Parallel
  ,
  [Parameter(HelpMessage = "Skips pulling the base images used by the dockerfiles.")]
  [switch]$SkipPull
  ,
  [Parameter(HelpMessage = "Skips building the solution image.")]
  [switch]$SkipSolution
  ,
  [Parameter(HelpMessage = "Runs in the context of a CI pipeline.")]
  [switch]$CI
)

$dockerComposeBaseCommand = "docker compose"

function Invoke-BuildSolutionAssets {
  # Build the solution images

  $dockerComposeCommand = $dockerComposeBaseCommand
  $dockerComposeCommand += " -f docker-compose.build.solution.yml build"

  if ($Parallel) {
    $dockerComposeCommand += " --parallel"
  }
  Write-Host "Executing $dockerComposeCommand"

  & ([scriptblock]::create($dockerComposeCommand))

  $LASTEXITCODE -ne 0 | Where-Object { $_ } | ForEach-Object { throw "Failed." }

  $dockerComposeCommand = $dockerComposeBaseCommand
  $dockerComposeCommand += " --env-file .env"
  $dockerComposeCommand += " -f docker/docker-compose.copy.solution.yml build"

  if ($Parallel) {
    $dockerComposeCommand += " --parallel"
  }
  Write-Host "Executing $dockerComposeCommand"

  & ([scriptblock]::create($dockerComposeCommand))

  $LASTEXITCODE -ne 0 | Where-Object { $_ } | ForEach-Object { throw "Failed." }
}

if (-not $SkipPull) {
  # Pulling the base images as a separate step because "docker compose build --pull" fails with the "lighthouse-solution" image which is never pushed to the Docker registry.
  if ($CI) {
    .\pull-build-images.ps1 -Topology $Topology -CI
  } else {
    .\pull-build-images.ps1 -Topology $Topology
  }
}

if ($null -eq $Services) {
  $Services = @()
}
if (-not $SkipSolution) {
  Invoke-BuildSolutionAssets
}

$fileSuffix = $(if ("$Topology" -eq "xp0") { "" } else { "-xp1" })

# Build the service images
$dockerComposeCommand = $dockerComposeBaseCommand
$dockerComposeCommand += " -f docker-compose$($fileSuffix).yml -f docker-compose$fileSuffix.build.yml build"

if ($Parallel) {
  $dockerComposeCommand += " --parallel"
}

$dockerComposeCommand += " $Services"

Write-Host "Executing $dockerComposeCommand"

& ([scriptblock]::create($dockerComposeCommand))
