namespace Sitecore.Feature.Demo.Models
{
    using Sitecore.XA.Foundation.Mvc.Models;
    using System.Collections.Generic;

    public class Visits : RenderingModelBase
    {
        public int EngagementValue { get; set; }
        public IEnumerable<PageView> PageViews { get; set; }
        public int TotalPageViews { get; set; }
        public int TotalVisits { get; set; }
        public IEnumerable<EngagementPlanState> EngagementPlanStates { get; set; }
    }
}