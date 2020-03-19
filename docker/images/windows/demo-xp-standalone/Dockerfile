# escape=`
ARG BASE_IMAGE
ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE

FROM $BASE_IMAGE as build
ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE

# SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

FROM $BASE_IMAGE as def

ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE

COPY --from=build ["c:\\inetpub\\wwwroot", "c:\\inetpub\\wwwroot"]
COPY ["${DEF}", "C:\\temp\\def\\"]
RUN Expand-Archive -Path $('c:\\temp\\def\\' +$env:DEF) -DestinationPath 'C:\\temp\\def'; `
    Copy-Item -Path 'C:\\temp\\def\\Content\\Website\\*' -Destination 'C:\\inetpub\\wwwroot' -Recurse -Force; 

FROM $BASE_IMAGE as sc_dam

ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE

COPY --from=def ["c:\\inetpub\\wwwroot", "c:\\inetpub\\wwwroot"]
COPY ["${SC_DAM}", "C:\\temp\\sc_dam\\"] 
RUN Expand-Archive -Path $('c:\\temp\\sc_dam\\' +$env:SC_DAM) -DestinationPath 'C:\\temp\\sc_dam'; `
    Copy-Item -Path 'C:\\temp\\sc_dam\\Content\\Website\\*' -Destination 'C:\\inetpub\\wwwroot' -Recurse -Force; 

FROM $BASE_IMAGE as sc_cmp

ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE

COPY --from=sc_dam ["c:\\inetpub\\wwwroot", "c:\\inetpub\\wwwroot"]
COPY ["${SC_CMP}", "C:\\temp\\sc_cmp\\"] 
RUN Expand-Archive -Path $('c:\\temp\\sc_cmp\\' +$env:SC_CMP) -DestinationPath 'C:\\temp\\sc_cmp'; `
    Copy-Item -Path 'C:\\temp\\sc_cmp\\Content\\Website\\*' -Destination 'C:\\inetpub\\wwwroot' -Recurse -Force; 

FROM $BASE_IMAGE as sc_sfmc_bse

ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE

COPY --from=sc_cmp ["c:\\inetpub\\wwwroot", "c:\\inetpub\\wwwroot"]
COPY ["${SC_SFMC_BDE}", "C:\\temp\\sc_sfmc_bde\\"] 
RUN Expand-Archive -Path $('c:\\temp\\sc_sfmc_bde\\' +$env:SC_SFMC_BDE) -DestinationPath 'C:\\temp\\sc_sfmc_bde'; `
    Copy-Item -Path 'C:\\temp\\sc_sfmc_bde\\Content\\Website\\*' -Destination 'C:\\inetpub\\wwwroot' -Recurse -Force; 

FROM $BASE_IMAGE as sc_sfmc_ce

ARG DEF
ARG SC_DAM
ARG SC_CMP
ARG SC_SFMC_BDE
ARG SC_SFMC_CE

COPY --from=sc_sfmc_bse ["c:\\inetpub\\wwwroot", "c:\\inetpub\\wwwroot"]
COPY ["${SC_SFMC_CE}", "C:\\temp\\sc_sfmc_ce\\"] 
RUN Expand-Archive -Path $('c:\\temp\\sc_sfmc_ce\\' +$env:SC_SFMC_CE) -DestinationPath 'C:\\temp\\sc_sfmc_ce'; `
    Copy-Item -Path 'C:\\temp\\sc_sfmc_ce\\Content\\Website\\*' -Destination 'C:\\inetpub\\wwwroot' -Recurse -Force; 

FROM $BASE_IMAGE as development

COPY --from=sc_sfmc_ce ["c:\\inetpub\\wwwroot", "c:\\inetpub\\wwwroot"]
COPY data/ /inetpub/wwwroot

#find transform files and do transformation
RUN (Get-ChildItem -Path 'C:\\inetpub\\wwwroot\\transforms\\web*.xdt' -Recurse ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\web.config' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; };
	
#find transform files and do transformation
RUN (Get-ChildItem -Path 'C:\\inetpub\\wwwroot\\transforms\\ConnectionStrings*.xdt' -Recurse ) | `
    ForEach-Object { & 'C:\\tools\\scripts\\Invoke-XdtTransform.ps1' -Path 'C:\\inetpub\\wwwroot\\App_Config\\ConnectionStrings.config' -XdtPath $_.FullName `
    -XdtDllPath 'C:\\tools\\bin\\Microsoft.Web.XmlTransform.dll'; }; 
