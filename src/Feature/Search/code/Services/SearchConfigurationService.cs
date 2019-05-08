using System.Linq;
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