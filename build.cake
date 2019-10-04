#addin nuget:?package=Cake.Azure&version=0.3.0
#addin nuget:?package=Cake.Http&version=0.7.0
#addin nuget:?package=Cake.Json&version=4.0.0
#addin nuget:?package=Cake.Powershell&version=0.4.8
#addin nuget:?package=Cake.XdtTransform&version=0.16.0
#addin nuget:?package=Newtonsoft.Json&version=11.0.2

#load "local:?path=CakeScripts/helper-methods.cake"
#load "local:?path=CakeScripts/xml-helpers.cake"

var target = Argument<string>("Target", "Default");
var configuration = new Configuration();
var cakeConsole = new CakeConsole();
var configJsonFile = "cake-config.json";
var unicornSyncScript = $"./scripts/Unicorn/Sync.ps1";
var packagingScript = $"./scripts/Packaging/generate-update-package.ps1";
var dacpacScript = $"./scripts/Packaging/generate-dacpac.ps1";
bool publishLocal = false;


/*===============================================
================ MAIN TASKS =====================
===============================================*/

Setup(context =>
{
  cakeConsole.ForegroundColor = ConsoleColor.Yellow;
  PrintHeader(ConsoleColor.DarkGreen);

  var configFile = new FilePath(configJsonFile);
  configuration = DeserializeJsonFromFile<Configuration>(configFile);
  configuration.SolutionFile =  $"{configuration.ProjectFolder}\\{configuration.SolutionName}";
  publishLocal = (target == "Publish-Local") || (target == "Docker-Container");
  if (publishLocal) {
    if (target == "Docker-Container"){
      configuration.BuildConfiguration = "DockerDeploy";
    }
    else
    {
      configuration.BuildConfiguration = "NoDeploy";
    }
  }

  if (publishLocal || target == "Build-TDS") {
    configuration.SolutionFile = configuration.SolutionFile.Replace(".sln",".TDS.sln");
  }
});

/*===============================================
============ Local Build - Main Tasks ===========
===============================================*/

Task("Base-PreBuild")
.WithCriteria(configuration != null)
.IsDependentOn("CleanAll")
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("Modify-PublishSettings");

Task("Base-Publish")
.WithCriteria(configuration != null)
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Publish-xConnect-Project");

Task("Default")
.IsDependentOn("Base-PreBuild")
.IsDependentOn("Base-Publish")
.IsDependentOn("Modify-Unicorn-Source-Folder")
.IsDependentOn("Post-Deploy")
.IsDependentOn("Apply-DotnetCore-Transforms");

Task("Build-TDS")
.IsDependentOn("Base-PreBuild")
.IsDependentOn("Restore-TDS-NuGetPackages")
.IsDependentOn("Publish-Core-Project")
.IsDependentOn("Apply-DotnetCore-Transforms")
.IsDependentOn("Build-Solution")
.IsDependentOn("Publish-xConnect-Project")
.IsDependentOn("Post-Deploy");

Task("Post-Deploy")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Sync-Unicorn")
.IsDependentOn("Deploy-EXM-Campaigns")
.IsDependentOn("Deploy-Marketing-Definitions")
.IsDependentOn("Rebuild-Core-Index")
.IsDependentOn("Rebuild-Master-Index")
.IsDependentOn("Rebuild-Web-Index")
.IsDependentOn("Rebuild-Test-Index");

Task("Docker-Container")
.IsDependentOn("Base-PreBuild")
.IsDependentOn("Base-Publish")
.IsDependentOn("Copy-to-Destination")
.IsDependentOn("Merge-and-Copy-Xml-Transform")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Apply-DotnetCore-Transforms");

Task("Quick-Deploy")
.IsDependentOn("Base-PreBuild")
.IsDependentOn("Base-Publish")
.IsDependentOn("Modify-Unicorn-Source-Folder");

Task("Publish-Local")
.WithCriteria(configuration != null)
.IsDependentOn("CleanPublishFolders")
.IsDependentOn("Base-PreBuild")
.IsDependentOn("Base-Publish")
.IsDependentOn("Copy-to-Destination")
.IsDependentOn("Merge-and-Copy-Xml-Transform")
.IsDependentOn("Generate-Dacpacs");

/*===============================================
================= SUB TASKS =====================
===============================================*/

Task("Restore-TDS-NuGetPackages").Does(()=>{
  NuGetRestore(configuration.SolutionFile);
});

Task("CleanAll")
.IsDependentOn("CleanBuildFolders")
.IsDependentOn("CleanPublishFolders");

Task("CleanBuildFolders").Does(() => {
  // Clean project build folders
  CleanDirectories($"{configuration.SourceFolder}/**/obj");
  CleanDirectories($"{configuration.SourceFolder}/**/bin");
});

Task("CleanPublishFolders").Does(()=> {
  CleanDirectories(configuration.PublishWebFolder);
  CleanDirectories(configuration.PublishxConnectFolder);
  CleanDirectories(configuration.PublishDataFolder);
  CleanDirectories(configuration.PublishTempFolder);
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
  var destination = configuration.WebsiteRoot;

  if (publishLocal) {
    destination = configuration.PublishTempFolder;
  }
    PublishProjects(configuration.FoundationSrcFolder, destination);
});

Task("Publish-Feature-Projects").Does(() => {
  var destination = configuration.WebsiteRoot;

  if (publishLocal) {
    destination = configuration.PublishTempFolder;
  }
  PublishProjects(configuration.FeatureSrcFolder, destination);
});

Task("Publish-Core-Project").Does(() => {
  var destination = configuration.WebsiteRoot;

  if (publishLocal) {
    destination = configuration.PublishTempFolder;
  }
  Information("Destination: " + destination);

  var projectFile = $"{configuration.SourceFolder}\\Build\\Build.Website\\code\\Build.Website.csproj";
  var publishFolder = $"{configuration.PublishTempFolder}";

  DotNetCoreMSBuildSettings buildSettings = new DotNetCoreMSBuildSettings();
  buildSettings.SetConfiguration(configuration.BuildConfiguration);

  DotNetCoreRestoreSettings restoreSettings = new DotNetCoreRestoreSettings {
    MSBuildSettings = buildSettings
  };

  DotNetCoreRestore(projectFile, restoreSettings);

  var settings = new DotNetCorePublishSettings {
    OutputDirectory = publishFolder,
    Configuration = configuration.BuildConfiguration
  };

  DotNetCorePublish(projectFile, settings);

  // Copy assembly files to webroot
  var assemblyFilesFilter = $@"{publishFolder}\*.dll";
  var assemblyFiles = GetFiles(assemblyFilesFilter).Select(x=>x.FullPath).ToList();
  EnsureDirectoryExists(destination+"\\bin");
  CopyFiles(assemblyFiles, (destination + "\\bin"), preserveFolderStructure: false);

  // Copy other output files to destination webroot
  var ignoredExtensions = new string[] { ".dll", ".exe", ".pdb", ".xdt" };
  var ignoredFilesPublishFolderPath = publishFolder.ToLower().Replace("\\", "/");
  var ignoredFiles = new string[] {
    $"{ignoredFilesPublishFolderPath}/web.config",
    $"{ignoredFilesPublishFolderPath}/build.website.deps.json",
    $"{ignoredFilesPublishFolderPath}/build.website.exe.config"
  };

  var contentFiles = GetFiles($"{publishFolder}\\**\\*")
                      .Where(file => !ignoredExtensions.Contains(file.GetExtension().ToLower()))
                      .Where(file => !ignoredFiles.Contains(file.FullPath.ToLower()));

  CopyFiles(contentFiles, destination, preserveFolderStructure: true);
});

Task("Copy-to-Destination").Does(()=>{
  var destination = configuration.WebsiteRoot;
  var publishTempFolder = $"{configuration.PublishTempFolder}";

  if (publishLocal) {
    destination = configuration.PublishWebFolder;
  }
  Information("Destination: " + destination);
 // Copy assembly files to publish destination
  var assemblyFilesFilter = $@"{publishTempFolder}\*.dll";
  var assemblyFiles = GetFiles(assemblyFilesFilter).Select(x=>x.FullPath).ToList();
  EnsureDirectoryExists(destination+"\\bin");
  CopyFiles(assemblyFiles, (destination + "\\bin"), preserveFolderStructure: false);

  // Copy other output files to publish destination
  var ignoredExtensions = new string[] { ".dll", ".exe", ".pdb", ".xdt", ".yml"};
  var ignoredFiles = new string[] { "web.config", "build.website.deps.json", "build.website.exe.config" };

  var contentFiles = GetFiles($"{publishTempFolder}\\**\\*")
  .Where(file => !ignoredExtensions.Contains(file.GetExtension().ToLower()))
  .Where(file => !ignoredFiles.Contains(file.Segments.LastOrDefault().ToLower()));

  CopyFiles(contentFiles, destination, preserveFolderStructure: true);


});
Task("Apply-DotnetCore-Transforms").Does(() => {
  var publishFolder = $"{configuration.PublishTempFolder}";
  var destination = configuration.WebsiteRoot;
  if (publishLocal) {
    destination = configuration.PublishWebFolder;
  }
  Transform(publishFolder, "transforms", destination);
});

Task("Publish-YML").Does(() => {

  var serializationFilesFilter = $@"{configuration.ProjectFolder}\src\**\*.yml";
  var destination = $@"{configuration.PublishTempFolder}\yml";
  Information($"Filter: {serializationFilesFilter} - Destination: {destination}");

  Func<IFileSystemInfo, bool> exclude_build_folder = fileSystemInfo => !fileSystemInfo.Path.FullPath.Contains("Build");

  if (!DirectoryExists(destination))
  {
    CreateFolder(destination);
  }
  try
  {
    var files = GetFiles(serializationFilesFilter,new GlobberSettings{Predicate = exclude_build_folder})
      .Select(x=>x.FullPath).ToList();

    CopyFiles(files , destination, preserveFolderStructure: true);
  }
  catch (System.Exception ex)
  {
    WriteError($"ERROR: {ex.Message}");
    Information(ex.StackTrace);
  }
});

Task("Create-UpdatePackage")
  .IsDependentOn("Publish-YML")
  .Does(()=> {
    StartPowershellFile(packagingScript, new PowershellSettings()
      .SetFormatOutput()
      .SetLogOutput()
      .WithArguments(args => {
        args.Append("target", $"{configuration.PublishTempFolder}\\yml")
          .Append("output", $"{configuration.PublishTempFolder}\\update\\package.update");
    }));
});

Task("Generate-Dacpacs")
.IsDependentOn("Create-UpdatePackage")
.Does(() => {
  StartPowershellFile(dacpacScript, new PowershellSettings()
    .SetFormatOutput()
    .SetLogOutput()
    .WithArguments(args => {
      args.Append("SitecoreAzureToolkitPath", $"{configuration.SitecoreAzureToolkitPath}")
        .Append("updatePackagePath", $"{configuration.PublishTempFolder}\\update\\package.update")
        .Append("destinationPath", $"{configuration.PublishDataFolder}");
  }));
});

Task("Publish-Project-Projects").Does(() => {
  var global = $"{configuration.ProjectSrcFolder}\\Global";
  var habitatHome = $"{configuration.ProjectSrcFolder}\\HabitatHome";
  var habitatHomeBasic = $"{configuration.ProjectSrcFolder}\\HabitatHomeBasic";

   var destination = configuration.WebsiteRoot;
  if (publishLocal) {
    destination = configuration.PublishTempFolder;
  }

  PublishProjects(global, destination);
  PublishProjects(habitatHome, destination);
  PublishProjects(habitatHomeBasic, destination);
});

Task("Publish-xConnect-Project").Does(() => {
  var xConnectProject = $"{configuration.ProjectSrcFolder}\\xConnect";
  var destination = configuration.XConnectRoot;

  if (publishLocal) {
    destination = configuration.PublishxConnectFolder;
  }
  PublishProjects(xConnectProject, destination);
});

Task("Apply-Xml-Transform").Does(() => {
  var layers = new string[] { configuration.FoundationSrcFolder, configuration.FeatureSrcFolder, configuration.ProjectSrcFolder};
  var publishDestination = configuration.WebsiteRoot;
   if (publishLocal) {
    publishDestination = configuration.PublishWebFolder;
  }
  foreach(var layer in layers)
  {
    Transform(layer,"code", publishDestination);
  }
});

Task("Merge-and-Copy-Xml-Transform").Does(() => {

  // Method will process all transforms from the temporary locations, merge them together and copy them to the temporary Publish\Web directory

  var PublishTempFolder = $"{configuration.PublishTempFolder}";
  var publishFolder = $"{configuration.PublishWebFolder}";

  Information($"Merging {PublishTempFolder}\\transforms to {publishFolder}");

  // Processing dotnet core transforms from NuGet references
  MergeTransforms($"{PublishTempFolder}\\transforms", $"{publishFolder}");

  // Processing project transformations
  var layers = new string[] {
    configuration.FoundationSrcFolder, configuration.FeatureSrcFolder, configuration.ProjectSrcFolder
  };

  foreach(var layer in layers) {
    Information($"Merging {layer} to {publishFolder}");
    MergeTransforms(layer,publishFolder);
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

Task("Turn-On-Unicorn").Does(() => {
  var webConfigFile = File($"{configuration.WebsiteRoot}/web.config");
  var xmlSetting = new XmlPokeSettings {
    Namespaces = new Dictionary<string, string> {
      {"patch", @"http://www.sitecore.net/xmlconfig/"}
    }
  };

  var unicornAppSettingXPath = "configuration/appSettings/add[@key='unicorn:define']/@value";
  XmlPoke(webConfigFile, unicornAppSettingXPath, "On", xmlSetting);
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

Task("Sync-Unicorn")
.WithCriteria(publishLocal != true)
.WithCriteria(target != "Build-TDS")
.IsDependentOn("Turn-On-Unicorn")
.Does(() => {
  var unicornUrl = configuration.InstanceUrl + "/unicorn.aspx";
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
  var url = $"{configuration.InstanceUrl}/utilities/deploymarketingdefinitions.aspx?apiKey={configuration.MarketingDefinitionsApiKey}";
  var responseBody = HttpGet(url, settings => {
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

Task("Generate-HabitatHomeUpdatePackages").Does(() => {
  StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Generate-HabitatHomeUpdatePackages.ps1", args => {
    args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json");
  });
});

Task("Generate-HabitatHomeWDP").Does(() => {
  StartPowershellFile ($"{configuration.ProjectFolder}\\Azure\\HelperScripts\\Generate-HabitatHomeWDP.ps1", args => {
    args.AppendQuoted($"{configuration.ProjectFolder}\\cake-config.json");
  });
});



/*===============================================
=============== Utility Tasks ===================
===============================================*/

Task("Prepare-Transform-Files").Does(()=>{
  var destination = $@"{configuration.WebsiteRoot}\Website\HabitatHome\";
  var layers = new string[] { configuration.FoundationSrcFolder, configuration.FeatureSrcFolder, configuration.ProjectSrcFolder};

  foreach(var layer in layers) {
    var xdtFiles = GetTransformFiles(layer);
    List<string> files;

    if (configuration.DeploymentTarget == "Azure") {
      files = xdtFiles.Select(x => x.FullPath).Where(x=>x.Contains(".azure")).ToList();
    }
    else {
      files = xdtFiles.Select(x => x.FullPath).Where(x=>!x.Contains(".azure")).ToList();
    }

    foreach (var file in files) {
      FilePath xdtFilePath = (FilePath)file;

      var fileToTransform = Regex.Replace(xdtFilePath.FullPath, ".+code/(.+/*.xdt)", "$1");
      fileToTransform = Regex.Replace(fileToTransform, ".sc-internal", "");
      fileToTransform = Regex.Replace(fileToTransform, ".azure","");

      FilePath sourceTransform = $"{destination}\\{fileToTransform}";

      if (!FileExists(sourceTransform)) {
        CreateFolder(sourceTransform.GetDirectory().FullPath);
        CopyFile(xdtFilePath.FullPath,sourceTransform);
      }
      else {
        MergeFile(sourceTransform.FullPath		// Source File
          , xdtFilePath.FullPath				// Tranforms file (*.xdt)
          , sourceTransform.FullPath);		// Target File
      }
    }
  }
});
RunTarget(target);
