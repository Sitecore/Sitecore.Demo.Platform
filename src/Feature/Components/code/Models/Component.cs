using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Models;
using Sitecore.Links;

namespace Sitecore.HabitatHome.Feature.Components.Models
{
    public class Component : ItemBase
    {
        private Site site;

        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Lead { get; set; }
        public string Content { get; set; }
        public Component TargetPage { get; set; }
        public LinkField TargetUrl { get; set; }
        public MediaItem Image { get; set; }
        public ImageField ImageField { get; set; }

        public Site Site => site ?? (site = new Site());

        //todo: use Foundation.SitecoreExtensions to resolve link field
        public string Url
        {
            get
            {
                var url = TargetPage?.Url;

                if (string.IsNullOrEmpty(url)) url = TargetUrl != null ? TargetUrl.GetFriendlyUrl() : string.Empty;
                if (string.IsNullOrEmpty(url)) url = LinkManager.GetItemUrl(Item);

                return url;
            }
        }
    }
}