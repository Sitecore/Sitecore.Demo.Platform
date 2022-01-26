# escape=`

ARG PARENT_IMAGE
ARG TOOLS_IMAGE

FROM ${TOOLS_IMAGE} as tools
FROM ${PARENT_IMAGE}

COPY --from=tools /tools/ /tools/

COPY ./ /Identity/

SHELL ["pwsh", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# perform transform
RUN (Get-Item -Path 'C:\\Identity\\transforms\\identityserver.xml.xdt' ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\Identity\\sitecore\\Sitecore.Plugin.IdentityServer\\Config\\identityserver.xml' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };