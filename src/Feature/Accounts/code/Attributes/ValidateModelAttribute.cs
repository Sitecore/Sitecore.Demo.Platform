using System.Web.Mvc;

namespace Sitecore.Demo.Platform.Feature.Accounts.Attributes
{
    public class ValidateModelAttribute : ActionFilterAttribute
  {
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      var viewData = filterContext.Controller.ViewData;

      if (!viewData.ModelState.IsValid)
      {
        filterContext.Result = new ViewResult
                               {
                                 ViewData = viewData,
                                 TempData = filterContext.Controller.TempData
                               };
      }
    }
  }
}