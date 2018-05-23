using Sitecore.Pipelines;
using System.Web.Mvc;
using System.Web.Routing;

namespace Sitecore.HabitatHome.Feature.Demo.Pipelines
{
    public class RegisterWebApiRoutes
    {
        public void Process(PipelineArgs args)
        {
            RouteTable.Routes.MapRoute("Feature.Demo.Api", "api/demo/{action}", new
            {
                controller = "Demo"
            });
        }
    }
}