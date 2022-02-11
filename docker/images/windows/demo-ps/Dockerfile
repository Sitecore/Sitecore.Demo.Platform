# escape=`
ARG BASE_IMAGE
ARG STANDALONE_IMAGE
ARG TOOLS_ASSETS

FROM ${TOOLS_ASSETS} as tools

FROM ${STANDALONE_IMAGE} as standalone 

RUN mkdir /res_files
WORKDIR /res_files
RUN Copy-Item /inetpub/wwwroot/App_Data/items/master sitecore/master -Recurse -Force; `
    Copy-Item /inetpub/wwwroot/App_Data/items/web sitecore/web -Recurse -Force; `
    If (Test-Path -Path \"c:/inetpub/wwwroot/sitecore modules/items/master\") `
    { `
        Copy-Item \"c:/inetpub/wwwroot/sitecore modules/items/master\" modules/master -Recurse -Force; `
    } `
    If (Test-Path -Path \"c:/inetpub/wwwroot/sitecore modules/items/web\") `
    { `
        Copy-Item \"c:/inetpub/wwwroot/sitecore modules/items/web\" modules/web -Recurse -Force; `
    }

FROM $BASE_IMAGE as SPS

COPY --from=standalone /res_files/ /sps/items/
COPY --from=tools /tools/ /tools/

WORKDIR /sps