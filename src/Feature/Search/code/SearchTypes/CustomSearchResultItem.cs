using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;

namespace Sitecore.HabitatHome.Feature.Search.SearchTypes
{
    public class CustomSearchResultItem : SearchResultItem
    {
        [IndexField("title")]
        public string Title { get; set; }

        [IndexField("lead")]
        public string Lead { get; set; }

        [IndexField("content")]
        public string Content { get; set; }

        [IndexField("newscontent")]
        public string NewsContent { get; set; }

        [IndexField("excludefromsearchresults")]
        public bool ExcludeFromSearchResults { get; set; }
    }
}