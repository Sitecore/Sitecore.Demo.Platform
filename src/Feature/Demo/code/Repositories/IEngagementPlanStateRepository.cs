namespace Sitecore.HabitatHome.Feature.Demo.Repositories
{
    using System.Collections.Generic;
    using Sitecore.HabitatHome.Feature.Demo.Models;

    public interface IEngagementPlanStateRepository
    {
        IEnumerable<EngagementPlanState> GetCurrent();
    }
}