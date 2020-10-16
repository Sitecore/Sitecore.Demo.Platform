using System.Collections.Generic;
using Sitecore.Demo.Platform.Feature.Demo.Models;

namespace Sitecore.Demo.Platform.Feature.Demo.Repositories
{
    public interface IEngagementPlanStateRepository
    {
        IEnumerable<EngagementPlanState> GetCurrent();
    }
}