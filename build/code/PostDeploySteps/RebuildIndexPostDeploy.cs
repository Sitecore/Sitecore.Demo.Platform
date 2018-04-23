using System.Collections.Specialized;
using HedgehogDevelopment.SitecoreProject.PackageInstallPostProcessor.Contracts;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Xml.Linq;

namespace Sitecore.Demo.Deployment.Web.PostDeploySteps
{
    public class RebuildIndexPostDeploy : IPostDeployAction
    {
        public void RunPostDeployAction(XDocument deployedItems, IPostDeployActionHost host, string parameter)
        {
            NameValueCollection parameters = HttpUtility.ParseQueryString(parameter ?? string.Empty);

            string websiteUrl = parameters["hostname"];
            string index = parameters["index"];
            string webRequestUrl =
                string.Format("https://{0}/utilities/rebuildindex.aspx?index={1}", websiteUrl, index);

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(webRequestUrl).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    host.LogMessage("TDS Post Deploy: Rebuild index " + parameter + " started");
                }
                else
                {
                    host.LogMessage("TDS Post Deploy: Could not rebuild index " + parameter + " response status code " + (int)response.StatusCode);
                }
            }
        }
    }
}