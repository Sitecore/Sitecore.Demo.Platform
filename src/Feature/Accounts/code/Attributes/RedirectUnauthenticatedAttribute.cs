using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Demo.Platform.Feature.Accounts.Services;
using Sitecore.DependencyInjection;

namespace Sitecore.Demo.Platform.Feature.Accounts.Attributes
{
    public class RedirectUnauthenticatedAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        private readonly IGetRedirectUrlService getRedirectUrlService;

        public RedirectUnauthenticatedAttribute()
        {
            this.getRedirectUrlService = ServiceLocator.ServiceProvider.GetService<IGetRedirectUrlService>();
        }

        public void OnAuthorization(AuthorizationContext context)
        {
            if (Context.User.IsAuthenticated)
            {
                return;
            }
            var link = this.getRedirectUrlService.GetRedirectUrl(AuthenticationStatus.Unauthenticated, context.HttpContext.Request.RawUrl);
            context.Result = new RedirectResult(link);
        }
    }
}