# escape=`
ARG BASE_IMAGE
ARG TOOLS_ASSETS

ARG SPE_ASSETS
ARG SXA_ASSETS
ARG HEADLESS_ASSETS
ARG SPS_ASSETS

ARG CMP_ASSETS
ARG SFCRM_ASSETS
ARG SITECORE_AI_ASSETS
ARG SOLUTION_IMAGE

FROM ${TOOLS_ASSETS} as tools
FROM ${SPE_ASSETS} as spe_assets
FROM ${SXA_ASSETS} as sxa_assets
FROM ${HEADLESS_ASSETS} as headless_assets
FROM ${SPS_ASSETS} as sps_assets

FROM ${CMP_ASSETS} as cmp_assets
FROM ${SFCRM_ASSETS} as sfcrm_assets
FROM ${SITECORE_AI_ASSETS} as sitecore_ai_assets

FROM ${SOLUTION_IMAGE} as solution

FROM ${BASE_IMAGE} as modules

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY --from=tools /tools/ /tools/

COPY --from=spe_assets /module/cm/content /inetpub/wwwroot

COPY --from=headless_assets /module/cd/content /inetpub/wwwroot
COPY --from=headless_assets /module/tools /module/headless/tools
RUN /module/headless/tools/Initialize-Content.ps1 -TargetPath /inetpub/wwwroot; `
    Remove-Item -Path /module -Recurse -Force;

COPY --from=sxa_assets /module/cd/content /inetpub/wwwroot
COPY --from=sxa_assets /module/tools /module/sxa/tools
RUN /module/sxa/tools/Initialize-Content.ps1 -TargetPath /inetpub/wwwroot; `
    Remove-Item -Path /module -Recurse -Force;

COPY --from=sps_assets /module/cm/content /inetpub/wwwroot

COPY --from=cmp_assets /module/cm/content /inetpub/wwwroot
COPY --from=sfcrm_assets /module/cm/content /inetpub/wwwroot
COPY --from=sitecore_ai_assets /module/cm/content /inetpub/wwwroot

# https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/icacls
# (OI) = directories object inherit, (CI) = directories container inherit, F = Full access
RUN icacls 'C:\inetpub\wwwroot' /grant 'IIS_IUSRS:(OI)(CI)F' /t | Out-Null

FROM modules as production

COPY --from=solution /solution/cd/ /inetpub/wwwroot/
COPY ./data/transforms/ /inetpub/wwwroot/transforms/
COPY ./tools/ /tools/

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Enable GraphQL enpoints for Sitecore databases from example config files - Requires JSS to be installed
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Web.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Web.config'

# Find transform files and do transformation
RUN (Get-ChildItem -Path 'C:\\inetpub\\wwwroot\\transforms\\web*.xdt' -Recurse ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\web.config' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };

RUN (Get-ChildItem -Path 'C:\\inetpub\\wwwroot\\transforms\\ConnectionStrings*.xdt' -Recurse ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\App_Config\\ConnectionStrings.config' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };

RUN (Get-ChildItem -Path 'C:\\inetpub\\wwwroot\\transforms\\Layers*.xdt' -Recurse ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\App_Config\\Layers.config' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };

# Disable custom config file for Redis in CD container
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Include\\Sitecore.Analytics.Tracking.Containers.config' -NewName 'Sitecore.Analytics.Tracking.Containers.config.disabled'

ENV COVEO_SERVER_URL=http://cd/

ENTRYPOINT [ "powershell.exe", "C:\\tools\\entrypoints\\aks\\boot.ps1" ]
