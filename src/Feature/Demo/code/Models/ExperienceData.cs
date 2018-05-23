namespace Sitecore.HabitatHome.Feature.Demo.Models
{
    using Sitecore.HabitatHome.Feature.Demo.Repositories;
    using Sitecore.HabitatHome.Feature.Demo.Services;
    using Sitecore.HabitatHome.Foundation.Accounts.Providers;
    using Sitecore.HabitatHome.Foundation.DependencyInjection;
    using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Services;

    [Service(Lifetime = Lifetime.Transient)]
    public class ExperienceData
    {
        public ExperienceData(VisitsRepository visitsRepository, PersonalInfoRepository personalInfoRepository, OnsiteBehaviorRepository onsiteBehaviorRepository, ReferralRepository referralRepository, ITrackerService trackerService)
        {
            this.Visits = visitsRepository.Get();
            this.PersonalInfo = personalInfoRepository.Get();
            this.OnsiteBehavior = onsiteBehaviorRepository.Get();
            this.Referral = referralRepository.Get();
            this.IsActive = trackerService.IsActive;
        }

        public Visits Visits { get; set; }
        public PersonalInfo PersonalInfo { get; set; }
        public OnsiteBehavior OnsiteBehavior { get; set; }
        public Referral Referral { get; set; }
        public bool IsActive { get; set; }
    }
}