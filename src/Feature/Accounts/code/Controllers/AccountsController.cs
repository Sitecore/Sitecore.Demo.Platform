using System.Web.Security;
using Sitecore.Diagnostics;
using Sitecore.Feature.Accounts.Attributes;

namespace Sitecore.Feature.Accounts.Controllers
{
    using Sitecore.Feature.Accounts.Models;
    using Sitecore.Feature.Accounts.Repositories;
    using Sitecore.Feature.Accounts.Services;
    using Sitecore.XA.Foundation.Mvc.Controllers;
    using System;
    using System.Web.Mvc;

    public class AccountsController : StandardController
    {
        //private IFedAuthLoginButtonRepository FedAuthLoginRepository { get; }
        private readonly IAccountRepository _accountRepository;
        //private INotificationService NotificationService { get; }
        private readonly IAccountsSettingsService _accountsSettingsService;

        private readonly IGetRedirectUrlService _getRedirectUrlService;
        //private IUserProfileService UserProfileService { get; }

        public AccountsController(IAccountsSettingsService accountsSettingsService,
            IGetRedirectUrlService getRedirectUrlService, IAccountRepository accountRepository)
        {
            this._accountsSettingsService = accountsSettingsService;
            this._getRedirectUrlService = getRedirectUrlService;
            this._accountRepository = accountRepository;
        }

        //private LoginInfo CreateLoginInfo(string returnUrl = null)
        //{
        //    return new LoginInfo
        //    {
        //        ReturnUrl = returnUrl,
        //        //LoginButtons = this.FedAuthLoginRepository.GetAll()
        //    };
        //}

        protected virtual ActionResult Login(LoginInfo loginInfo, Func<string, ActionResult> redirectAction)
        {
            var user = this._accountRepository.Login(loginInfo.Email, loginInfo.Password);
            if (user == null)
            {
                this.ModelState.AddModelError("invalidCredentials", Sitecore.Globalization.Translate.Text("USerNotFound"));
                return this.View(loginInfo);
            }

            var redirectUrl = loginInfo.ReturnUrl;
            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = this._getRedirectUrlService.GetRedirectUrl(AuthenticationStatus.Authenticated);
            }

            return redirectAction(redirectUrl);
        }

        public ActionResult Login(string returnUrl = null)
        {
            var loginModel = GetModel();
            return this.View("Login", loginModel);
        }

        //[HttpPost]
        //[ValidateModel]
        //public ActionResult Register(RegistrationInfo registrationInfo)
        //{
        //    if (this._accountRepository.Exists(registrationInfo.Email))
        //    {
        //        this.ModelState.AddModelError(nameof(registrationInfo.Email), UserAlreadyExistsError);

        //        return this.View(registrationInfo);
        //    }

        //    try
        //    {
        //        this._accountRepository.RegisterUser(registrationInfo.Email, registrationInfo.Password, this.UserProfileService.GetUserDefaultProfileId());

        //        var link = this._getRedirectUrlService.GetRedirectUrl(AuthenticationStatus.Authenticated);
        //        return this.Redirect(link);
        //    }
        //    catch (MembershipCreateUserException ex)
        //    {
        //        Log.Error($"Can't create user with {registrationInfo.Email}", ex, this);
        //        this.ModelState.AddModelError(nameof(registrationInfo.Email), ex.Message);

        //        return this.View(registrationInfo);
        //    }
        //}
        
        [HttpPost]
        [ValidateModel]
        public ActionResult _Login(LoginInfo loginInfo)
        {
            return this.Login(loginInfo, redirectUrl => this.Json(new LoginResult
            {
                RedirectUrl = redirectUrl
            }));
        }

        [HttpPost]
        [ValidateModel]
        public ActionResult Login(LoginInfo loginInfo)
        {
            return this.Login(loginInfo, redirectUrl => new RedirectResult(redirectUrl));
        }

        protected override object GetModel()
        {
            return _accountRepository.GetModel();
        }

        protected override string GetIndexViewName()
        {
            return "~/Views/Accounts/Login.cshtml";
        }

        public static string UserAlreadyExistsError => Sitecore.Globalization.Translate.Text("UserAlreadyExists");

        private static string ForgotPasswordEmailNotConfigured => Sitecore.Globalization.Translate.Text("EmailNotConfigured");

        private static string UserDoesNotExistError => Sitecore.Globalization.Translate.Text("UserDoesNotExist");
    }
}