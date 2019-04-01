namespace Sitecore.HabitatHome.Feature.Demo.Repositories
{
    using Sitecore.Analytics.Tracking;
    using Sitecore.HabitatHome.Feature.Demo.Models;

    public interface IPageViewRepository
    {
        PageView Get(ICurrentPageContext pageContext);
    }
}