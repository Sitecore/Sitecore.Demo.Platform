using Sitecore.Analytics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.SubmitActions;
using Sitecore.HabitatHome.Feature.CRM.SubmitActions.Models;
using Sitecore.HabitatHome.Feature.CRM.TriggerDefPipelineBatch;
using System.Linq;

namespace Sitecore.HabitatHome.Feature.CRM.SubmitActions
{
    public class SyncToSalesforceCrmAction : AnalyticsActionBase<SyncToSalesforceCrmActionData>
    {
        SyncToSalesforceCrm syncToSalesforceCrm = new SyncToSalesforceCrm();

        public SyncToSalesforceCrmAction(ISubmitActionData submitActionData) : base(submitActionData)
        {
        }

        protected override bool Execute(SyncToSalesforceCrmActionData data, FormSubmitContext formSubmitContext)
        {

            var idenvals = Tracker.Current.Session.Contact.Identifiers;
            var test = idenvals.FirstOrDefault(x => x.Identifier.ToString() == "extranet");

            return syncToSalesforceCrm.Sync();
        }
    }
}