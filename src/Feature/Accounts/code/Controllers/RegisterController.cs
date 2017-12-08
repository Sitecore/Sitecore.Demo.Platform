namespace Sitecore.Feature.Accounts.Controllers
{
    using Sitecore.Diagnostics;
    using Sitecore.Feature.Accounts.Attributes;
    using Sitecore.Feature.Accounts.Models;
    using Sitecore.Feature.Accounts.Repositories;
    using Sitecore.Feature.Accounts.Services;
    using Sitecore.XA.Foundation.Mvc.Controllers;
    using System.Web.Mvc;
    using System.Web.Security;

    public class RegisterController : StandardController
    {
        //private IFedAuthLoginButtonRepository FedAuthLoginRepository { get; }
        private readonly IRegisterRepository _registerRepository;
        //private INotificationService NotificationService { get; }
        private readonly IAccountsSettingsService _accountsSettingsService;

        private readonly IGetRedirectUrlService _getRedirectUrlService;
        private IUserProfileService _userProfileService { get; }

        public RegisterController(IAccountsSettingsService accountsSettingsService,
            IGetRedirectUrlService getRedirectUrlService, IRegisterRepository registerRepository, IUserProfileService userProfileService)
        {
            this._accountsSettingsService = accountsSettingsService;
            this._getRedirectUrlService = getRedirectUrlService;
            this._registerRepository = registerRepository;
            this._userProfileService = userProfileService;
        }

        public ActionResult Register()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateModel]
        public ActionResult Register(RegistrationInfo registrationInfo)
        {
            if (this._registerRepository.Exists(registrationInfo.Email))
            {
                this.ModelState.AddModelError(nameof(registrationInfo.Email), UserAlreadyExistsError);

                return this.View(registrationInfo);
            }

            try
            {
                this._registerRepository.RegisterUser(registrationInfo.Email, registrationInfo.Password, this._userProfileService.GetUserDefaultProfileId());

                var link = this._getRedirectUrlService.GetRedirectUrl(AuthenticationStatus.Authenticated);
                return this.Redirect(link); //TODO: make this actually redirect
            }
            catch (MembershipCreateUserException ex)
            {
                Log.Error($"Can't create user with {registrationInfo.Email}", ex, this);
                this.ModelState.AddModelError(nameof(registrationInfo.Email), ex.Message);

                return this.View(registrationInfo);
            }
        }

        protected override object GetModel()
        {
            return _registerRepository.GetModel();
        }

        protected override string GetIndexViewName()
        {
            return "~/Views/Accounts/Register.cshtml";
        }

        public static string UserAlreadyExistsError => Sitecore.Globalization.Translate.Text("UserAlreadyExists");

        private static string ForgotPasswordEmailNotConfigured => Sitecore.Globalization.Translate.Text("EmailNotConfigured");

        private static string UserDoesNotExistError => Sitecore.Globalization.Translate.Text("UserDoesNotExist");
    }
}