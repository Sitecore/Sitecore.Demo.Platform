[CmdletBinding()]
Param(
    [string[]]$Folders = @(".\Publish\Web", ".\Publish\xConnect", ".\Publish\Data")
)

foreach ($folder in $Folders)
{
    Get-ChildItem -Path $folder -Recurse | Remove-Item -force -recurse -Exclude .gitkeep
}

Get-ChildItem .\data -Recurse | Remove-Item -Force -Recurse -Exclude .gitkeep