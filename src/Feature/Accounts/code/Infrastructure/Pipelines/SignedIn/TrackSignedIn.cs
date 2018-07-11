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
        private readonly IUpdateContactFacetsService _updateContactFacetsService;
        private readonly FederatedAuthenticationConfiguration _federatedAuthenticationConfiguration;

        public TrackSignedIn(IAccountTrackerService accountTrackerService, IUpdateContactFacetsService updateContactFacetsService, FederatedAuthenticationConfiguration federatedAuthenticationConfiguration)
        {
            _accountTrackerService = accountTrackerService;
            _updateContactFacetsService = updateContactFacetsService;
            _federatedAuthenticationConfiguration = federatedAuthenticationConfiguration;
        }
                                                                                                                  
        public override void Process(SignedInArgs args)
        {
            //Do not track the user signin if this is a response to a membership provider login
            var provider = this.GetProvider(args.Context.Identity);
            if (provider.Name == Owin.Authentication.Constants.LocalIdentityProvider)
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