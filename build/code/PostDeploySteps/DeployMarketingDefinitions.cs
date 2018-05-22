using HedgehogDevelopment.SitecoreProject.PackageInstallPostProcessor.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Marketing.Definitions;
using Sitecore.Marketing.Definitions.MarketingAssets;
using Sitecore.Marketing.xMgmt.Pipelines.DeployDefinition;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace Sitecore.Demo.Deployment.Web.PostDeploySteps
{
    public class DeployMarketingDefinitions : IPostDeployAction
    {
        public void RunPostDeployAction(XDocument deployedItems, IPostDeployActionHost host, string parameter)
        {
            IServiceProvider provider = ServiceLocator.ServiceProvider.GetService<IServiceProvider>();
            DefinitionManagerFactory factory = provider.GetDefinitionManagerFactory();
            DeploymentManager manager = new DeploymentManager(factory);

            CultureInfo culture = CultureInfo.CurrentCulture;
            manager.DeployAllAsync<IMarketingAssetDefinition>(culture);

            host.LogMessage("TDS Post Deploy: Deploying Marketing Definitions");
        }
    }
}