using System;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    public interface IGetRedirectUrlService
    {
        string GetRedirectUrl(AuthenticationStatus status, string returnUrl = null);

        string GetDefaultRedirectUrl(AuthenticationStatus status);
    }
}