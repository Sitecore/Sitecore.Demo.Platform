# escape=`
ARG BASE_IMAGE
ARG ASSETS_IMAGE

FROM $ASSETS_IMAGE as assets
FROM $BASE_IMAGE as production

COPY --from=assets ["C:\\tools\\", "C:\\tools\\"]
COPY docker/images/windows/demo-id/ /Identity/

#perform transform
RUN (Get-Item -Path 'C:\\Identity\\transforms\\identityserver.xml.xdt' ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\Identity\\sitecore\\Sitecore.Plugin.IdentityServer\\Config\\identityserver.xml' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };