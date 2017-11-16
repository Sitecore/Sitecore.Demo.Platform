namespace Sitecore.Feature.Demo.Repositories
{
    using Sitecore.Analytics;
    using Sitecore.Feature.Demo.Models;
    using Sitecore.Foundation.DependencyInjection;
    using Sitecore.XA.Foundation.Mvc.Repositories.Base;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    [Service(typeof(IReferralRepository))]
    public class ReferralRepository : ModelRepository, IReferralRepository
    {
        private readonly ICampaignRepository campaignRepository;

        public ReferralRepository(ICampaignRepository campaignRepository)
        {
            this.campaignRepository = campaignRepository;
        }

        public override IRenderingModelBase GetModel()
        {
            Referral model = new Referral();
            FillBaseProperties(model);

            var campaigns = this.CreateCampaigns().ToArray();
            model.ReferringSite = GetReferringSite();
            model.TotalNoOfCampaigns = campaigns.Length;
            model.Campaigns = campaigns;
            model.Keywords = GetKeywords();

            return model;
        }

        private static string GetKeywords()
        {
            string keywords = null;
            if (Tracker.Current != null)
            {
                keywords = Tracker.Current.Interaction.Keywords;
            }
            return keywords;
        }

        private string GetReferringSite()
        {
            if (Tracker.Current == null || HttpContext.Current == null)
            {
                return null;
            }
            var referringSite = Tracker.Current.Interaction.ReferringSite;
            return referringSite != null && referringSite.Equals(HttpContext.Current.Request.Url.Host, StringComparison.InvariantCultureIgnoreCase) ? null : referringSite;
        }

        private IEnumerable<Campaign> CreateCampaigns()
        {
            var activeCampaign = this.GetActiveCampaign();
            if (activeCampaign != null)
            {
                yield return activeCampaign;
            }

            foreach (var campaign in this.GetHistoricCampaigns())
            {
                yield return campaign;
            }
        }

        private IEnumerable<Campaign> GetHistoricCampaigns()
        {
            return this.campaignRepository.GetHistoric();
        }

        private Campaign GetActiveCampaign()
        {
            return this.campaignRepository.GetCurrent();
        }
    }
}