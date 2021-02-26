[CmdletBinding()]
param (
	[Parameter(Mandatory)]
	[ValidateScript( { Test-Path $_ -PathType Container })]
	[string]$ResourcesDirectory,

	[string]$SqlServer = "(local)",

	[string]$SqlAdminUser,

	[string]$SqlAdminPassword,

	[string]$DatabaseOwner,

	[switch]$EnableContainedDatabaseAuth,

	[switch]$SkipStartingServer,

	[string]$SqlElasticPoolName,

	[string]$InstallModulePath
)

function Invoke-Sqlcmd {
	param(
		[string]$SqlDatabase,
		[string]$SqlServer,
		[string]$SqlAdminUser,
		[string]$SqlAdminPassword,
		[string]$Query
	)

	$arguments = " -Q ""$Query"" -S '$SqlServer' "

	if ($SqlAdminUser -and $SqlAdminPassword) {
		$arguments += " -U '$SqlAdminUser' -P '$SqlAdminPassword'"
	}
	if ($SqlDatabase) {
		$arguments += " -d '$SqlDatabase'"
	}

	Invoke-Expression "sqlcmd $arguments"
}

function Add-SqlAzureConditionWrapper {
	param(
		[string]$SqlQuery
	)

	return "DECLARE @serverEdition nvarchar(256) = CONVERT(nvarchar(256),SERVERPROPERTY('edition'));
		IF @serverEdition <> 'SQL Azure'
		BEGIN
			$SqlQuery
		END;
		GO"
}

function New-DatabaseElasticPool {
	param(
		[string]$SqlElasticPoolName,
		[string]$SqlServer,
		[string]$SqlAdminUser,
		[string]$SqlAdminPassword,
		[string]$SqlDatabase
	)
	Write-Host "Start creating database '$SqlDatabase' with azure elastic pool '$SqlElasticPoolName'"
	$Query = "CREATE DATABASE [$SqlDatabase]
	( SERVICE_OBJECTIVE = ELASTIC_POOL ( name = [$SqlElasticPoolName] ))"

	& sqlcmd -Q $Query -S "$SqlServer" -U "$SqlAdminUser" -P "$SqlAdminPassword"

	if ($LASTEXITCODE -ne 0) {
		throw "sqlcmd exited with code $LASTEXITCODE while creating database $databaseName with elasic pool"
	}
}

if (-not $SkipStartingServer) {
	Start-Service MSSQLSERVER;
}

Write-Host "Start publishing dacpacs"
Write-Verbose  "sql server $SqlServer"
Write-Verbose  "sql SqlAdminUser $SqlAdminUser"
Write-Host  "sql SqlAdminPassword $SqlAdminPassword"

if ($EnableContainedDatabaseAuth) {
	$sqlcmd = Add-SqlAzureConditionWrapper -SqlQuery "DECLARE @containedAuthenticationEnabled int = CONVERT(int, (SELECT [value] FROM sys.configurations WHERE name = 'contained database authentication'));
	IF @containedAuthenticationEnabled = 0
	BEGIN
		EXEC sys.sp_configure N'contained database authentication', N'1'
		exec ('RECONFIGURE WITH OVERRIDE')
	END"

	Invoke-Sqlcmd -SqlServer:$SqlServer -SqlAdminUser:$SqlAdminUser -SqlAdminPassword:$SqlAdminPassword -Query $sqlcmd
	if ($LASTEXITCODE -ne 0) {
		throw "sqlcmd exited with code $LASTEXITCODE"
	}
	Write-Verbose "Enabled contained databases"
}

$sqlPackageExePath = "C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe"
$resourcesDirectories = @($ResourcesDirectory)
$resourcesDirectories += Get-ChildItem $ResourcesDirectory -Directory | Select-Object -ExpandProperty FullName
$resourcesDirectories | ForEach-Object {
	Get-ChildItem $_ -Filter *.dacpac | ForEach-Object {
		$dacpac = $_
		$sourceFile = $dacpac.FullName
		$databaseName = $dacpac.BaseName
		Write-Host "Publishing $($dacpac.FullName)..."
		if ($databaseName -like "*Azure" -or $databaseName -eq "Sitecore.Xdb.Collection.Database.Sql" `
				-or $databaseName -like "Sitecore.Xdb.Collection.Shard*") {
			Write-Verbose "Skip $($databaseName) database"
			return
		}
		if ($databaseName -eq "Connect.master" -or $databaseName -eq "XOptimization.master") {
			Write-Verbose "---------------------------------------------------"
			Write-Verbose "publishing to master database"
			$databaseName = "Sitecore.master"
			return
		}

		if ($SqlElasticPoolName) {
			New-DatabaseElasticPool -SqlElasticPoolName $SqlElasticPoolName -SqlServer $SqlServer -SqlAdminUser $SqlAdminUser -SqlAdminPassword $SqlAdminPassword -SqlDatabase $databaseName
		}

		if ($SqlAdminUser -and $SqlAdminPassword) {
			& $sqlPackageExePath /a:Publish /sf:$sourceFile /tsn:"$SqlServer" /tdn:$databaseName /tu:"$SqlAdminUser" /tp:"$SqlAdminPassword" /p:AllowIncompatiblePlatform=True

		}
		else {
			& $sqlPackageExePath /a:Publish /sf:$sourceFile /tsn:"$SqlServer" /tdn:$databaseName /p:AllowIncompatiblePlatform=True
		}

		if ($LASTEXITCODE -ne 0) {
			throw "sqlpackage exited with code $LASTEXITCODE while deploying $($dacpac.FullName)"
		}

		Write-Information -Message "Deployed $($databaseName) database" -InformationAction Continue

		if (![string]::IsNullOrEmpty($DatabaseOwner)) {
			$sqlcmd = Add-SqlAzureConditionWrapper -SqlQuery "EXEC sp_changedbowner '$DatabaseOwner'"

			Invoke-Sqlcmd -SqlServer:$SqlServer -SqlAdminUser:$SqlAdminUser -SqlAdminPassword:$SqlAdminPassword -SqlDatabase "$databaseName" -Query $sqlcmd

			if ($LASTEXITCODE -ne 0) {
				throw "sqlcmd exited with code $LASTEXITCODE while changing owner of $databaseName"
			}
			Write-Verbose "$($databaseName) database owner is changed to $DatabaseOwner"
		}
	}
}

$modulePath = $InstallModulePath
$moduleDirectories = @($modulePath)

$moduleDirectories += Get-ChildItem $modulePath -Directory | Select-Object -ExpandProperty FullName
$DatabasePrefix = "Sitecore"

$moduleDirectories | ForEach-Object {
	Get-ChildItem $_ -Filter *.dacpac | ForEach-Object {
		$dacpac = $_
		$sourceFile = $dacpac.FullName
		$databaseName = $dacpac.BaseName
		if (-not $databaseName -like "Sitecore*") {
			$databaseName = "$DatabasePrefix`." + $databaseName
		}
		$databaseName = $databaseName -replace "security", "core"
		Write-Host "Publishing $($dacpac.FullName)..."
		& $sqlPackageExePath /a:Publish /sf:$sourceFile /tdn:$databaseName /tsn:$SqlServer /tu:"$SqlAdminUser" /tp:"$SqlAdminPassword" /p:AllowIncompatiblePlatform=True

	}
}
#& $sqlPackageExePath /a:Publish /sf:"C:\resources\sxa\Sitecore.master.dacpac" /tdn:"Sitecore.Web" /tsn:$SqlServer /tu:"$SqlAdminUser" /tp:"$SqlAdminPassword" /p:AllowIncompatiblePlatform=True

# # do modules
# $TextInfo = (Get-Culture).TextInfo
# Copy-Item -Path ($modulePath + "data\Sitecore.Master.dacpac") -Destination ($modulePath + "web.dacpac")
# Copy-Item -Path ($modulePath + "data\Sitecore.Master.dacpac") -Destination ($modulePath + "master.dacpac")
# Copy-Item -Path ($modulePath + "data\Sitecore.Core.dacpac") -Destination ($modulePath + "core.dacpac")
# Copy-Item -Path ($modulePath + "security\Sitecore.Core.dacpac") -Destination ($modulePath + "security.dacpac")
# $items = Get-ChildItem -Path $modulePath -Include "data\Sitecore.Core.dacpac", "data\Sitecore.Master.dacpac", "security.dacpac", "data\Sitecore.Web.dacpac", "z.descendants.dacpac"
# Write-Host $items

# Get-ChildItem -Path $modulePath  -Recurse | ForEach-Object {
# 	Write-Host "deploying lighthouse"
# 	$dacpacPath = $_.FullName
# 	$databaseName = "$DatabasePrefix`." + $TextInfo.ToTitleCase($_.BaseName)

# 	# HACK: Apply Roles & Users to the Core database
# 	if ($_.BaseName -eq "security")
# 	{
# 		$databaseName = "$DatabasePrefix`." + "core"
# 	}

# 	if ($_.BaseName -eq "z.descendants")
# 	{
# 		$databaseName = "$DatabasePrefix`." + "Master"
# 		Write-Host "Installing $dacpacPath on $databaseName"
# 		Write-Host "test $sqlPackageExePath"
# 		& $sqlPackageExePath /a:Publish /sf:$dacpacPath /tdn:$databaseName /tsn:$SqlServer /tu:"$SqlAdminUser" /tp:"$SqlAdminPassword" /p:AllowIncompatiblePlatform=True
# 	}

# 	if ($_.BaseName -eq "z.descendants")
# 	{
# 		$databaseName = "$DatabasePrefix`." + "Web"
# 	}

# 	# Install
# 	Write-Host "Installing $dacpacPath on $databaseName"
# 	Write-Host "test $sqlPackageExePath"
# 	& $sqlPackageExePath /a:Publish /sf:$dacpacPath /tsn:$SqlServer /tdn:$databaseName /tu:"$SqlAdminUser" /tp:"$SqlAdminPassword" /p:AllowIncompatiblePlatform=True
# }

if (-not $SkipStartingServer) {
	Stop-Service MSSQLSERVER;
}