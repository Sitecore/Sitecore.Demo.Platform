using System.Collections.Generic;

namespace Sitecore.HabitatHome.Feature.Search.Models
{
    public class SearchResultsViewModel
    {
        public string SearchTerm { get; set; }

        //public List<News> SearchResultsItems { get; set; }

        public int NumberOfSearchResults { get; set; }

        public int NumberOfPages { get; set; }
    }
}