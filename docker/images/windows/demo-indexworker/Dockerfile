# escape=`
ARG BASE_IMAGE
ARG SOLUTION_IMAGE

FROM ${SOLUTION_IMAGE} as solution

FROM $BASE_IMAGE as production

COPY --from=solution /solution/xconnect/App_Data/jobs/continuous/IndexWorker /worker/
