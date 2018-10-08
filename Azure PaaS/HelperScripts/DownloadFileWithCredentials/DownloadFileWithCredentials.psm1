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
		[string]$Assetfilename
    )

    if ($PSCmdlet.ShouldProcess($SourceUri, "Download $SourceUri to $Destinationfolder")) {

        try {
            Write-Verbose "Downloading $SourceUri to $Destinationfolder"

			$DestinationPath = Join-Path $Destinationfolder $Assetfilename

            if ($Credentials) {
                
                $user = $Credentials.GetNetworkCredential().username
                
                $password = $Credentials.GetNetworkCredential().password
                $loginRequest = Invoke-RestMethod -Uri https://dev.sitecore.net/api/authorization -Method Post -ContentType "application/json" -Body "{username: '$user', password: '$password'}" -SessionVariable session -UseBasicParsing
        
				$ProgressPreference = 'SilentlyContinue'
				Invoke-WebRequest -Uri $SourceUri -OutFile $DestinationPath -WebSession $session -UseBasicParsing
            }
			else
			{
                Write-Verbose "here"
                Invoke-WebRequest -Uri $SourceUri -OutFile $DestinationPath -UseBasicParsing
            }
        } catch {
            Write-Error -Message ("Error downloading $SourceUri" + ": $($_.Exception.Message)")
        }
    }
}

# Register-SitecoreInstallExtension -Command Invoke-DownloadFileWithCredentialsTask -As DownloadFileWithCredentials -Type Task -Force