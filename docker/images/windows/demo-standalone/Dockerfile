# escape=`
ARG BASE_IMAGE
ARG BUILD_IMAGE
ARG ASSETS_IMAGE

FROM $ASSETS_IMAGE as assets
FROM ${BUILD_IMAGE} as build

ARG INTERNAL_NUGET_SOURCE
ARG SYSTEM_ACCESSTOKEN
ARG OPENCALAIS_ACCESSTOKEN

COPY docker/images/windows/demo-standalone/ docker/
COPY build* nuget.config publishsettings.targets Sitecore.Demo.Platform.sln ./build/
COPY scripts/Packaging build/scripts/Packaging
COPY cake-config-container.json ./cake-config.json
COPY src/ ./src
COPY FrontEnd/ ./FrontEnd

RUN Copy-Item ./build/* .

RUN .\build.ps1 -DeploymentTarget DockerBuild -Target Content-Management
RUN (Get-Content /out/demo-standalone/App_Config/Include/Feature/Sitecore.Demo.Platform.Feature.ContentTagging.config).replace('{calaisAccessToken}', $env:OPENCALAIS_ACCESSTOKEN) | Set-Content /out/demo-standalone/App_Config/Include/Feature/Sitecore.Demo.Platform.Feature.ContentTagging.config -Verbose
FROM $BASE_IMAGE as production

COPY --from=assets ["C:\\tools\\", "C:\\tools\\"]
COPY --from=build /out/demo-standalone/ /inetpub/wwwroot/
COPY --from=build /docker/Data/transforms/ /inetpub/wwwroot/transforms/
COPY --from=build /docker/Data/bin/ /inetpub/wwwroot/bin/

# Enable GraphQL enpoints for Sitecore databases from example config files
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Core.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Core.config'
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Master.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Master.config'
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Web.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Web.config'

RUN (Get-ChildItem -Path 'C:\\inetpub\\wwwroot\\transforms\\web*.xdt' -Recurse ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\web.config' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };

#find transform files and do transformation
RUN (Get-ChildItem -Path 'C:\\inetpub\\wwwroot\\transforms\\ConnectionStrings*.xdt' -Recurse ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\App_Config\\ConnectionStrings.config' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };

RUN icacls 'C:\inetpub\wwwroot' /grant 'IIS_IUSRS:(F)' /t