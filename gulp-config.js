module.exports = function () {
    var instanceRoot = "C:\\inetpub\\wwwroot\\habitathome.dev.local";   
    var config = {
        websiteRoot: instanceRoot + "\\",
        instanceUrl: "https://habitathome.dev.local/",
        xConnectRoot: "C:\\inetpub\\wwwroot\\habitathome_xconnect.dev.local\\",
        sitecoreLibraries: instanceRoot + "\\bin",
        solutionName: "HabitatHome",                          
        buildConfiguration: "preview",
        buildToolsVersion: 15.0,
        buildMaxCpuCount: 1,
        buildVerbosity: "minimal",
        buildPlatform: "Any CPU",
        publishPlatform: "AnyCpu",
        runCleanBuilds: false,
        messageStatisticsApiKey: "97CC4FC13A814081BF6961A3E2128C5B",
        marketingDefinitionsApiKey: "DF7D20E837254C6FBFA2B854C295CB61"
    };
    return config;
}
