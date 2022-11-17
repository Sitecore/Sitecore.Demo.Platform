using System;
using Sitecore.Data.Managers;
using Sitecore.Publishing;
using Sitecore.SecurityModel;

namespace Sitecore.Demo.Platform.Global.Website.Utilities
{
	public partial class Publish : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Sitecore.Context.SetActiveSite("shell");
			using (new SecurityDisabler())
			{
				var master = Configuration.Factory.GetDatabase("master");
				var web = Configuration.Factory.GetDatabase("web");
				var handle = PublishManager.PublishSmart(Sitecore.Client.ContentDatabase, new[] { web }, LanguageManager.GetLanguages(master).ToArray(), Sitecore.Context.Language);
				PublishManager.WaitFor(handle);
			}
		}
	}
}
