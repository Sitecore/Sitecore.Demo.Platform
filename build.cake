#addin nuget:?package=Cake.Azure&version=0.3.0
#addin nuget:?package=Cake.Http&version=0.7.0
#addin nuget:?package=Cake.Json&version=4.0.0
#addin nuget:?package=Cake.Powershell&version=0.4.8
#addin nuget:?package=Cake.XdtTransform&version=0.16.0
#addin nuget:?package=Newtonsoft.Json&version=11.0.2
#addin nuget:?package=Cake.SitecoreDemo&version=930.2.2

var target = Argument<string>("Target", "Default");
var deploymentTarget = Argument<string>("DeploymentTarget", "IIS"); // Possible values are 'IIS', 'Docker' and 'DockerBuild'
bool usePublicFeedOnly = HasArgument ("PublicFeedsOnly");
var configuration = new Configuration();
var cakeConsole = new CakeConsole();
var configJsonFile = "cake-config.json";
var unicornSyncScript = $"./scripts/Unicorn/Sync.ps1";
var packagingScript = $"./scripts/Packaging/generate-update-package.ps1";
var dacpacScript = $"./scripts/Packaging/generate-dacpac.ps1";
bool publishLocal = false;
bool syncUnicorn = true;
bool applyTransforms = true;
/*===============================================
================ MAIN TASKS =====================
===============================================*/

Setup(context =>
{
  cakeConsole.ForegroundColor = ConsoleColor.Yellow;
  PrintHeader(cakeConsole, ConsoleColor.DarkGreen);

  var configFile = new FilePath(configJsonFile);
  configuration = DeserializeJsonFromFile<Configuration>(configFile);
  configuration.SolutionFile =  $"{configuration.ProjectFolder}\\{configuration.SolutionName}";
  configuration.PublishWebFolder = $"{configuration.ProjectFolder}\\data\\cm\\src";
  configuration.PublishxConnectFolder = $"{configuration.ProjectFolder}\\data\\xconnect\\src";

  if (deploymentTarget.Contains("Docker"))  {
    configuration.UnicornSerializationFolder = "c:\\unicorn"; // This maps to the container's volume setting (see docker-compose.yml)
    applyTransforms = false;
  }

  if (deploymentTarget == "DockerBuild")  {
    configuration.PublishWebFolder = $"{configuration.ProjectFolder}\\docker\\images\\windows\\demo-xp-standalone\\Data";
    configuration.PublishDataFolder = $"{configuration.ProjectFolder}\\docker\\images\\windows\\demo-xp-sqldev\\Data";
    configuration.PublishxConnectFolder = $"{configuration.ProjectFolder}\\docker\\images\\windows\\demo-xp-xconnect\\Data";
    configuration.PublishxConnectIndexWorkerFolder = $"{configuration.ProjectFolder}\\docker\\images\\windows\\demo-xp-xconnect-indexworker\\Data";
    publishLocal = true;
    syncUnicorn = false;
  }

  if (deploymentTarget == "Docker") {
    configuration.WebsiteRoot = $"{configuration.ProjectFolder}\\data\\cm\\src\\";
    configuration.XConnectRoot = $"{configuration.ProjectFolder}\\data\\xconnect\\src\\";
    configuration.PublishxConnectIndexWorkerFolder = $"{configuration.ProjectFolder}\\data\\xconnect-indexworker\\src\\";
    configuration.InstanceUrl = "http://127.0.0.1:44001";     // This is based on the CM container's settings (see docker-compose.yml)
  }

// Automatically add additional NuGet source to local feed at build time
// Requires environment variables
//   SYSTEM_ACCESSTOKEN:      DevOps Personal Access Token
//   INTERNAL_NUGET_SOURCE:   feed's URL
  var accessToken = EnvironmentVariable ("SYSTEM_ACCESSTOKEN");
  var internalFeed = EnvironmentVariable ("INTERNAL_NUGET_SOURCE");

  if (!string.IsNullOrEmpty(internalFeed)){
    var feed = new {
      Name = "sc-demo-packages-internal",
      Source = internalFeed
    };
    if (NuGetHasSource (source: feed.Source)) {
      Information("Removing internal NuGet feed");
      NuGetRemoveSource (
        name: feed.Name,
        source: feed.Source
      );
    }
    if (!string.IsNullOrEmpty(accessToken) && !usePublicFeedOnly && !string.IsNullOrEmpty(internalFeed)) {
      // Add the authenticated feed source
      var feedSettings = new NuGetSourcesSettings {
      UserName = "VSTS",
        Password = accessToken,
        IsSensitiveSource = true

      };
      Information("Adding internal NuGet feed");
      NuGetAddSource (
        name: feed.Name,
        source: feed.Source,
        settings: feedSettings
      );

    }
  }
  // end automatically add NuGet feed
});

/*===============================================
============ Local Build - Main Tasks ===========
===============================================*/

Task("Base-PreBuild")
.IsDependentOn("CleanBuildFolders")
.IsDependentOn("Copy-Sitecore-Lib")
.IsDependentOn("Modify-PublishSettings");

Task("Base-Publish")
.IsDependentOn("Publish-Core-Project")
.IsDependentOn("Publish-FrontEnd-Project")
.IsDependentOn("Apply-DotnetCore-Transforms")
.IsDependentOn("Publish-All-Projects")
.IsDependentOn("Copy-to-Destination")
.IsDependentOn("Publish-xConnect-Project")
.IsDependentOn("Publish-xConnect-Project-IndexWorker")
.IsDependentOn("Modify-Unicorn-Source-Folder");

Task("Default")
.IsDependentOn("Base-PreBuild")
.IsDependentOn("Base-Publish")
.IsDependentOn("Merge-and-Copy-Xml-Transform")
.IsDependentOn("Generate-Dacpacs")
.IsDependentOn("Post-Deploy");

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
.IsDependentOn("Apply-DotnetCore-Transforms")
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
  CleanDirectories(configuration.PublishxConnectFolder);
  CleanDirectories(configuration.PublishDataFolder);
  CleanDirectories(configuration.PublishTempFolder);
});

/*===============================================
=============== Generic Tasks ===================
===============================================*/

Task("Copy-Sitecore-Lib")
.WithCriteria(()=>(configuration.BuildConfiguration == "Local"))
.Does(() => {
  CopySitecoreLib(configuration);
});

Task("Publish-All-Projects")
.IsDependentOn("Build-Solution")
.IsDependentOn("Publish-Foundation-Projects")
.IsDependentOn("Publish-Feature-Projects")
.IsDependentOn("Publish-Project-Projects");

Task("Build-Solution")
.IsDependentOn("Copy-Sitecore-Lib")
.Does(() => {
  MSBuild(configuration.SolutionFile, cfg => InitializeMSBuildSettings(cfg, configuration));
});

Task("Publish-Foundation-Projects").Does(() => {
  PublishSourceProjects(publishLocal, configuration.FoundationSrcFolder, configuration);
});

Task("Publish-Feature-Projects").Does(() => {
  PublishSourceProjects(publishLocal, configuration.FeatureSrcFolder, configuration);
});

Task("Publish-Core-Project").Does(() => {
  var projectFile = $"{configuration.SourceFolder}\\Build\\Build.Shared\\code\\Build.Shared.csproj";
  PublishCoreProject(projectFile, publishLocal, configuration);
});

Task("Publish-FrontEnd-Project").Does(() => {
  PublishFrontEndProject(publishLocal, configuration);
});

Task("Copy-to-Destination").Does(() => {
  CopyToDestination(publishLocal, configuration);
});

Task("Apply-DotnetCore-Transforms")
.WithCriteria(() => (!publishLocal && applyTransforms))
.Does(() => {
  ApplyDotnetCoreTransforms(configuration, publishLocal);
});

Task("Publish-YML")
.WithCriteria(() => publishLocal)
.Does(() => {
  PublishYML(configuration);
});

Task("Create-UpdatePackage")
.WithCriteria(() => publishLocal)
.IsDependentOn("Publish-YML")
.Does(() => {
  CreateUpdatePackage(configuration, packagingScript);
});

Task("Generate-Dacpacs")
.WithCriteria(() => publishLocal)
.IsDependentOn("Create-UpdatePackage")
.Does(() => {
  GenerateDacpacs(configuration, dacpacScript);
});

Task("Publish-Project-Projects").Does(() => {
  var global = $"{configuration.ProjectSrcFolder}\\Global";
  var habitatHome = $"{configuration.ProjectSrcFolder}\\HabitatHome";

  PublishSourceProjects(publishLocal, global, configuration);
  PublishSourceProjects(publishLocal, habitatHome, configuration);
});

Task("Publish-xConnect-Project").Does(() => {
  PublishXConnectProjects(publishLocal, configuration);
});

Task("Publish-xConnect-Project-IndexWorker")
.WithCriteria(() => (deploymentTarget.Contains("Docker")))
.Does(() => {
  var xConnectProject = $"{configuration.ProjectSrcFolder}\\xConnect";
  var destination = configuration.PublishxConnectIndexWorkerFolder;
  PublishProjects(xConnectProject, destination, configuration);
});

Task("Apply-Xml-Transform")
.WithCriteria(() => (!publishLocal && applyTransforms))
.Does(() => {
  ApplyXmlTransform(configuration, publishLocal);
});

Task("Merge-and-Copy-Xml-Transform")
.WithCriteria(() => (publishLocal || !applyTransforms))
.Does(() => {
  MergeAndCopyXmlTransform(configuration);
});

Task("Modify-Unicorn-Source-Folder")
.Does(() => {
  var destination = configuration.WebsiteRoot;
  if (publishLocal) {
    destination = configuration.PublishWebFolder;
  }
  var zzzDevSettingsFile = File($"{destination}/App_config/Include/Project/z.DevSettings.config");
  ModifyUnicornSourceFolder(configuration, zzzDevSettingsFile, "sourceFolder");
});

Task("Turn-On-Unicorn")
.WithCriteria(() => (syncUnicorn && deploymentTarget != "Docker"))
.Does(() => {
  TurnOnUnicorn(configuration);
});

Task("Modify-PublishSettings").Does(() => {
  ModifyPublishSettings(configuration);
});

Task("Sync-Unicorn")
.IsDependentOn("Turn-On-Unicorn")
.IsDependentOn("Modify-Unicorn-Source-Folder")
.WithCriteria(() => syncUnicorn)
.Does(() => {
  SyncUnicorn(configuration, unicornSyncScript);
});

Task("Deploy-EXM-Campaigns")
.WithCriteria(() => !publishLocal)
.Does(() => {
  Spam(() => DeployExmCampaigns(configuration), configuration.DeployExmTimeout);
});

Task("Deploy-Marketing-Definitions")
.WithCriteria(() => !publishLocal)
.Does(() => {
  DeployMarketingDefinitions(configuration);
});
		
Task("Rebuild-Indexes")
.WithCriteria(() => !publishLocal).Does(() => {
  var indexes = new string[] {
    "core","master","web","test"
  };
  foreach (var index in indexes){
    RebuildIndex($"sitecore_{index}_index", configuration);
  }
});

RunTarget(target);
