namespace Sitecore.Feature.Demo.Models
{
    using Sitecore.XA.Foundation.Mvc.Models;
    using Sitecore.XConnect;
    using System.Collections.Generic;

    public class OnsiteBehavior : RenderingModelBase
    {
        public IEnumerable<Profile> ActiveProfiles { get; set; }
        public IEnumerable<Profile> HistoricProfiles { get; set; }
        public IEnumerable<PageEvent> Goals { get; set; }
        public IEnumerable<Outcome> Outcomes { get; set; }
        public IEnumerable<PageEvent> PageEvents { get; set; }
    }
}