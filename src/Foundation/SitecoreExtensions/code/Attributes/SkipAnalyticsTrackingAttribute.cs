using Sitecore.Analytics;   
using System.Web.Mvc;

namespace Sitecore.HabitatHome.Foundation.SitecoreExtensions.Attributes
{
    public class SkipAnalyticsTrackingAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest() && Tracker.IsActive)
            {
                Tracker.Current?.CurrentPage?.Cancel();
            }
        }
    }
}