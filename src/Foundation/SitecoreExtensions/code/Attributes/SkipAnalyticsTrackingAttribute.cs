using System.Web.Mvc;
using Sitecore.Analytics;

namespace Sitecore.Demo.Platform.Foundation.SitecoreExtensions.Attributes
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