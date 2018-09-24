using Sitecore.Analytics;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using System;
using Sitecore.Data.Items;
using Sitecore.Pipelines.SessionEnd;
using Sitecore.Services.Core.Model;
using Sitecore.DataExchange.Local.Extensions;
using Sitecore.DataExchange.Local.Runners;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Repositories;

namespace Sitecore.HabitatHome.Feature.CRM.Pipelines
{
    public class SyncContactToSalesforceCrmOnSessionEnd
    {
        private string SalesforceDefPipelineBatchId => Settings.GetSetting("Feature.CRM.Salesforce.RunPipelineBatchIdOnSessionEnd");

        public void Process(SessionEndArgs args)
        {
            if (Tracker.Current?.Contact == null)
            {
                return;
            }

            try
            {
                Guid pipelineItemId = Guid.Parse(SalesforceDefPipelineBatchId);
                IItemModelRepository itemModelRepo = Sitecore.DataExchange.Context.ItemModelRepository;
                ItemModel pipelineBatchItemModel = itemModelRepo.Get(pipelineItemId);
                var converter = pipelineBatchItemModel.GetConverter<PipelineBatch>(Sitecore.DataExchange.Context.ItemModelRepository);
                var convertResult = converter.Convert(pipelineBatchItemModel);
                PipelineBatch pipelineBatch = convertResult.ConvertedValue;
                PipelineBatchContext pipelineBatchContext = new PipelineBatchContext();
                var runner = new InProcessPipelineBatchRunner();
                runner.RunAsync(pipelineBatch, pipelineBatchContext);

                Log.Info("Successfully synced xConnect contact to Salesforce on session end via DEF", this);
            }
            catch (Exception ex)
            {
                Log.Error($"Error syncing xConnect contact to Salesforce on session end via DEF. PipelineBatchId = {SalesforceDefPipelineBatchId}", ex, this);
            }

        }
    }
}