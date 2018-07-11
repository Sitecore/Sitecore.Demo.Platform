using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Pipelines;

namespace Sitecore.HabitatHome.Feature.Accounts.Infrastructure.Pipelines
{
    public class RegisterWebApiRoutes
    {
        public void Process(PipelineArgs args)
        {
            RouteTable.Routes.MapRoute("Feature.Accounts.Api", "api/accounts/{action}", new
            {
                controller = "Accounts"
            });
        }
    }
}