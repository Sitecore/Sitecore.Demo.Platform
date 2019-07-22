using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Models;

namespace Sitecore.HabitatHome.Feature.Components.Models
{
    public class Metadata : ItemBase
    {
        public string MetadataTitle { get; set; }

        public string MetadataDescription { get; set; }

        public string MetadataKeywords { get; set; }
    }
}