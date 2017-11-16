//namespace Sitecore.Feature.Demo.Models
//{
//    using Sitecore.Feature.Demo.Repositories;
//    using Sitecore.Foundation.DependencyInjection;

//    [Service(Lifetime = Lifetime.Transient)]
//    public class ExperienceData
//    {
//        public ExperienceData(VisitsRepository visitsRepository, PersonalInformationRepository personalInfoRepository, OnsiteBehaviorRepository onsiteBehaviorRepository, ReferralRepository referralRepository, ITrackerService trackerService)
//        {
//            this.Visits = (Visits)visitsRepository.GetModel();
//            this.PersonalInformation = personalInfoRepository.Get();
//            this.OnsiteBehavior = onsiteBehaviorRepository.Get();
//            this.Referral = referralRepository.Get();
//            this.IsActive = trackerService.IsActive;
//        }

//        public Visits Visits { get; set; }
//        public PersonalInformation PersonalInformation { get; set; }
//        public OnsiteBehavior OnsiteBehavior { get; set; }
//        public Referral Referral { get; set; }
//        public bool IsActive { get; set; }
//    }
//}