using Sitecore.Pipelines.SessionEnd;

namespace Sitecore.HabitatHome.Feature.CrmSalesforce.Pipelines
{
    public class SyncToSalesforceCrmOnSessionEnd
    {
        public void Process(SessionEndArgs args)
        {
            var syncToSalesforceCrm = new TriggerDefPipelineBatch.SyncToSalesforceCrm();
            syncToSalesforceCrm.Sync();
        }
    }
}