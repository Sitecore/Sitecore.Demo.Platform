# escape=`
ARG BASE_IMAGE
ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE

FROM $BASE_IMAGE as final

ENV INSTALL_MODULE_PATH  "c:\\install-module"

COPY . \install-module
    
FROM final as def

ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE


RUN Expand-Archive -Path ($env:INSTALL_MODULE_PATH + '\\' + $env:DEF) -DestinationPath 'C:\\temp\\def'; `
    & (Join-Path $env:INSTALL_MODULE_PATH "\\Install-Databases.ps1") -InstallPath $env:INSTALL_PATH -DataPath $env:DATA_PATH -ModulePath 'C:\\temp\\def' -DatabasePrefix Sitecore; 

FROM def as sc_dam

ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE


RUN Expand-Archive -Path ($env:INSTALL_MODULE_PATH + '\\' + $env:SC_DAM) -DestinationPath 'C:\\temp\\sc_dam'; `
    & (Join-Path $env:INSTALL_MODULE_PATH "\\Install-Databases.ps1") -InstallPath $env:INSTALL_PATH -DataPath $env:DATA_PATH -ModulePath 'C:\\temp\\sc_dam' -DatabasePrefix Sitecore; 

FROM sc_dam as sc_cmp

ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE


RUN Expand-Archive -Path ($env:INSTALL_MODULE_PATH + '\\' +  $env:SC_CMP) -DestinationPath 'C:\\temp\\sc_cmp'; `
    & (Join-Path $env:INSTALL_MODULE_PATH "\\Install-Databases.ps1") -InstallPath $env:INSTALL_PATH -DataPath $env:DATA_PATH -ModulePath 'C:\\temp\\sc_cmp' -DatabasePrefix Sitecore; 

FROM sc_cmp as sc_sfmc_bde

ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE


RUN Expand-Archive -Path ($env:INSTALL_MODULE_PATH + '\\' + $env:SC_SFMC_BDE) -DestinationPath 'C:\\temp\\sf_sfmc_bde'; `
    & (Join-Path $env:INSTALL_MODULE_PATH "\\Install-Databases.ps1") -InstallPath $env:INSTALL_PATH -DataPath $env:DATA_PATH -ModulePath 'C:\\temp\\sf_sfmc_bde' -DatabasePrefix Sitecore; 

FROM sc_sfmc_bde as sc_sfmc_ce

ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE


RUN Expand-Archive -Path ($env:INSTALL_MODULE_PATH + '\\' + $env:SC_SFMC_CE) -DestinationPath 'C:\\temp\\sc_sfmc_ce'; `
    & (Join-Path $env:INSTALL_MODULE_PATH "\\Install-Databases.ps1") -InstallPath $env:INSTALL_PATH -DataPath $env:DATA_PATH -ModulePath 'C:\\temp\\sc_sfmc_ce' -DatabasePrefix Sitecore; 

FROM sc_sfmc_ce as development

RUN & (Join-Path $env:INSTALL_MODULE_PATH "\\Install-Databases.ps1") -InstallPath $env:INSTALL_PATH -DataPath $env:DATA_PATH -ModulePath $env:INSTALL_MODULE_PATH -DatabasePrefix Sitecore; ` 
    Remove-Item $env:INSTALL_MODULE_PATH -Recurse
