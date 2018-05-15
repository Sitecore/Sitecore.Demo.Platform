namespace Sitecore.HabitatHome.Feature.Accounts.Infrastructure.Pipelines
{
    using Sitecore.HabitatHome.Feature.Accounts.Services;
    using Sitecore.HabitatHome.Foundation.Accounts.Pipelines;
    using Sitecore.HabitatHome.Foundation.DependencyInjection;

    public class TrackRegistered
    {
        private readonly IAccountTrackerService accountTrackerService;
        private readonly IUpdateContactFacetsService updateContactFacetsService;

        public TrackRegistered(IAccountTrackerService accountTrackerService, IUpdateContactFacetsService updateContactFacetsService)
        {
            this.accountTrackerService = accountTrackerService;
            this.updateContactFacetsService = updateContactFacetsService;
        }

        public void Process(AccountsPipelineArgs args)
        {
            this.updateContactFacetsService.UpdateContactFacets(args.User.Profile);
            this.accountTrackerService.TrackRegistration();
        }

    }
}