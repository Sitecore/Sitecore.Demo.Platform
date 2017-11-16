using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Feature.Demo.Repositories
{
    using Sitecore.Feature.Demo.Models;
    using Sitecore.Foundation.SitecoreExtensions.Services;
    using Sitecore.XA.Foundation.Mvc.Repositories.Base;

    [Service(typeof(ISidebarRepository))]
    public class SidebarRepository : ModelRepository, ISidebarRepository
    {
        private readonly VisitsRepository _visitsRepository;
        private readonly PersonalInfoRepository _personalInformationRepository;
        private readonly OnsiteBehaviorRepository _onsiteBehaviorRepository;
        private readonly ReferralRepository _referralRepository;
        private readonly ITrackerService _trackerService;

        public SidebarRepository(VisitsRepository visitsRepository,
            PersonalInfoRepository personalInformationRepository,
            OnsiteBehaviorRepository onsiteBehaviorRepository, 
            ReferralRepository referralRepository, 
            ITrackerService trackerService)
        {
            _visitsRepository = visitsRepository;
            _personalInformationRepository = personalInformationRepository;
            _onsiteBehaviorRepository = onsiteBehaviorRepository;
            _referralRepository = referralRepository;
            _trackerService = trackerService;
        }

        public override IRenderingModelBase GetModel()
        {
            SidebarModel model = new SidebarModel();
            FillBaseProperties(model);
            model.Visits = _visitsRepository.Get();
            model.PersonalInformation = _personalInformationRepository.Get();
            model.OnsiteBehavior = _onsiteBehaviorRepository.Get();
            model.Referral = _referralRepository.Get();
            model.IsActive = _trackerService.IsActive;

            return model;
        }
    }
}