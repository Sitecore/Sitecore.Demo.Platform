using Sitecore.Analytics;
using Sitecore.Analytics.XConnect.Facets;
using Sitecore.HabitatHome.Foundation.Accounts.Models.Facets;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;
using Sitecore.XConnect;

namespace Sitecore.HabitatHome.Foundation.Accounts.Rules
{
    public class SportTypeFacetCondition<T> : OperatorCondition<T> where T : RuleContext
    {
        public string SportTypeKey { get; set; }

        protected override bool Execute(T ruleContext)
        {
            if (!Tracker.Current.IsActive)
            {
                return false;
            }

            var facets = Tracker.Current.Contact.GetFacet<IXConnectFacets>("XConnectFacets");
            Facet facet = null;
            if (facets?.Facets?.TryGetValue(SportType.DefaultKey, out facet) ?? false)
            {
                var sportTypeFacet = facet as SportType;
                return sportTypeFacet?.Value == SportTypeKey;
            }

            return false;
        }
    }
}