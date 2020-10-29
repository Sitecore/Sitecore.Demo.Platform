# escape=`
ARG BASE_IMAGE
ARG SOLUTION_IMAGE
ARG ASSETS_IMAGE

FROM $ASSETS_IMAGE as assets
FROM ${SOLUTION_IMAGE} as solution
FROM $BASE_IMAGE as build

ARG OPENCALAIS_ACCESSTOKEN

COPY --from=assets ["/tools/", "/tools/"]
COPY --from=solution /solution/cm/ /inetpub/wwwroot/
COPY docker/images/windows/demo-standalone/Data/transforms/ /inetpub/wwwroot/transforms/
COPY docker/images/windows/demo-standalone/Data/bin/ /inetpub/wwwroot/bin/

RUN (Get-Content /inetpub/wwwroot/App_Config/Include/Feature/Sitecore.Demo.Platform.Feature.ContentTagging.config).replace('{calaisAccessToken}', $env:OPENCALAIS_ACCESSTOKEN) | Set-Content /inetpub/wwwroot/App_Config/Include/Feature/Sitecore.Demo.Platform.Feature.ContentTagging.config -Verbose

# Enable GraphQL enpoints for Sitecore databases from example config files
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Core.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Core.config'
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Master.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Master.config'
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Web.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Web.config'

RUN (Get-ChildItem -Path '/inetpub/wwwroot/transforms/web*.xdt' -Recurse ) | `
    ForEach-Object { & '/tools/scripts/Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\web.config' -XdtPath $_.FullName `
    -XdtDllPath '/tools/bin/Microsoft.Web.XmlTransform.dll'; };

# Find transform files and do transformation
RUN (Get-ChildItem -Path '/inetpub/wwwroot/transforms/ConnectionStrings*.xdt' -Recurse ) | `
    ForEach-Object { & '/tools/scripts/Invoke-XdtTransform.ps1' -Path '/inetpub/wwwroot/app_config/ConnectionStrings.config' -XdtPath $_.FullName `
    -XdtDllPath '/tools/bin/Microsoft.Web.XmlTransform.dll'; };

FROM $BASE_IMAGE as production

RUN icacls 'C:\inetpub\wwwroot' /grant 'IIS_IUSRS:(F)' /t

COPY --from=assets ["/tools/", "/tools/"]
COPY --from=build /inetpub/wwwroot /inetpub/wwwroot