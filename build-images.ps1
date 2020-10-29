Param (
  [Parameter(HelpMessage = "Skips pulling the base images used by the dockerfiles.")]
  [switch]$SkipPull,

  [Parameter(HelpMessage = "Set memory limit for the build container. Passed to the docker-compose build command as --memory <Value>")]
  [string]$Memory
)

if ($SkipPull -eq $false) {
  # Pulling the base images as a separate step because "docker-compose build --pull" fails with the "lighthouse-solution" image which is never pushed to the Docker registry.
  .\pull-base-images.ps1
}

# Build the images
if ([string]::IsNullOrEmpty($Memory)) {
  docker-compose build
}
else {
  docker-compose build --memory $Memory
}
