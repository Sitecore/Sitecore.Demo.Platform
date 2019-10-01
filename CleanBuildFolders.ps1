[CmdletBinding()]
Param(
    [string[]]$Folders = @(".\Publish\Web", ".\Publish\xConnect")
)

foreach ($folder in $Folders)
{
    Get-ChildItem -Path $folder -Recurse | Remove-Item -force -recurse
}