[CmdletBinding()]
Param(
    [string[]]$Folders = @(
        ".\data\indexworker\src",
        ".\data\sql",
        ".\data\solr",
        ".\data\cm\src",
        ".\data\cm\backup",
        ".\data\cd\src",
        ".\data\cd\backup",
        ".\data\xconnect\src"
    )
)

foreach ($folder in $Folders)
{
    Get-ChildItem -Path (Resolve-Path $folder) -Recurse | Remove-Item -force -recurse -Exclude ".gitignore", ".gitkeep", "*.pem", "certs_config.yaml", "readme.md"
}

Get-ChildItem .\data -Recurse | Remove-Item -Force -Recurse -Exclude ".gitignore", ".gitkeep", "*.pem", "certs_config.yaml", "readme.md"