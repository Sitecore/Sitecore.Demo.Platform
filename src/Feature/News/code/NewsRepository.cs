using System;
using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.HabitatHome.Feature.News
{
    public class NewsRepository
    {
        public static Item ResolveNewsItemByUrl(string urlPath)
        {
            try
            {
                if (string.IsNullOrEmpty(urlPath)) return null;

                var term = urlPath.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries).Last();

                term = term.Replace("-", " ");


                if (string.IsNullOrEmpty(term))
                    return null;

                var newsIndex = ContentSearchManager.GetIndex("sitecore_master_index");
                using (var context = newsIndex.CreateSearchContext())
                {
                    var results = context.GetQueryable<SearchResultItem>().Where(x =>
                        x.Paths.Contains(new ID(Context.Site.StartPath)) && x.TemplateId == Templates.News.ID).ToList();

                    if (results.Any())
                    {
                        var newsItem = results.First().GetItem();
                        return newsItem;
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
        }
    }
}