using Sitecore.Data.Items;
using Sitecore.HabitatHome.Feature.News.Models;

namespace Sitecore.HabitatHome.Feature.News.Repositories
{
    public interface INewsRepository
    {
        Item ResolveNewsItemByUrl(string urlPath);

        NewsOverviewViewModel GetNewsItems(int page = 1, int numberOfItems = 10);
    }
}