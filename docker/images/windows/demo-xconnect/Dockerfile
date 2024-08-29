# escape=`
ARG BASE_IMAGE
ARG TOOLS_ASSETS
ARG SOLUTION_IMAGE
ARG GP_IMAGE

FROM ${SOLUTION_IMAGE} as solution
FROM ${TOOLS_ASSETS} as tools
FROM ${GP_IMAGE} as gp

FROM ${BASE_IMAGE} as modules

COPY --from=tools /tools/ /tools/

FROM modules as production

COPY --from=solution /solution/xconnect/ /inetpub/wwwroot/

COPY ./data/transforms/ /inetpub/wwwroot/transforms/

COPY --from=gp \module\xconnect\content\App_Data\Config C:\inetpub\wwwroot\App_Data\Config
COPY --from=gp \module\xconnect\content\App_Data\Models C:\inetpub\wwwroot\App_Data\Models

RUN (Get-ChildItem -Path '/inetpub/wwwroot/transforms/sc.XConnect.Collection.Model.Plugins.xml.xdt' -Recurse ) | `
    ForEach-Object { & '/tools/scripts/Invoke-XdtTransform.ps1' -Path '/inetpub/wwwroot/App_Data/Config/Sitecore/Collection/sc.XConnect.Collection.Model.Plugins.xml' -XdtPath $_.FullName `
    -XdtDllPath '/tools/bin/Microsoft.Web.XmlTransform.dll'; };