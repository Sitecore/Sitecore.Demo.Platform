#addin nuget:?package=Cake.Azure&version=0.3.0
#addin nuget:?package=Cake.Http&version=0.5.0
#addin nuget:?package=Cake.Json&version=3.0.1
#addin nuget:?package=Cake.Powershell&version=0.4.7
#addin nuget:?package=Cake.XdtTransform&version=0.16.0
#addin nuget:?package=Newtonsoft.Json&version=11.0.1

#load "local:?path=CakeScripts/helper-methods.cake"
#load "local:?path=CakeScripts/xml-helpers.cake"

var target = Argument<string>("Target", "Default");
var configuration = new Configuration();
var cakeConsole = new CakeConsole();
var configJsonFile = "cake-config.json";
var unicornSyncScript = $"./scripts/Unicorn/Sync.ps1";
var deploymentRootPath ="";
var deploymentTarget = "";
bool deployLocal = false;
string topology = null;

var devSitecoreUserName = Argument("DEV_SITECORE_USERNAME", EnvironmentVariable("DEV_SITECORE_USERNAME"));
var devSitecorePassword = Argument("DEV_SITECORE_PASSWORD", EnvironmentVariable("DEV_SITECORE_PASSWORD"));

/*===============================================
================ MAIN TASKS =====================
===============================================*/

Setup(context =>
{
	cakeConsole.ForegroundColor = ConsoleColor.Yellow;
	PrintHeader(ConsoleColor.DarkGreen);
	
    var configFile = new FilePath(configJsonFile);
    configuration = DeserializeJsonFromFile<Configuration>(configFile);
     if(configuration.Topology == "single")
    {
        topology = "XPSingle";
    }
    else if(configuration.Topology == "scaled")
    {
        topology = "XP";
    }
    
    deploymentTarget = Argument<string>("deploymentTarget",configuration.DeploymentTarget);
    
    if (target.Contains("Azure")){
        // DeploymentTarget is set to either Local or OnPrem but the user has selected Deploy to Azure.
        // Automatically switch the deploymentTarget based on the build target
        deploymentTarget = "Azure";
    }
    deployLocal = deploymentTarget == "Local";
    
    switch (deploymentTarget){
        case "OnPrem":
            deploymentRootPath = $"{configuration.DeployFolder}\\{configuration.Version}\\OnPrem\\{topology}";
            break;
        case "Azure":
            deploymentRootPath = $"{configuration.DeployFolder}\\{configuration.Version}\\Cloud\\{topology}";
            break;
        case "Local":
            if (target == "Build-WDP"){
                throw new Exception("Invalid DeploymentTarget in cake-config.json file. Valid values for DeploymentTarget are 'OnPrem' and 'Azure' when using -Target Build-WDP");
            }
            deploymentRootPath = configuration.WebsiteRoot;
            break;
    }

        if ((target.Contains("WDP") || target.Contains("Azure")) && 
    ((string.IsNullOrEmpty(devSitecorePassword)) || (string.IsNullOrEmpty(devSitecoreUserName)))){
        cakeConsole.WriteLine("");
        cakeConsole.WriteLine("");
        Warning("       ***********  WARNING  ***************        ");
        cakeConsole.WriteLine("");
        Warning("You have not supplied your dev.sitecore.com credentials.");
        Warning("Some of the build tasks selected require assets that are hosted on dev.sitecore.com.");
        Warning("If these assets have not previously been downloaded, the script will fail.");
        Warning("You can avoid this warning by supplying values for 'DEV_SITECORE_USERNAME' and 'DEV_SITECORE_PASSWORD' as environment variables or ScriptArgs");
        cakeConsole.WriteLine("");
        Information("Example: .\\build.ps1 -Target Build-WDP -ScriptArgs --DEV_SITECORE_USERNAME=your_user@email.com, --DEV_SITECORE_PASSWORD=<your-password>");
        cakeConsole.WriteLine("");
        Warning("       *************************************        ");
    }
});
   

/*===============================================
============ Local Build - Main Tasks ===========
===============================================*/
Task("Default")
.WithCriteria(configuration != null)
.IsDependentOn("CleanBuildFolders")
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("Modify-PublishSettings")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Publish-xConnect-Project")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Modify-SXA-Variable")
.IsDependentOn("Modify-Unicorn-Source-Folder")
.IsDependentOn("Post-Deploy");

Task("Post-Deploy")
.IsDependentOn("Sync-Unicorn")
.IsDependentOn("Deploy-EXM-Campaigns")
.IsDependentOn("Deploy-Marketing-Definitions")
.IsDependentOn("Rebuild-Core-Index")
.IsDependentOn("Rebuild-Master-Index")
.IsDependentOn("Rebuild-Web-Index")
.IsDependentOn("Rebuild-Test-Index");

Task("Quick-Deploy")
.WithCriteria(configuration != null)
.IsDependentOn("CleanBuildFolders")
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("Modify-PublishSettings")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Modify-Unicorn-Source-Folder")
.IsDependentOn("Modify-SXA-Variable")
.IsDependentOn("Publish-xConnect-Project");

/*===============================================
=========== Packaging - Main Tasks ==============
===============================================*/
Task("Build-WDP")
.WithCriteria(configuration != null)
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("CleanAll")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Publish-xConnect-Project")
.IsDependentOn("Publish-YML")
.IsDependentOn("Prepare-Transform-Files")
.IsDependentOn("Publish-Post-Steps")
.IsDependentOn("Create-WDP");

/*===============================================
======== Azure Deployment - Main Tasks ==========
===============================================*/
Task("Default-Azure")
.WithCriteria(configuration != null)
.IsDependentOn("Build-WDP")
.IsDependentOn("Azure-Upload")
.IsDependentOn("Azure-Deploy");

Task("Azure-Upload")
.WithCriteria(configuration != null)
.IsDependentOn("Run-Prerequisites")
.IsDependentOn("Prepare-Azure-Deploy-CDN")
.IsDependentOn("Azure-Upload-Packages");

Task("Azure-Deploy")
.WithCriteria(configuration != null)
.IsDependentOn("Azure-Upload")
.IsDependentOn("Azure-Site-Deploy");

/*===============================================
================= SUB TASKS =====================
===============================================*/

Task("CleanAll")
.IsDependentOn("CleanBuildFolders")
.IsDependentOn("CleanDeployFolder");

Task("CleanBuildFolders").Does(() => {
    // Clean project build folders
    CleanDirectories($"{configuration.SourceFolder}/**/obj");
    CleanDirectories($"{configuration.SourceFolder}/**/bin");

});

Task("CleanDeployFolder").Does(() => {
var deploymentTarget ="";
if (configuration.DeploymentTarget == "OnPrem" || configuration.DeploymentTarget == "Local"){
    deploymentTarget = "OnPrem";
}
else{
    deploymentTarget = "Cloud";
}
var folderBase = $"{configuration.Version}\\{deploymentTarget}\\{topology}";
    // Clean deployment folders
     string[] folders = { $"\\{folderBase}\\assets\\HabitatHome", $"\\{folderBase}\\assets\\HabitatHomeCD", $"\\{folderBase}\\Website", $"\\{folderBase}\\assets\\habitatHome_xConnect", $"\\{folderBase}\\assets\\Data Exchange Framework\\WDPWorkFolder", $"\\{folderBase}\\assets\\Data Exchange Framework CD\\WDPWorkFolder" };

    foreach (string folder in folders)
    {
        Information($"Cleaning: {folder}");
        if (DirectoryExists($"{configuration.DeployFolder}{folder}"))
        {
            try
            {
                CleanDirectories($"{configuration.DeployFolder}{folder}");
            } catch
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine($"The folder under path \'{configuration.DeployFolder}{folder}\' is still in use by a process. Exiting...");
                Console.ResetColor();
                Environment.Exit(0);
            }
        }
    }
});

/*===============================================
=============== Generic Tasks ===================
===============================================*/

Task("Copy-Sitecore-Lib")
    .WithCriteria(()=>(configuration.BuildConfiguration == "Local"))
    .Does(()=> {
        var files = GetFiles($"{configuration.WebsiteRoot}/bin/Sitecore*.dll");
        var destination = "./lib";
        EnsureDirectoryExists(destination);
        CopyFiles(files, destination);
}); 

Task("Publish-All-Projects")
.IsDependentOn("Build-Solution")
.IsDependentOn("Publish-Core-Project")
.IsDependentOn("Publish-Foundation-Projects")
.IsDependentOn("Publish-Feature-Projects")
.IsDependentOn("Publish-Project-Projects");


Task("Build-Solution")
.IsDependentOn("Copy-Sitecore-Lib")
.Does(() => {
    MSBuild(configuration.SolutionFile, cfg => InitializeMSBuildSettings(cfg));
});

Task("Publish-Foundation-Projects").Does(() => {
    var destination = deploymentRootPath;
    
    if (!deployLocal){
        destination = $"{deploymentRootPath}\\Website\\HabitatHome";
    }
    PublishProjects(configuration.FoundationSrcFolder, destination);
});

Task("Publish-Feature-Projects").Does(() => {
     var destination = deploymentRootPath;
    if (!deployLocal){
        destination = $"{deploymentRootPath}\\Website\\HabitatHome";
    }
     PublishProjects(configuration.FeatureSrcFolder, destination);
});

Task("Publish-Core-Project").Does(() => {	
    var destination = deploymentRootPath;
    if (!deployLocal){
        destination = $"{deploymentRootPath}\\Website\\HabitatHome";
    }
	
	Information("Destination: " + destination);

	var projectFile = $"{configuration.ProjectSrcFolder}\\Build\\Build.Website\\Build.Website.csproj";
	var publishFolder = $"{configuration.ProjectFolder}\\publish";
	
	DotNetCoreRestore(projectFile);

	var settings = new DotNetCorePublishSettings
        {
            OutputDirectory = publishFolder,
			Configuration = configuration.BuildConfiguration
        };
 
    DotNetCorePublish(projectFile, settings);

	// Copy assembly files to webroot
    var assemblyFilesFilter = $@"{publishFolder}\*.dll";
    var assemblyFiles = GetFiles(assemblyFilesFilter).Select(x=>x.FullPath).ToList();
    CopyFiles(assemblyFiles, (destination + "\\bin"), preserveFolderStructure: false);
	
	// Copy other output files to destination webroot
	var ignoredExtensions = new string[] { ".dll", ".exe", ".pdb", ".xdt" };
	var ignoredFiles = new string[] { "web.config", "build.website.deps.json", "build.website.exe.config" };

	var contentFiles = GetFiles($"{publishFolder}\\**\\*")
						.Where(file => !ignoredExtensions.Contains(file.GetExtension().ToLower()))
						.Where(file => !ignoredFiles.Contains(file.Segments.LastOrDefault().ToLower()));

	CopyFiles(contentFiles, destination, preserveFolderStructure: true);
   

	// Apply transforms    	 
    var xdtFiles = GetFiles($"{publishFolder}\\**\\*.xdt");

    foreach (var file in xdtFiles)
    {
        if (file.FullPath.Contains(".azure"))
        {
            continue;
        }
        
        Information($"Applying configuration transform:{file.FullPath}");
        var fileToTransform = Regex.Replace(file.FullPath, ".+transforms/(.+)/*.xdt", "$1");
        fileToTransform = Regex.Replace(fileToTransform, ".sc-internal", "");
        var sourceTransform = $"{configuration.WebsiteRoot}\\{fileToTransform}";
        
        XdtTransformConfig(sourceTransform			                // Source File
                            , file.FullPath			                // Tranforms file (*.xdt)
                            , sourceTransform);		                // Target File
    }
});

Task("Publish-Project-Projects").Does(() => {
    var global = $"{configuration.ProjectSrcFolder}\\Global";
    var habitatHome = $"{configuration.ProjectSrcFolder}\\HabitatHome";
    var habitatHomeBasic = $"{configuration.ProjectSrcFolder}\\HabitatHomeBasic";
    
    var destination = deploymentRootPath;
    if (!deployLocal){
        destination = $"{deploymentRootPath}\\Website\\HabitatHome";
    }

    PublishProjects(global, destination);
    PublishProjects(habitatHome, destination);
    PublishProjects(habitatHomeBasic, destination);
});

Task("Publish-xConnect-Project").Does(() => {
    var xConnectProject = $"{configuration.ProjectSrcFolder}\\xConnect";
    var destination = configuration.XConnectRoot;
	
   if (!deployLocal){
        destination = $"{deploymentRootPath}\\Website\\habitatHome_xConnect";
    }
    PublishProjects(xConnectProject, destination);
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
    var destination =  $@"{deploymentRootPath}\temp\transforms";
    
    if (!deployLocal){
        destination = $@"{deploymentRootPath}\Website\HabitatHome\temp\transforms";
    }
    CreateFolder(destination);

    try
    {
        var files = new List<string>();
        foreach(var layer in layers)
        {
            var xdtFiles = GetTransformFiles(layer).Select(x => x.FullPath).Where(x=>!x.Contains(".azure")).ToList();
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
    var zzzDevSettingsFile = File($"{configuration.WebsiteRoot}/App_config/Include/Project/z.DevSettings.config");
    
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

Task("Modify-SXA-Variable").Does(() => {
	var webConfigFile = File($"{configuration.WebsiteRoot}/Web.config");
	var appSetting = "configuration/appSettings/add[@key='sxa:define']/@value";
	var appSettingValue = configuration.SXA ? "On" : "Off";	
    XmlPoke(webConfigFile, appSetting, appSettingValue);
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

    var authenticationFile = new FilePath($"{configuration.WebsiteRoot}/App_config/Include/Unicorn/Unicorn.zSharedSecret.config");
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
	Spam(() => DeployExmCampaigns(), configuration.DeployExmTimeout);
});

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

Task("Rebuild-Test-Index").Does(() => {
    RebuildIndex("sitecore_testing_index");
});


/*===============================================
============ Packaging Tasks ====================
===============================================*/
Task("Create-WDP")
.IsDependentOn("Prepare-BuildEnvironment")
.IsDependentOn("Generate-HabitatHomeUpdatePackages")
.IsDependentOn("Generate-HabitatHomeWDP");

Task("Publish-YML").Does(() => {

	var serializationFilesFilter = $@"{configuration.ProjectFolder}\**\*.yml";
    var destination = $@"{deploymentRootPath}\Website\HabitatHome\App_Data";

    if (!DirectoryExists(destination)){
        CreateFolder(destination);
    }

    try
    {
        var files = GetFiles(serializationFilesFilter).Select(x=>x.FullPath).ToList();

        CopyFiles(files , destination, preserveFolderStructure: true);
    }
    catch (System.Exception ex)
    {
        WriteError(ex.Message);
    }


});


Task("Generate-HabitatHomeUpdatePackages").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Generate-HabitatHomeUpdatePackages.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json");
        });
		});

Task("Generate-HabitatHomeWDP").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Generate-HabitatHomeWDP.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json");
        });
		});


/*===============================================
=============== Azure Tasks =====================
===============================================*/

Task("Run-Prerequisites")
.WithCriteria(configuration != null)
.IsDependentOn("Capture-UserData")
.IsDependentOn("Prepare-Environments");

Task("Capture-UserData").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\AzureUser-Config-Capture.ps1", args => {
        args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json");
            });
        });

Task("Prepare-Environments").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Prepare-Environment.ps1", args => {
        args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json");
        args.AppendSecret(devSitecoreUserName);
        args.AppendSecret(devSitecorePassword);
        });
    });    
Task("Prepare-BuildEnvironment").Does(() => {
    StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Prepare-BuildEnvironment.ps1", args => {
        args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json");
        args.AppendSecret(devSitecoreUserName);
        args.AppendSecret(devSitecorePassword);
        });
    });  



Task("Prepare-Azure-Deploy-CDN").Does(() => {
   	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Prepare-Azure-Deploy-CDN.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json");
        });
	
});



Task("Publish-Post-Steps").Does(() => {

	var serializationFilesFilter = $@"{configuration.ProjectFolder}\**\*.poststep";
    var destination = $@"{deploymentRootPath}\Website\HabitatHome\App_Data\poststeps";

    if (!DirectoryExists(destination))
    {
        CreateFolder(destination);
    }

    try
    {
        var files = GetFiles(serializationFilesFilter).Select(x=>x.FullPath).ToList();

        CopyFiles(files, destination, preserveFolderStructure: false);
    }
    catch (System.Exception ex)
    {
        WriteError(ex.Message);
    }


});


Task("Azure-Upload-Packages").Does(() => {

if(HasArgument("SkipScUpload"))
{
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Upload-Packages.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json")
                .Append("-SkipScUpload");
        });
}
else
{
    StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Upload-Packages.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json");
        });
}
});

Task("Azure-Site-Deploy")
.IsDependentOn("Deploy-To-Azure");
// .IsDependentOn("Scale-Down");

Task("Deploy-To-Azure").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Azure-Deploy.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json");
        });
		});

Task("Scale-Down").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Azure-Scaledown.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json");
        });
    });

/*===============================================
=============== Utility Tasks ===================
===============================================*/

Task("Prepare-Transform-Files").Does(()=>{
    
    var destination = $@"{deploymentRootPath}\Website\HabitatHome\";  
    var layers = new string[] { configuration.FoundationSrcFolder, configuration.FeatureSrcFolder, configuration.ProjectSrcFolder};
  
    foreach(var layer in layers)
    {
        var xdtFiles = GetTransformFiles(layer);
        
        List<string> files;
        
        if (configuration.DeploymentTarget == "Azure"){
            files = xdtFiles.Select(x => x.FullPath).Where(x=>x.Contains(".azure")).ToList();
        }
        else{
            files = xdtFiles.Select(x => x.FullPath).Where(x=>!x.Contains(".azure")).ToList();
        }

        foreach (var file in files)
        {
            FilePath xdtFilePath = (FilePath)file;
            
            var fileToTransform = Regex.Replace(xdtFilePath.FullPath, ".+code/(.+/*.xdt)", "$1");
            fileToTransform = Regex.Replace(fileToTransform, ".sc-internal", "");
            fileToTransform = Regex.Replace(fileToTransform, ".azure","");

            FilePath sourceTransform = $"{destination}\\{fileToTransform}";
            
            if (!FileExists(sourceTransform)){
                CreateFolder(sourceTransform.GetDirectory().FullPath);
                CopyFile(xdtFilePath.FullPath,sourceTransform);
            }
            else {
                MergeFile(sourceTransform.FullPath	    // Source File
                        , xdtFilePath.FullPath			// Tranforms file (*.xdt)
                        , sourceTransform.FullPath);		// Target File
            }
        }
    }
});

RunTarget(target);