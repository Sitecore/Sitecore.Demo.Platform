Param (
  [Parameter(HelpMessage = "Skips pulling the base images used by the dockerfiles.")]
  [switch]$SkipPull,

  [Parameter(HelpMessage = "Set memory limit for the build container. Passed to the docker-compose build command as --memory <Value>")]
  [string]$Memory,

  [Parameter(HelpMessage = "Set Docker Compose services to build. Passed to the docker-compose build command.")]
  [string[]]$Services
)

if ($SkipPull -eq $false) {
  # Pulling the base images as a separate step because "docker-compose build --pull" fails with the "lighthouse-solution" image which is never pushed to the Docker registry.
  .\pull-build-images.ps1
}

if ($null -eq $Services) {
  $Services = @()
}

# Build the images
if ([string]::IsNullOrEmpty($Memory)) {
  docker-compose -f docker-compose.yml -f docker-compose.build.yml -f docker-compose.build.solution.yml build $Services
}
else {
  docker-compose -f docker-compose.yml -f docker-compose.build.yml -f docker-compose.build.solution.yml build --memory $Memory $Services
}
