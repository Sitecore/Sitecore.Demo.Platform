using Sitecore.Data.Items;

namespace Sitecore.HabitatHome.Feature.Components.Models
{
    public class Settings : ItemBase
    {
        public MediaItem Brand { get; set; }
        public string Title { get; set; }
    }
}