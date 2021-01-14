using System.Text.RegularExpressions;
using Coveo.Framework.Processor;
using Coveo.SearchProvider.Pipelines;

namespace Sitecore.Demo.Platform.Foundation.CoveoIndexing.Processors
{
	/// <summary>
	/// Ensures Coveo indexes web database items using the CD base URI.
	/// </summary>
	public class EnsureWebDatabaseUseCdUrisPostItemProcessing : IProcessor<CoveoPostItemProcessingPipelineArgs>
	{
		private const string HOSTNAME_REGEX_PATTERN = "://[^/]+";
		private const string CD_REGEX_REPLACEMENT = "://cd";

		public void Process(CoveoPostItemProcessingPipelineArgs p_Args)
		{
			if (p_Args == null || p_Args.CoveoItem == null || p_Args.CoveoItem.Metadata == null)
			{
				return;
			}

			string databaseName = (string) p_Args.CoveoItem.Metadata["_database"];

			if (databaseName.ToLower() == "web") {
				Regex hostnameRegex = new Regex(HOSTNAME_REGEX_PATTERN, RegexOptions.IgnoreCase | RegexOptions.Compiled);

				p_Args.CoveoItem.ClickableUri = hostnameRegex.Replace(p_Args.CoveoItem.ClickableUri, CD_REGEX_REPLACEMENT);
				p_Args.CoveoItem.PrintablePath = hostnameRegex.Replace(p_Args.CoveoItem.PrintablePath, CD_REGEX_REPLACEMENT);
				p_Args.CoveoItem.Uri = hostnameRegex.Replace(p_Args.CoveoItem.Uri, CD_REGEX_REPLACEMENT);
			}
		}
	}
}
