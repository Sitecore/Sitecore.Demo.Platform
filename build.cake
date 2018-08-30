#addin "Cake.XdtTransform"
#addin "Cake.Powershell"
#addin "Cake.Http"
#addin "Cake.Json"
#addin "Newtonsoft.Json"


using System.Text.RegularExpressions;
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
.IsDependentOn("Nuget-Restore")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Modify-Unicorn-Source-Folder")
.IsDependentOn("Sync-Unicorn")
.IsDependentOn("Publish-Transforms")
.IsDependentOn("Publish-xConnect-Project")
.IsDependentOn("Deploy-EXM-Campaigns")
.IsDependentOn("Deploy-Marketing-Definitions")
.IsDependentOn("Rebuild-Core-Index")
.IsDependentOn("Rebuild-Master-Index")
.IsDependentOn("Rebuild-Web-Index");

Task("Quick-Deploy")
.WithCriteria(configuration != null)
.IsDependentOn("Clean")
.IsDependentOn("Nuget-Restore")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Publish-Transforms")
.IsDependentOn("Publish-xConnect-Project");

/*===============================================
================= SUB TASKS =====================
===============================================*/

Task("Clean").Does(() => {
    CleanDirectories($"{configuration.SourceFolder}/**/obj");
    CleanDirectories($"{configuration.SourceFolder}/**/bin");
});

Task("Nuget-Restore").Does(() => NuGetRestore(configuration.SolutionFile));

Task("Publish-All-Projects")
.IsDependentOn("Build-Solution")
.IsDependentOn("Publish-Foundation-Projects")
.IsDependentOn("Publish-Feature-Projects")
.IsDependentOn("Publish-Project-Projects");


Task("Build-Solution").Does(() => {
    MSBuild(configuration.SolutionFile, cfg => cfg.SetConfiguration(configuration.BuildConfiguration)
                                                  .SetVerbosity(Verbosity.Minimal)
                                                  .SetMSBuildPlatform(MSBuildPlatform.Automatic)
                                                  .SetPlatformTarget(PlatformTarget.MSIL)
                                                  .UseToolVersion(MSBuildToolVersion.VS2017));
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

    PublishProjects(common, configuration.WebsiteRoot);
    PublishProjects(habitat, configuration.WebsiteRoot);
    PublishProjects(habitatHome, configuration.WebsiteRoot);
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
    var directoryPath = new DirectoryPath(configuration.SourceFolder);

    try
    {
        foreach(var layer in layers)
        {
            var xdtFiles = GetTransformFiles(layer);
            foreach(var xdtFile in xdtFiles)
            {
                var destinationFile = xdtFile.FullPath.Replace(directoryPath.FullPath, destination);
                CopyFile(xdtFile, new FilePath(destinationFile));
            }
        }   
    }
    catch (System.Exception ex)
    {
        cakeConsole.WriteError(ex.Message);
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
    var url = $"{configuration.InstanceUrl}utilities/deployemailcampaigns.aspx?apiKey={configuration.MessageStatisticsApiKey}";
    string responseBody = HttpGet(url);

    Information(responseBody);
});

Task("Deploy-Marketing-Definitions").Does(() => {
    var url = $"{configuration.InstanceUrl}utilities/deploymarketingdefinitions.aspx?apiKey={configuration.MarketingDefinitionsApiKey}";
    string responseBody = HttpGet(url);

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

/*===============================================
================= HELPER METHODS ================
===============================================*/

public class Configuration
{
    public string WebsiteRoot {get;set;}
    public string XConnectRoot {get;set;}
    public string InstanceUrl {get;set;}
    public string SolutionName {get;set;}
    public string ProjectFolder {get;set;}
    public string BuildConfiguration {get;set;}
    public string MessageStatisticsApiKey {get;set;}
    public string MarketingDefinitionsApiKey {get;set;}

    public string SourceFolder => $"{ProjectFolder}\\src";
    public string FoundationSrcFolder => $"{SourceFolder}\\Foundation";
    public string FeatureSrcFolder => $"{SourceFolder}\\Feature";
    public string ProjectSrcFolder => $"{SourceFolder}\\Project";

    public string SolutionFile => $"{ProjectFolder}\\{SolutionName}";
}

private void PrintHeader(ConsoleColor foregroundColor)
{
    cakeConsole.ForegroundColor = foregroundColor;
    cakeConsole.WriteLine("     "); 
    cakeConsole.WriteLine("     "); 
    cakeConsole.WriteLine(@"   ) )       /\\                  ");
    cakeConsole.WriteLine(@"  =====     /  \\                 ");                     
    cakeConsole.WriteLine(@" _|___|____/ __ \\____________    ");
    cakeConsole.WriteLine(@"|:::::::::/ ==== \\:::::::::::|   ");
    cakeConsole.WriteLine(@"|:::::::::/ ====  \\::::::::::|   ");
    cakeConsole.WriteLine(@"|::::::::/__________\:::::::::|  ");
    cakeConsole.WriteLine(@"|_________|  ____  |_________|                                                               ");
    cakeConsole.WriteLine(@"| ______  | / || \\ | _______ |            _   _       _     _ _        _     _   _");
    cakeConsole.WriteLine(@"||  |   | | ====== ||   |   ||           | | | |     | |   (_) |      | |   | | | |");
    cakeConsole.WriteLine(@"||--+---| | |    | ||---+---||           | |_| | __ _| |__  _| |_ __ _| |_  | |_| | ___  _ __ ___   ___");
    cakeConsole.WriteLine(@"||__|___| | |   o| ||___|___||           |  _  |/ _` | '_ \\| | __/ _` | __| |  _  |/ _ \\| '_ ` _ \\ / _ \\");
    cakeConsole.WriteLine(@"|======== | |____| |=========|           | | | | (_| | |_) | | || (_| | |_  | | | | (_) | | | | | |  __/");
    cakeConsole.WriteLine(@"(^^-^^^^^- |______|-^^^--^^^)            \\_| |_/\\__,_|_.__/|_|\\__\\__,_|\\__| \\_| |_/\\___/|_| |_| |_|\\___|");
    cakeConsole.WriteLine(@"(,, , ,, , |______|,,,, ,, ,)");
    cakeConsole.WriteLine(@"','',,,,'  |______|,,,',',;;");
    cakeConsole.WriteLine(@"     "); 
    cakeConsole.WriteLine(@"     "); 
    cakeConsole.WriteLine(@" --------------------  ------------------");
    cakeConsole.WriteLine("   " + "The Habitat Home source code, tools and processes are examples of Sitecore Features.");
    cakeConsole.WriteLine("   " + "Habitat Home is not supported by Sitecore and should be used at your own risk.");
    cakeConsole.WriteLine("     "); 
    cakeConsole.WriteLine("     ");
    cakeConsole.ResetColor();
}

private void PublishProjects(string rootFolder, string websiteRoot)
{
    var projects = GetFiles($"{rootFolder}\\**\\code\\*.csproj");

    foreach (var project in projects)
    {
        MSBuild(project, cfg => cfg.SetConfiguration(configuration.BuildConfiguration)
                                   .SetVerbosity(Verbosity.Minimal)
                                   .SetMSBuildPlatform(MSBuildPlatform.Automatic)
                                   .SetPlatformTarget(PlatformTarget.MSIL)
                                   .UseToolVersion(MSBuildToolVersion.VS2017)
                                   .WithTarget("Clean")
                                   .WithTarget("Build")
                                   .WithProperty("DeployOnBuild", "true")
                                   .WithProperty("DeployDefaultTarget", "WebPublish")
                                   .WithProperty("WebPublishMethod", "FileSystem")
                                   .WithProperty("DeleteExistingFiles", "false")
                                   .WithProperty("publishUrl", websiteRoot)
                                   .WithProperty("BuildProjectReferences", "false"));
    }
}

private FilePathCollection GetTransformFiles(string rootFolder)
{
    Func<IFileSystemInfo, bool> exclude_obj_bin_folder =fileSystemInfo => !fileSystemInfo.Path.FullPath.Contains("/obj/") || !fileSystemInfo.Path.FullPath.Contains("/bin/");

    var xdtFiles = GetFiles($"{rootFolder}\\**\\*.xdt", exclude_obj_bin_folder);

    return xdtFiles;
}

private void Transform(string rootFolder) {
    var xdtFiles = GetTransformFiles(rootFolder);

    foreach (var file in xdtFiles)
    {
        Information($"Applying configuration transform:{file.FullPath}");
        var fileToTransform = Regex.Replace(file.FullPath, ".+code/(.+)/*.xdt", "$1");
        var sourceTransform = $"{configuration.WebsiteRoot}\\{fileToTransform}";
        
        XdtTransformConfig(sourceTransform			                // Source File
                            , file.FullPath			                // Tranforms file (*.xdt)
                            , sourceTransform);		                // Target File
    }
}

private void RebuildIndex(string indexName)
{
    var url = $"{configuration.InstanceUrl}utilities/indexrebuild.aspx?index={indexName}";
    string responseBody = HttpGet(url);
}

RunTarget(target);