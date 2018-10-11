#addin "Cake.XdtTransform"
#addin "Cake.Powershell"
#addin "Cake.Http"
#addin "Cake.Json"
#addin "Newtonsoft.Json"


#load "local:?path=CakeScripts/helper-methods.cake"


var target = Argument<string>("Target", "Default");
var configuration = new Configuration();
var cakeConsole = new CakeConsole();
var configJsonFile = "cake-config.json";
var unicornSyncScript = $"./scripts/Unicorn/Sync.ps1";

/*===============================================
================ MAIN TASKS =====================
===============================================*/

Setup(context =>
{
	cakeConsole.ForegroundColor = ConsoleColor.Yellow;
	PrintHeader(ConsoleColor.DarkGreen);
	
    var configFile = new FilePath(configJsonFile);
    configuration = DeserializeJsonFromFile<Configuration>(configFile);
});


Task("Default")
.WithCriteria(configuration != null)
.IsDependentOn("Clean")
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("Modify-PublishSettings")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Modify-Unicorn-Source-Folder")
.IsDependentOn("Publish-Transforms")
.IsDependentOn("Post-Deploy");

Task("Post-Deploy")
.IsDependentOn("Sync-Unicorn")
.IsDependentOn("Publish-xConnect-Project")
.IsDependentOn("Deploy-EXM-Campaigns")
.IsDependentOn("Deploy-Marketing-Definitions")
.IsDependentOn("Rebuild-Core-Index")
.IsDependentOn("Rebuild-Master-Index")
.IsDependentOn("Rebuild-Web-Index");

Task("Quick-Deploy")
.WithCriteria(configuration != null)
.IsDependentOn("Clean")
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("Modify-PublishSettings")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Modify-Unicorn-Source-Folder")
.IsDependentOn("Publish-Transforms")
.IsDependentOn("Publish-xConnect-Project");

/*===============================================
================= SUB TASKS =====================
===============================================*/

Task("Clean").Does(() => {
    CleanDirectories($"{configuration.SourceFolder}/**/obj");
    CleanDirectories($"{configuration.SourceFolder}/**/bin");
});

Task("Copy-Sitecore-Lib")
    .WithCriteria(()=>(configuration.BuildConfiguration == "preview"))
    .Does(()=> {
        var files = GetFiles($"{configuration.WebsiteRoot}/bin/Sitecore*.dll");
        var destination = "./lib/Sitecore";
        EnsureDirectoryExists(destination);
        CopyFiles(files, destination);
}); 
Task("Publish-All-Projects")
.IsDependentOn("Build-Solution")
.IsDependentOn("Publish-Foundation-Projects")
.IsDependentOn("Publish-Feature-Projects")
.IsDependentOn("Publish-Project-Projects");


Task("Build-Solution").Does(() => {
    MSBuild(configuration.SolutionFile, cfg => InitializeMSBuildSettings(cfg));
});

Task("Publish-Foundation-Projects").Does(() => {
    PublishProjects(configuration.FoundationSrcFolder, configuration.WebsiteRoot);
});

Task("Publish-Feature-Projects").Does(() => {
    PublishProjects(configuration.FeatureSrcFolder, configuration.WebsiteRoot);
});

Task("Publish-Project-Projects").Does(() => {
    var common = $"{configuration.ProjectSrcFolder}\\Common";
    var habitat = $"{configuration.ProjectSrcFolder}\\Habitat";
    var habitatHome = $"{configuration.ProjectSrcFolder}\\HabitatHome";
    var habitatHomeBasic = $"{configuration.ProjectSrcFolder}\\HabitatHomeBasic";

    PublishProjects(common, configuration.WebsiteRoot);
    PublishProjects(habitat, configuration.WebsiteRoot);
    PublishProjects(habitatHome, configuration.WebsiteRoot);
    PublishProjects(habitatHomeBasic, configuration.WebsiteRoot);
});

Task("Publish-xConnect-Project").Does(() => {
    var xConnectProject = $"{configuration.ProjectSrcFolder}\\xConnect";

    PublishProjects(xConnectProject, configuration.XConnectRoot);
});

Task("Apply-Xml-Transform").Does(() => {
    var layers = new string[] { configuration.FoundationSrcFolder, configuration.FeatureSrcFolder, configuration.ProjectSrcFolder};

    foreach(var layer in layers)
    {
        Transform(layer);
    }
});

Task("Publish-Transforms").Does(() => {
    var layers = new string[] { configuration.FoundationSrcFolder, configuration.FeatureSrcFolder, configuration.ProjectSrcFolder};
    var destination = $@"{configuration.WebsiteRoot}\temp\transforms";

    CreateFolder(destination);

    try
    {
        var files = new List<string>();
        foreach(var layer in layers)
        {
            var xdtFiles = GetTransformFiles(layer).Select(x => x.FullPath).ToList();
            files.AddRange(xdtFiles);
        }   

        CopyFiles(files, destination, preserveFolderStructure: true);
    }
    catch (System.Exception ex)
    {
        WriteError(ex.Message);
    }
});

Task("Modify-Unicorn-Source-Folder").Does(() => {
    var zzzDevSettingsFile = File($"{configuration.WebsiteRoot}/App_config/Include/Project/z.Common.Website.DevSettings.config");
    
	var rootXPath = "configuration/sitecore/sc.variable[@name='{0}']/@value";
    var sourceFolderXPath = string.Format(rootXPath, "sourceFolder");
    var directoryPath = MakeAbsolute(new DirectoryPath(configuration.SourceFolder)).FullPath;

    var xmlSetting = new XmlPokeSettings {
        Namespaces = new Dictionary<string, string> {
            {"patch", @"http://www.sitecore.net/xmlconfig/"}
        }
    };
    XmlPoke(zzzDevSettingsFile, sourceFolderXPath, directoryPath, xmlSetting);
});

Task("Modify-PublishSettings").Does(() => {
    var publishSettingsOriginal = File($"{configuration.ProjectFolder}/publishsettings.targets");
    var destination = $"{configuration.ProjectFolder}/publishsettings.targets.user";

    CopyFile(publishSettingsOriginal,destination);

	var importXPath = "/ns:Project/ns:Import";

    var publishUrlPath = "/ns:Project/ns:PropertyGroup/ns:publishUrl";

    var xmlSetting = new XmlPokeSettings {
        Namespaces = new Dictionary<string, string> {
            {"ns", @"http://schemas.microsoft.com/developer/msbuild/2003"}
        }
    };
    XmlPoke(destination,importXPath,null,xmlSetting);
    XmlPoke(destination,publishUrlPath,$"{configuration.InstanceUrl}",xmlSetting);
});
Task("Sync-Unicorn").Does(() => {
    var unicornUrl = configuration.InstanceUrl + "unicorn.aspx";
    Information("Sync Unicorn items from url: " + unicornUrl);

    var authenticationFile = new FilePath($"{configuration.WebsiteRoot}/App_config/Include/Unicorn.SharedSecret.config");
    var xPath = "/configuration/sitecore/unicorn/authenticationProvider/SharedSecret";

    string sharedSecret = XmlPeek(authenticationFile, xPath);

    
    StartPowershellFile(unicornSyncScript, new PowershellSettings()
                                                        .SetFormatOutput()
                                                        .SetLogOutput()
                                                        .WithArguments(args => {
                                                            args.Append("secret", sharedSecret)
                                                                .Append("url", unicornUrl);
                                                        }));
});

Task("Deploy-EXM-Campaigns").Does(() => {
    DeployExmCampaigns();
}).OnError(() => {
	Information("Retrying Deploy-EXM-Campaigns");
	DeployExmCampaigns();
});;

Task("Deploy-Marketing-Definitions").Does(() => {
    var url = $"{configuration.InstanceUrl}utilities/deploymarketingdefinitions.aspx?apiKey={configuration.MarketingDefinitionsApiKey}";
    var responseBody = HttpGet(url, settings =>
	{
		settings.AppendHeader("Connection", "keep-alive");
	});

    Information(responseBody);
});

Task("Rebuild-Core-Index").Does(() => {
    RebuildIndex("sitecore_core_index");
});

Task("Rebuild-Master-Index").Does(() => {
    RebuildIndex("sitecore_master_index");
});

Task("Rebuild-Web-Index").Does(() => {
    RebuildIndex("sitecore_web_index");
});



RunTarget(target);