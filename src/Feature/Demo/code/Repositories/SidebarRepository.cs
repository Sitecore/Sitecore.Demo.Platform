using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Feature.Demo.Repositories
{
    using Sitecore.Feature.Demo.Models;
    using Sitecore.XA.Foundation.Mvc.Repositories.Base;

    [Service(typeof(ISidebarRepository))]
    public class SidebarRepository : ModelRepository, ISidebarRepository
    {
        private readonly IVisitsRepository _visitsRepository;
        private readonly IPersonalInformationRepository _personalInformationRepository;
        private readonly IOnsiteBehaviorRepository _onsiteBehaviorRepository;
        private readonly IReferralRepository _referralRepository;

        public SidebarRepository(IVisitsRepository visitsRepository,
            IPersonalInformationRepository personalInformationRepository,
            IOnsiteBehaviorRepository onsiteBehaviorRepository, IReferralRepository referralRepository)
        {
            this._visitsRepository = visitsRepository;
            this._personalInformationRepository = personalInformationRepository;
            this._onsiteBehaviorRepository = onsiteBehaviorRepository;
            this._referralRepository = referralRepository;
        }

        public override IRenderingModelBase GetModel()
        {
            SidebarModel model = new SidebarModel();
            FillBaseProperties(model);
            model.Visits = (Visits)_visitsRepository.GetModel();
            model.PersonalInformation = (PersonalInformation)_personalInformationRepository.GetModel();
            model.OnsiteBehavior = (OnsiteBehavior) _onsiteBehaviorRepository.GetModel();
            model.Referral = (Referral) _referralRepository.GetModel();

            return model;
        }
    }
}