using Sitecore.HabitatHome.Feature.Accounts.Services;
using Sitecore.HabitatHome.Foundation.Accounts.Pipelines;    

namespace Sitecore.HabitatHome.Feature.Accounts.Infrastructure.Pipelines
{            
    public class TrackRegistered
    {
        private readonly IAccountTrackerService _accountTrackerService;
        private readonly IContactFacetsService _updateContactFacetsService;

        public TrackRegistered(IAccountTrackerService accountTrackerService, IContactFacetsService updateContactFacetsService)
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