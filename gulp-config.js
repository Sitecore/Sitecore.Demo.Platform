module.exports = function () {
    var instanceRoot = "C:\\inetpub\\wwwroot\\habitat.dev.local";
    var config = {
        websiteRoot: instanceRoot + "\\",
        xConnectRoot: "C:\\inetpub\\wwwroot\\habitat_xconnect.dev.local\\",
        sitecoreLibraries: instanceRoot + "\\bin",
        licensePath: instanceRoot + "\\App_Data\\license.xml",
        packageXmlBasePath: ".\\src\\Project\\Habitat\\code\\App_Data\\packages\\habitat.xml",
        packagePath: instanceRoot + "\\App_Data\\packages",
        solutionName: "Habitat",                          
        buildConfiguration: "Debug",
        buildToolsVersion: 15.0,
        buildMaxCpuCount: 1,
        buildVerbosity: "minimal",
        buildPlatform: "Any CPU",
        publishPlatform: "AnyCpu",
        runCleanBuilds: false
    };
    return config;
}
