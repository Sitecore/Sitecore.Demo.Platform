using System.Linq;
using Sitecore.Data.Items;
using Sitecore.HabitatHome.Foundation.DependencyInjection;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.HabitatHome.Feature.News.Services
{
    [Service(typeof(INewsSettingsService))]
    public class NewsSettingsService : INewsSettingsService
    {
        public Item GetNewsSettingsItem()
        {
            return Context.Site.GetSettingsItem()?.Children.FirstOrDefault(x => x.IsDerived(Templates.NewsSettings.ID));
        }
    }
}