namespace Sitecore.Feature.Demo.Repositories
{
    using Sitecore.Feature.Demo.Models;
    using System.Collections.Generic;

    public interface ICampaignRepository
    {
        Campaign GetCurrent();
        IEnumerable<Campaign> GetHistoric();
    }
}