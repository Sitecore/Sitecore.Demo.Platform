# escape=`
ARG BASE_IMAGE
ARG SFMCBDE_ASSETS

FROM ${SFMCBDE_ASSETS} as sfmcbde

FROM ${BASE_IMAGE}

COPY --from=sfmcbde /module/tenantservice/content /inetpub/wwwroot

# Fix for missing support for environment variable to set config.json
COPY global.asax /inetpub/wwwroot/global.asax
COPY web.config /inetpub/wwwroot/web.config
