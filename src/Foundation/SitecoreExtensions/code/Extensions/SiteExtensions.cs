using System.Linq;

namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
    using System;
    using Sitecore;
    using Sitecore.Data;
    using Sitecore.Data.Items;

    using Sitecore.Sites;

    public static class SiteExtensions
    {
        public static Item GetContextItem(this SiteContext site, ID derivedFromTemplateID)
        {
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            var startItem = site.GetStartItem();
            return startItem?.GetAncestorOrSelfOfTemplate(derivedFromTemplateID);
        }

        public static Item GetRootItem(this SiteContext site)
        {
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            return site.Database.GetItem(Context.Site.RootPath);
        }

        public static Item GetStartItem(this SiteContext site)
        {
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            return site.Database.GetItem(Context.Site.StartPath);
        }

        public static Item GetSettingsItem(this SiteContext site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            return site.GetRootItem().Children
                .FirstOrDefault(x => x.TemplateID == new ID("{BE2A1204-F7BA-4BE3-933D-190C97496700}"));
        }
    }
}