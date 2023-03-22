# escape=`
ARG BASE_IMAGE
ARG TOOLS_ASSETS
ARG SOLUTION_IMAGE

FROM ${TOOLS_ASSETS} as tools

FROM ${SOLUTION_IMAGE} as solution

FROM $BASE_IMAGE as production

COPY --from=tools /tools/ /tools/

COPY --from=solution /solution/xconnect/App_Data/jobs/continuous/IndexWorker /service/
