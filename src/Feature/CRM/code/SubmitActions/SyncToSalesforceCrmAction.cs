using Sitecore.Analytics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.SubmitActions;
using Sitecore.Demo.Feature.Platform.CRM.SubmitActions.Models;
using Sitecore.Demo.Feature.Platform.CRM.TriggerDefPipelineBatch;
using System.Linq;

namespace Sitecore.Demo.Feature.Platform.CRM.SubmitActions
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