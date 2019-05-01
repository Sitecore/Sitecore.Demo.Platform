using System;
using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.HabitatHome.Feature.News.Models;

namespace Sitecore.HabitatHome.Feature.News
{
    public class NewsRepository
    {
        public static Item ResolveNewsItemByUrl(string urlPath)
        {
            try
            {
                if (string.IsNullOrEmpty(urlPath)) return null;

                var newsSlug = urlPath.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries).Last();

                if (string.IsNullOrEmpty(newsSlug))
                    return null;
                var item = Context.Site.Database.GetItem($"{Context.Site.ContentStartPath}/Data/News");
                var indexname = $"sitecore_{Context.Site.Database.Name.ToLowerInvariant()}_index";
                if (item != null)
                {
                    var newsIndex = ContentSearchManager.GetIndex(indexname);
                    using (var context = newsIndex.CreateSearchContext())
                    {
                        var results = context.GetQueryable<NewsSearchResultItem>()
                            .Where(x => x.Paths.Contains(item.ID) && x.TemplateId == Templates.News.ID &&
                                        x.NewsSlug == newsSlug).ToList();

                        if (results.Any())
                        {
                            var newsItem = results.First().GetItem();
                            return newsItem;
                        }
                    }

                    return null;
                }
            }


            catch (Exception e)
            {
                Log.Error("An error occured on news wildcard ResolveNewsItemByUrl", e.InnerException,
                    typeof(NewsRepository));
                return null;
            }

            return null;
        }
    }
}