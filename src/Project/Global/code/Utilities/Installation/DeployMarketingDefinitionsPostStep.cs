using System.Collections.Specialized;
using Sitecore.Install.Framework;

namespace  Sitecore.HabitatHome.Global.Website.Utilities.Installation
{
    public class DeployMarketingDefinitionsPostStep : IPostStep
    {
        public void Run(ITaskOutput output, NameValueCollection metaData)
        {
            //IServiceProvider provider = ServiceLocator.ServiceProvider.GetService<IServiceProvider>();
            //DefinitionManagerFactory factory = provider.GetDefinitionManagerFactory();
            //DeploymentManager manager = new DeploymentManager(factory);

            //CultureInfo culture = CultureInfo.CurrentCulture;
            //Task deploymentTask = manager.DeployAllAsync<IMarketingAssetDefinition>(culture);
            //deploymentTask.Wait();

            //Diagnostics.Log.Info("Deploying Marketing Definitions", this);
        }
    }
}