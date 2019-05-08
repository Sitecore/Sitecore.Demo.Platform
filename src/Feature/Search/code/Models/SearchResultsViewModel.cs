using System.Collections.Generic;
using Sitecore.Data.Items;

namespace Sitecore.HabitatHome.Feature.Search.Models
{
    public class SearchResultsViewModel
    {
        public string SearchTerm { get; set; }

        public List<Item> SearchResultsItems { get; set; }

        public int NumberOfSearchResults { get; set; }

        public int NumberOfPages { get; set; }
    }
}