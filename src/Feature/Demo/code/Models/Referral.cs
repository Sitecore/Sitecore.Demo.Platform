namespace Sitecore.Feature.Demo.Models
{
    using Sitecore.XA.Foundation.Mvc.Models;
    using System.Collections.Generic;

    public class Referral : RenderingModelBase
    {
        public string ReferringSite { get; set; }
        public int TotalNoOfCampaigns { get; set; }
        public IEnumerable<Campaign> Campaigns { get; set; }
        public string Keywords { get; set; }
    }
}