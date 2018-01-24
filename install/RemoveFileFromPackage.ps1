Param(
    [string]$UpdatePackage,
    [string]$FilesToRemove
)
[Reflection.Assembly]::LoadWithPartialName('System.IO.Compression')
$package = Get-Item $UpdatePackage
$path = Split-Path -Path $package
$updateFileName = Split-Path $package -Leaf
$zipFileName = $updateFileName.Replace("update", "zip")

Get-Item $UpdatePackage | Rename-Item -NewName {[System.IO.Path]::ChangeExtension($_.Name,".zip")} | Out-String

$archive = Get-Item $(Join-Path "$path"  "$($zipFileName)")

Expand-Archive -Path $archive  -DestinationPath . -Force
Remove-Item $archive

try {
    
    $packagePath = Join-Path $path "package.zip"
    $mode = [IO.Compression.ZipArchiveMode]::Update
    $packageStream = New-Object IO.FileStream($packagePath, [IO.FileMode]::Open)
    $zip = New-Object IO.Compression.ZipArchive($packageStream, $mode)

    ($zip.Entries | Where-Object { $FilesToRemove -contains $_.Name }) | ForEach-Object { $_.Delete() }
    
    $zip.Dispose()

    $packageStream.Close()
    $packageStream.Dispose()
    $zipFileFullName = $(Join-Path $path $zipFileName)
    Compress-Archive -Path $packagePath -DestinationPath $zipFileFullName -Force
    Get-Item $zipFileFullName | Rename-Item -NewName {[System.IO.Path]::ChangeExtension($_.Name,".update")}
}
catch {
    Write-Host $_.Exception.Message
}
finally {

    $zip.Dispose()

    $packageStream.Close()

    $packageStream.Dispose()
}