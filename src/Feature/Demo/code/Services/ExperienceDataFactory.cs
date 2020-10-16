using Sitecore.Demo.Platform.Feature.Demo.Models;
using Sitecore.Demo.Platform.Feature.Demo.Repositories;
using Sitecore.Demo.Platform.Foundation.DependencyInjection;
using Sitecore.Demo.Platform.Foundation.SitecoreExtensions.Services;

namespace Sitecore.Demo.Platform.Feature.Demo.Services
{
    [Service(typeof(IExperienceDataFactory))]
    public class ExperienceDataFactory : IExperienceDataFactory
    {
        private readonly IVisitsRepository visitsRepository;
        private readonly IPersonalInfoRepository personalInfoRepository;
        private readonly IOnsiteBehaviorRepository onsiteBehaviorRepository;
        private readonly IReferralRepository referralRepository;
        private readonly ITrackerService trackerService;

        public ExperienceDataFactory(IVisitsRepository visitsRepository, IPersonalInfoRepository personalInfoRepository, IOnsiteBehaviorRepository onsiteBehaviorRepository, IReferralRepository referralRepository, ITrackerService trackerService)
        {
            this.visitsRepository = visitsRepository;
            this.personalInfoRepository = personalInfoRepository;
            this.onsiteBehaviorRepository = onsiteBehaviorRepository;
            this.referralRepository = referralRepository;
            this.trackerService = trackerService;
        }

        public ExperienceData Get()
        {
            return new ExperienceData
            {
                Visits = visitsRepository.Get(),
                PersonalInfo = personalInfoRepository.Get(),
                OnsiteBehavior = onsiteBehaviorRepository.Get(),
                Referral = referralRepository.Get(),
                IsActive = trackerService.IsActive
            };
        }
    }
}