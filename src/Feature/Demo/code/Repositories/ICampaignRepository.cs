using System.Collections.Generic;
using Sitecore.Demo.Platform.Feature.Demo.Models;

namespace Sitecore.Demo.Platform.Feature.Demo.Repositories
{
    public interface ICampaignRepository
  {
    Campaign GetCurrent();
    IEnumerable<Campaign> GetHistoric();
  }
}