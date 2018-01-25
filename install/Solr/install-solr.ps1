Param(
    [string]$solrVersion = "6.6.2",
    [string]$installFolder = "c:\solr",
    [string]$solrPort = "8983",
    [string]$solrHost = "localhost",
    [bool]$solrSSL = $TRUE,
    [string]$nssmVersion = "2.24",
    [string]$JREVersion = "1.8.0_151",
	[string]$keystoreSecret = "secret",
	[string]$KeystoreFile = 'solr-ssl.keystore.jks',
	[string]$SolrDomain = 'localhost',
	[switch]$Clobber
)

$JREPath = "C:\Program Files\Java\jre$JREVersion"

$solrName = "solr-$solrVersion"
$solrRoot = "$installFolder\$solrName"
$nssmRoot = "$installFolder\nssm-$nssmVersion"
$solrPackage = "http://archive.apache.org/dist/lucene/solr/$solrVersion/$solrName.zip"
$nssmPackage = "http://nssm.cc/release/nssm-$nssmVersion.zip"
$downloadFolder = "C:\Projects\Sitecore.Habitat\install\assets"

## Verify elevated
## https://superuser.com/questions/749243/detect-if-powershell-is-running-as-administrator
$elevated = [bool](([System.Security.Principal.WindowsIdentity]::GetCurrent()).groups -match "S-1-5-32-544")
if(!($elevated))
{
    throw "In order to install services, please run this script elevated."
}

function downloadAndUnzipIfRequired
{
    Param(
        [string]$toolName,
        [string]$toolFolder,
        [string]$toolZip,
        [string]$toolSourceFile,
        [string]$installRoot
    )

    if(!(Test-Path -Path $toolFolder))
    {
        if(!(Test-Path -Path $toolZip))
        {
            Write-Host "Downloading $toolName..."
            Start-BitsTransfer -Source $toolSourceFile -Destination $toolZip
        }

        Write-Host "Extracting $toolName to $toolFolder..."
        Expand-Archive $toolZip -DestinationPath $installRoot
    }
}

# download & extract the solr archive to the right folder
$solrZip = "$downloadFolder\$solrName.zip"
downloadAndUnzipIfRequired "Solr" $solrRoot $solrZip $solrPackage $installFolder

# download & extract the nssm archive to the right folder
$nssmZip = "$downloadFolder\nssm-$nssmVersion.zip"
downloadAndUnzipIfRequired "NSSM" $nssmRoot $nssmZip $nssmPackage $installFolder

# Ensure Java environment variable
$jreVal = [Environment]::GetEnvironmentVariable("JAVA_HOME", [EnvironmentVariableTarget]::Machine)
if($jreVal -ne $JREPath)
{
    Write-Host "Setting JAVA_HOME environment variable"
    [Environment]::SetEnvironmentVariable("JAVA_HOME", $JREPath, [EnvironmentVariableTarget]::Machine)
}


$ErrorActionPreference = 'Stop'

### PARAM VALIDATION
if($keystoreSecret -ne 'secret') {
	Write-Error 'The keystore password must be "secret", because Solr apparently ignores the parameter'
}

if((Test-Path $KeystoreFile)) {
	if($Clobber) {
		Write-Host "Removing $KeystoreFile..."
		Remove-Item $KeystoreFile
	} else {
		$KeystorePath = Resolve-Path $KeystoreFile
		Write-Error "Keystore file $KeystorePath already existed. To regenerate it, pass -Clobber."
	}
}

$P12Path = [IO.Path]::ChangeExtension($KeystoreFile, 'p12')
if((Test-Path $P12Path)) {
	if($Clobber) {
		Write-Host "Removing $P12Path..."
		Remove-Item $P12Path
	} else {
		$P12Path = Resolve-Path $P12Path
		Write-Error "Keystore file $P12Path already existed. To regenerate it, pass -Clobber."
	}
}

try {
	$keytool = (Get-Command 'keytool.exe').Source
} catch {
	try {
		$path = $Env:JAVA_HOME + '\bin\keytool.exe'
		Write-Host $path
		if (Test-Path $path) {
			$keytool = (Get-Command $path).Source
		}
	} catch {
		$keytool = Read-Host "keytool.exe not on path. Enter path to keytool (found in JRE bin folder)"

		if([string]::IsNullOrEmpty($keytool) -or -not (Test-Path $keytool)) {
			Write-Error "Keytool path was invalid."
		}
	}
}

### DOING STUFF

Write-Host ''
Write-Host 'Generating JKS keystore...'
& $keytool -genkeypair -alias solr-ssl -keyalg RSA -keysize 2048 -keypass $keystoreSecret -storepass $keystoreSecret -validity 9999 -keystore $KeystoreFile -ext SAN=DNS:$SolrDomain,IP:127.0.0.1 -dname "CN=$SolrDomain, OU=Organizational Unit, O=Organization, L=Location, ST=State, C=Country"

Write-Host ''
Write-Host 'Generating .p12 to import to Windows...'
& $keytool -importkeystore -srckeystore $KeystoreFile -destkeystore $P12Path -srcstoretype jks -deststoretype pkcs12 -srcstorepass $keystoreSecret -deststorepass $keystoreSecret

Write-Host ''
Write-Host 'Trusting generated SSL certificate...'
$secureStringKeystorePassword = ConvertTo-SecureString -String $keystoreSecret -Force -AsPlainText
$root = Import-PfxCertificate -FilePath $P12Path -Password $secureStringKeystorePassword -CertStoreLocation Cert:\LocalMachine\Root
Write-Host 'SSL certificate is now locally trusted. (added as root CA)'

if(-not $KeystoreFile.EndsWith('solr-ssl.keystore.jks')) {
	Write-Warning 'Your keystore file is not named "solr-ssl.keystore.jks"'
	Write-Warning 'Solr requires this exact name, so make sure to rename it before use.'
}

$KeystorePath = Resolve-Path $KeystoreFile
Copy-Item $KeystorePath -Destination "$solrRoot\server\etc\solr-ssl.keystore.jks" -Force
 # Update solr cfg to use keystore & right host name
 if(Test-Path -Path "$solrRoot\bin\solr.in.cmd.old")
 {
		 Write-Host "Resetting solr.in.cmd" -ForegroundColor Green
		 Remove-Item "$solrRoot\bin\solr.in.cmd"
		 Rename-Item -Path "$solrRoot\bin\solr.in.cmd.old" -NewName "$solrRoot\bin\solr.in.cmd"   
 }

	 Write-Host "Rewriting solr config"

	 $cfg = Get-Content "$solrRoot\bin\solr.in.cmd"
	 Rename-Item "$solrRoot\bin\solr.in.cmd" "$solrRoot\bin\solr.in.cmd.old"
	 $certStorePath = "etc/solr-ssl.keystore.jks"
	 $newCfg = $cfg | ForEach-Object { $_ -replace "REM set SOLR_SSL_KEY_STORE=etc/solr-ssl.keystore.jks", "set SOLR_SSL_KEY_STORE=$certStorePath" }
	 $newCfg = $newCfg | ForEach-Object { $_ -replace "REM set SOLR_SSL_KEY_STORE_PASSWORD=secret", "set SOLR_SSL_KEY_STORE_PASSWORD=$keystoreSecret" }
	 $newCfg = $newCfg | ForEach-Object { $_ -replace "REM set SOLR_SSL_TRUST_STORE=etc/solr-ssl.keystore.jks", "set SOLR_SSL_TRUST_STORE=$certStorePath" }
	 $newCfg = $newCfg | ForEach-Object { $_ -replace "REM set SOLR_SSL_TRUST_STORE_PASSWORD=secret", "set SOLR_SSL_TRUST_STORE_PASSWORD=$keystoreSecret" }
	 $newCfg = $newCfg | ForEach-Object { $_ -replace "REM set SOLR_HOST=192.168.1.1", "set SOLR_HOST=$solrHost" }
	 $newCfg | Set-Content "$solrRoot\bin\solr.in.cmd"

# install the service & runs
$svc = Get-Service "$solrName" -ErrorAction SilentlyContinue
if(!($svc))
{
    Write-Host "Installing Solr service"
    &"$installFolder\nssm-$nssmVersion\win64\nssm.exe" install "$solrName" "$solrRoot\bin\solr.cmd" "-f" "-p $solrPort"
    $svc = Get-Service "$solrName" -ErrorAction SilentlyContinue
}

if($svc.Status -ne "Running")
{
	Write-Host "Starting Solr service..."
	Start-Service "$solrName"
}
elseif ($svc.Status -eq "Running")
{
	Write-Host "Restarting Solr service..."
	Restart-Service "$solrName"
}

        
Start-Sleep -s 5

# finally prove it's all working
$protocol = "http"
if($solrSSL)
{
    $protocol = "https"
}

Invoke-Expression "start $($protocol)://$($solrHost):$solrPort/solr/#/"

Write-Host ''
Write-Host 'Done!' -ForegroundColor Green