using Sitecore.Demo.Platform.Feature.CRM.CustomCollectionModels;
using Sitecore.XConnect.Client.Configuration;
using Sitecore.XConnect;
using Sitecore.Diagnostics;
using Sitecore.Analytics;
using Sitecore.Analytics.Model;
using Sitecore.XConnect.Client;
using System.Linq;
using Sitecore.Demo.Platform.Foundation.Accounts.Pipelines;
using Sitecore.Demo.Platform.Foundation.Accounts.Providers;
using Sitecore.Demo.Platform.Foundation.Accounts.Services;

namespace Sitecore.Demo.Platform.Feature.CRM.Pipelines
{
    public class SetMemberTierFacetOnRegister
    {
        private readonly string[] facetsToUpdate = {
            CustomSalesforceContactInformation.DefaultFacetKey
        };

        private readonly IContactFacetsProvider contactFacetsProvider;
        private readonly IExportFileService exportFileService;

        public void Process(AccountsPipelineArgs args)
        {
            var contactFacetService = new ContactFacetService(contactFacetsProvider, exportFileService);
            var contactReference = contactFacetService.GetContactId();

            if (contactReference == null)
            {
                return;
            }

            using (var client = SitecoreXConnectClientConfiguration.GetClient())
            {
                try
                {
                    var contact = client.Get(contactReference, new ContactExpandOptions(this.facetsToUpdate));
                    if (contact == null)
                    {
                        return;
                    }

                    if (contact.Facets.ContainsKey(CustomSalesforceContactInformation.DefaultFacetKey))
                    {
                        CustomSalesforceContactInformation facet = (CustomSalesforceContactInformation)contact.Facets[CustomSalesforceContactInformation.DefaultFacetKey];
                        if (string.IsNullOrEmpty(facet.MemberTier))
                        {
                            facet.MemberTier = "Bronze";
                            client.SetFacet(contact, CustomSalesforceContactInformation.DefaultFacetKey, facet);
                        }
                    }
                    else
                    {
                        client.SetFacet(contact, CustomSalesforceContactInformation.DefaultFacetKey, new CustomSalesforceContactInformation { MemberTier = "Bronze" });
                    }

                    client.Submit();
                }
                catch (XdbExecutionException ex)
                {
                    Log.Error($"Could not update the xConnect contact facets", ex, this);
                }
            }
        }
    }
}
