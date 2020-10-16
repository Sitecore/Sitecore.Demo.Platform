# escape=`
ARG BASE_IMAGE

FROM $BASE_IMAGE as production

WORKDIR /app

COPY docker/images/windows/demo-ps/. .

ENTRYPOINT [ "powershell", "/app/start.ps1" ] 

