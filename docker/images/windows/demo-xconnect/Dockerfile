# escape=`
ARG BASE_IMAGE
ARG SOLUTION_IMAGE

FROM ${SOLUTION_IMAGE} as solution

FROM $BASE_IMAGE as production

COPY --from=solution /solution/xconnect/ /inetpub/wwwroot/
COPY ./data/transforms/ /inetpub/wwwroot/transforms/

RUN (Get-ChildItem -Path '/inetpub/wwwroot/transforms/sc.XConnect.Collection.Model.Plugins.xml.xdt' -Recurse ) | `
    ForEach-Object { & '/tools/scripts/Invoke-XdtTransform.ps1' -Path '/inetpub/wwwroot/App_Data/Config/Sitecore/Collection/sc.XConnect.Collection.Model.Plugins.xml' -XdtPath $_.FullName `
    -XdtDllPath '/tools/bin/Microsoft.Web.XmlTransform.dll'; };