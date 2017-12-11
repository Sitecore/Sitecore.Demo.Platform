using Sitecore.Diagnostics;
using Sitecore.Foundation.Alerts.Extensions;
using Sitecore.Foundation.Alerts.Models;

namespace Sitecore.Feature.Accounts.Controllers
{
    using Sitecore.Feature.Accounts.Attributes;
    using Sitecore.Feature.Accounts.Models;
    using Sitecore.Feature.Accounts.Repositories;
    using Sitecore.Feature.Accounts.Services;
    using Sitecore.XA.Foundation.Mvc.Controllers;
    using System;
    using System.Web.Mvc;

    public class AccountsController : StandardController
    {                                                                               
        private readonly IAccountRepository _accountRepository;
        private readonly INotificationService _notificationService;
        private readonly IAccountsSettingsService _accountsSettingsService;

        private readonly IGetRedirectUrlService _getRedirectUrlService;
        //private IUserProfileService UserProfileService { get; }

        public AccountsController(IAccountsSettingsService accountsSettingsService,
            IGetRedirectUrlService getRedirectUrlService, IAccountRepository accountRepository, IFedAuthLoginButtonRepository fedAuthLoginRepository, INotificationService notificationService)
        {
            this._accountsSettingsService = accountsSettingsService;
            this._getRedirectUrlService = getRedirectUrlService;
            this._accountRepository = accountRepository;
            this._notificationService = notificationService;
        }                   

        protected virtual ActionResult Login(LoginInfo loginInfo, Func<string, ActionResult> redirectAction)
        {
            var user = this._accountRepository.Login(loginInfo.Email, loginInfo.Password);
            if (user == null)
            {
                this.ModelState.AddModelError("invalidCredentials", Sitecore.Globalization.Translate.Text("UserNotFound"));
                return this.View(loginInfo);
            }

            var redirectUrl = loginInfo.ReturnUrl;
            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = this._getRedirectUrlService.GetRedirectUrl(AuthenticationStatus.Authenticated);
            }

            return redirectAction(redirectUrl);
        }

        [HttpGet]
        public ActionResult Login(string returnUrl = null)
        {
            var loginModel = GetModel();
            return this.View("Login", loginModel);
        }
        
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

        //[RedirectAuthenticated]
        public ActionResult ForgotPassword()
        {
            try
            {
                this._accountsSettingsService.GetForgotPasswordMailTemplate();
            }
            catch (Exception)
            {
                return this.InfoMessage(InfoMessage.Error(ForgotPasswordEmailNotConfigured));
            }

            return this.View();
        }

        [HttpPost]
        [ValidateModel]
        //[RedirectAuthenticated]
        public ActionResult ForgotPassword(PasswordResetInfo model)
        {
            if (!this._accountRepository.Exists(model.Email))
            {
                this.ModelState.AddModelError(nameof(model.Email), UserDoesNotExistError);

                return this.View(model);
            }

            try
            {
                var newPassword = this._accountRepository.RestorePassword(model.Email);
                this._notificationService.SendPassword(model.Email, newPassword);
                return this.InfoMessage(InfoMessage.Success(Sitecore.Globalization.Translate.Text("PasswordResetSuccess")));
            }
            catch (Exception ex)
            {
                Log.Error($"Can't reset password for user {model.Email}", ex, this);
                this.ModelState.AddModelError(nameof(model.Email), ex.Message);

                return this.View(model);
            }
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