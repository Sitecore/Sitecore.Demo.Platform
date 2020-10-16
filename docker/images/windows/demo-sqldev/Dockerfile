# escape=`

ARG BASE_IMAGE
ARG BUILD_IMAGE

FROM ${BUILD_IMAGE} as npm

RUN curl.exe -o node.zip https://nodejs.org/dist/v12.16.3/node-v12.16.3-win-x64.zip; `
    mkdir 'C:\\Program Files\\node'; `
    tar.exe -xf node.zip -C 'C:\\Program Files\\node' --strip-components=1; `
    Remove-Item node.zip

RUN setx PATH $($env:PATH + ';C:\\Program Files\\node;') /M


FROM npm as build

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

COPY FrontEnd/Lighthouse/.npmrc ./FrontEnd/Lighthouse/
COPY FrontEnd/Lighthouse/package.json ./FrontEnd/Lighthouse/
COPY FrontEnd/Lighthouse/npm-shrinkwrap.template.json ./FrontEnd/Lighthouse/npm-shrinkwrap.json
WORKDIR /FrontEnd/Lighthouse
RUN npm install
WORKDIR /

COPY FrontEnd/ ./FrontEnd

COPY items/ ./items

COPY build-themes.ps1 .

RUN New-Item -ItemType SymbolicLink -Path "C:\FrontEnd\Fins\node_modules" -Target "C:\FrontEnd\Lighthouse\node_modules" -Verbose
RUN New-Item -ItemType SymbolicLink -Path "C:\FrontEnd\Financial\node_modules" -Target "C:\FrontEnd\Lighthouse\node_modules" -Verbose

RUN .\build-themes.ps1

COPY scripts/Packaging/ /packaging

RUN Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force

RUN .\packaging\generate-update-package.ps1 -target /items -output /output

FROM $BASE_IMAGE as data

COPY --from=build /output/ /output/

COPY docker/images/windows/demo-sqldev/data/ /output

RUN C:\DeployDatabases.ps1 -ResourcesDirectory "/output/security"

RUN C:\DeployDatabases.ps1 -ResourcesDirectory "/output/data"

RUN C:\DeployDatabases.ps1 -ResourcesDirectory "/output/descendants"

RUN Remove-Item "/output" -Recurse

FROM data as production

ENV USER_PASSWORD="b"
ENV ADMIN_USER_NAME="sitecore\superuser"
ENV SITECORE_ADMIN_PASSWORD="b"
ENV DISABLE_DEFAULT_ADMIN=FALSE
ENV EXM_BASE_URL=http://cd
ENV SQL_SERVER="mssql"

COPY /docker/images/windows/demo-sqldev/sql /sql
COPY /docker/images/windows/demo-sqldev/HashPassword.ps1 /docker/images/windows/demo-sqldev/start-override.ps1 /

CMD .\start-override -sa_password $env:sa_password -ACCEPT_EULA $env:ACCEPT_EULA -attach_dbs \"$env:attach_dbs\" -Verbose