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
    }
}