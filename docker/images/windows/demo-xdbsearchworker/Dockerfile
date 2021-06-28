# escape=`
ARG BASE_IMAGE
ARG TOOLS_ASSETS
ARG SITECORE_AI_ASSETS
ARG SFMCBDE_ASSETS
ARG SOLUTION_IMAGE

FROM ${TOOLS_ASSETS} as tools
FROM ${SITECORE_AI_ASSETS} as sitecore_ai_assets
FROM ${SFMCBDE_ASSETS} as sfmcbde_assets

FROM ${SOLUTION_IMAGE} as solution

FROM $BASE_IMAGE as production

COPY --from=tools /tools/ /tools/
COPY --from=sitecore_ai_assets /module/xconnect /service
COPY --from=sfmcbde_assets /module/xdbsearchworker/content /service

COPY --from=solution /solution/xconnect/App_Data/jobs/continuous/IndexWorker /service/
