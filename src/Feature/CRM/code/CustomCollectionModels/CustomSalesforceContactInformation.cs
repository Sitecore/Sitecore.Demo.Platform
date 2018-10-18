using Sitecore.XConnect;
using System;

namespace Sitecore.HabitatHome.Feature.CRM.CustomCollectionModels
{
    [FacetKey("CustomSalesforceContact")]
    [Serializable]
    public class CustomSalesforceContactInformation : Facet
    {
        public const string DefaultFacetKey = "CustomSalesforceContact";
        public string WelcomeJourneyStatus { get; set; }
    }
}