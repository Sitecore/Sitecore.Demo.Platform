using Sitecore.DependencyInjection;

namespace Sitecore.Feature.Accounts.Attributes
{
    using Sitecore.Feature.Accounts.Services;
    using System.Web.Mvc;

    public class RedirectAuthenticatedAttribute : ActionFilterAttribute
    {
        private readonly IGetRedirectUrlService _getRedirectUrlService;

        public RedirectAuthenticatedAttribute()
        {
            this._getRedirectUrlService = (IGetRedirectUrlService) ServiceLocator.ServiceProvider.GetService(typeof(IGetRedirectUrlService));
        }


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!Context.PageMode.IsNormal)
                return;
            if (!Context.User.IsAuthenticated)
                return;
            var link = this._getRedirectUrlService.GetRedirectUrl(AuthenticationStatus.Authenticated);
            filterContext.Result = new RedirectResult(link);
        }
    }
}