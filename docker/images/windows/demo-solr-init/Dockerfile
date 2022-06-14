# escape=`
ARG BASE_IMAGE
ARG SXA_ASSETS

FROM ${SXA_ASSETS} as sxa_assets

FROM $BASE_IMAGE

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

COPY --from=sxa_assets C:\module\solr\cores-sxa.json C:\data\cores-sxa.json

ENTRYPOINT ["powershell.exe", ".\\Start.ps1", "-SitecoreSolrConnectionString $env:SITECORE_SOLR_CONNECTION_STRING", `
    "-SolrCorePrefix $env:SOLR_CORE_PREFIX_NAME", `
    "-SolrSitecoreConfigsetSuffixName $env:SOLR_SITECORE_CONFIGSET_SUFFIX_NAME", `
    "-SolrReplicationFactor $env:SOLR_REPLICATION_FACTOR", `
    "-SolrNumberOfShards $env:SOLR_NUMBER_OF_SHARDS", `
    "-SolrMaxShardsPerNodes $env:SOLR_MAX_SHARDS_NUMBER_PER_NODES", `
    "-SolrXdbSchemaFile .\\data\\schema.json", `
    "-SolrCollectionsToDeploy $env:SOLR_COLLECTIONS_TO_DEPLOY"]