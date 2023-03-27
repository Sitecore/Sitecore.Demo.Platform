using System;

namespace Sitecore.Demo.Platform.Global.Website.Utilities
{
	using Sitecore.Configuration;

	public partial class Restart : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var web = Factory.GetDatabase("web");
			web.RemoteEvents.EventQueue.Cleanup(0);

			try
			{
				var master = Factory.GetDatabase("master");
				master.RemoteEvents.EventQueue.Cleanup(0);

				var core = Factory.GetDatabase("core");
				core.RemoteEvents.EventQueue.Cleanup(0);
			}
			catch
			{
				// Ignore on CD
			}

			Sitecore.Install.Installer.RestartServer();
		}
	}
}
