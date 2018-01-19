param(
    $instance =""
)

$config = Get-Content -Raw -Path "$PSSCriptRoot\warmup-config.json" | ConvertFrom-Json
if ($instance -eq ""){
    $instanceName = $config.instanceName
}
else {
$instanceName = $instance    
}
Write-Host $instanceName



Function Get-SitecoreSession {
  param(
    [Parameter(Mandatory=$true,Position=0)]
    [string]$site,
    [Parameter(Mandatory=$true,Position=1)]
    [string]$username,
    [Parameter(Mandatory=$true,Position=2)]
    [string]$password

  )

  # Login - to create web session with authorisation cookies
  $loginPage = "$site/sitecore/login"
  $login = Invoke-WebRequest $loginPage -SessionVariable webSession
  $form = $login.forms[0]
  $form.fields["UserName"] = $username
  $form.fields["Password"] = $password
  
  Write-Host ""
  Write-Host "logging in"
  
  $request = Invoke-WebRequest -Uri $loginPage -WebSession $webSession -Method POST -Body $form | Out-Null

  $webSession
  
  Write-Host "login done"
}

Function RequestPage {
	param(
		[Parameter(Mandatory=$true,Position=0)]
		[string]$url,
		[Parameter(Mandatory=$true,Position=1)]
		[object]$webSession
	)
	Get-Date
	Write-Host "requesting $url ..."
	try { $request = Invoke-WebRequest $url -WebSession $webSession -TimeoutSec 60000 } catch {
      $status = $_.Exception.Response.StatusCode.Value__
	  if ($status -ne 200){
		Write-Host "ERROR Something went wrong while requesting $url" -foregroundcolor red
	  }
	}
	
	Get-Date
	Write-Host "Done"
	Write-Host ""
}
   
$session = Get-SitecoreSession $instanceName sitecore\admin b

foreach ($page in $config.urls) {
	RequestPage "https://$instanceName$($page.url)" $session
}


Write-Host " "
Write-Host "press any key to close"

$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")










