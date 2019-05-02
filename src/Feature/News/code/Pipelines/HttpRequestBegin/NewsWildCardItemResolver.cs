using System;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.HttpRequest;

namespace Sitecore.HabitatHome.Feature.News.Pipelines.HttpRequestBegin
{
    public class NewsWildCardItemResolver : HttpRequestProcessor
    {
        public override void Process(HttpRequestArgs args)
        {
            if (Context.Site.Name.Equals("shell", StringComparison.InvariantCultureIgnoreCase) ||
                Context.Domain.Name.Equals("sitecore", StringComparison.InvariantCultureIgnoreCase))
                return;

            Assert.ArgumentNotNull(args, "args");


            if (Context.Database == null
                || args.Url.ItemPath.Length == 0
                || Context.Item == null
                || !Context.Item.TemplateID.Equals(Templates.News.ID))
                return;

            var newsItem = NewsRepository.ResolveNewsItemByUrl(args.Url.FilePath);

            if (newsItem != null)
            {
                Context.Items[Foundation.SitecoreExtensions.Constants.WildCardItemResolvedKey] = true;
                Context.Items[Foundation.SitecoreExtensions.Constants.WildCardItemResolvedOldContextItemKey] =
                    Context.Item;
                Context.Item = newsItem;
            }
        }
    }
}