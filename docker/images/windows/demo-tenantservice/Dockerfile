# escape=`
ARG BASE_IMAGE
ARG SFMCBDE_ASSETS

FROM ${SFMCBDE_ASSETS} as sfmcbde

FROM ${BASE_IMAGE}

COPY --from=sfmcbde /module/tenantservice/content /inetpub/wwwroot