# escape=`
ARG BASE_IMAGE
ARG BUILD_IMAGE
ARG ASSETS_IMAGE

FROM $ASSETS_IMAGE as assets
FROM ${BUILD_IMAGE} as build

ARG INTERNAL_NUGET_SOURCE
ARG SYSTEM_ACCESSTOKEN

COPY docker/images/windows/demo-cd/ docker/
COPY build* nuget.config publishsettings.targets Sitecore.Demo.Platform.sln ./build/
COPY scripts/Packaging build/scripts/Packaging
COPY cake-config-container.json ./cake-config.json
COPY src/ ./src

RUN Copy-Item ./build/* . -Force

RUN .\build.ps1 -DeploymentTarget DockerBuild -Target Content-Delivery

FROM $BASE_IMAGE as production

COPY --from=assets ["C:\\tools\\", "C:\\tools\\"]
COPY --from=build /out/demo-cd/ /inetpub/wwwroot/
COPY --from=build /docker/Data/transforms/ /inetpub/wwwroot/transforms/

# Disable custom config file for Redis in CD container
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Include\\Sitecore.Analytics.Tracking.Containers.config' -NewName 'Sitecore.Analytics.Tracking.Containers.config.disabled'

# Switch from Redis to InProc provider in web.config in CD container
RUN (Get-ChildItem -Path 'C:\\inetpub\\wwwroot\\transforms\\web*.xdt' -Recurse ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\web.config' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };

RUN (Get-ChildItem -Path 'C:\\inetpub\\wwwroot\\transforms\\ConnectionStrings*.xdt' -Recurse ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\App_Config\\ConnectionStrings.config' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };
