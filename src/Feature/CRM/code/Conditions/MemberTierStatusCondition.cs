using Sitecore.Analytics;
using Sitecore.Diagnostics;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;
using System;
using System.Linq;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.Configuration;
using Sitecore.Demo.Platform.Feature.CRM.CustomCollectionModels;

namespace Sitecore.Demo.Platform.Feature.CRM.Conditions
{
    public class MemberTierStatusCondition<T> : StringOperatorCondition<T> where T : RuleContext
    {
        public string MemberTierStatus { get; set; }

        protected override bool Execute(T ruleContext)
        {
            try
            {
                var id = GetContactId();
                if (id == null)
                {
                    return false; ;
                }
                var contactReference = new IdentifiedContactReference(id.Source, id.Identifier);

                using (var client = SitecoreXConnectClientConfiguration.GetClient())
                {
                    var existingContact = client.Get<XConnect.Contact>(contactReference, new ContactExpandOptions(new string[] { CustomSalesforceContactInformation.DefaultFacetKey }));
                    CustomSalesforceContactInformation customSalesforceFacet = existingContact.GetFacet<CustomSalesforceContactInformation>(CustomSalesforceContactInformation.DefaultFacetKey);
                    if (customSalesforceFacet == null)
                        return false;
                    var memberTierStatusCondition = customSalesforceFacet.MemberTier;
                    return Compare(memberTierStatusCondition, MemberTierStatus);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error evaluating MemberTierStatusCondition. ID: {ruleContext.Item.ID}", ex, this);
                return false;
            }
        }

        private Analytics.Model.Entities.ContactIdentifier GetContactId()
        {
            if (Tracker.Current?.Contact == null)
            {
                return null;
            }
            return Tracker.Current.Contact.Identifiers.FirstOrDefault();
        }
    }
}
