# escape=`
ARG BASE_IMAGE
ARG SOLUTION_IMAGE
ARG ASSETS_IMAGE

FROM $ASSETS_IMAGE as assets
FROM ${SOLUTION_IMAGE} as solution

FROM $BASE_IMAGE as build

COPY --from=assets ["C:\\tools\\", "C:\\tools\\"]
COPY --from=solution /solution/cd/ /inetpub/wwwroot/
COPY ./data/transforms/ /inetpub/wwwroot/transforms/

# Switch from Redis to InProc provider in web.config in CD container
RUN (Get-ChildItem -Path 'C:\\inetpub\\wwwroot\\transforms\\web*.xdt' -Recurse ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\web.config' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };

RUN (Get-ChildItem -Path 'C:\\inetpub\\wwwroot\\transforms\\ConnectionStrings*.xdt' -Recurse ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\App_Config\\ConnectionStrings.config' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };

FROM $BASE_IMAGE as production

COPY --from=assets ["C:\\tools\\", "C:\\tools\\"]
COPY --from=build /inetpub/wwwroot /inetpub/wwwroot

# Disable custom config file for Redis in CD container
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Include\\Sitecore.Analytics.Tracking.Containers.config' -NewName 'Sitecore.Analytics.Tracking.Containers.config.disabled'

COPY ./tools/ /tools/

ENTRYPOINT [ "powershell.exe", "C:\\tools\\entrypoints\\aks\\boot.ps1" ]
