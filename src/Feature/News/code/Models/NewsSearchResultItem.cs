using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;

namespace Sitecore.HabitatHome.Feature.News.Models
{
    public class NewsSearchResultItem : SearchResultItem
    {
        [IndexField("newsslug")]
        public string NewsSlug { get; set; }

        [IndexField("newsdate")]
        public string NewsDate { get; set; }
    }
}