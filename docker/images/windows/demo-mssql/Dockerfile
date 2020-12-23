# escape=`

ARG BASE_IMAGE
ARG SOLUTION_IMAGE

FROM ${SOLUTION_IMAGE} as solution
FROM $BASE_IMAGE as data

COPY --from=solution /solution/db /output/

COPY ./data/ /output

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN $modules = @('security', 'data', 'descendants'); `
    foreach($module in $modules) { C:\DeployDatabases.ps1 -ResourcesDirectory /output/$module; Remove-Item -Path /output/$module -Recurse -Force; }

FROM data as production

ENV USER_PASSWORD="b"
ENV ADMIN_USER_NAME="sitecore\superuser"
ENV SITECORE_ADMIN_PASSWORD="b"
ENV DISABLE_DEFAULT_ADMIN=FALSE
ENV EXM_BASE_URL=http://cd
ENV SQL_SERVER="mssql"
ENV START_OVERRIDE_SCRIPTS="start-override-platform-windows.ps1"

COPY ./sql /sql

COPY ./HashPassword.ps1 ./start-override.ps1 ./start-override-platform-windows.ps1 /

CMD ./start-override -sa_password $env:sa_password -ACCEPT_EULA $env:ACCEPT_EULA -attach_dbs \"$env:attach_dbs\" -Verbose