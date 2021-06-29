# escape=`
ARG BASE_IMAGE
ARG DEF_ASSETS
ARG SFMCBDE_ASSETS
ARG TOOLS_ASSETS
ARG SITECORE_AI_ASSETS

FROM ${DEF_ASSETS} as def_assets
FROM ${SFMCBDE_ASSETS} as sfmcbde_assets
FROM ${TOOLS_ASSETS} as tools
FROM ${SITECORE_AI_ASSETS} as sitecore_ai_assets

FROM $BASE_IMAGE as production

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY --from=tools /tools/ /tools/
COPY --from=def_assets /module/xdbautomationworker/content /service
COPY --from=sfmcbde_assets /module/xdbautomationworker/content /service
COPY --from=sitecore_ai_assets /module/xconnect /service