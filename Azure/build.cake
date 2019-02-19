#addin "Cake.XdtTransform"
#addin "Cake.Powershell"
#addin "Cake.Http"
#addin "Cake.Json"
#addin "Cake.Azure"
#addin "Newtonsoft.Json"


#load "local:?path=CakeScripts/helper-methods.cake"


var target = Argument<string>("Target", "Default");
var configuration = new Configuration();
var cakeConsole = new CakeConsole();
var configJsonFile = "cake-config.json";
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

Task("Default")
.WithCriteria(configuration != null)
.IsDependentOn("Build")
.IsDependentOn("Azure-Upload")
.IsDependentOn("Azure-Deploy");

Task("Run-Prerequisites")
.WithCriteria(configuration != null)
.IsDependentOn("Capture-UserData")
.IsDependentOn("Prepare-Environments");

Task("Build")
.WithCriteria(configuration != null)
.IsDependentOn("Clean")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Publish-xConnect-Project")
.IsDependentOn("Publish-YML")
.IsDependentOn("Publish-Azure-Transforms")
.IsDependentOn("Publish-Post-Steps")
.IsDependentOn("Package-Build");

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

Task("Clean").Does(() => {

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

    CleanDirectories($"{configuration.SourceFolder}/**/obj");
    CleanDirectories($"{configuration.SourceFolder}/**/bin");
});

Task("Capture-UserData").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\AzureUser-Config-Capture.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\Azure\\cake-config.json");
        });
		});

Task("Prepare-Environments").Does(() => {
	StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Env-Prep.ps1", args =>
        {
            args.AppendQuoted($"{configuration.ProjectFolder}\\Azure\\cake-config.json");
        });
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
    PublishProjects(configuration.FoundationSrcFolder, $"{configuration.DeployFolder}\\{configuration.Version}\\{topology}\\Website\\HabitatHome");
});

Task("Publish-Feature-Projects").Does(() => {
    PublishProjects(configuration.FeatureSrcFolder, $"{configuration.DeployFolder}\\{configuration.Version}\\{topology}\\Website\\HabitatHome");
});

Task("Publish-Project-Projects").Does(() => {
    var common = $"{configuration.ProjectSrcFolder}\\Common";
    var habitat = $"{configuration.ProjectSrcFolder}\\Habitat";
    var habitatHome = $"{configuration.ProjectSrcFolder}\\HabitatHome";

    PublishProjects(common, $"{configuration.DeployFolder}\\{configuration.Version}\\{topology}\\Website\\HabitatHome");
    PublishProjects(habitat, $"{configuration.DeployFolder}\\{configuration.Version}\\{topology}\\Website\\HabitatHome");
    PublishProjects(habitatHome, $"{configuration.DeployFolder}\\{configuration.Version}\\{topology}\\Website\\HabitatHome");
});

Task("Publish-xConnect-Project").Does(() => {
    var xConnectProject = $"{configuration.ProjectSrcFolder}\\xConnect";
	var xConnectDeployFolder = $"{configuration.DeployFolder}\\{configuration.Version}\\{topology}\\Website\\xConnect";
	
    PublishProjects(xConnectProject, xConnectDeployFolder);
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
    var destination = $@"{configuration.DeployFolder}\{configuration.Version}\{topology}\Website\HabitatHome\temp\transforms";

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
