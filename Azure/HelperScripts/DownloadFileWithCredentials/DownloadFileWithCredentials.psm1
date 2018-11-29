<#
	This function downloads assets locally to a destination folder location for which a source URL is provided
#>

Function Invoke-DownloadFileWithCredentialsTask {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceUri,
        [Parameter(Mandatory = $true)]
        [ValidateScript( { Test-Path -Path (Split-Path -Path $_ -Parent) })]
        [ValidateScript( { Test-Path -Path $_ -PathType Leaf -IsValid })]
        [string]$Destinationfolder,
        [PSCredential]$Credentials,
		[string]$Assetfilename,
		[string]$TypeSource
    )

    if ($PSCmdlet.ShouldProcess($SourceUri, "Download $SourceUri to $Destinationfolder")) {

		$ProgressPreference = 'SilentlyContinue'

        try {
            Write-Verbose "Downloading $SourceUri to $Destinationfolder"

			$DestinationPath = Join-Path $Destinationfolder $Assetfilename

            if ($Credentials -and ($TypeSource -eq "sitecore")) {
                
                $user = $Credentials.GetNetworkCredential().username
                
                $password = $Credentials.GetNetworkCredential().password
                $loginRequest = Invoke-RestMethod -Uri https://dev.sitecore.net/api/authorization -Method Post -ContentType "application/json" -Body "{username: '$user', password: '$password'}" -SessionVariable session -UseBasicParsing
        
				Invoke-WebRequest -Uri $SourceUri -OutFile $DestinationPath -WebSession $session -UseBasicParsing
            }
			elseif ($TypeSource -eq "github")
			{
                Write-Verbose "here"
				[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
                Invoke-WebRequest -Uri $SourceUri -OutFile $DestinationPath -UseBasicParsing
            }
        } catch {
            Write-Error -Message ("Error downloading $SourceUri" + ": $($_.Exception.Message)")
        }
    }
}