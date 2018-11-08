using Sitecore.HabitatHome.Feature.Accounts.Services;
using Sitecore.HabitatHome.Foundation.Accounts.Pipelines;
using Sitecore.HabitatHome.Foundation.Accounts.Services;

namespace Sitecore.HabitatHome.Feature.Accounts.Infrastructure.Pipelines
{            
    public class TrackRegistered
    {
        private readonly IAccountTrackerService _accountTrackerService;
        private readonly IUserProfileService _userProfileService;

        public TrackRegistered(IAccountTrackerService accountTrackerService, IUserProfileService userProfileService)
        {
            _accountTrackerService = accountTrackerService;
            _userProfileService = userProfileService;
        }

        public void Process(AccountsPipelineArgs args)
        {
            _userProfileService.UpdateContactFacetData(args.User.Profile);
            _accountTrackerService.TrackRegistration();
        }             
    }
}