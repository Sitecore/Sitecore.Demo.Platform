using Sitecore.Analytics.Tracking;
using Sitecore.Demo.Platform.Feature.Demo.Models;

namespace Sitecore.Demo.Platform.Feature.Demo.Repositories
{
    public interface IPageViewRepository
    {
        PageView Get(ICurrentPageContext pageContext);
    }
}