using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.Diagnostics;
using Sitecore.HabitatHome.Feature.Search.Models;
using Sitecore.HabitatHome.Feature.Search.SearchTypes;
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

            if (string.IsNullOrEmpty(searchTerm)) return model;
            var searchResultItems = new List<SearchResult>();

            var item = Context.Site.Database.GetItem(Context.Site.ContentStartPath);
            var index = ContentSearchManager.GetIndex(indexName);
            try
            {
                using (var context = index.CreateSearchContext())
                {
                    var pathQuery = PredicateBuilder.True<CustomSearchResultItem>();
                    pathQuery = pathQuery.And(x => x.Paths.Contains(item.ID));

                    var excludeFromSearchResultsQuery = PredicateBuilder.True<CustomSearchResultItem>();
                    excludeFromSearchResultsQuery = excludeFromSearchResultsQuery.And(x => !x.ExcludeFromSearchResults);

                    var searchTermQuery = PredicateBuilder.True<CustomSearchResultItem>();
                    searchTermQuery = searchTermQuery.Or(p => p.Title.Contains(searchTerm));
                    searchTermQuery = searchTermQuery.Or(p => p.Lead.Contains(searchTerm));
                    searchTermQuery = searchTermQuery.Or(p => p.Content.Contains(searchTerm));
                    searchTermQuery = searchTermQuery.Or(p => p.NewsContent.Contains(searchTerm));

                    var templateQuery = PredicateBuilder.False<CustomSearchResultItem>();
                    foreach (var supportedTemplate in _searchConfigurationService.GetSearchPageSupportedTemplates()) templateQuery = templateQuery.Or(p => p.TemplateId == supportedTemplate.ID);

                    var predicate = PredicateBuilder.True<CustomSearchResultItem>();
                    predicate = predicate.And(pathQuery);
                    predicate = predicate.And(excludeFromSearchResultsQuery);
                    predicate = predicate.And(templateQuery);
                    predicate = predicate.And(searchTermQuery);

                    var results = context.GetQueryable<CustomSearchResultItem>(new CultureExecutionContext(Context.Language.CultureInfo)).Where(predicate).Skip((page - 1) * numberOfItems).Take(numberOfItems);

                    model.NumberOfSearchResults = results.GetResults().TotalSearchResults;
                    var pagesCount = (double) model.NumberOfSearchResults / numberOfItems;
                    model.NumberOfPages = (int) Math.Ceiling(pagesCount);

                    if (results.Any())
                        foreach (var searchResultItem in results)
                        {
                            var resultItem = searchResultItem.GetItem();
                            var searchresult = new SearchResult {Item = resultItem};
                            searchResultItems.Add(searchresult);
                        }
                }
            }
            catch (Exception exception)
            {
                Log.Error("An error occured in SearchService GetSearchResults", exception.InnerException, typeof(SearchService));
            }

            model.SearchResultItems = searchResultItems;

            return model;
        }
    }
}