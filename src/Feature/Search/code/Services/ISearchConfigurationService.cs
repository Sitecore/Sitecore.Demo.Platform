using Sitecore.Data.Items;

namespace Sitecore.HabitatHome.Feature.Search.Services
{
    public interface ISearchConfigurationService
    {
        Item GetSearchConfigurationSettingsItem();
    }
}