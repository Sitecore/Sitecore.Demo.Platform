# escape=`
ARG BASE_IMAGE
ARG TOOLS_ASSETS
ARG SITECORE_AI_ASSETS
ARG DCRM_ASSETS
ARG SOLUTION_IMAGE

FROM ${SOLUTION_IMAGE} as solution
FROM ${TOOLS_ASSETS} as tools
FROM ${SITECORE_AI_ASSETS} as sitecore_ai_assets
FROM ${DCRM_ASSETS} as dcrm_assets

FROM ${BASE_IMAGE} as modules

COPY --from=tools /tools/ /tools/
COPY --from=sitecore_ai_assets /module/xconnect /inetpub/wwwroot

FROM modules as production

COPY --from=solution /solution/xconnect/ /inetpub/wwwroot/
COPY --from=dcrm_assets /module/models /inetpub/wwwroot/App_Data/Models/
COPY ./data/transforms/ /inetpub/wwwroot/transforms/

RUN (Get-ChildItem -Path '/inetpub/wwwroot/transforms/sc.XConnect.Collection.Model.Plugins.xml.xdt' -Recurse ) | `
    ForEach-Object { & '/tools/scripts/Invoke-XdtTransform.ps1' -Path '/inetpub/wwwroot/App_Data/Config/Sitecore/Collection/sc.XConnect.Collection.Model.Plugins.xml' -XdtPath $_.FullName `
    -XdtDllPath '/tools/bin/Microsoft.Web.XmlTransform.dll'; };