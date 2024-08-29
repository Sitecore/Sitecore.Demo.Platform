# escape=`
ARG BASE_IMAGE
ARG TOOLS_ASSETS
ARG GP_IMAGE

FROM ${TOOLS_ASSETS} as tools
FROM ${GP_IMAGE} as gp

FROM $BASE_IMAGE as production

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY --from=tools /tools/ /tools/

COPY --from=gp \module\xconnect\content\App_Data\jobs\continuous\AutomationEngine\ C:\service
