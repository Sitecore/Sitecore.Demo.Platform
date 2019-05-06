using System;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.HabitatHome.Feature.News.Repositories;
using Sitecore.HabitatHome.Feature.News.Services;
using Sitecore.Pipelines.HttpRequest;

namespace Sitecore.HabitatHome.Feature.News.Pipelines.HttpRequestBegin
{
    public class NewsWildCardItemResolver : HttpRequestProcessor
    {
        private readonly INewsRepository _newsRepository;
        private readonly INewsSettingsService _newsSettingsService;

        public NewsWildCardItemResolver(INewsRepository newsRepository, INewsSettingsService newsSettingsService)
        {
            _newsRepository = newsRepository;
            _newsSettingsService = newsSettingsService;
        }

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


            var newsItem = _newsRepository.ResolveNewsItemByUrl(args.Url.FilePath);

            if (newsItem != null)
            {
                Context.Items[Foundation.SitecoreExtensions.Constants.WildCardItemResolvedKey] = true;
                Context.Items[Foundation.SitecoreExtensions.Constants.WildCardItemResolvedOldContextItemKey] = Context.Item;
                Context.Item = newsItem;
            }
            else
            {
                // Redirect to the configured Not Found Page for the given incorrect news slug
                var newsSettingsItem = _newsSettingsService.GetNewsSettingsItem();
                var newsSlugNotFoundItem = Context.Database.GetItem(new ID(newsSettingsItem[Templates.NewsSettings.Fields.NewsSlugNotFoundPage]));
                if (newsSlugNotFoundItem != null)
                {
                    Context.Items[Foundation.SitecoreExtensions.Constants.WildCardItemResolvedKey] = true;
                    Context.Items[Foundation.SitecoreExtensions.Constants.WildCardItemResolvedOldContextItemKey] = Context.Item;
                    Context.Item = newsSlugNotFoundItem;
                }
            }
        }
    }
}