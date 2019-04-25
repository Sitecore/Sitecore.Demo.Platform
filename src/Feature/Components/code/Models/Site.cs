using Sitecore.Data.Items;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Models;

namespace Sitecore.HabitatHome.Feature.Components.Models
{
    public class Site : ItemBase
    {
        private Component home;
        private ItemBase lastSettingsGot;
        private Item settingsItem;

        public Site()
        {
            Item = Context.Database.GetItem(Context.Site.ContentStartPath);
        }

        public Component Home
        {
            get
            {
                if (home == null)
                {
                    home = new Component();
                    home.Item = Context.Database.GetItem(Context.Site.StartPath);
                }

                return home;
            }
        }

        public Item SettingsItem
        {
            get
            {
                var sitePath = Context.Site.ContentStartPath;
                if (settingsItem == null && Item != null)
                    settingsItem = Context.Database.GetItem($"{sitePath}/{Item["Settings"]}");
                return settingsItem;
            }
        }

        public T GetSettings<T>() where T : ItemBase, new()
        {
            if (lastSettingsGot == null ||
                lastSettingsGot.GetType() != typeof(T))
                lastSettingsGot = new T {Item = SettingsItem};
            return lastSettingsGot as T;
        }
    }
}