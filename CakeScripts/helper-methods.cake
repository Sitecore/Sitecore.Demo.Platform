using System.Text.RegularExpressions;

/*===============================================
================= HELPER METHODS ================
===============================================*/

public class Configuration
{
  private MSBuildToolVersion _msBuildToolVersion;

  public string WebsiteRoot {get;set;}
  public string XConnectRoot {get;set;}
  public string InstanceUrl {get;set;}
  public string SolutionName {get;set;}
  public string ProjectFolder {get;set;}
  public string BuildConfiguration {get;set;}
  public string MessageStatisticsApiKey {get;set;}
  public string MarketingDefinitionsApiKey {get;set;}
  public bool RunCleanBuilds {get;set;}
  public int DeployExmTimeout {get;set;}
  public string DeployFolder {get;set;}
  public string Version {get;set;}
  public string SitecoreAzureToolkitPath {get;set;}
  public string PublishTempFolder {get;set;}
  public string SolutionFile {get;set;}
  public string UnicornSerializationFolder {get;set;}
  public string BuildToolVersions
  {
    set
    {
      if(!Enum.TryParse(value, out this._msBuildToolVersion)) {
        this._msBuildToolVersion = MSBuildToolVersion.Default;
      }
    }
  }

  public string SourceFolder => $"{ProjectFolder}\\src";
  public string FoundationSrcFolder => $"{SourceFolder}\\Foundation";
  public string FeatureSrcFolder => $"{SourceFolder}\\Feature";
  public string ProjectSrcFolder => $"{SourceFolder}\\Project";
  public string PublishWebFolder {get;set;}
  public string PublishxConnectFolder {get;set;}
  public string PublishDataFolder {get;set;}

  public MSBuildToolVersion MSBuildToolVersion => this._msBuildToolVersion;
  public string BuildTargets => this.RunCleanBuilds ? "Clean;Build" : "Build";
}

public void PrintHeader(ConsoleColor foregroundColor) {
  cakeConsole.ForegroundColor = foregroundColor;
  cakeConsole.WriteLine("     ");
  cakeConsole.WriteLine("     ");
  cakeConsole.WriteLine(@"   ) )       /\                  ");
  cakeConsole.WriteLine(@"  =====     /  \                 ");
  cakeConsole.WriteLine(@" _|___|____/ __ \____________    ");
  cakeConsole.WriteLine(@"|:::::::::/ ==== \:::::::::::|   ");
  cakeConsole.WriteLine(@"|:::::::::/ ====  \::::::::::|   ");
  cakeConsole.WriteLine(@"|::::::::/__________\:::::::::|  ");
  cakeConsole.WriteLine(@"|_________|  ____  |_________|                                                               ");
  cakeConsole.WriteLine(@"| ______  | / || \ | _______ |            _   _       _     _ _        _     _   _");
  cakeConsole.WriteLine(@"||  |   | | ====== ||   |   ||           | | | |     | |   (_) |      | |   | | | |");
  cakeConsole.WriteLine(@"||--+---| | |    | ||---+---||           | |_| | __ _| |__  _| |_ __ _| |_  | |_| | ___  _ __ ___   ___");
  cakeConsole.WriteLine(@"||__|___| | |   o| ||___|___||           |  _  |/ _` | '_ \| | __/ _` | __| |  _  |/ _ \| '_ ` _ \ / _ \");
  cakeConsole.WriteLine(@"|======== | |____| |=========|           | | | | (_| | |_) | | || (_| | |_  | | | | (_) | | | | | |  __/");
  cakeConsole.WriteLine(@"(^^-^^^^^- |______|-^^^--^^^)            \_| |_/\__,_|_.__/|_|\__\__,_|\__| \_| |_/\___/|_| |_| |_|\___|");
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

public void PublishProjects(string rootFolder, string publishRoot)
{
  Func<IFileSystemInfo, bool> excludes = fileSystemInfo => !fileSystemInfo.Path.FullPath.Contains("CRM");

  var projects = GetFiles($"{rootFolder}\\**\\code\\*.csproj", excludes);
  Information("Publishing " + rootFolder + " to " + publishRoot);
  foreach (var project in projects) {
    MSBuild(project, cfg => InitializeMSBuildSettingsInternal(cfg)
      .WithTarget(configuration.BuildTargets)
      .WithProperty("DeployOnBuild", "true")
      .WithProperty("DeployDefaultTarget", "WebPublish")
      .WithProperty("WebPublishMethod", "FileSystem")
      .WithProperty("DeleteExistingFiles", "false")
      .WithProperty("publishUrl", publishRoot)
      .WithProperty("BuildProjectReferences", "false")
      );
  }
}

public void PublishCoreProjects(string rootFolder, string publishRoot)
{

}

public FilePathCollection GetTransformFiles(string rootFolder)
{
  Func<IFileSystemInfo, bool> exclude_obj_bin_folder = fileSystemInfo => !fileSystemInfo.Path.FullPath.Contains("/obj/") && !fileSystemInfo.Path.FullPath.Contains("/bin/");

  var xdtFiles = GetFiles($"{rootFolder}\\**\\*.xdt", exclude_obj_bin_folder);

  return xdtFiles;
}

public void Transform(string rootFolder, string filter, string publishDestination, string[] excludePatterns)
{
  var xdtFiles = GetTransformFiles(rootFolder);


  foreach (var file in xdtFiles) {

    if (excludePatterns.Any(s => file.FullPath.ToLower().Contains(s.ToLower()))) {
      Information ($"Skipping {file}");
      continue;
    }
    Information($"Applying configuration transform:{file.FullPath}");
    var fileToTransform = Regex.Replace(file.FullPath, $".+{filter}/(.*.config).?(.*).xdt", "$1");
    fileToTransform = Regex.Replace(fileToTransform, ".sc-internal", "");
    var sourceTransform = $"{publishDestination}\\{fileToTransform}";

    XdtTransformConfig(sourceTransform		// Source File
              , file.FullPath		// Tranforms file (*.xdt)
              , sourceTransform);	// Target File
  }
}

public void RebuildIndex(string indexName)
{
  var url = $"{configuration.InstanceUrl}/utilities/indexrebuild.aspx?index={indexName}";
  string responseBody = HttpGet(url);
}

public void DeployExmCampaigns()
{
  var url = $"{configuration.InstanceUrl}/utilities/deployemailcampaigns.aspx?apiKey={configuration.MessageStatisticsApiKey}";
  var responseBody = HttpGet(url, settings => {
    settings.AppendHeader("Connection", "keep-alive");
  });
  Information(responseBody);
}

public MSBuildSettings InitializeMSBuildSettings(MSBuildSettings settings)
{
  InitializeMSBuildSettingsInternal(settings)
    .WithRestore();
  return settings;
}

private MSBuildSettings InitializeMSBuildSettingsInternal(MSBuildSettings settings)
{
  settings.SetConfiguration(configuration.BuildConfiguration)
    .SetVerbosity(Verbosity.Minimal)
    .SetMSBuildPlatform(MSBuildPlatform.Automatic)
    .SetPlatformTarget(PlatformTarget.MSIL)
    .UseToolVersion(configuration.MSBuildToolVersion)
    .SetMaxCpuCount(1);
  return settings;
}

public void CreateFolder(string folderPath)
{
  if (!DirectoryExists(folderPath)) {
    CreateDirectory(folderPath);
  }
}

public void Spam(Action action, int? timeoutMinutes = null)
{
  Exception lastException = null;
  var startTime = DateTime.Now;
  while (timeoutMinutes == null || (DateTime.Now - startTime).TotalMinutes < timeoutMinutes) {
    try {
      action();

      Information($"Completed in {(DateTime.Now - startTime).Minutes} min {(DateTime.Now - startTime).Seconds} sec.");
      return;
    }
    catch (AggregateException aex) {
      foreach (var x in aex.InnerExceptions)
      {
        Information($"{x.GetType().FullName}: {x.Message}");
      }
      lastException = aex;
    }
    catch (Exception ex) {
      Information($"{ex.GetType().FullName}: {ex.Message}");
      lastException = ex;
    }
  }

  throw new TimeoutException($"Unable to complete within {timeoutMinutes} minutes.", lastException);
}

public void WriteError(string errorMessage)
{
  cakeConsole.ForegroundColor = ConsoleColor.Red;
  cakeConsole.WriteError(errorMessage);
  cakeConsole.ResetColor();
}

public void MergeTransforms(string source, string destination, string[] excludePatterns)
{
  var xdtFiles = GetTransformFiles(source);

  foreach (var file in xdtFiles)
  {
    if (excludePatterns.Any(s => file.FullPath.ToLower().Contains(s.ToLower()))) {
      Information ($"Skipping {file}");
      continue;
    }

    FilePath xdtFilePath =(FilePath)file;
    Information($"Processing {xdtFilePath}");
    FilePath fileToTransform = Regex.Replace(file.FullPath, "(.*.config).?(.*)", "$1.xdt");

    fileToTransform = ((FilePath)$"{source}").GetRelativePath((FilePath)fileToTransform);
    FilePath sourceTransform = $"{(FilePath)fileToTransform}";

    var targetTansformPath = ((DirectoryPath)destination).CombineWithFilePath((FilePath)sourceTransform);

    if (!FileExists(targetTansformPath)) {
      CreateFolder(targetTansformPath.GetDirectory().FullPath);
      CopyFile(xdtFilePath.FullPath, targetTansformPath);
    }
    else {
      MergeFile(targetTansformPath.FullPath	    // Source File
        , xdtFilePath.FullPath			// Tranforms file (*.xdt)
        , targetTansformPath.FullPath);		// Target File
    }
  }
}
