<%@ Application Inherits="System.Web.HttpApplication" Language="C#" %>
<%@ Import Namespace="Microsoft.Extensions.Configuration" %>
<%@ Import Namespace="Serilog" %>
<%@ Import Namespace="Sitecore.DataExchange.Web.Serilog" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Web.Hosting" %>
<%@ Import Namespace="System.Web.Http" %>
<%@ Import Namespace="Sitecore.DataExchange.Web" %>
<%@ Import Namespace="Sitecore.DataExchange.Web.Configurations" %>

<script runat="server">
    private static readonly string configurationDirectorySetting = ConfigurationManager.AppSettings["configurationDirectoryRoot"];
    private static readonly string configurationEnvironmentSetting = ConfigurationManager.AppSettings["configurationEnvironment"];
    private static readonly string[] configurationArguments = new string[0];
    private static readonly string _siteName = HostingEnvironment.SiteName;
    private static readonly string _applicationId = HostingEnvironment.ApplicationID;
    private static FileSystemWatcher _configFolderWatcher;
    private string _serverName;

    private IConfiguration Configuration { get; set; }

    public static void ConfigurationReload(object sender)
    {
        HostingEnvironment.InitiateShutdown();
    }

    public static void ConfigurationReload(object sender, EventArgs args)
    {
        ConfigurationReload(sender);
    }

    protected void Application_Start()
    {
        string executeUrl = Environment.GetEnvironmentVariable("TenantService_SfmcJourney_ExecuteUrl");
        string configPath = HostingEnvironment.MapPath("~\\sync-activity\\config.json");
        string config = File.ReadAllText(configPath);
        config = config.Replace("<Execute URL>", executeUrl).Replace("<Publish URL>", executeUrl);
        File.WriteAllText(configPath, config);

        _serverName = Server.MachineName;

        Application["ApplicationStartTime"] = DateTime.UtcNow;
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _serverName = Server.MachineName;
        string str = HostingEnvironment.MapPath(configurationDirectorySetting) ?? "~/App_Data";
        Configuration = (IConfiguration)new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddDefault(_serverName, HostingEnvironment.SiteName, new DirectoryInfo(str).FullName, configurationEnvironmentSetting, configurationArguments).Build(true);
        SubscribeToConfigChanges(Configuration, str, this);
        GlobalConfiguration.Configuration.Properties.GetOrAdd("SitecoreConfigurationRoot", Configuration);
        Start_Application_Logging();
        DataExchangeFramework_Context_Init();
        GlobalConfiguration.Configure(WebApiConfig.Register);
    }

    protected void Start_Application_Logging()
    {
        if (Configuration == null)
            return;
        LoggerConfigurationExtension.RegisterLoggerFromConfiguration(Configuration);
        Log.Logger.Information("Data Exchange Web Test Host Application Start, Machine: {0}, Site: {1}, AppId: {2}", _serverName, _siteName, _applicationId);
    }

    private static void SubscribeToConfigChanges(
      IConfiguration config,
      string configPath,
      object scope)
    {
        config.GetReloadToken().RegisterChangeCallback(ConfigurationReload, scope);
        SubscribeToFileChanges(configPath, "sc.*.xml", true, ConfigurationReload);
    }

    private static void SubscribeToFileChanges(
      string path,
      string filter,
      bool includeSubdirectories,
      EventHandler handler)
    {
        _configFolderWatcher = new FileSystemWatcher(path, filter);
        _configFolderWatcher.Changed += handler.Invoke;
        _configFolderWatcher.Created += handler.Invoke;
        _configFolderWatcher.Deleted += handler.Invoke;
        _configFolderWatcher.Renamed += handler.Invoke;
        _configFolderWatcher.Error += (sender, args) =>
            {
                Log.Error(args.GetException(), "An error was detected while monitoring configuration file changes.");
                handler(sender, args);
            };
        _configFolderWatcher.IncludeSubdirectories = includeSubdirectories;
        _configFolderWatcher.EnableRaisingEvents = true;
    }

    private void DataExchangeFramework_Context_Init()
    {
    }

    private void End_ApplicationLogging()
    {
        Log.Information("Data Exchange Web Test Host Application End, Machine: {0}, Site: {1}, AppId: {2}", _serverName, _siteName, _applicationId);
        Log.CloseAndFlush();
    }

    private void Logging_Error()
    {
        Log.Logger.Error("Data Exchange Web Application Error: {0}", Context.Error);
    }
</script>
