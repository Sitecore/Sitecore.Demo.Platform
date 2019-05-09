using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Models;

namespace Sitecore.HabitatHome.Feature.Search.Models
{
    public class SearchResult : ItemBase
    {
        public string Title { get; set; }

        public string Lead { get; set; }
    }
}