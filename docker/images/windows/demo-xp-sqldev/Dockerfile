# escape=`
ARG BASE_IMAGE

FROM $BASE_IMAGE as final

ENV INSTALL_MODULE_PATH  "c:\\install-module"

COPY . \install-module

RUN & (Join-Path $env:INSTALL_MODULE_PATH "\\Install-Databases.ps1") -InstallPath $env:INSTALL_PATH -DataPath $env:DATA_PATH -ModulePath $env:INSTALL_MODULE_PATH -DatabasePrefix Sitecore; ` 
    Remove-Item $env:INSTALL_MODULE_PATH -Recurse
    
