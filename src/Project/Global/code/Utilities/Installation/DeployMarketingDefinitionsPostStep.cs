using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Install.Framework;
using Sitecore.Marketing.Definitions;
using Sitecore.Marketing.Definitions.MarketingAssets;
using Sitecore.Marketing.xMgmt.Pipelines.DeployDefinition;

namespace  Sitecore.Demo.Website.Utilities.Installation
{
    public class DeployMarketingDefinitionsPostStep : IPostStep
    {
        public void Run(ITaskOutput output, NameValueCollection metaData)
        {
            IServiceProvider provider = ServiceLocator.ServiceProvider.GetService<IServiceProvider>();
            DefinitionManagerFactory factory = provider.GetDefinitionManagerFactory();
            DeploymentManager manager = new DeploymentManager(factory);

            CultureInfo culture = CultureInfo.CurrentCulture;
            Task deploymentTask = manager.DeployAllAsync<IMarketingAssetDefinition>(culture);
            deploymentTask.Wait();

            Diagnostics.Log.Info("Deploying Marketing Definitions", this);
        }
    }
}