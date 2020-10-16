using Sitecore.Demo.Platform.Feature.Accounts.Services;
using Sitecore.Demo.Platform.Foundation.Accounts.Pipelines;

namespace Sitecore.Demo.Platform.Feature.Accounts.Infrastructure.Pipelines
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