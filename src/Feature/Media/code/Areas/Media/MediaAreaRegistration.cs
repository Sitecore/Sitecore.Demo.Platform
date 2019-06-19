using System.Web.Mvc;

namespace Sitecore.HabitatHome.Feature.Media.Areas.Media
{
    public class MediaAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Media";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DemoMedia",
                "Media/{controller}/{action}/{id}",
                new {action = "Index", id = UrlParameter.Optional},
                namespaces: new[] { "Sitecore.HabitatHome.Feature.Media.Controllers" }
            );
        }
    }
}