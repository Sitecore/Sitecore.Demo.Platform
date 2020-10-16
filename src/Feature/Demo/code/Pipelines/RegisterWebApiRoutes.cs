using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Pipelines;

namespace Sitecore.Demo.Platform.Feature.Demo.Pipelines
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