# escape=`
ARG BASE_IMAGE

FROM $BASE_IMAGE as builder

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY temp/ /install/
COPY *.xdt /tools/
COPY *.ps1 /tools/

RUN $env:INSTALL_TEMP = 'C:\\tools'; `
    Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/v5.0.2/nuget.exe' -UseBasicParsing -OutFile (Join-Path $env:INSTALL_TEMP '\\nuget.exe'); `
    & (Join-Path $env:INSTALL_TEMP '\\nuget.exe') install 'Microsoft.Web.Xdt' -Version '3.0.0' -OutputDirectory $env:INSTALL_TEMP;

FROM $BASE_IMAGE as final

COPY --from=builder [ "/install/", "/install/" ]
COPY --from=builder [ "/tools/", "/tools/" ]


RUN $env:INSTALL_TEMP = 'C:\\install'; `
    $env:TOOLS = 'C:\\tools'; `
    $env:SITE_PATH = 'C:\\inetpub\\wwwroot'; `
    Copy-Item -Path "$env:INSTALL_TEMP\\*" $env:SITE_PATH -Force -Recurse -Verbose; `
    $env:XDTDLL_PATH = Join-Path $env:TOOLS '\\bin\\Microsoft.Web.XmlTransform.dll'; `
    Copy-Item -Path (Join-Path $env:TOOLS "\\Microsoft.Web.Xdt.*\\lib\\net40\\Microsoft.Web.XmlTransform.dll") -Destination $env:XDTDLL_PATH; `
    & (Join-Path $env:TOOLS "\\Invoke-XdtTransform.ps1") -Path (Join-Path $env:SITE_PATH '\\Web.config') -XdtPath (Join-Path $env:INSTALL_TEMP '\\transforms\\Web.config.xdt') -XdtDllPath $env:XDTDLL_PATH; `
    & (Join-Path $env:TOOLS "\\Invoke-XdtTransform.ps1") -Path (Join-Path $env:SITE_PATH '\\App_Config\\Layers.config') -XdtPath (Join-Path $env:INSTALL_TEMP '\\transforms\\App_Config\\Layers.config.xdt') -XdtDllPath $env:XDTDLL_PATH; `
    $env:IIS_PATH = 'IIS:\Sites\Default Web Site'; `
    Clear-WebConfiguration -PSPath $env:IIS_PATH -Filter '/system.webserver/rewrite/rules/rule'; `
    New-Item -Path (Join-Path $env:SITE_PATH '\\upload') -ItemType 'Directory' -Force | Out-Null; `
    Remove-Item -Path $env:INSTALL_TEMP -Force -Recurse;