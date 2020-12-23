using System;

namespace Sitecore.Demo.Platform.Global.Website.Pipelines.GetStartUrl
{
	using Sitecore.Pipelines.GetStartUrl;

	public class ConvertToRelativeUrl : GetStartUrlProcessor
	{
		public override void Process(GetStartUrlArgs args)
		{
			if (args.Result.IsAbsoluteUri)
			{
				args.Result = new Uri(args.Result.PathAndQuery, UriKind.Relative);
			}
		}
	}
}
