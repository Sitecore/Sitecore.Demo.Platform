using Microsoft.Extensions.DependencyInjection;
using Sitecore.Configuration;
using Sitecore.DependencyInjection;
using Sitecore.Marketing.Definitions;
using Sitecore.Marketing.Definitions.MarketingAssets;
using Sitecore.Marketing.xMgmt.Pipelines.DeployDefinition;
using System;
using System.Globalization;

namespace Sitecore.HabitatHome.Global.Website.Utilities
{
    public partial class DeployMarketingDefinitions : System.Web.UI.Page
    {
        private string GetApiKey()
        {
            return Settings.GetSetting("MarketingDefinitions.ApiKey");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string apiKey = Request.QueryString["apiKey"];
            if (!string.Equals(apiKey, GetApiKey()))
            {
                Response.Write("Invalid API Key");
                Sitecore.Diagnostics.Log.Warn("DeployMarketingDefinitions utility: Invalid API key", this);
                Response.End();
            }

            try
            {
                IServiceProvider provider = ServiceLocator.ServiceProvider.GetService<IServiceProvider>();
                DefinitionManagerFactory factory = provider.GetDefinitionManagerFactory();
                DeploymentManager manager = new DeploymentManager(factory);

                CultureInfo culture = CultureInfo.CurrentCulture;
                manager.DeployAllAsync<IMarketingAssetDefinition>(culture);

                Sitecore.Diagnostics.Log.Info("Deploying Marketing Definitions", this);
            }
            catch(Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(string.Format("Deploying Marketing Definitions failed: {0}", ex.Message), this);
            }
        }
    }
}