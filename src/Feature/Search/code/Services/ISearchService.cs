using Sitecore.HabitatHome.Feature.Search.Models;

namespace Sitecore.HabitatHome.Feature.Search.Services
{
    public interface ISearchService
    {
        SearchResultsViewModel GetSearchResults(string searchTerm, int page = 1, int numberOfItems = 10);
    }
}