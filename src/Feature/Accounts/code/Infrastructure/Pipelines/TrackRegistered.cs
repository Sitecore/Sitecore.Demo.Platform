using Sitecore.HabitatHome.Feature.Accounts.Services;
using Sitecore.HabitatHome.Foundation.Accounts.Pipelines;    

namespace Sitecore.HabitatHome.Feature.Accounts.Infrastructure.Pipelines
{            
    public class TrackRegistered
    {
        private readonly IAccountTrackerService _accountTrackerService;
        private readonly IUpdateContactFacetsService _updateContactFacetsService;

        public TrackRegistered(IAccountTrackerService accountTrackerService, IUpdateContactFacetsService updateContactFacetsService)
        {
            _accountTrackerService = accountTrackerService;
            _updateContactFacetsService = updateContactFacetsService;
        }

        public void Process(AccountsPipelineArgs args)
        {
            _updateContactFacetsService.UpdateContactFacets(args.User.Profile);
            _accountTrackerService.TrackRegistration();
        }             
    }
}