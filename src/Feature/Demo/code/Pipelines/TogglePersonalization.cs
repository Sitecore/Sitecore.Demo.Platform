using Sitecore.Diagnostics;
using Sitecore.Personalization.Mvc.Pipelines.Response.CustomizeRendering;

namespace Sitecore.Demo.Platform.Feature.Demo.Pipelines
{
	public class TogglePersonalization : Personalize
	{
		public override void Process(CustomizeRenderingArgs args)
		{
			Assert.ArgumentNotNull((object)args, nameof(args));
			if (args.PageContext.RequestContext.HttpContext.Request.Cookies[Constants.TogglePersonalizationCookie]?.Value?.ToLower() == "false")
			{
				return;
			}
			else
			{
				base.Process(args);
			}
		}		
	}
}
