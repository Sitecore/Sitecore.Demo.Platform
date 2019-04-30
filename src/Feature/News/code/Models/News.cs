using System;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Models;

namespace Sitecore.HabitatHome.Feature.News.Models
{
    public class News : ItemBase
    {
        public string NewsTitle { get; set; }

        public string NewsSummary { get; set; }

        public string NewsContent { get; set; }

        public string NewsSlug { get; set; }

        public MediaItem NewsImage { get; set; }

        public DateField NewsDate { get; set; }
    }
}