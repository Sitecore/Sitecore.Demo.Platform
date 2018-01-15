using HedgehogDevelopment.SitecoreProject.PackageInstallPostProcessor.Contracts;
using Sitecore.Security.Accounts;
using System.Xml.Linq;

namespace Sitecore.Demo.Deployment.Web.Extensions.TDS
{
    public class CreateUserPostDeployStep : IPostDeployAction
    {
        public void RunPostDeployAction(XDocument deployedItems, IPostDeployActionHost host, string parameter)
        {
            string demoAdminUsername = "demoadmin";                                                        
            string demoAdminDomainUsername = Security.Domains.Domain.GetDomain("sitecore").GetFullName(demoAdminUsername);

            host.LogMessage("Checking if user exists: " + demoAdminDomainUsername);

            if (!User.Exists(demoAdminDomainUsername))
            {
                host.LogMessage("Creating user: " + demoAdminDomainUsername);

                var user = User.Create(demoAdminDomainUsername, "demopass");
                user.Profile.FullName = "Demo Administrator";
                user.Profile.IsAdministrator = true;
                user.Profile.Email = "admin@demo.com";
                user.Profile.Save();
            }
        }
    }
}