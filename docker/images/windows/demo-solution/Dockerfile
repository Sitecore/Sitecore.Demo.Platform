# escape=`

ARG BASE_IMAGE
ARG BUILD_IMAGE
ARG COVEO_ASSETS_IMAGE

FROM $COVEO_ASSETS_IMAGE as coveo_assets

FROM ${BUILD_IMAGE} as npm

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN curl.exe -o node.zip https://nodejs.org/dist/v12.16.3/node-v12.16.3-win-x64.zip; `
    mkdir 'C:\\Program Files\\node'; `
    tar.exe -xf node.zip -C 'C:\\Program Files\\node' --strip-components=1; `
    Remove-Item node.zip
RUN setx PATH $($env:PATH + ';C:\\Program Files\\node;') /M

FROM npm as frontend

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY FrontEnd/Lighthouse/.npmrc ./FrontEnd/Lighthouse/
COPY FrontEnd/Lighthouse/package.json ./FrontEnd/Lighthouse/
COPY FrontEnd/Lighthouse/npm-shrinkwrap.template.json ./FrontEnd/Lighthouse/npm-shrinkwrap.json
WORKDIR /FrontEnd/Lighthouse
RUN npm install
WORKDIR /

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

COPY items/ ./items

COPY FrontEnd/ ./FrontEnd
COPY build-themes.ps1 .

RUN New-Item -ItemType SymbolicLink -Path "C:\FrontEnd\Fins\node_modules" -Target "C:\FrontEnd\Lighthouse\node_modules" -Verbose
RUN New-Item -ItemType SymbolicLink -Path "C:\FrontEnd\Financial\node_modules" -Target "C:\FrontEnd\Lighthouse\node_modules" -Verbose
RUN New-Item -ItemType SymbolicLink -Path "C:\FrontEnd\FinancialBlue\node_modules" -Target "C:\FrontEnd\Lighthouse\node_modules" -Verbose

RUN .\build-themes.ps1

FROM ${BUILD_IMAGE} as data

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY scripts/Packaging/ /packaging

COPY --from=frontend /items /items

# Install latest PackageProvider (required for Sitecore.Courier)
RUN Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force
RUN .\packaging\generate-update-package.ps1 -target /items -output /out/db

FROM ${BUILD_IMAGE} as build

ARG INTERNAL_NUGET_SOURCE
ARG INTERNAL_NUGET_SOURCE_USERNAME="VSTS"
ARG INTERNAL_NUGET_SOURCE_PASSWORD
ARG PRERELEASE="$false"

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY build* nuget.config publishsettings.targets Sitecore.Demo.Platform.sln ./build/
COPY cake-config-container.json ./cake-config.json
COPY --from=coveo_assets /module/cm/content/bin/Coveo.AbstractLayer.dll /module/cm/content/bin/Coveo.Framework.dll /module/cm/content/bin/Coveo.SearchProvider.Rest.dll /module/cm/content/bin/Coveo.SearchProviderBase.dll /lib/
COPY src/ ./src
COPY FrontEnd/ ./FrontEnd
COPY tools/ ./tools

RUN Copy-Item ./build/* .

RUN .\build.ps1 -DeploymentTarget DockerBuild -Target Content-Management -Prerelease:$($env:PRERELEASE)

FROM ${BUILD_IMAGE} as xconnect

ARG INTERNAL_NUGET_SOURCE
ARG INTERNAL_NUGET_SOURCE_USERNAME="VSTS"
ARG INTERNAL_NUGET_SOURCE_PASSWORD

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR /src
COPY src/Project/xConnect .
COPY nuget.config .
COPY docker/images/windows/demo-solution/scripts/ .

RUN .\Add-InternalNugetFeed.ps1

RUN msbuild /m:1 /v:minimal /p:Configuration="Debug" /p:Platform=AnyCPU /p:DeployOnBuild=true /p:DeployDefaultTarget=WebPublish /p:WebPublishMethod=FileSystem /p:DeleteExistingFiles=false /p:publishUrl=C:\out\demo-xconnect /p:BuildProjectReferences=false /target:Build "code/Sitecore.Demo.Platform.XConnect.csproj"

FROM ${BASE_IMAGE} as solution

COPY --from=build /out/demo-standalone /solution/cm
COPY --from=build /out/demo-cd /solution/cd
COPY --from=data /out/db /solution/db
COPY --from=xconnect /out/demo-xconnect /solution/xconnect