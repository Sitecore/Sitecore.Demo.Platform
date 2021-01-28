using System;
using System.IO;
using System.Web.UI;

namespace Sitecore.Demo.Platform.Foundation.CoveoIndexing.Utilities
{
	public partial class IsCoveoActivated : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			bool isActivated = File.Exists("C:\\inetpub\\wwwroot\\App_Config\\Include\\Coveo\\Coveo.SearchProvider.Custom.config");

			Response.Write($"{{ \"IsActivated\": {isActivated.ToString().ToLower()} }}");
			Response.End();
		}
	}
}
