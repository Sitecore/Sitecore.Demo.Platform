using Sitecore.HabitatHome.Feature.Accounts.Services;
using Sitecore.HabitatHome.Foundation.Accounts.Pipelines;

namespace Sitecore.HabitatHome.Feature.Accounts.Infrastructure.Pipelines
{                                                                   
    public class TrackLoggedOut
    {
        private readonly IAccountTrackerService _accountTrackerService;

        public TrackLoggedOut(IAccountTrackerService accountTrackerService)
        {
            _accountTrackerService = accountTrackerService;
        }

        public void Process(AccountsPipelineArgs args)
        {
            _accountTrackerService.TrackLogout(args.UserName);
        }
    }
}