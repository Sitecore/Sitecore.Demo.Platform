$ErrorActionPreference = 'Stop'
$ScriptPath = Split-Path $MyInvocation.MyCommand.Path
$MicroCHAP = $ScriptPath + '\MicroCHAP.dll'
Add-Type -Path $MicroCHAP

Function Sync-Unicorn {
	Param(
		[Parameter(Mandatory=$True)]
		[string]$ControlPanelUrl,

		[Parameter(Mandatory=$True)]
		[string]$SharedSecret,

		[string[]]$Configurations,

		[string]$Verb = 'Sync',

		[switch]$SkipTransparentConfigs,

		[switch]$DebugSecurity
	)

	# PARSE THE URL TO REQUEST
	$parsedConfigurations = '' # blank/default = all
	
	if($Configurations) {
		$parsedConfigurations = ($Configurations) -join "^"
	}

	$skipValue = 0
	if($SkipTransparentConfigs) {
		$skipValue = 1
	}

	$url = "{0}?verb={1}&configuration={2}&skipTransparentConfigs={3}" -f $ControlPanelUrl, $Verb, $parsedConfigurations, $skipValue 

	if($DebugSecurity) {
		Write-Host "Sync-Unicorn: Preparing authorization for $url"
	}

	# GET AN AUTH CHALLENGE
	$challenge = Get-Challenge -ControlPanelUrl $ControlPanelUrl

	if($DebugSecurity) {
		Write-Host "Sync-Unicorn: Received challenge from remote server: $challenge"
	}

	# CREATE A SIGNATURE WITH THE SHARED SECRET AND CHALLENGE
	$signatureService = New-Object MicroCHAP.SignatureService -ArgumentList $SharedSecret

	$signature = $signatureService.CreateSignature($challenge, $url, $null)

	if($DebugSecurity) {
		Write-Host "Sync-Unicorn: MAC '$($signature.SignatureSource)'"
		Write-Host "Sync-Unicorn: HMAC '$($signature.SignatureHash)'"
		Write-Host "Sync-Unicorn: If you get authorization failures compare the values above to the Sitecore logs."
	}

	Write-Host "Sync-Unicorn: Executing $Verb..."

	# USING THE SIGNATURE, EXECUTE UNICORN
	[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
	$result = Invoke-StreamingWebRequest -Uri $url -Mac $signature.SignatureHash -Nonce $challenge

	if($result.TrimEnd().EndsWith('****ERROR OCCURRED****')) {
		throw "Unicorn $Verb to $url returned an error. See the preceding log for details."
	}

	# Uncomment this if you want the console results to be returned by the function
	# $result
}

Function Get-Challenge {
	Param(
		[Parameter(Mandatory=$True)]
		[string]$ControlPanelUrl
	)

	$url = "$($ControlPanelUrl)?verb=Challenge"

	[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
	$result = Invoke-WebRequest -Uri $url -TimeoutSec 360 -UseBasicParsing

	$result.Content
}

Function Invoke-StreamingWebRequest($Uri, $MAC, $Nonce) {
	$responseText = new-object -TypeName "System.Text.StringBuilder"

	$request = [System.Net.WebRequest]::Create($Uri)
	$request.Headers["X-MC-MAC"] = $MAC
	$request.Headers["X-MC-Nonce"] = $Nonce
	$request.Timeout = 10800000

	$response = $request.GetResponse()
	$responseStream = $response.GetResponseStream()
	$responseStreamReader = new-object System.IO.StreamReader $responseStream
	
	while(-not $responseStreamReader.EndOfStream) {
		$line = $responseStreamReader.ReadLine()

		if($line.StartsWith('Error:')) {
			Write-Host $line.Substring(7) -ForegroundColor Red
		}
		elseif($line.StartsWith('Warning:')) {
			Write-Host $line.Substring(9) -ForegroundColor Yellow
		}
		elseif($line.StartsWith('Debug:')) {
			Write-Host $line.Substring(7) -ForegroundColor Gray
		}
		elseif($line.StartsWith('Info:')) {
			Write-Host $line.Substring(6) -ForegroundColor White
		}
		else {
			Write-Host $line -ForegroundColor White
		}

		[void]$responseText.AppendLine($line)
	}

	return $responseText.ToString()
}

Export-ModuleMember -Function Sync-Unicorn