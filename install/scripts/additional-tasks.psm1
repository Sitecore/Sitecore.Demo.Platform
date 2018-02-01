
function Add-HabitatHomeBindingDetails {
    param(
        $siteName,
        $habitatHomeHostName
    )
    
    Set-HostsEntry -IPAddress 127.0.0.1 -HostName $habitatHomeHostName
    Set-Binding $siteName "http" * 80 $habitatHomeHostName
    Add-HabitatHomeSSLBinding -SiteName $siteName -HostName $habitatHomeHostName
    
   # Set-Binding $siteName "https" * 443 $habitatHomeHostName
}

function Set-Binding
{
    param 
    (
        [Parameter(Mandatory=$True)][string]$siteName,
        [Parameter(Mandatory=$True)][string]$protocol,
        [Parameter(Mandatory=$True)][string]$ipAddress,
        [Parameter(Mandatory=$True)][string]$port,
        [Parameter(Mandatory=$True)][string]$dnsName
    )
    begin {}
    process
    {

        # Change the IIS metadata so we can see the SSL\Port configuration in IIS Manager
        $bindingInformation = "$($ipAddress):$($port):$($dnsName)"
        $escaped = [Regex]::Escape($bindingInformation)
        $cmd = invoke-expression "$($env:WINDIR)\system32\inetsrv\Appcmd list site `"$siteName`" /Config"
        If($cmd -Match $escaped)
        {
            Write-Verbose "Binding already exists for site '$siteName', binding '$bindingInformation'."
        }
        Else
        {
            $cmd = invoke-expression "$($env:WINDIR)\system32\inetsrv\Appcmd set site /site.name: `"$siteName`" /+`"bindings.[protocol='$protocol',bindingInformation='$bindingInformation',sslFlags='0']`" /commit:apphost"
            if($cmd -Like "SITE object * changed")
            {
                Write-Verbose "Binding successfully set for site '$siteName', binding '$bindingInformation'."
            }
            Else
            {
                Write-Host "Error, unable to create IIS metadata for binding '$dnsName'. Response from command '$cmd'." -ForegroundColor red
                return 1; 
            }
        }

        return 0;
    }
    end { }  
}


function Add-HabitatHomeSSLBinding {

	param(
		[Parameter(Mandatory=$true)]
		[ValidateNotNullOrEmpty()]
		[string] $SiteName
        ,
        [Parameter(Mandatory=$true)]
		[ValidateNotNullOrEmpty()]
		[string] $HostName
		,
		[Parameter(Mandatory=$false)]
		[ValidateScript({ $_.StartsWith("cert:\", "CurrentCultureIgnoreCase")})]
		[string] $CertStoreLocation = 'Cert:\LocalMachine\My'
		,
		[int] $Port = 443
	)

	$functionPrefix = "$($MyInvocation.MyCommand.Name):"

	AssertPath "IIS:\Sites\$SiteName" 'HostName'

	AssertCertificateStore $CertStoreLocation

	If (! ( Get-WebBinding -Name $HostName -Protocol https ) )
	{
		$certificate = GetCertificateByDnsName -CertStoreLocation $CertStoreLocation -DnsName $SiteName

		if ($null -eq $certificate)
		{
			throw "Failed to find certificate for DnsName '$HostName' in $CertStoreLocation"
		}

		$certHash = $($certificate.thumbprint)

		Write-Verbose "$functionPrefix add SSL certificate binding to $HostName`:${Port} using $certHash"
		AddSslCertMapping -HostNamePort "${HostName}:${Port}" -CertHash $certHash

		Write-Verbose "$functionPrefix creating SNI enabled HTTPS protocol web binding for $HostName port ${Port}"
		New-WebBinding -Name $SiteName -Port ${Port} -Protocol https -HostHeader $HostName -SslFlags 1

		Write-Verbose "$functionPrefix successfully added SSL Binding for $HostName`:${Port}"
	}
	Else
	{
		Write-Verbose "$functionPrefix HTTPS WebBinding already exists, skipping"
    }
}

Export-ModuleMember Add-HabitatHomeBindingDetails