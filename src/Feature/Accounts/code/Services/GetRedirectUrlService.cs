using System;
using System.Web;
using Sitecore.HabitatHome.Foundation.DependencyInjection;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Extensions;
using Sitecore.SecurityModel;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{                                                                             
    [Service(typeof(IGetRedirectUrlService))]
    public class GetRedirectUrlService : IGetRedirectUrlService
    {
        private readonly IAccountsSettingsService _accountsSettingsService;
        private const string ReturnUrlQuerystring = "ReturnUrl";

        public GetRedirectUrlService(IAccountsSettingsService accountsSettingsService)
        {
            _accountsSettingsService = accountsSettingsService;
        }

        public string GetRedirectUrl(AuthenticationStatus status, string returnUrl = null)
        {
            string redirectUrl = GetDefaultRedirectUrl(status);
            if (!string.IsNullOrEmpty(returnUrl))
            {
                redirectUrl = AddReturnUrl(redirectUrl, returnUrl);
            }

            return redirectUrl;
        }

        private string AddReturnUrl(string baseUrl, string returnUrl)
        {
           return baseUrl + "?" + ReturnUrlQuerystring + "=" + HttpUtility.UrlEncode(returnUrl);
        }

        public string GetDefaultRedirectUrl(AuthenticationStatus status)
        {
            switch (status)
            {
                case AuthenticationStatus.Unauthenticated:
                    return _accountsSettingsService.GetPageLinkOrDefault(Context.Item, Templates.AccountsSettings.Fields.LoginPage, Context.Site.GetStartItem());
                case AuthenticationStatus.Authenticated:
                    using (new SecurityDisabler())
                    {
                        // redirectUrl may be requested prior to access being granted by authentication
                        return _accountsSettingsService.GetPageLinkOrDefault(Context.Item, Templates.AccountsSettings.Fields.AfterLoginPage, Context.Site.GetStartItem());
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}