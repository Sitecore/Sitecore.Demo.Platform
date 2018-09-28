using Sitecore.DataExchange.Tools.SalesforceConnect.Facets;
using Sitecore.XConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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