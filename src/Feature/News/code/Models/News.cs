using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Models;
using Sitecore.Links;

namespace Sitecore.HabitatHome.Feature.News.Models
{
    public class News : ItemBase
    {
        private Item newsSettingsItem;
        private string newsUrl;
        private string newsWildcardItemUrl;

        public string NewsTitle { get; set; }

        public string NewsSummary { get; set; }

        public string NewsContent { get; set; }

        public string NewsSlug { get; set; }

        public MediaItem NewsImage { get; set; }

        public DateField NewsDate { get; set; }

        public string NewsUrl
        {
            get
            {
                if (string.IsNullOrEmpty(newsUrl)) newsUrl = $"{ParentOfNewsWildcardItemUrl}/{NewsSlug}";
                return newsUrl;
            }
        }

        public string ParentOfNewsWildcardItemUrl
        {
            get
            {
                if (string.IsNullOrEmpty(newsWildcardItemUrl))
                {
                    var wildcardItem =
                        Context.Database.GetItem(
                            new ID(NewsSettingsItem[Templates.NewsSettings.Fields.NewsWildcardItem]));
                    newsWildcardItemUrl = wildcardItem != null ? LinkManager.GetItemUrl(wildcardItem.Parent) : "/news";
                }

                return newsWildcardItemUrl;
            }
        }

        public Item NewsSettingsItem
        {
            get
            {
                var sitePath = Context.Site.ContentStartPath;
                if (newsSettingsItem == null && Item != null)
                    newsSettingsItem = Context.Database.GetItem($"{sitePath}/Settings/News Settings");
                return newsSettingsItem;
            }
        }
    }
}