# escape=`
ARG BASE_IMAGE
ARG TOOLS_ASSETS

FROM ${TOOLS_ASSETS} as tools

FROM $BASE_IMAGE as modules

COPY --from=tools /tools/ /tools/

FROM modules as production

COPY ./ /Identity/

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

#perform transform
RUN (Get-Item -Path 'C:\\Identity\\transforms\\identityserver.xml.xdt' ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\Identity\\sitecore\\Sitecore.Plugin.IdentityServer\\Config\\identityserver.xml' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };