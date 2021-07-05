# escape=`
ARG BASE_IMAGE
ARG SXA_ASSETS

FROM ${SXA_ASSETS} as sxa_assets

FROM $BASE_IMAGE

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY --from=sxa_assets C:\module\solr\cores-sxa.json C:\data\cores-sxa.json