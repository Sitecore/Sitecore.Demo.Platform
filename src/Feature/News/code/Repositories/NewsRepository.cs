using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.HabitatHome.Feature.News.Models;
using Sitecore.HabitatHome.Foundation.DependencyInjection;

namespace Sitecore.HabitatHome.Feature.News.Repositories
{
    [Service(typeof(INewsRepository))]
    public class NewsRepository : INewsRepository
    {
        private readonly string indexName = "sitecore_web_index";

        public Item ResolveNewsItemByUrl(string urlPath)
        {
            try
            {
                if (string.IsNullOrEmpty(urlPath)) return null;

                var newsSlug = urlPath.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries).Last();

                if (string.IsNullOrEmpty(newsSlug))
                    return null;
                var item = Context.Site.Database.GetItem($"{Context.Site.ContentStartPath}/Data/News");

                if (item != null)
                {
                    var newsIndex = ContentSearchManager.GetIndex(indexName);
                    using (var context = newsIndex.CreateSearchContext())
                    {
                        var results = context.GetQueryable<NewsSearchResultItem>(new CultureExecutionContext(Context.Language.CultureInfo))
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
                Log.Error("An error occured in NewsRepository ResolveNewsItemByUrl", e.InnerException, typeof(NewsRepository));
                return null;
            }

            return null;
        }

        public NewsOverviewViewModel GetNewsItems(int page = 1, int numberOfItems = 10)
        {
            var model = new NewsOverviewViewModel();

            var list = new List<Models.News>();
            var item = Context.Site.Database.GetItem($"{Context.Site.ContentStartPath}/Data/News");
            var newsIndex = ContentSearchManager.GetIndex(indexName);
            try
            {
                using (var context = newsIndex.CreateSearchContext())
                {
                    var results = context.GetQueryable<NewsSearchResultItem>(new CultureExecutionContext(Context.Language.CultureInfo))
                        .Where(x => x.Paths.Contains(item.ID) && x.TemplateId == Templates.News.ID)
                        .OrderByDescending(x => x.NewsDate).Skip((page - 1) * numberOfItems).Take(numberOfItems);

                    model.NumberOfSearchResults = results.GetResults().TotalSearchResults;
                    var pagesCount = (double) model.NumberOfSearchResults / numberOfItems;
                    model.NumberOfPages = (int) Math.Ceiling(pagesCount);

                    if (results.Any())
                        foreach (var newsSearchResultItem in results)
                        {
                            var news = new Models.News {Item = newsSearchResultItem.GetItem()};
                            list.Add(news);
                        }
                }
            }
            catch (Exception exception)
            {
                Log.Error("An error occured in NewsRepository GetNewsItems", exception.InnerException, typeof(NewsRepository));
            }

            model.NewsItems = list;

            return model;
        }
    }
}