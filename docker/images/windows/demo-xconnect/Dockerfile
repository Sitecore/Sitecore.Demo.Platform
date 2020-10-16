# escape=`
ARG BASE_IMAGE
ARG BUILD_IMAGE
ARG ASSETS_IMAGE

FROM $ASSETS_IMAGE as assets
FROM ${BUILD_IMAGE} as build

ARG INTERNAL_NUGET_SOURCE
ARG SYSTEM_ACCESSTOKEN
WORKDIR /src
COPY src/Project/xConnect .

RUN msbuild /m:1 /v:minimal /p:Configuration="Debug" /p:Platform=AnyCPU /p:DeployOnBuild=true /p:DeployDefaultTarget=WebPublish /p:WebPublishMethod=FileSystem /p:DeleteExistingFiles=false /p:publishUrl=C:\out\demo-xconnect /p:BuildProjectReferences=false /target:Build "code/Sitecore.Demo.Platform.XConnect.csproj"

FROM $BASE_IMAGE as production

COPY --from=assets ["C:\\tools\\", "C:\\tools\\"]

COPY --from=build /out/demo-xconnect/ /inetpub/wwwroot/