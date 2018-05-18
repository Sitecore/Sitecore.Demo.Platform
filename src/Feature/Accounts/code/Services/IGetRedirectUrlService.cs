namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    using System.Web.Mvc;

    public interface IGetRedirectUrlService
    {
        string GetRedirectUrl(AuthenticationStatus status, string returnUrl = null);
        string GetDefaultRedirectUrl(AuthenticationStatus status);
    }
}