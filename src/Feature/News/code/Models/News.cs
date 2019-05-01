using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Models;

namespace Sitecore.HabitatHome.Feature.News.Models
{
    public class News : ItemBase
    {
        private string newsUrl;
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
                if (string.IsNullOrEmpty(newsUrl)) newsUrl = $"/news/{NewsSlug}";
                return newsUrl;
            }
        }
    }
}