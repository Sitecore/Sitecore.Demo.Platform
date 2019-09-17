# escape=`
ARG BASE_IMAGE

FROM $BASE_IMAGE as builder

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY temp/* /install/

RUN $env:INSTALL_TEMP = 'C:\\install'; `
    Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/v5.0.2/nuget.exe' -UseBasicParsing -OutFile (Join-Path $env:INSTALL_TEMP '\\nuget.exe'); `
    & (Join-Path $env:INSTALL_TEMP '\\nuget.exe') install 'Microsoft.Web.Xdt' -Version '3.0.0' -OutputDirectory $env:INSTALL_TEMP;

FROM $BASE_IMAGE as final

COPY --from=builder [ "/install/", "/install/" ]
COPY temp/ /install/web/
COPY *.xdt /install/
COPY *.ps1 /install/

RUN $env:SITENAME = 'sc'; `
    $env:INSTALL_TEMP = 'C:\\install'; `
    $env:SITE_PATH = 'C:\\inetpub\\{0}' -f $env:SITENAME; `
		Copy-Item -Path (Join-Path $env:INSTALL_TEMP "web\\*") $env:SITE_PATH -Force -Verbose; `
		$env:XDTDLL_PATH = Join-Path $env:SITE_PATH '\\bin\\Microsoft.Web.XmlTransform.dll'; `
    Copy-Item -Path (Join-Path $env:INSTALL_TEMP "\\Microsoft.Web.Xdt.*\\lib\\net40\\Microsoft.Web.XmlTransform.dll") -Destination $env:XDTDLL_PATH; `
    & (Join-Path $env:INSTALL_TEMP "\\Invoke-XdtTransform.ps1") -Path (Join-Path $env:SITE_PATH '\\Web.config') -XdtPath (Join-Path $env:INSTALL_TEMP '\\web\\transforms\\Web.config.xdt') -XdtDllPath $env:XDTDLL_PATH; `
    #& (Join-Path $env:INSTALL_TEMP "\\Invoke-XdtTransform.ps1") -Path (Join-Path $env:SITE_PATH '\\App_Config\\Include\\Project\\Demo.Website.config') -XdtPath (Join-Path $env:INSTALL_TEMP '\\sccpl\\Xdts\\App_Config\\Include\\Project\\Demo.Website.config.xdt') -XdtDllPath $env:XDTDLL_PATH; `
    #& (Join-Path $env:INSTALL_TEMP "\\Invoke-XdtTransform.ps1") -Path (Join-Path $env:SITE_PATH '\\App_Config\\Include\\Feature\\Feature.Accounts.config') -XdtPath (Join-Path $env:INSTALL_TEMP '\\sccpl\\Xdts\\App_Config\\Include\\Feature\\Feature.Accounts.config.xdt') -XdtDllPath $env:XDTDLL_PATH; `
    & (Join-Path $env:INSTALL_TEMP "\\Invoke-XdtTransform.ps1") -Path (Join-Path $env:SITE_PATH '\\App_Config\\ConnectionStrings.config') -XdtPath (Join-Path $env:INSTALL_TEMP '\\ConnectionStrings.config.xdt') -XdtDllPath $env:XDTDLL_PATH; `
		& (Join-Path $env:INSTALL_TEMP "\\Invoke-XdtTransform.ps1") -Path (Join-Path $env:SITE_PATH '\\App_Config\\Layers.config') -XdtPath (Join-Path $env:INSTALL_TEMP '\\web\\transforms\\App_Config\\Layers.config.xdt') -XdtDllPath $env:XDTDLL_PATH; `
    $env:IIS_PATH = 'IIS:\Sites\{0}' -f $env:SITENAME; `
    Clear-WebConfiguration -PSPath $env:IIS_PATH -Filter '/system.webserver/rewrite/rules/rule'; `
    New-Item -Path (Join-Path $env:SITE_PATH '\\upload') -ItemType 'Directory' | Out-Null; `
    Remove-Item -Path $env:INSTALL_TEMP -Force -Recurse;
