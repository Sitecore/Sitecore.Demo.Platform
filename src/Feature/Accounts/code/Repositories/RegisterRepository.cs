using Sitecore.Feature.Accounts.Models;

namespace Sitecore.Feature.Accounts.Repositories
{
    using Sitecore.Diagnostics;
    using Sitecore.Feature.Accounts.Services;
    using Sitecore.Foundation.Accounts.Pipelines;
    using Sitecore.Foundation.DependencyInjection;
    using Sitecore.Security.Accounts;
    using Sitecore.XA.Foundation.Mvc.Repositories.Base;

    [Service(typeof(IRegisterRepository))]
    public class RegisterRepository : ModelRepository, IRegisterRepository
    {
        public IAccountTrackerService AccountTrackerService { get; }
        private readonly PipelineService _pipelineService;
        private readonly IAccountRepository _accountRepository;

        public RegisterRepository(PipelineService pipelineService, IAccountTrackerService accountTrackerService, IAccountRepository accountRepository)
        {
            this.AccountTrackerService = accountTrackerService;
            this._pipelineService = pipelineService;
            this._accountRepository = accountRepository;
        }

        public void RegisterUser(string email, string password, string profileId)
        {
            Assert.ArgumentNotNullOrEmpty(email, nameof(email));
            Assert.ArgumentNotNullOrEmpty(password, nameof(password));

            var fullName = Context.Domain.GetFullName(email);
            try
            {

                Assert.IsNotNullOrEmpty(fullName, "Can't retrieve full userName");

                var user = User.Create(fullName, password);
                user.Profile.Email = email;
                if (!string.IsNullOrEmpty(profileId))
                {
                    user.Profile.ProfileItemId = profileId;
                }

                user.Profile.Save();
                this._pipelineService.RunRegistered(user);
            }
            catch
            {
                AccountTrackerService.TrackRegistrationFailed(email);
                throw;
            }

            _accountRepository.Login(email, password);
        }

        public bool Exists(string userName)
        {
            var fullName = Context.Domain.GetFullName(userName);

            return User.Exists(fullName);
        }

        public override IRenderingModelBase GetModel()
        {
            RegistrationInfo model = new RegistrationInfo();
            FillBaseProperties(model);

            return model;
        }
    }
}