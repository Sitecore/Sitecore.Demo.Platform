Param(
  [Parameter(Mandatory)]
  [string]$SqlServer,

  [Parameter(Mandatory)]
  [string]$SqlAdminUser,

  [Parameter(Mandatory)]
  [string]$SqlAdminPassword,

  [Parameter(Mandatory)]
  [string] $SitecoreUserPassword
)

$userinfo = ./HashPassword.ps1 $SitecoreUserPassword
$passwordParam = ("EncodedPassword='" + $userinfo.Password + "'")
$saltParam = ("EncodedSalt='" + $userinfo.Salt + "'")
$paramsUser = $passwordParam, $saltParam

Invoke-Sqlcmd -ServerInstance $SqlServer -Username $SqlAdminUser -Password $SqlAdminPassword -InputFile "C:\sql\ResetDemoUsers.sql" -Variable $paramsUser
Write-Host "$(Get-Date -Format $timeFormat): Invoke ResetDemoUsers.sql"