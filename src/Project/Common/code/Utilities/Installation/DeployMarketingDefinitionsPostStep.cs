using System;
using System.Collections.Specialized;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Install.Framework;
using Sitecore.Marketing.Definitions;
using Sitecore.Marketing.Definitions.MarketingAssets;
using Sitecore.Marketing.xMgmt.Pipelines.DeployDefinition;

namespace Sitecore.HabitatHome.Common.Website.Utilities.Installation
{
    public class DeployMarketingDefinitionsPostStep : IPostStep
    {
        public void Run(ITaskOutput output, NameValueCollection metaData)
        {
            IServiceProvider provider = ServiceLocator.ServiceProvider.GetService<IServiceProvider>();
            DefinitionManagerFactory factory = provider.GetDefinitionManagerFactory();
            DeploymentManager manager = new DeploymentManager(factory);

            CultureInfo culture = CultureInfo.CurrentCulture;
            manager.DeployAllAsync<IMarketingAssetDefinition>(culture);

            Diagnostics.Log.Info("Deploying Marketing Definitions", this);
        }
    }
}