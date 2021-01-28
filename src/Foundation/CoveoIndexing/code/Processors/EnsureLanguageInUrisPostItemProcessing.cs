using System.Reflection;
using System.Text.RegularExpressions;
using Coveo.Framework.CNL;
using Coveo.Framework.Log;
using Coveo.Framework.Processor;
using Coveo.SearchProvider.Pipelines;
using Sitecore.ContentSearch;

namespace Sitecore.Demo.Platform.Foundation.CoveoIndexing.Processors
{
	/// <summary>
	/// Ensures the item language is present after the hostname.
	/// </summary>
	public class EnsureLanguageInUrisPostItemProcessing : IProcessor<CoveoPostItemProcessingPipelineArgs>
	{
		private static readonly ILogger _Logger = CoveoLogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private const string LANGUAGE_REGEX_GROUP_NAME = "language";
		private const string IS_LANGUAGE_IN_URI_REGEX_PATTERN = "[^:]+://[^/]+/?(?<" + LANGUAGE_REGEX_GROUP_NAME + ">[^/]*)";
		private const string ADD_LANGUAGE_TO_URI_REGEX_PATTERN = "([^:]+://[^/]+)(.*)";

		public void Process(CoveoPostItemProcessingPipelineArgs p_Args)
		{
			_Logger.TraceEntering("Process(CoveoPostItemProcessingPipelineArgs)");
			Precondition.NotNull(p_Args, () => () => p_Args);
			Precondition.NotNull(p_Args.Item, () => () => p_Args.Item);
			Precondition.NotNull(p_Args.CoveoItem, () => () => p_Args.CoveoItem);

			string itemLanguage = ((SitecoreIndexableItem) p_Args.Item)?.Item?.Language?.Name;
			if (string.IsNullOrEmpty(itemLanguage))
			{
				return;
			}
			itemLanguage = itemLanguage.ToLower();

			_Logger.Trace("LANGUAGE FOUND: " + itemLanguage);

			if (!IsLanguageInUri(p_Args.CoveoItem.ClickableUri, itemLanguage))
			{
				p_Args.CoveoItem.ClickableUri = AddLanguageToUri(p_Args.CoveoItem.ClickableUri, itemLanguage);
			}

			if (!IsLanguageInUri(p_Args.CoveoItem.PrintablePath, itemLanguage))
			{
				p_Args.CoveoItem.PrintablePath = AddLanguageToUri(p_Args.CoveoItem.PrintablePath, itemLanguage);
			}

			if (!IsLanguageInUri(p_Args.CoveoItem.Uri, itemLanguage))
			{
				p_Args.CoveoItem.Uri = AddLanguageToUri(p_Args.CoveoItem.Uri, itemLanguage);
			}

			_Logger.TraceExiting("Process(CoveoPostItemProcessingPipelineArgs)");
		}

		private bool IsLanguageInUri(string uri, string language)
		{
			Match regexMatch = Regex.Match(uri, IS_LANGUAGE_IN_URI_REGEX_PATTERN);
			return regexMatch.Success && regexMatch.Groups.Count == 2 && regexMatch.Groups[LANGUAGE_REGEX_GROUP_NAME].Success && string.Compare(regexMatch.Groups[LANGUAGE_REGEX_GROUP_NAME].Value, language, true) == 0;
		}

		private string AddLanguageToUri(string uri, string language)
		{
			return Regex.Replace(uri, ADD_LANGUAGE_TO_URI_REGEX_PATTERN, "$1/" + language + "$2");
		}
	}
}
