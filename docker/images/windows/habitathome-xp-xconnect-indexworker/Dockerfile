# escape=`
ARG BASE_IMAGE

FROM $BASE_IMAGE

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY ./temp/App_Data/jobs/continuous/IndexWorker /IndexWorker/

ENTRYPOINT [ "powershell", "C:\\Boot.ps1", "C:\\IndexWorker\\App_Config\\ConnectionStrings.config", "C:\\IndexWorker\\XConnectSearchIndexer.exe" ]