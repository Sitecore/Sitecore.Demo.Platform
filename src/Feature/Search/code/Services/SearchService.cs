using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.HabitatHome.Feature.Search.Models;
using Sitecore.HabitatHome.Foundation.DependencyInjection;

namespace Sitecore.HabitatHome.Feature.Search.Services
{
    [Service(typeof(ISearchService))]
    public class SearchService : ISearchService
    {
        private readonly ISearchConfigurationService _searchConfigurationService;
        private readonly string indexName = "sitecore_web_index";

        public SearchService(ISearchConfigurationService searchConfigurationService)
        {
            _searchConfigurationService = searchConfigurationService;
        }

        public SearchResultsViewModel GetSearchResults(string searchTerm, int page = 1, int numberOfItems = 10)
        {
            var model = new SearchResultsViewModel {SearchTerm = searchTerm};
            var list = new List<Item>();

            var item = Context.Site.Database.GetItem(Context.Site.ContentStartPath);
            var index = ContentSearchManager.GetIndex(indexName);
            try
            {
                using (var context = index.CreateSearchContext())
                {
                    var pathQuery = PredicateBuilder.True<SearchResultItem>();
                    pathQuery = pathQuery.And(x => x.Paths.Contains(item.ID));

                    var templateQuery = PredicateBuilder.False<SearchResultItem>();
                    foreach (var supportedTemplate in _searchConfigurationService.GetSearchPageSupportedTemplates()) templateQuery = templateQuery.Or(p => p.TemplateId == supportedTemplate.ID);

                    var predicate = PredicateBuilder.True<SearchResultItem>();
                    predicate = predicate.And(pathQuery);
                    predicate = predicate.And(templateQuery);

                    var results = context.GetQueryable<SearchResultItem>(new CultureExecutionContext(Context.Language.CultureInfo)).Where(predicate).Skip(page - 1).Take(numberOfItems);

                    model.NumberOfSearchResults = results.GetResults().TotalSearchResults;
                    model.NumberOfPages = model.NumberOfSearchResults / numberOfItems;

                    if (results.Any())
                        foreach (var searchResultItem in results)
                        {
                            var resultItem = searchResultItem.GetItem();
                            list.Add(resultItem);
                        }
                }
            }
            catch (Exception exception)
            {
                Log.Error("An error occured in SearchService GetSearchResults", exception.InnerException, typeof(SearchService));
            }

            model.SearchResultsItems = list;

            return model;
        }
    }
}