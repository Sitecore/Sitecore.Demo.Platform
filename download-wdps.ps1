$downloadFolder = Join-Path $PSScriptRoot "download"

if (-not (Test-Path -Path  $downloadFolder -PathType Container)) {
    # create folder
    New-Item -ItemType Directory $downloadFolder
    # download assets to download folder
    az storage file download-batch --account-name dockerassets  --source docker-assets/sales-demo-wdp --destination $downloadFolder
}

Get-ChildItem $downloadFolder -Filter "*scwdp*" -Recurse | ForEach-Object {
    # copt assets to working folders
    Copy-Item $_.FullName -Destination (Join-Path $PSScriptRoot docker\images\windows\demo-xp-sqldev)
    Copy-Item $_.FullName -Destination (Join-Path $PSScriptRoot docker\images\windows\demo-xp-standalone)
}

