# escape=`
ARG BASE_IMAGE

FROM $BASE_IMAGE as production

COPY ./ /Identity/

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

#perform transform
RUN (Get-Item -Path 'C:\\Identity\\transforms\\identityserver.xml.xdt' ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\Identity\\sitecore\\Sitecore.Plugin.IdentityServer\\Config\\identityserver.xml' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };