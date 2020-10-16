using Sitecore.Demo.Platform.Feature.Accounts.Services;
using Sitecore.Demo.Platform.Foundation.Accounts.Pipelines;

namespace Sitecore.Demo.Platform.Feature.Accounts.Infrastructure.Pipelines
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