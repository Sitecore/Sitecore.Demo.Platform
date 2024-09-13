# escape=`
ARG BASE_IMAGE
ARG TOOLS_ASSETS
ARG SOLUTION_IMAGE
ARG GP_IMAGE

FROM ${TOOLS_ASSETS} as tools
FROM ${GP_IMAGE} as gp

FROM ${SOLUTION_IMAGE} as solution

FROM $BASE_IMAGE as production

COPY --from=tools /tools/ /tools/

COPY --from=solution /solution/xconnect/App_Data/jobs/continuous/IndexWorker /service/

COPY --from=gp \module\xconnect\content\App_Data\jobs\continuous\IndexWorker\ C:\service
