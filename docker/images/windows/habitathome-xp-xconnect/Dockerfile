# escape=`
ARG BASE_IMAGE

FROM $BASE_IMAGE 

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY ./temp/* /inetpub/xconnect/

ENTRYPOINT [ "powershell", "C:\\Boot.ps1", "C:\\inetpub\\xconnect\\App_Config\\ConnectionStrings.config", "'C:\\ServiceMonitor.exe w3svc xconnect'" ]