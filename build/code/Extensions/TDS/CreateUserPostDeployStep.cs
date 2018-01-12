using HedgehogDevelopment.SitecoreProject.PackageInstallPostProcessor.Contracts;
using Sitecore.Security.Accounts;
using System.Xml.Linq;

namespace Sitecore.Demo.Deployment.Web.Extensions.TDS
{
    public class CreateUserPostDeployStep : IPostDeployAction
    {
        public void RunPostDeployAction(XDocument deployedItems, IPostDeployActionHost host, string parameter)
        {
            string demoAdminUsername = "sitecore/demoadmin";
            host.LogMessage("demoadminusername: " + demoAdminUsername);

            string demoAdminDomainUsername = Security.Domains.Domain.GetDomain("sitecore").GetFullName("demoadmin");
            host.LogMessage("demoadmindomainusername: " + demoAdminDomainUsername);

            if (!User.Exists(demoAdminUsername))
            {
                var user = User.Create(demoAdminUsername, "demopass");
                user.Profile.Email = "support@sitecore.net";
                user.Profile.Save();
            }
        }
    }
}