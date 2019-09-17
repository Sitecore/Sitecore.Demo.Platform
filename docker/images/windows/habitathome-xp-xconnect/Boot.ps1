[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateScript( { Test-Path $_ -PathType "Leaf" })]
    [string]$ConnectionStringsConfigPath
    ,
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$EntryPoint
)

if ([string]::IsNullOrEmpty($env:SITECORE_SQL_USERNAME))
{
    throw "`$env:SITECORE_SQL_USERNAME was null or empty."
}

if ([string]::IsNullOrEmpty($env:SITECORE_SQL_PASSWORD))
{
    throw "`$env:SITECORE_SQL_PASSWORD was null or empty."
}

Write-Host ("### Updating '{0}'..." -f $ConnectionStringsConfigPath)

$fileContent = Get-Content -Path $ConnectionStringsConfigPath | Out-String
$fileContent = ($fileContent -replace "user id=sa", ("User ID={0}" -f $env:SITECORE_SQL_USERNAME))
$fileContent = ($fileContent -replace "password=HASH-epsom-sunset-cost7!", ("Password={0}" -f $env:SITECORE_SQL_PASSWORD))
$fileContent | Out-File -FilePath $ConnectionStringsConfigPath -Encoding utf8

Write-Host "### Ready."

& ([scriptblock]::create($EntryPoint))