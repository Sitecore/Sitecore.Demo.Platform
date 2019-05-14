using System.Reflection;
using System.Web.Mvc;

namespace Sitecore.HabitatHome.Foundation.SitecoreExtensions.Forms
{
    public class ValidateFormHandler : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            var controller = controllerContext.HttpContext.Request.Form["fhController"];
            var action = controllerContext.HttpContext.Request.Form["fhAction"];

            return !string.IsNullOrWhiteSpace(controller)
                   && !string.IsNullOrWhiteSpace(action)
                   && controller == controllerContext.Controller.GetType().Name
                   && methodInfo.Name == action;
        }
    }
}