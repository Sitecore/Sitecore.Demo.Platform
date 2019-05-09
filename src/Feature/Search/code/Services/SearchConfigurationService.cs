using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.HabitatHome.Foundation.DependencyInjection;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.HabitatHome.Feature.Search.Services
{
    [Service(typeof(ISearchConfigurationService))]
    public class SearchConfigurationService : ISearchConfigurationService
    {
        public Item GetSearchConfigurationSettingsItem()
        {
            return Context.Site.GetSettingsItem()?.Children.FirstOrDefault(x => x.IsDerived(Templates.SearchConfigurationSettings.ID));
        }

        public Item GetSearchPage()
        {
            return GetSearchConfigurationSettingsItem() != null ? Context.Site.Database.GetItem(GetSearchConfigurationSettingsItem()[Templates.SearchConfigurationSettings.Fields.SearchPage]) : null;
        }

        public List<Item> GetSearchPageSupportedTemplates()
        {
            var items = new List<Item>();
            if (GetSearchConfigurationSettingsItem() != null)
            {
                var values = GetSearchConfigurationSettingsItem()[Templates.SearchConfigurationSettings.Fields.SearchPageSupportedTemplates].Split(System.Convert.ToChar("|"));
                items.AddRange(values.Select(guid => Context.Site.Database.GetItem(new ID(guid))).Where(item => item != null));
            }

            return items;
        }

        public int GetSearchPageDefaultNumberOfItems()
        {
            var defaultNumber = 10;
            var searchConfigurationSettingsItem = GetSearchConfigurationSettingsItem();
            if (searchConfigurationSettingsItem == null) return defaultNumber;
            var number = searchConfigurationSettingsItem[Templates.SearchConfigurationSettings.Fields.SearchPageDefaultNumberOfItems];
            if (!string.IsNullOrEmpty(number) && int.TryParse(number, out defaultNumber)) return defaultNumber;

            return defaultNumber;
        }
    }
}