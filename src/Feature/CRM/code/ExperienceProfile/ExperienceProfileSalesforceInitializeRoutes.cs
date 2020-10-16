using Sitecore.Cintel.Endpoint.Plumbing;
using Sitecore.Pipelines;
using System.Web.Http;
using System.Web.Routing;

namespace Sitecore.Demo.Platform.Feature.CRM.ExperienceProfile
{
    public class ExperienceProfileSalesforceInitializeRoutes : InitializeRoutes
    {
        public override void Process(PipelineArgs args)
        {
            this.RegisterRoutes(RouteTable.Routes, args);
        }

        protected new void RegisterRoutes(RouteCollection routes, PipelineArgs args)
        {
            base.RegisterRoutes(routes, args);
            routes.Remove(routes["cintel_contact_entity"]);
            routes.MapHttpRoute("cintel_contact_entity", "sitecore/api/ao/v1/contacts/{contactId}", (object)new
            {
                controller = "ExperienceProfileSalesforceContact",
                action = "Get"
            });
        }
    }
}
