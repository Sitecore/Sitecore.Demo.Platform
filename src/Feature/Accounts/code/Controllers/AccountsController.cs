using Sitecore.Data.Fields;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Feature.Accounts.Controllers
{
    using Sitecore.Diagnostics;
    using Sitecore.Feature.Accounts.Attributes;
    using Sitecore.Feature.Accounts.Models;
    using Sitecore.Feature.Accounts.Repositories;
    using Sitecore.Feature.Accounts.Services;
    using Sitecore.Foundation.Alerts.Extensions;
    using Sitecore.Foundation.Alerts.Models;
    using Sitecore.XA.Foundation.Mvc.Controllers;
    using System;
    using System.Web.Mvc;
    using System.Web.Security;

    public class AccountsController : StandardController
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IRegisterRepository _registerRepository;
        private readonly INotificationService _notificationService;
        private readonly IAccountsSettingsService _accountsSettingsService;
        private readonly IUserProfileService _userProfileService;

        private readonly IGetRedirectUrlService _getRedirectUrlService;
        //private IUserProfileService UserProfileService { get; }

        public AccountsController(IAccountsSettingsService accountsSettingsService,
            IGetRedirectUrlService getRedirectUrlService, IAccountRepository accountRepository,
            IRegisterRepository registerRepository, IFedAuthLoginButtonRepository fedAuthLoginRepository,
            IUserProfileService userProfileService, INotificationService notificationService)
        {
            this._accountsSettingsService = accountsSettingsService;
            this._getRedirectUrlService = getRedirectUrlService;
            this._accountRepository = accountRepository;
            this._registerRepository = registerRepository;
            this._userProfileService = userProfileService;
            this._notificationService = notificationService;
        }

        protected virtual ActionResult Login(LoginInfo loginInfo, Func<string, ActionResult> redirectAction)
        {
            var user = this._accountRepository.Login(loginInfo.Email, loginInfo.Password);
            if (user == null)
            {
                this.ModelState.AddModelError("invalidCredentials",
                    Sitecore.Globalization.Translate.Text("UserNotFound"));
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

        public ActionResult Register()
        {
            RegistrationInfo model = new RegistrationInfo();
            InternalLinkField link = Context.Site.GetSettingsItem().Fields[Templates.AccountsSettings.Fields.AfterLoginPage];
            if (link.TargetItem == null)
            {
                throw new Exception($"{link.InnerField.Name} link isn't set");
            }

            model.ReturnUrl = link.TargetItem.Url();
            
            return this.View("~/Views/Accounts/Register.cshtml", model);
        }

        [HttpPost]
        [ValidateModel]
        //[RedirectAuthenticated]
        public ActionResult Register(RegistrationInfo registrationInfo)
        {
            if (this._registerRepository.Exists(registrationInfo.Email))
            {
                this.ModelState.AddModelError(nameof(registrationInfo.Email), UserAlreadyExistsError);

                return this.View("~/Views/Accounts/Register.cshtml", registrationInfo);
            }

            try
            {
                this._registerRepository.RegisterUser(registrationInfo.Email, registrationInfo.Password,
                    this._userProfileService.GetUserDefaultProfileId());

                var link = this._getRedirectUrlService.GetRedirectUrl(AuthenticationStatus.Authenticated);
                registrationInfo.ReturnUrl = link;
                return this.View("~/Views/Accounts/Register.cshtml",
                    registrationInfo); //TODO: make this actually redirect
            }
            catch (MembershipCreateUserException ex)
            {
                Log.Error($"Can't create user with {registrationInfo.Email}", ex, this);
                this.ModelState.AddModelError(nameof(registrationInfo.Email), ex.Message);

                return this.View("~/Views/Accounts/Register.cshtml", registrationInfo);
            }
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
                return this.InfoMessage(
                    InfoMessage.Success(Sitecore.Globalization.Translate.Text("PasswordResetSuccess")));
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

        private static string ForgotPasswordEmailNotConfigured =>
            Sitecore.Globalization.Translate.Text("EmailNotConfigured");

        private static string UserDoesNotExistError => Sitecore.Globalization.Translate.Text("UserDoesNotExist");
    }
}