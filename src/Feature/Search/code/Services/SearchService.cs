using Sitecore.HabitatHome.Feature.Search.Models;
using Sitecore.HabitatHome.Foundation.DependencyInjection;

namespace Sitecore.HabitatHome.Feature.Search.Services
{
    [Service(typeof(ISearchService))]
    public class SearchService : ISearchService
    {
        public SearchResultsViewModel GetSearchResults(string searchTerm, int page = 1, int numberOfItems = 10)
        {
            return null;
        }
    }
}