# escape=`

ARG SOLUTION_DATA_IMAGE
ARG SOLUTION_XCONNECT_IMAGE
ARG SOLUTION_CONTENT_IMAGE

ARG BASE_IMAGE

FROM ${SOLUTION_CONTENT_IMAGE} as content
FROM ${SOLUTION_DATA_IMAGE} as data
FROM ${SOLUTION_XCONNECT_IMAGE} as xconnect

FROM $BASE_IMAGE

COPY --from=content /solution/cm /solution/cm
COPY --from=content /solution/cd /solution/cd
COPY --from=data /solution/db /solution/db
COPY --from=xconnect /solution/xconnect /solution/xconnect