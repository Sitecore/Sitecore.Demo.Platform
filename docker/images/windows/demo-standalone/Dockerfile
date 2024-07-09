# escape=`
ARG BASE_IMAGE
ARG SOLUTION_IMAGE
ARG TOOLS_ASSETS

ARG SPE_ASSETS
ARG SXA_ASSETS
ARG HEADLESS_ASSETS

ARG CMP_ASSETS
ARG XGEN_ASSETS

ARG MANAGEMENT_SERVICES_IMAGE
ARG DELTA_ASSET_IMAGE

FROM ${TOOLS_ASSETS} as tools

FROM ${SPE_ASSETS} as spe_assets
FROM ${SXA_ASSETS} as sxa_assets
FROM ${HEADLESS_ASSETS} as headless_assets
FROM ${MANAGEMENT_SERVICES_IMAGE} AS management_services
FROM ${DELTA_ASSET_IMAGE} as deltaAssetImage

FROM ${CMP_ASSETS} as cmp_assets
FROM ${XGEN_ASSETS} as xgen_assets

FROM ${SOLUTION_IMAGE} as solution

FROM ${BASE_IMAGE} as modules

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Install GIT
WORKDIR C:\tools
RUN Invoke-WebRequest 'https://github.com/git-for-windows/git/releases/download/v2.30.1.windows.1/Git-2.30.1-64-bit.exe' -OutFile 'git.exe';
RUN Start-Process "c:\tools\git.exe" -ArgumentList '/SP-', '/VERYSILENT', '/NORESTART', '/NOCANCEL', '/CLOSEAPPLICATIONS', '/RESTARTAPPLICATIONS' -Wait -NoNewWindow;
WORKDIR C:\inetpub\wwwroot

COPY --from=tools /tools/ /tools/

COPY --from=management_services /module/cm/content /inetpub/wwwroot

COPY --from=spe_assets /module/cm/content /inetpub/wwwroot

COPY --from=sxa_assets /module/cm/content /inetpub/wwwroot
COPY --from=sxa_assets /module/tools /module/sxa/tools
RUN /module/sxa/tools/Initialize-Content.ps1 -TargetPath /inetpub/wwwroot; `
    Remove-Item -Path /module -Recurse -Force;

COPY --from=headless_assets /module/cm/content /inetpub/wwwroot
COPY --from=headless_assets /module/db /inetpub/wwwroot/App_Data/items/core

COPY --from=cmp_assets /module/cm/content /inetpub/wwwroot
COPY --from=xgen_assets /module/cm/content /inetpub/wwwroot

# https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/icacls
# (OI) = directories object inherit, (CI) = directories container inherit, F = Full access
RUN icacls 'C:\inetpub\wwwroot' /grant 'IIS_IUSRS:(OI)(CI)F' /t | Out-Null

FROM modules as production

# Copy Product Hotfixes
# COPY --from=deltaAssetImage /platform/cm/inetpub/wwwroot/ /inetpub/wwwroot/

COPY --from=solution /solution/cm/ /inetpub/wwwroot/

COPY ./hotfix/ /inetpub/wwwroot/
COPY ./Data/transforms/ /inetpub/wwwroot/transforms/
COPY ./tools/ /tools/

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Enable GraphQL enpoints for Sitecore databases from example config files - Requires JSS to be installed
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Core.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Core.config'
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Master.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Master.config'
RUN Rename-Item -Path 'C:\\inetpub\\wwwroot\\App_Config\\Sitecore\\Services.GraphQL\\Sitecore.Services.GraphQL.Content.Web.config.example' -NewName 'Sitecore.Services.GraphQL.Content.Web.config'

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

RUN	C:\windows\System32\inetsrv\appcmd.exe set config -section:system.applicationHost/applicationPools /applicationPoolDefaults.processModel.loadUserProfile:"true" /commit:apphost;

ENTRYPOINT [ "powershell.exe", "C:\\tools\\entrypoints\\aks\\boot.ps1" ]
