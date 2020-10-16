#addin nuget:?package=Cake.Azure&version=0.4.0
#addin nuget:?package=Cake.Http&version=0.7.0
#addin nuget:?package=Cake.Json&version=4.0.0
#addin nuget:?package=Cake.Powershell&version=0.4.8
#addin nuget:?package=Cake.XdtTransform&version=0.16.0
#addin nuget:?package=Newtonsoft.Json&version=11.0.2
#addin nuget:?package=Cake.Npm&version=0.17.0
#addin nuget:?package=Cake.SitecoreDemo&version=930.5.2

var target = Argument<string>("Target", "Default");
var deploymentTarget = Argument<string>("DeploymentTarget", "IIS"); // Possible values are 'IIS', 'Docker' and 'DockerBuild'
bool usePublicFeedOnly = HasArgument ("PublicFeedsOnly");
var configuration = new Configuration();
var platform = new CakePlatform();
var runtime = new CakeRuntime();
var environment = new CakeEnvironment(platform, runtime);
var cakeConsole = new CakeConsole(environment);
var configJsonFile = Argument<string>("Configuration", "cake-config.json");
var unicornSyncScript = $"./scripts/Unicorn/Sync.ps1";
bool publishLocal = false;
bool syncUnicorn = true;
bool applyTransforms = true;
string[] publishDestinations;
/*===============================================
================ MAIN TASKS =====================
===============================================*/

Setup(context =>
{
  cakeConsole.ForegroundColor = ConsoleColor.Yellow;
  PrintHeader(cakeConsole, ConsoleColor.DarkGreen);

  var configFile = new FilePath(configJsonFile);
  configuration = DeserializeJsonFromFile<Configuration>(configFile);
  configuration.SolutionFile =  $"{configuration.SolutionName}";

  if (deploymentTarget.Contains("Docker"))  {
    applyTransforms = false;
  }

  if (deploymentTarget == "DockerBuild")  {
    publishLocal = true;
    syncUnicorn = false;
  }

  publishDestinations = new string[] {
    configuration.PublishWebFolder,
    configuration.PublishWebFolderCD
  };
});


/*===============================================
============ Local Build - Main Tasks ===========
===============================================*/

Task("Base-PreBuild")
.IsDependentOn("CleanBuildFolders")
.IsDependentOn("Modify-PublishSettings");

Task("Base-Publish")
.IsDependentOn("Publish-FrontEnd-Project")
//.IsDependentOn("Apply-DotnetCore-Transforms")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Publish-xConnect-Project")
.IsDependentOn("Publish-xConnect-Project-IndexWorker")
.IsDependentOn("Modify-ContentHub-Variable")
.IsDependentOn("Modify-Unicorn-Source-Folder");

Task("Default")
.IsDependentOn("Base-PreBuild")
.IsDependentOn("Base-Publish")
.IsDependentOn("Merge-and-Copy-Xml-Transform")
.IsDependentOn("FrontEnd-Themes-Build")
.IsDependentOn("Generate-Dacpacs")
.IsDependentOn("Post-Deploy");

Task("Content-Management")
.IsDependentOn("Publish-FrontEnd-Project")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Modify-ContentHub-Variable")
.IsDependentOn("Modify-Unicorn-Source-Folder")
.IsDependentOn("Merge-and-Copy-Xml-Transform");

Task("Content-Delivery")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Merge-and-Copy-Xml-Transform");

Task("xConnect")
.IsDependentOn("Publish-xConnect-Project");

Task("IndexWorker")
.IsDependentOn("Publish-xConnect-Project-IndexWorker");

Task("Post-Deploy")
.IsDependentOn("Apply-Xml-Transform")
.IsDependentOn("Sync-Unicorn")
.IsDependentOn("Deploy-EXM-Campaigns")
.IsDependentOn("Deploy-Marketing-Definitions")
.IsDependentOn("Rebuild-Indexes");

Task("Quick-Deploy")
.IsDependentOn("Base-PreBuild")
.IsDependentOn("Base-Publish");

Task("Redeploy")
//.IsDependentOn("Publish-Core-Project")
.IsDependentOn("Sync-Unicorn");

/*===============================================
================= SUB TASKS =====================
===============================================*/
Task("CleanAll")
.IsDependentOn("CleanBuildFolders")
.IsDependentOn("CleanPublishFolders");

Task("CleanBuildFolders").Does(() => {
  // Clean Project build folders
  CleanDirectories($"{configuration.SourceFolder}/**/obj");
  CleanDirectories($"{configuration.SourceFolder}/**/bin");
  CleanDirectories(configuration.PublishTempFolder);
});

Task("CleanPublishFolders").Does(() => {
  CleanDirectories(configuration.PublishWebFolder);
  CleanDirectories(configuration.PublishWebFolderCD);
  CleanDirectories(configuration.PublishxConnectFolder);
  CleanDirectories(configuration.PublishxConnectIndexWorkerFolder);
  CleanDirectories(configuration.PublishDataFolder);
  CleanDirectories(configuration.PublishTempFolder);
});

/*===============================================
=============== Generic Tasks ===================
===============================================*/

Task("Publish-All-Projects")
.IsDependentOn("Build-Solution")
.IsDependentOn("Publish-Foundation-Projects")
.IsDependentOn("Publish-Feature-Projects")
.IsDependentOn("Publish-Project-Projects");

Task("Build-Solution")
.Does(() => {
  MSBuild(configuration.SolutionFile, cfg => InitializeMSBuildSettings(configuration, cfg));
});

Task("Publish-Foundation-Projects").Does(() => {
  PublishSourceProjects(configuration, configuration.FoundationSrcFolder, publishDestinations);
});

Task("Publish-Feature-Projects").Does(() => {
  PublishSourceProjects(configuration, configuration.FeatureSrcFolder, publishDestinations);
});

Task("Publish-FrontEnd-Project").Does(() => {
  var frontEndFiles = $"{configuration.FrontEndFolder}\\*";
  PublishFrontEndProject(frontEndFiles, configuration.PublishWebFolder);
});

// Task("Apply-DotnetCore-Transforms")
// .WithCriteria(() => (!publishLocal && applyTransforms))
// .Does(() => {
//   ApplyDotnetCoreTransforms(configuration.PublishTempFolder, configuration.PublishWebFolderCD);
// });

Task("Generate-Dacpacs")
.WithCriteria(() => publishLocal)
.Does(() => {
  StartPowershellFile($"{configuration.ProjectFolder}\\scripts\\Packaging\\generate-update-package.ps1", new PowershellSettings()
              .WithArguments(args =>
              {
                args.Append("target", $"{configuration.ProjectFolder}\\items")
                    .Append("output", $"{configuration.PublishDataFolder}");
              }));
});

Task("Publish-Project-Projects").Does(() => {
  var global = $"{configuration.ProjectSrcFolder}\\Global";
  var sitecoredemo = $"{configuration.ProjectSrcFolder}\\SitecoreDemo";

  PublishSourceProjects(configuration, global, publishDestinations);
  PublishSourceProjects(configuration, sitecoredemo, publishDestinations);
});

Task("Publish-xConnect-Project").Does(() => {
  var xConnectProject = $"{configuration.ProjectSrcFolder}\\xConnect";
  PublishProjects(configuration, xConnectProject, configuration.PublishxConnectFolder);
});

Task("Publish-xConnect-Project-IndexWorker")
.WithCriteria(() => (deploymentTarget.Contains("Docker")))
.Does(() => {
  var xConnectProject = $"{configuration.ProjectSrcFolder}\\xConnect";
  PublishProjects(configuration, xConnectProject, configuration.PublishxConnectIndexWorkerFolder);
});

Task("Apply-Xml-Transform")
.WithCriteria(() => (!publishLocal && applyTransforms))
.Does(() => {
  var layers = new string[]{"Foundation", "Feature", "Project"};
  ApplyXmlTransform(layers, configuration.PublishWebFolder);
});

Task("Merge-and-Copy-Xml-Transform")
.WithCriteria(() => (publishLocal || !applyTransforms))
.Does(() => {
  var layers = new string[] { configuration.FoundationSrcFolder, configuration.FeatureSrcFolder, configuration.ProjectSrcFolder };
  MergeAndCopyXmlTransform(layers, "", configuration.PublishWebFolder);
  // Copy all transforms to the CD
  var cdTransformsPath = $"{configuration.PublishWebFolderCD}\\transforms";
  EnsureDirectoryExists(cdTransformsPath);
  var xdtFiles = GetFiles($"{configuration.PublishWebFolder}\\transforms\\**\\*");
  CopyFiles(xdtFiles, cdTransformsPath, true);
});

Task("Modify-Unicorn-Source-Folder")
.Does(() => {
  var zzzDevSettingsFile = File($"{configuration.PublishWebFolder}/App_config/Include/Project/z.DevSettings.config");
  cakeConsole.WriteLine($"dev settings: {zzzDevSettingsFile}");
  ModifyUnicornSourceFolder(configuration.UnicornSerializationFolder, zzzDevSettingsFile, "sourceFolder");
});

Task("Turn-On-Unicorn")
.WithCriteria(() => (syncUnicorn && deploymentTarget != "Docker"))
.Does(() => {
  TurnOnUnicorn(configuration.PublishWebFolder);
});

Task("Modify-PublishSettings").Does(() => {
  ModifyPublishSettings(configuration.ProjectFolder, configuration.InstanceUrl);
});

Task("Sync-Unicorn")
.IsDependentOn("Turn-On-Unicorn")
.IsDependentOn("Modify-Unicorn-Source-Folder")
.WithCriteria(() => syncUnicorn)
.Does(() => {
  SyncUnicorn(configuration.InstanceUrl, configuration.PublishWebFolder, unicornSyncScript);
});

Task("Deploy-EXM-Campaigns")
.WithCriteria(() => !publishLocal)
.Does(() => {
  Spam(() => DeployExmCampaigns(configuration), configuration.DeployExmTimeout);
});

Task("Deploy-Marketing-Definitions")
.WithCriteria(() => !publishLocal)
.Does(() => {
  DeployMarketingDefinitions(configuration.InstanceUrl, configuration.MarketingDefinitionsApiKey);
});

Task("Rebuild-Indexes")
.WithCriteria(() => !publishLocal).Does(() => {
  var indexes = new string[] {
    "core", "master", "web", "test"
  };
  foreach (var index in indexes){
    RebuildIndex(configuration, $"sitecore_{index}_index");
  }
});

Task("FrontEnd-Npm-Install").Does(() => {
  FrontEndNpmInstall($"{configuration.FrontEndFolder}\\*");
});

Task("FrontEnd-Themes-Build")
.IsDependentOn("FrontEnd-Npm-Install")
.Does(() =>
{
  var frontEndFolder = $"{configuration.ProjectFolder}\\FrontEnd";

  StartPowershellFile("./build-themes.ps1", new PowershellSettings()
              .SetLogOutput()
              .WithArguments(args =>
              {
                args.Append("FrontEndPath", frontEndFolder)
                    .Append("ItemsPath", $"{configuration.ProjectFolder}\\items");
              }));
});

Task("Modify-ContentHub-Variable")
.WithCriteria(() => (!deploymentTarget.Contains("Docker")))
.Does(() => {
    ModifyContentHubVariable(configuration.PublishWebFolder, configuration.IsContentHubEnabled);
});

RunTarget(target);
