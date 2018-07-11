using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.HabitatHome.Feature.Accounts.Models;
using Sitecore.HabitatHome.Feature.Accounts.Services;
using Sitecore.HabitatHome.Foundation.DependencyInjection;
using Sitecore.HabitatHome.Foundation.Dictionary.Repositories;                 
using Sitecore.Pipelines.GetSignInUrlInfo;

namespace Sitecore.HabitatHome.Feature.Accounts.Repositories
{
    [Service(typeof(IFedAuthLoginButtonRepository))]
    public class FedAuthLoginButtonRepository : IFedAuthLoginButtonRepository
    {

        private readonly BaseCorePipelineManager _pipelineManager;
        private readonly IAccountsSettingsService _accountsSettingsService;

        public FedAuthLoginButtonRepository(BaseCorePipelineManager pipelineManager, IAccountsSettingsService accountsSettingsService)
        {
            _pipelineManager = pipelineManager;
            _accountsSettingsService = accountsSettingsService;
        }
        public IEnumerable<FedAuthLoginButton> GetAll()
        {
            var returnUrl = _accountsSettingsService.GetPageLinkOrDefault(Context.Item, Templates.AccountsSettings.Fields.AfterLoginPage);
            var args = new GetSignInUrlInfoArgs(Context.Site.Name, returnUrl);
            GetSignInUrlInfoPipeline.Run(_pipelineManager, args);
            if (args.Result == null)
            {
                throw new InvalidOperationException("Could not retrieve federated authentication logins");
            }

            return args.Result.Select(CreateFedAuthLoginButton).ToArray();
        }

        private static FedAuthLoginButton CreateFedAuthLoginButton(SignInUrlInfo signInInfo)
        {
            var caption = DictionaryPhraseRepository.Current.Get($"/Accounts/Sign in providers/{signInInfo.IdentityProvider}", $"Sign in with {signInInfo.Caption}");
            string iconClass = null;
            switch (signInInfo.IdentityProvider.ToLower())
            {
                case "facebook":
                    iconClass = "fa fa-facebook";
                    break;
                case "google":
                    iconClass = "fa fa-google";
                    break;
                case "linkedin":
                    iconClass = "fa fa-linkedin";
                    break;
                case "twitter":
                    iconClass = "fa fa-twitter";
                    break;
                default:
                    iconClass = "fa fa-cloud";
                    break;
            }

            return new FedAuthLoginButton
            {
                Provider = signInInfo.IdentityProvider,
                IconClass = iconClass,
                Href = signInInfo.Href,
                Caption = caption,
            };
        }
    }
}