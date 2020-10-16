using System.Collections.Generic;

namespace Sitecore.Demo.Platform.Feature.Demo.Models
{
    public class Referral
  {
    public string ReferringSite { get; set; }
    public int TotalNoOfCampaigns { get; set; }
    public IEnumerable<Campaign> Campaigns { get; set; }
        public string Keywords { get; set; }
  }
}