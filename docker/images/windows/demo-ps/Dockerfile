# escape=`
ARG BASE_IMAGE
ARG TOOLS_ASSETS

FROM ${TOOLS_ASSETS} as tools

FROM $BASE_IMAGE as production

COPY --from=tools /tools/ /tools/

WORKDIR /sps