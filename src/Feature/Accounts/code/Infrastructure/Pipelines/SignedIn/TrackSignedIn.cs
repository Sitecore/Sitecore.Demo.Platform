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
        private readonly FederatedAuthenticationConfiguration _federatedAuthenticationConfiguration;
        private readonly IUserProfileService _userProfileService;

        public TrackSignedIn(IAccountTrackerService accountTrackerService, FederatedAuthenticationConfiguration federatedAuthenticationConfiguration, IUserProfileService userProfileService)
        {
            _accountTrackerService = accountTrackerService;
            _federatedAuthenticationConfiguration = federatedAuthenticationConfiguration;
            _userProfileService = userProfileService;
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
            _userProfileService.UpdateContactFacetData(args.User.InnerUser.Profile);
        }

        private IdentityProvider GetProvider(ClaimsIdentity identity)
        {
            return _federatedAuthenticationConfiguration.GetIdentityProvider(identity);
        }
    }
}