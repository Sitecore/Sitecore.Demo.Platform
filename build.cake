#addin "Cake.Azure"
#addin "Cake.Http"
#addin "Cake.Json"
#addin "Cake.Powershell"
#addin "Cake.XdtTransform"
#addin "Newtonsoft.Json"

#load "local:?path=CakeScripts/helper-methods.cake"

var target = Argument<string>("Target", "Default");
var configuration = new Configuration();
var cakeConsole = new CakeConsole();
var configJsonFile = "cake-config.json";
var unicornSyncScript = $"./scripts/Unicorn/Sync.ps1";
string topology = null;


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

});



/*===============================================
============ Local Build - Main Tasks ===========
===============================================*/
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
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("Modify-PublishSettings")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Modify-Unicorn-Source-Folder")
.IsDependentOn("Publish-Transforms")
.IsDependentOn("Publish-xConnect-Project");

/*===============================================
=========== Packaging - Main Tasks ==============
===============================================*/
Task("Build-WDP")
.WithCriteria(configuration != null)
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("Clean")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Publish-xConnect-Project")
.IsDependentOn("Publish-YML")
.IsDependentOn("Publish-Azure-Transforms")
.IsDependentOn("Publish-Post-Steps")
.IsDependentOn("Package-Build");

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
.IsDependentOn("Prepare-Azure-Deploy-CDN")
.IsDependentOn("Azure-Upload-Packages");

Task("Azure-Deploy")
.WithCriteria(configuration != null)
.IsDependentOn("Azure-Site-Deploy");

/*===============================================
================= SUB TASKS =====================
===============================================*/


/*===============================================
=============== Generic Tasks ===================
===============================================*/


Task("Clean").Does(() => {
    // Clean project build folders
    CleanDirectories($"{configuration.SourceFolder}/**/obj");
    CleanDirectories($"{configuration.SourceFolder}/**/bin");

    // Clean deployment folders
     string[] folders = { $"\\{configuration.Version}\\{topology}\\assets\\HabitatHome", $"\\{configuration.Version}\\{topology}\\assets\\HabitatHomeCD", "\\Website", $"\\{configuration.Version}\\{topology}\\assets\\Xconnect", $"\\{configuration.Version}\\{topology}\\assets\\Data Exchange Framework\\WDPWorkFolder", $"\\{configuration.Version}\\{topology}\\assets\\Data Exchange Framework CD\\WDPWorkFolder" };

    foreach (string folder in folders)
    {
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
.IsDependentOn("Publish-Foundation-Projects")
.IsDependentOn("Publish-Feature-Projects")
.IsDependentOn("Publish-Project-Projects");


Task("Build-Solution")
.IsDependentOn("Copy-Sitecore-Lib")
.Does(() => {
    MSBuild(configuration.SolutionFile, cfg => InitializeMSBuildSettings(cfg));
});

Task("Publish-Foundation-Projects").Does(() => {
    PublishProjects(configuration.FoundationSrcFolder, configuration.WebsiteRoot);
});

Task("Publish-Feature-Projects").Does(() => {
    PublishProjects(configuration.FeatureSrcFolder, configuration.WebsiteRoot);
});

Task("Publish-Project-Projects").Does(() => {
    var global = $"{configuration.ProjectSrcFolder}\\Global";
    var habitatHome = $"{configuration.ProjectSrcFolder}\\HabitatHome";
    var habitatHomeBasic = $"{configuration.ProjectSrcFolder}\\HabitatHomeBasic";

    PublishProjects(global, configuration.WebsiteRoot);
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
    string destination = null;
    
    
    if (target.Contains("Azure")){
        destination = $@"{configuration.DeployFolder}\{configuration.Version}\{topology}\Website\HabitatHome\temp\transforms";
    }
    else
    {
        destination =  $@"{configuration.WebsiteRoot}\temp\transforms";
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



/*===============================================
============ Packaging Tasks ====================
===============================================*/
Task("Package-Build")
.IsDependentOn("Generate-HabitatUpdatePackages")
.IsDependentOn("ConvertTo-SCWDPs");

Task("Generate-HabitatUpdatePackages").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Generate-HabitatUpdatePackages.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\Azure\\cake-config.json");
        });
		});

Task("ConvertTo-SCWDPs").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\ConvertTo-SCWDPs.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\Azure\\cake-config.json");
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
        args.AppendQuoted($"{configuration.ProjectFolder}\\Azure\\cake-config.json");
            });
        });

Task("Prepare-Environments").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Env-Prep.ps1", args => {
        args.AppendQuoted($"{configuration.ProjectFolder}\\Azure\\cake-config.json");
        });
    });    

Task("Publish-Azure-Transforms").Does(()=>{

       var codeFoldersFilter = $@"{configuration.ProjectFolder}\**\code";
       var destination = $@"{configuration.DeployFolder}\{configuration.Version}\{topology}\Website\HabitatHome";  

       if (!DirectoryExists(destination))
       {
             CreateFolder(destination);
       }

        try
        {    
            var projectDirectories = GetDirectories(codeFoldersFilter);
         
            foreach (var directory in projectDirectories)
            {
                var xdtFiles = GetTransformFiles(directory.FullPath).ToList();

                foreach (var file in xdtFiles)
                {
                    var relativeFilePath = file.FullPath;

                    if (!file.FullPath.Contains(".azure."))
                    {
                        continue;
                    }

                    var indexToTrimFrom = file.FullPath.IndexOf("/code/", StringComparison.InvariantCultureIgnoreCase);

                    if (indexToTrimFrom > -1)
                    {
                        relativeFilePath = file.FullPath.Substring(indexToTrimFrom + 5);
                    }
                    else
                    {
                        relativeFilePath = file.GetFilename().FullPath;
                    }

                    var destinationFilePath = new FilePath ($@"{destination}{relativeFilePath.Replace(".azure", string.Empty)}");

                    if (!DirectoryExists(destinationFilePath.GetDirectory().FullPath))
                    {
                         CreateFolder(destinationFilePath.GetDirectory().FullPath);
                    }                   
                   
                    CopyFile(file.FullPath, destinationFilePath.FullPath);
                                       
                }  
            }   

        }
        catch (System.Exception ex)
        {
            WriteError(ex.Message);
        }

});


Task("Prepare-Azure-Deploy-CDN").Does(() => {
   	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Prepare-Azure-Deploy-CDN.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\Azure\\cake-config.json");
        });
	
});

Task("Publish-YML").Does(() => {

	var serializationFilesFilter = $@"{configuration.ProjectFolder}\**\*.yml";
    var destination = $@"{configuration.DeployFolder}\{configuration.Version}\{topology}\Website\HabitatHome\App_Data";

    if (!DirectoryExists(destination))
    {
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


Task("Publish-Post-Steps").Does(() => {

	var serializationFilesFilter = $@"{configuration.ProjectFolder}\**\*.poststep";
    var destination = $@"{configuration.DeployFolder}\{configuration.Version}\{topology}\Website\HabitatHome\App_Data\poststeps";

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
            args.AppendQuoted($"{configuration.ProjectFolder}\\Azure\\cake-config.json")
                .Append("-SkipScUpload");
        });
}
else
{
    StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Upload-Packages.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\Azure\\cake-config.json");
        });
}
});

Task("Azure-Site-Deploy")
.IsDependentOn("Deploy-To-Azure");
// .IsDependentOn("Scale-Down");

Task("Deploy-To-Azure").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Azure-Deploy.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\Azure\\cake-config.json");
        });
		});

Task("Scale-Down").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Azure-Scaledown.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\Azure\\cake-config.json");
        });
    });

RunTarget(target);