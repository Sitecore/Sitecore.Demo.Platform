using Sitecore.Analytics;
using Sitecore.Analytics.XConnect.Facets;
using Sitecore.HabitatHome.Foundation.Accounts.Models.Facets;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;
using Sitecore.XConnect;

namespace Sitecore.HabitatHome.Foundation.Accounts.Rules
{
    public class SportNameFacetCondition<T> : OperatorCondition<T> where T : RuleContext
    {
        public string SportNameKey { get; set; }

        protected override bool Execute(T ruleContext)
        {
            if (!Tracker.Current.IsActive)
            {
                return false;
            }

            var facets = Tracker.Current.Contact.GetFacet<IXConnectFacets>("XConnectFacets");
            Facet facet = null;
            if (facets?.Facets?.TryGetValue(SportName.DefaultKey, out facet) ?? false)
            {
                var sportNameFacet = facet as SportName;
                return sportNameFacet?.Value == SportNameKey;
            }

            return false;
        }
    }
}