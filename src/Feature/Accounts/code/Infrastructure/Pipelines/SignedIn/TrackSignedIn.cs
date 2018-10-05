using System.Security.Claims;
using Sitecore.Analytics;
using Sitecore.HabitatHome.Feature.Accounts.Services;
using Sitecore.Owin.Authentication.Configuration;
using Sitecore.Owin.Authentication.Pipelines.CookieAuthentication.SignedIn;

namespace Sitecore.HabitatHome.Feature.Accounts.Infrastructure.Pipelines.SignedIn
{                                                                                     
    public class TrackSignedIn : SignedInProcessor
    {
        private readonly IAccountTrackerService _accountTrackerService;
        private readonly IContactFacetsService _updateContactFacetsService;
        private readonly FederatedAuthenticationConfiguration _federatedAuthenticationConfiguration;

        public TrackSignedIn(IAccountTrackerService accountTrackerService, IContactFacetsService updateContactFacetsService, FederatedAuthenticationConfiguration federatedAuthenticationConfiguration)
        {
            _accountTrackerService = accountTrackerService;
            _updateContactFacetsService = updateContactFacetsService;
            _federatedAuthenticationConfiguration = federatedAuthenticationConfiguration;
        }
                                                                                                                  
        public override void Process(SignedInArgs args)
        {
            //Do not track the user signin if this is a response to a membership provider login or a sitecore backend signin
            var provider = this.GetProvider(args.Context.Identity);
            if (provider.Name == Owin.Authentication.Constants.LocalIdentityProvider || Context.Domain.Name == "sitecore")
            {
                return;
            }

            if (Tracker.Current == null)
            {
                Tracker.Initialize();
            }

            _accountTrackerService.TrackLoginAndIdentifyContact(provider.Name, args.User.Id);
            _updateContactFacetsService.UpdateContactFacets(args.User.InnerUser.Profile);
        }

        private IdentityProvider GetProvider(ClaimsIdentity identity)
        {
            return _federatedAuthenticationConfiguration.GetIdentityProvider(identity);
        }
    }
}