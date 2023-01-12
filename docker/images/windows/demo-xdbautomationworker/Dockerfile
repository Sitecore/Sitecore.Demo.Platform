# escape=`
ARG BASE_IMAGE
ARG DEF_ASSETS
ARG SFMCBDE_ASSETS
ARG TOOLS_ASSETS
FROM ${DEF_ASSETS} as def_assets
FROM ${SFMCBDE_ASSETS} as sfmcbde_assets
FROM ${TOOLS_ASSETS} as tools

FROM $BASE_IMAGE as production

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY --from=tools /tools/ /tools/
COPY --from=def_assets /module/xdbautomationworker/content /service
COPY --from=sfmcbde_assets /module/xdbautomationworker/content /service
COPY --from=sfmcbde_assets /module/xdttransform/xdbautomationworker/transforms /inetpub/wwwroot/transforms/