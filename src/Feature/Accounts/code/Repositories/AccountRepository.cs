using System;
using System.Web.Security;
using Sitecore.Demo.Platform.Feature.Accounts.Models;
using Sitecore.Demo.Platform.Feature.Accounts.Services;
using Sitecore.Demo.Platform.Foundation.Accounts.Pipelines;
using Sitecore.Demo.Platform.Foundation.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;
using Sitecore.Security.Authentication;

namespace Sitecore.Demo.Platform.Feature.Accounts.Repositories
{
    [Service(typeof(IAccountRepository))]
    public class AccountRepository : IAccountRepository
    {
        private readonly IAccountTrackerService _accountTrackerService;
        private readonly PipelineService _pipelineService;

        public AccountRepository(PipelineService pipelineService, IAccountTrackerService accountTrackerService)
        {
            _accountTrackerService = accountTrackerService;
            _pipelineService = pipelineService;
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
                _accountTrackerService.TrackLoginFailed(accountName);
                return null;
            }

            var user = AuthenticationManager.GetActiveUser();
            _pipelineService.RunLoggedIn(user);
            return user;
        }

        public void Logout()
        {
            var user = AuthenticationManager.GetActiveUser();
            AuthenticationManager.Logout();
            if (user != null)
            {
                _pipelineService.RunLoggedOut(user);
            }
        }

        public string RestorePassword(string userName)
        {
            var domainName = Context.Domain.GetFullName(userName);
            var user = Membership.GetUser(domainName);
            if (user == null)
            {
                throw new ArgumentException($"Could not reset password for user '{userName}'", nameof(userName));
            }

            return user.ResetPassword();
        }

        public void RegisterUser(RegistrationInfo registrationInfo, string profileId)
        {
            Assert.ArgumentNotNullOrEmpty(registrationInfo.Email, "email");
            Assert.ArgumentNotNullOrEmpty(registrationInfo.Password, "password");

            var fullName = Context.Domain.GetFullName(registrationInfo.Email);
            try
            {

                Assert.IsNotNullOrEmpty(fullName, "Can't retrieve full userName");

                var user = User.Create(fullName, registrationInfo.Password);
                user.Profile.Email = registrationInfo.Email;
                user.Profile.FullName = registrationInfo.FirstName + " " + registrationInfo.LastName;
                user.Profile.SetCustomProperty(Constants.UserProfile.Fields.FirstName, registrationInfo.FirstName);
                user.Profile.SetCustomProperty(Constants.UserProfile.Fields.LastName, registrationInfo.LastName);

                if (!string.IsNullOrEmpty(profileId))
                {
                    user.Profile.ProfileItemId = profileId;
                }

                user.Profile.Save();
                _pipelineService.RunRegistered(user);
            }
            catch
            {
                _accountTrackerService.TrackRegistrationFailed(registrationInfo.Email);
                throw;
            }

            Login(registrationInfo.Email, registrationInfo.Password);
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            Assert.ArgumentNotNullOrEmpty(oldPassword, nameof(oldPassword));
            Assert.ArgumentNotNullOrEmpty(newPassword, nameof(newPassword));

            var accountName = string.Empty;
            var domain = Context.Domain;
            if (domain != null)
            {
                accountName = domain.GetFullName(userName);
            }

            var user = Membership.GetUser(accountName);
            Assert.ArgumentNotNull(user, nameof(user));
            
            return user.ChangePassword(oldPassword, newPassword);
        }
    }
}