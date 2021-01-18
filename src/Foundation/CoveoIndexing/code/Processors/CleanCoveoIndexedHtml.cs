using System;
using Coveo.Framework.Processor;
using Coveo.SearchProvider.Processors.FetchPageContent.PostProcessing;
using HtmlAgilityPack;
using Sitecore.Diagnostics;

namespace Sitecore.Demo.Platform.Foundation.CoveoIndexing.Processors
{
	/// <summary>
	/// Removes select HTML nodes from Coveo fetched HTML before indexing items.
	/// </summary>
	public class CleanCoveoIndexedHtml : IFetchPageContentHtmlPostProcessingProcessor, IProcessor<FetchPageContentHtmlPostProcessingArgs>
	{
		public void Process(FetchPageContentHtmlPostProcessingArgs p_Args)
		{
			if (string.IsNullOrEmpty(p_Args.HtmlContent))
			{
				return;
			}

			try
			{
				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(p_Args.HtmlContent);
				var documentNode = htmlDocument.DocumentNode;

				var cssSelectorsToRemove = new string[] {
					"header",
					"footer",
					"#sidebar", // demo sidebar
					".privacy-warning", // Cookie confirmation warning
					".product-items-block", // Recommended content at the end of articles
					".link-list.social-links", // Articles social links
					".page-teaser", // Home page articles at the bottom
					".page-list.article-list", // List of articles on the articles page
					".list-pagination", // Pager on the articles page
					".page-selector", // Pager on the locations page
					".facet-component" // Distance facet on the locations page
				};

				foreach (string cssSelector in cssSelectorsToRemove)
				{
					RemoveNodes(documentNode, cssSelector);
				}

				p_Args.HtmlContent = documentNode.InnerHtml;
			}
			catch (Exception ex)
			{
				Log.Error($"An error occurred while cleaning HTML of item {p_Args.Item.Id} at {p_Args.Uri}", ex, this);
			}
		}

		private void RemoveNodes(HtmlNode documentNode, string cssSelector)
		{
			var nodesToRemove = documentNode.QuerySelectorAll(cssSelector);
			if (nodesToRemove == null)
			{
				return;
			}

			foreach (var nodeToRemove in nodesToRemove)
			{
				nodeToRemove.Remove();
			}
		}
	}
}
