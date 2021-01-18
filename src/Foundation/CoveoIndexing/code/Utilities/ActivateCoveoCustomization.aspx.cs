using System;
using System.IO;
using System.Web.UI;
using System.Xml;

namespace Sitecore.Demo.Platform.Foundation.CoveoIndexing.Utilities
{
	public partial class ActivateCoveoCustomization : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			bool isActivated = File.Exists("C:\\inetpub\\wwwroot\\App_Config\\Include\\Coveo\\Coveo.SearchProvider.Custom.config");
			if (!isActivated)
			{
				Response.Write("{ \"IsActivated\": \"false\" }");
				Response.End();
			}

			ActivateCustomization();

			Response.Write("{ \"IsActivated\": \"true\" }");
			Response.End();
		}

		private void ActivateCustomization()
		{
			string[] foundationFilesToRename = Directory.GetFiles("C:\\inetpub\\wwwroot\\App_Config\\include\\Foundation", "*.demo", SearchOption.AllDirectories);
			RenameFiles(foundationFilesToRename, "demo");

			string[] projectFilesToRename = Directory.GetFiles("C:\\inetpub\\wwwroot\\App_Config\\include\\Project", "*.demo", SearchOption.AllDirectories);
			RenameFiles(projectFilesToRename, "demo");
		}

		private void RenameFiles(string[] filesToRename, string extension)
		{
			foreach (string fileName in filesToRename)
			{
				FileInfo fileInfo = new FileInfo(fileName);
				fileInfo.MoveTo(fileName.Replace($".config.{extension}", ".config"));
			}
		}
	}
}
