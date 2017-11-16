namespace Sitecore.Feature.Demo.Repositories
{
    using Sitecore.Feature.Demo.Models;
    using Sitecore.Foundation.DependencyInjection;
    using Sitecore.XA.Foundation.Mvc.Repositories.Base;
    using System.Linq;

    [Service(typeof(IOnsiteBehaviorRepository))]
    public class OnsiteBehaviorRepository : ModelRepository, IOnsiteBehaviorRepository
    {
        private readonly PageEventRepository pageEventRepository;
        private readonly OutcomeRepository outcomesRepository;
        private readonly ProfileRepository profileRepository;

        public OnsiteBehaviorRepository(ProfileRepository profileRepository, PageEventRepository pageEventRepository, OutcomeRepository outcomesRepository)
        {
            this.pageEventRepository = pageEventRepository;
            this.outcomesRepository = outcomesRepository;
            this.profileRepository = profileRepository;
        }

        public override IRenderingModelBase GetModel()
        {
            OnsiteBehavior model = new OnsiteBehavior();
            FillBaseProperties(model);
            model.ActiveProfiles = profileRepository.GetProfiles(ProfilingTypes.Active).ToArray();
            model.HistoricProfiles = profileRepository.GetProfiles(ProfilingTypes.Historic).ToArray();
            model.Goals = pageEventRepository.GetLatest().ToArray();
            model.Outcomes = outcomesRepository.GetLatest().ToArray();
            model.PageEvents = pageEventRepository.GetLatest().ToArray();

            return model;
        }
    }
}