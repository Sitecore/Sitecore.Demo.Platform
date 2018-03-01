using HedgehogDevelopment.SitecoreProject.PackageInstallPostProcessor.Contracts; 
using System.Collections.Specialized;
using System.Net.Http;
using System.Web;
using System.Xml.Linq;

namespace Sitecore.Demo.Deployment.Web.PostDeploySteps
{
    public class DeployEmailCampaigns : IPostDeployAction
    {
        public void RunPostDeployAction(XDocument deployedItems, IPostDeployActionHost host, string parameter)
        {
            NameValueCollection parameters = HttpUtility.ParseQueryString(parameter ?? string.Empty);

            string hostname = parameters["hostname"] ?? string.Empty;
            string apiKey = parameters["apiKey"] ?? string.Empty;

            if (!string.IsNullOrEmpty(hostname))
            {
                string url = string.Format("https://{0}/utilities/deployemailcampaigns.aspx?apiKey={1}", hostname, apiKey);

                using (var client = new HttpClient())
                {
                    host.LogMessage("TDS Post Deploy: Executing get request {0}", url);
                    var result = client.GetAsync(url).Result;
                    host.LogMessage("TDS Post Deploy: Request executed with status code {0}", result.StatusCode);
                }
            }
        }
    }
}