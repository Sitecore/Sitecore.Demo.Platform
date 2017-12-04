namespace Sitecore.Feature.Accounts.Repositories
{
    using Sitecore.Feature.Accounts.Models;
    using Sitecore.Feature.Accounts.Services;
    using Sitecore.Foundation.Accounts.Pipelines;
    using Sitecore.Foundation.DependencyInjection;
    using Sitecore.Foundation.SitecoreExtensions.Extensions;
    using Sitecore.Security.Accounts;
    using Sitecore.Security.Authentication;
    using Sitecore.XA.Foundation.Mvc.Repositories.Base;
    using System;
    using System.Web.Security;

    [Service(typeof(IAccountRepository))]
    public class AccountRepository : ModelRepository, IAccountRepository
    {
        public IAccountTrackerService AccountTrackerService { get; }
        private readonly PipelineService _pipelineService;
        private readonly IFedAuthLoginButtonRepository _fedAuthLoginButtonRepository;

        public AccountRepository(PipelineService pipelineService, IAccountTrackerService accountTrackerService, IFedAuthLoginButtonRepository fedAuthLoginButtonRepository)
        {
            this.AccountTrackerService = accountTrackerService;
            this._pipelineService = pipelineService;
            this._fedAuthLoginButtonRepository = fedAuthLoginButtonRepository;
        }

        public bool Exists(string userName)
        {
            var fullName = Context.Domain.GetFullName(userName);

            return User.Exists(fullName);
        }

        public User Login(string userName, string password)
        {
            var accountName = string.Empty;
            var domain = Context.Domain;
            if (domain != null)
            {
                accountName = domain.GetFullName(userName);
            }

            var result = AuthenticationManager.Login(accountName, password);
            if (!result)
            {
                AccountTrackerService.TrackLoginFailed(accountName);
                return null;
            }

            var user = AuthenticationManager.GetActiveUser();
            this._pipelineService.RunLoggedIn(user);
            return user;
        }

        public void Logout()
        {
            var user = AuthenticationManager.GetActiveUser();
            AuthenticationManager.Logout();
            if (user != null)
                this._pipelineService.RunLoggedOut(user);
        }

        public string RestorePassword(string userName)
        {
            var domainName = Context.Domain.GetFullName(userName);
            var user = Membership.GetUser(domainName);
            if (user == null)
                throw new ArgumentException($"Could not reset password for user '{userName}'", nameof(userName));
            return user.ResetPassword();
        }
        
        public override IRenderingModelBase GetModel()
        {
            LoginInfo model = new LoginInfo();
            FillBaseProperties(model);
            model.ReturnUrl = Links.LinkManager.GetItemUrl(Context.Site.GetStartItem()); //todo: replace with settings item -> after login url        
            model.LoginButtons = _fedAuthLoginButtonRepository.GetAll();

            return model;
        }
    }
}