using Sitecore.Analytics;
using Sitecore.Demo.Platform.Feature.Accounts.Services;
using Sitecore.Demo.Platform.Foundation.Accounts.Pipelines;

namespace Sitecore.Demo.Platform.Feature.Accounts.Infrastructure.Pipelines
{
    public class TrackLoggedIn
    {
        private readonly IAccountTrackerService _accountTrackerService;

        public TrackLoggedIn(IAccountTrackerService accountTrackerService)
        {
            _accountTrackerService = accountTrackerService;
        }

        public void Process(LoggedInPipelineArgs args)
        {
            var contactId = args.ContactId;
            _accountTrackerService.TrackLoginAndIdentifyContact(args.Source, args.UserName);
            args.ContactId = Tracker.Current?.Contact?.ContactId;
            args.PreviousContactId = contactId;
        }
    }
}