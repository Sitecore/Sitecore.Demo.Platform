# escape=`
ARG BASE_IMAGE

FROM $BASE_IMAGE as final

COPY . ${INSTALL_PATH}

RUN $env:DB_PREFIX = 'Sitecore'; `
    & (Join-Path $env:INSTALL_PATH "\\Install-Databases.ps1") -InstallPath $env:INSTALL_PATH -DataPath $env:DATA_PATH -DatabasePrefix $env:DB_PREFIX; `
    Get-ChildItem -Path $env:INSTALL_PATH -Exclude '*.mdf', '*.ldf' -Recurse -Force | Remove-Item -Recurse -Force;
