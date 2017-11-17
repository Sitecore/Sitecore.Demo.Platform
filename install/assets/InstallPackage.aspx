<%@ Page Language="c#" Inherits="System.Web.UI.Page" Debug="true" CodePage="65001" %>

<%@ OutputCache Location="None" VaryByParam="none" %>
<%@ Import Namespace="Sitecore.Data" %>
<%@ Import Namespace="Sitecore.Data.Items" %>
<%@ Import Namespace="Sitecore.Data.Serialization" %>
<%@ Import Namespace="Sitecore.ContentSearch.Maintenance" %>
<%@ Import Namespace="Sitecore.Data.Events" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html lang="en" xml:lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>Welcome to Sitecore</title>
  <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
  <meta name="CODE_LANGUAGE" content="C#" />
  <meta name="vs_defaultClientScript" content="JavaScript" />
  <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />
</head>
<body>
  <form id="mainform" method="post" runat="server">
  </form>
</body>
</html>

<script language="c#" runat="server">
  public void Page_Load(object sender, EventArgs args)
  {
    string packageFileName = string.Empty;

    try
    {
      packageFileName = Request.QueryString["package"];
      string path = Sitecore.IO.FileUtil.MapPath(packageFileName);
      IndexCustodian.PauseIndexing();
      Sitecore.Diagnostics.Log.Info("Installing package {0}", path);
      
      using (new Sitecore.SecurityModel.SecurityDisabler())
      {
        using (new Sitecore.Security.Accounts.UserSwitcher("sitecore\\admin", true))
        {
          Sitecore.Install.Installer installer = new Sitecore.Install.Installer();
          installer.InstallPackage(path);
          installer.InstallSecurity(path);
          if (Request.QueryString["sps"] != "true")
          {
            InstallPostStep(installer, path);
          }
        }
      }
    }
    catch(Exception ex)
    {
      Sitecore.Diagnostics.Log.Error("Failed to install package " + packageFileName, ex, this);
      throw ex;
    }
  }
  
  protected void InstallPostStep (Sitecore.Install.Installer installer, string path)
  {
    try 
    {
    Sitecore.Install.Framework.IProcessingContext context = Sitecore.Install.Installer.CreateInstallationContext();
    Sitecore.Install.Framework.ISource<Sitecore.Install.Framework.PackageEntry> source = new Sitecore.Install.Zip.PackageReader(path);
    Sitecore.Install.Metadata.MetadataView view = new Sitecore.Install.Metadata.MetadataView(context);
    Sitecore.Install.Metadata.MetadataSink sink = new Sitecore.Install.Metadata.MetadataSink(view);
    sink.Initialize(context);
    source.Populate(sink);
    installer.ExecutePostStep(view.PostStep, context);
    }
    catch (Exception ex)
    {
    Sitecore.Diagnostics.Log.Error("Post steps failed with error", ex);
    }
  }
</script>
