namespace Sitecore.Demo.Platform.Feature.Accounts.Services
{
    public interface IGetRedirectUrlService
    {
        string GetRedirectUrl(AuthenticationStatus status, string returnUrl = null);

        string GetDefaultRedirectUrl(AuthenticationStatus status);
    }
}