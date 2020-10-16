[CmdletBinding()]
Param(
  [string]$FrontEndPath = ".\Frontend",
  [string]$ItemsPath = ".\items"
)
function Update-ThemeTokens($filePath, $itemPath) {
  $blob = [Convert]::ToBase64String([IO.File]::ReadAllBytes($filePath))
  $size = (Get-Item $filePath).length
  $content = Get-Content -Path "$itemPath"
  $content = $content -replace '%blob%', $blob
  $content = $content -replace '%size%', $size
  $itemPath = "$itemPath" -replace '.template', ''
  $content | Set-Content -Path "$itemPath"
}

Get-ChildItem $FrontEndPath -Depth 0 -Directory | ForEach-Object {
  Push-Location (Join-Path $FrontEndPath $_.Name)
  npm --loglevel=error run build
  Pop-Location
  $css = [io.path]::combine($FrontEndPath, $_.Name, "styles\pre-optimized-min.css")
  $item = [io.path]::combine($ItemsPath, "Project\Global\Themes\Demo SXA Sites\", $_.Name, "styles\pre-optimized-min.yml.template")
  Update-ThemeTokens $css $item

  $js = [io.path]::combine($FrontEndPath, $_.Name, "scripts\pre-optimized-min.js")
  $item = [io.path]::combine($ItemsPath, "Project\Global\Themes\Demo SXA Sites\", $_.Name, "scripts\pre-optimized-min.yml.template")
  Update-ThemeTokens $js $item
}
