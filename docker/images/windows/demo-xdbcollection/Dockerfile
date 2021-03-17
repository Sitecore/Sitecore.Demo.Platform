# escape=`
ARG BASE_IMAGE
ARG SOLUTION_IMAGE
# ARG ASSETS_IMAGE

# FROM $ASSETS_IMAGE as assets
FROM ${SOLUTION_IMAGE} as solution

FROM $BASE_IMAGE as production

# COPY --from=assets ["/tools/", "/tools/"]
COPY --from=solution /solution/xconnect/ /inetpub/wwwroot/