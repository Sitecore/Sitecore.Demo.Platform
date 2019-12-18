[CmdletBinding()]
Param(
    [string[]]$Folders = @(".\data\cm\src", ".\data\xconnect\src")
)

foreach ($folder in $Folders)
{
    Get-ChildItem -Path (Resolve-Path $folder) -Recurse | Remove-Item -force -recurse -Exclude .gitkeep
}

Get-ChildItem .\data -Recurse | Remove-Item -Force -Recurse -Exclude .gitkeep