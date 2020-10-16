using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Abstractions;
using Sitecore.Configuration;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using Sitecore.Marketing.Definitions.AutomationPlans.Model;
using Sitecore.Marketing.Definitions.Campaigns;
using Sitecore.Marketing.Definitions.Events;
using Sitecore.Marketing.Definitions.Funnels;
using Sitecore.Marketing.Definitions.Goals;
using Sitecore.Marketing.Definitions.MarketingAssets;
using Sitecore.Marketing.Definitions.Outcomes.Model;
using Sitecore.Marketing.Definitions.PageEvents;
using Sitecore.Marketing.Definitions.Profiles;
using Sitecore.Marketing.Definitions.Segments;
using Sitecore.Marketing.xMgmt.Pipelines.DeployDefinition;
using Sitecore.Marketing.xMgmt.ReferenceData.Observers.Activation.Taxonomy.Deployment;
using Sitecore.XConnect.Client;

namespace Sitecore.Demo.Website.Utilities
{
    public partial class DeployMarketingDefinitions : System.Web.UI.Page
    {
        private DeploymentManager _definitionDeploymentManager;
        private IDeployManager _taxonomyDeploymentManager;
        private BaseJobManager _jobManager;

        private string GetApiKey()
        {
            return Settings.GetSetting("MarketingDefinitions.ApiKey");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this._definitionDeploymentManager = ServiceLocator.ServiceProvider.GetService<DeploymentManager>();
            this._taxonomyDeploymentManager = ServiceLocator.ServiceProvider.GetService<IDeployManager>();
            this._jobManager = ServiceLocator.ServiceProvider.GetService<BaseJobManager>();

            string apiKey = Request.QueryString["apiKey"];
            if (!string.Equals(apiKey, GetApiKey()))
            {
                Response.Write("Invalid API Key");
                Log.Warn("DeployMarketingDefinitions utility: Invalid API key", this);
                Response.End();
            }

            DefaultJobOptions defaultJobOptions = new DefaultJobOptions(
                string.Format("Deploy all definitions. Deployment job id: {0}.", (object) Guid.NewGuid()),
                "Sitecore.Marketing.Client", "website", (object) this, "DeployAllDefinitions");
            Log.Debug("Starting a job to deploy all definitions.");
            _jobManager.Start((BaseJobOptions) defaultJobOptions);
        }

        protected virtual async Task DeployDefinitionTypes()
        {
            var core = Factory.GetDatabase("core");
            core.PropertyStore.SetBoolValue("DisablePublishItemObserver", true);
            Log.Warn("DeployMarketingDefinitions: Disabled PublishItemObserver", this);
            CultureInfo culture = CultureInfo.InvariantCulture;

            await _definitionDeploymentManager.DeployAllAsync<IAutomationPlanDefinition>(culture);
            await _definitionDeploymentManager.DeployAllAsync<ICampaignActivityDefinition>(culture);
            await _definitionDeploymentManager.DeployAllAsync<IEventDefinition>(culture);
            await _definitionDeploymentManager.DeployAllAsync<IFunnelDefinition>(culture);
            await _definitionDeploymentManager.DeployAllAsync<IGoalDefinition>(culture);
            await _definitionDeploymentManager.DeployAllAsync<IMarketingAssetDefinition>(culture);
            await _definitionDeploymentManager.DeployAllAsync<IOutcomeDefinition>(culture);
            await _definitionDeploymentManager.DeployAllAsync<IPageEventDefinition>(culture);
            await _definitionDeploymentManager.DeployAllAsync<IProfileDefinition>(culture);
            await _definitionDeploymentManager.DeployAllAsync<ISegmentDefinition>(culture);

            Sitecore.Context.Database.PropertyStore.SetBoolValue("DisablePublishItemObserver", false);
            Log.Warn("DeployMarketingDefinitions: Enabled PublishItemObserver", this);
        }

        // This method is used, just refered indirectly from new DefaultJobOptions() in Page_Load
        public void DeployAllDefinitions()
        {
            ((Func<Task>) (async () =>
            {
                await this.DeployDefinitionTypes().ConfigureAwait(false);
                await this._taxonomyDeploymentManager
                    .DeployAsync(Sitecore.Marketing.Taxonomy.WellKnownIdentifiers.Items.Taxonomies.TaxonomyRootId)
                    .ConfigureAwait(false);
            })).SuspendContextLock();
        }
    }
}