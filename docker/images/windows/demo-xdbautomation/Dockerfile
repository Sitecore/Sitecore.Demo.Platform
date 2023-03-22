# escape=`
ARG BASE_IMAGE
ARG TOOLS_ASSETS

FROM ${TOOLS_ASSETS} as tools

FROM $BASE_IMAGE as production

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY --from=tools /tools/ /tools/