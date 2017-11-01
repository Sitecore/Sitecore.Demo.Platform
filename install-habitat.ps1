#####################################################
# 
#  Install Sitecore
# 
#####################################################
. $PSScriptRoot\settings.ps1
if (Test-Path $PSScriptRoot\settings.user.ps1){
	. $PSScriptRoot\settings.user.ps1
}
Write-Host ""
Write-Host "*******************************************************" -ForegroundColor Green
Write-Host " Installing Habitat Home Demo on Sitecore $SitecoreVersion" -ForegroundColor Green
Write-Host " Sitecore: $SitecoreSiteName" -ForegroundColor Green
Write-Host "*******************************************************" -ForegroundColor Green
function Copy-Packages {
    if (!(Test-Path $AssetsRoot\Packages)) {
        throw "Packages folder not found"
    }
    $packageSource = "$AssetsRoot\Packages"
    $tempFolderFullPath = "$webroot\$SitecoreSiteName\temp\Packages"

    #Check for existence of packages
    $di = gci $AssetsRoot\Packages | Measure-Object
    if ($di.Count -eq 0)
    {
        Write-Host "No additional packages found to install" -ForegroundColor Yellow
    }
    else {
        # Install all packages in numerical order
            Copy-Item -Path $packageSource -Filter *.zip -Destination $tempFolderFullPath -Verbose -Recurse
    }
         
    
}

function Install-Script {
    if (!(Test-Path $AssetsRoot)) {
        throw "$AssetsRoot not found"
    }
   
    try 
    {
        Write-Host "Copying tools to webroot" -ForegroundColor Green
        Copy-Item "$AssetsRoot\InstallPackage.aspx" -Destination "$webroot\$SitecoreSiteName" -Force
    }
    catch
    {
        write-host "Failed to copy InstallPackage.aspx to web root" -ForegroundColor Red
    }
}
function Install-Packages {
    $packagesPath = "$webroot\$SitecoreSiteName\temp\Packages"
    $packageInstallerUrl = "https://$SitecoreSiteName/InstallPackage.aspx?package=/temp/Packages/"
    $di = gci $packagesPath | Measure-Object
    if ($di.Count -eq 0)
    {
        Write-Host "No  packages found to install" -ForegroundColor Yellow
    }
    else {
        dir $packagesPath | ForEach-Object {
            $url = $packageInstallerUrl + $_ 
            $request = [system.net.WebRequest]::Create($url)
            $request.Timeout = 2400000
            Write-Host $url
            Write-Host "Installing Package : $_" -ForegroundColor Green
            $request.GetResponse()  
        }
    }
}

function Deploy-Habitat {
    npm install
    .\node_modules\.bin\gulp
}
. .\install-xp0.ps1
Copy-Packages
Install-Script
Install-Packages
Deploy-Habitat
