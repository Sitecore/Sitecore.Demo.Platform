using System.Collections.Generic;
using Sitecore.Data.Items;

namespace Sitecore.HabitatHome.Feature.Search.Services
{
    public interface ISearchConfigurationService
    {
        int GetSearchPageDefaultNumberOfItems();

        Item GetSearchConfigurationSettingsItem();

        Item GetSearchPage();

        List<Item> GetSearchPageSupportedTemplates();
    }
}