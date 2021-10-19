# escape=`

ARG BASE_IMAGE
ARG SOLUTION_IMAGE

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
ARG XGEN_ASSETS
ARG HORIZON_INTEGRATION_ASSETS
ARG SITECORE_AI_ASSETS
ARG SITECORE_AI_DASHBOARD_ASSETS

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
FROM ${SITECORE_AI_DASHBOARD_ASSETS} as sitecore_ai_dashboard_assets

FROM ${SOLUTION_IMAGE} as solution

FROM ${BASE_IMAGE} as modules

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY --from=spe_assets /module/db /module_data/spe_assets
COPY --from=sxa_assets /module/db /module_data/sxa_assets
COPY --from=headless_assets /module/db /module_data/headless_assets
COPY --from=sps_assets /module/db /module_data/sps_assets

COPY --from=def_assets /module/db /module_data/def_assets
COPY --from=sfcrm_assets /module/db /module_data/sfcrm_assets
COPY --from=dcrm_assets /module/db /module_data/dcrm_assets
COPY --from=sfmcbde_assets /module/db /module_data/sfmcbde_assets
COPY --from=sfmcce_assets /module/db /module_data/sfmcce_assets
COPY --from=cmp_assets /module/db /module_data/cmp_assets
COPY --from=horizon_integration_assets /module/db /module_data/horizon_integration_assets
COPY --from=xgen_assets /module/db /module_data/xgen_assets
COPY --from=sitecore_ai_assets /module/db /module_data/sitecore_ai_assets
COPY --from=sitecore_ai_dashboard_assets /module/db /module_data/sitecore_ai_dashboard_assets

# Additional DBs
COPY ./db /module_data/db

RUN $modules = @('spe_assets','sxa_assets','headless_assets','sps_assets','def_assets', 'sfcrm_assets', 'dcrm_assets', 'sfmcbde_assets', 'sfmcce_assets', 'cmp_assets', 'horizon_integration_assets', 'xgen_assets', 'sitecore_ai_assets', 'sitecore_ai_dashboard_assets', 'db'); `
    foreach($module in $modules) { C:\DeployDatabases.ps1 -ResourcesDirectory /module_data/$module; Remove-Item -Path /module_data/$module -Recurse -Force; }

FROM modules as data

COPY --from=solution /solution/db /output/

COPY ./data/ /output

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN $modules = @('security', 'data'); `
    foreach($module in $modules) { C:\DeployDatabases.ps1 -ResourcesDirectory /output/$module; Remove-Item -Path /output/$module -Recurse -Force; }

FROM data as production

ENV USER_PASSWORD="b"
ENV ADMIN_USER_NAME="sitecore\superuser"
ENV SITECORE_ADMIN_PASSWORD="b"
ENV DISABLE_DEFAULT_ADMIN=FALSE
ENV EXM_BASE_URL=http://cd
ENV SQL_SERVER="mssql"
ENV START_OVERRIDE_SCRIPTS="start-override-platform-windows.ps1"
ENV COVEO_ADMIN_USER_NAME="sitecore\coveoadmin"

COPY ./sql /sql

COPY ./HashPassword.ps1 ./start-override.ps1 ./start-override-platform-windows.ps1 /

CMD ./start-override -sa_password $env:sa_password -ACCEPT_EULA $env:ACCEPT_EULA -attach_dbs \"$env:attach_dbs\" -Verbose