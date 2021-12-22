# escape=`
ARG BASE_IMAGE
ARG SOLUTION_IMAGE
ARG TOOLS_ASSETS

ARG SPE_ASSETS
ARG SXA_ASSETS
ARG HEADLESS_ASSETS
ARG SPS_ASSETS

ARG DEF_ASSETS
ARG SFCRM_ASSETS
ARG DCRM_ASSETS
ARG SFMCBDE_ASSETS
ARG SFMCCE_ASSETS
ARG CMP_ASSETS
ARG HORIZON_INTEGRATION_ASSETS
ARG XGEN_ASSETS
ARG SITECORE_AI_ASSETS

FROM ${TOOLS_ASSETS} as tools

FROM ${SPE_ASSETS} as spe_assets
FROM ${SXA_ASSETS} as sxa_assets
FROM ${HEADLESS_ASSETS} as headless_assets
FROM ${SPS_ASSETS} as sps_assets

FROM ${DEF_ASSETS} as def_assets
FROM ${SFCRM_ASSETS} as sfcrm_assets
FROM ${DCRM_ASSETS} as dcrm_assets
FROM ${SFMCBDE_ASSETS} as sfmcbde_assets
FROM ${SFMCCE_ASSETS} as sfmcce_assets
FROM ${CMP_ASSETS} as cmp_assets
FROM ${HORIZON_INTEGRATION_ASSETS} as horizon_integration_assets
FROM ${XGEN_ASSETS} as xgen_assets
FROM ${SITECORE_AI_ASSETS} as sitecore_ai_assets

FROM ${SOLUTION_IMAGE} as solution

FROM ${BASE_IMAGE} as modules

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY --from=tools /tools/ /tools/

COPY --from=spe_assets /module/cm/content /inetpub/wwwroot

COPY --from=sxa_assets /module/cm/content /inetpub/wwwroot
COPY --from=sxa_assets /module/tools /module/sxa/tools
RUN /module/sxa/tools/Initialize-Content.ps1 -TargetPath /inetpub/wwwroot; `
    Remove-Item -Path /module -Recurse -Force;

COPY --from=headless_assets /module/cm/content /inetpub/wwwroot
COPY --from=sps_assets /module/cm/content /inetpub/wwwroot

COPY --from=def_assets /module/cm/content /inetpub/wwwroot
COPY --from=sfcrm_assets /module/cm/content /inetpub/wwwroot
COPY --from=dcrm_assets /module/cm/content /inetpub/wwwroot
COPY --from=sfmcbde_assets /module/cm/content /inetpub/wwwroot
COPY --from=sfmcce_assets /module/cm/content /inetpub/wwwroot
COPY --from=cmp_assets /module/cm/content /inetpub/wwwroot
COPY --from=horizon_integration_assets /module/cm/content /inetpub/wwwroot
COPY --from=xgen_assets /module/cm/content /inetpub/wwwroot
COPY --from=sitecore_ai_assets /module/cm/content /inetpub/wwwroot

# https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/icacls
# (OI) = directories object inherit, (CI) = directories container inherit, F = Full access
RUN icacls 'C:\inetpub\wwwroot' /grant 'IIS_IUSRS:(OI)(CI)F' /t | Out-Null


FROM modules as production

COPY --from=solution /solution/cm/ /inetpub/wwwroot/

COPY ./hotfix/ /inetpub/wwwroot/
COPY ./Data/transforms/ /inetpub/wwwroot/transforms/
COPY --from=dcrm_assets /module/transforms/cm/Web.config.xdt /inetpub/wwwroot/transforms/Web.dcrm.config.xdt
COPY --from=dcrm_assets /module/transforms/cm/App_Config/ConnectionStrings.config.xdt /inetpub/wwwroot/transforms/App_Config/ConnectionStrings.dcrm.config.xdt
COPY ./tools/ /tools/

# Fix for missing file in asset image
COPY JourneyListDialog.xml /inetpub/wwwroot/sitecore/shell/Applications/Dialogs/DataExchangeFramework/JourneyListDialog.xml

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Enable GraphQL enpoints for Sitecore databases from example config files - Requires JSS to be installed
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Core.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Core.config'
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Master.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Master.config'
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Web.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Web.config'

# Fix for PowerShell Console failing to open from Sitecore desktop menu - Requires PSE to be installed
RUN Rename-Item -Path 'C:\inetpub\wwwroot\App_Config\Include\Spe\Spe.IdentityServer.config.disabled' -NewName 'Spe.IdentityServer.config'

# Find transform files and do transformation
RUN (Get-ChildItem -Path '/inetpub/wwwroot/transforms/web*.xdt' -Recurse ) | `
    ForEach-Object { & '/tools/scripts/Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\web.config' -XdtPath $_.FullName `
    -XdtDllPath '/tools/bin/Microsoft.Web.XmlTransform.dll'; };

RUN (Get-ChildItem -Path '/inetpub/wwwroot/transforms/ConnectionStrings*.xdt' -Recurse ) | `
    ForEach-Object { & '/tools/scripts/Invoke-XdtTransform.ps1' -Path '/inetpub/wwwroot/app_config/ConnectionStrings.config' -XdtPath $_.FullName `
    -XdtDllPath '/tools/bin/Microsoft.Web.XmlTransform.dll'; };

RUN (Get-ChildItem -Path '/inetpub/wwwroot/transforms/Layers*.xdt' -Recurse ) | `
    ForEach-Object { & '/tools/scripts/Invoke-XdtTransform.ps1' -Path '/inetpub/wwwroot/app_config/Layers.config' -XdtPath $_.FullName `
    -XdtDllPath '/tools/bin/Microsoft.Web.XmlTransform.dll'; };

ENV HOST_CD=http://cd
ENV COVEO_SERVER_URL=http://cm/

RUN	C:\windows\System32\inetsrv\appcmd.exe set config -section:system.applicationHost/applicationPools /applicationPoolDefaults.processModel.loadUserProfile:"true" /commit:apphost;

ENTRYPOINT [ "powershell.exe", "C:\\tools\\entrypoints\\aks\\boot.ps1" ]
