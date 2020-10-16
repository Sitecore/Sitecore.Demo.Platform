using System.Linq;
using Sitecore.Demo.Platform.Feature.Demo.Models;
using Sitecore.Demo.Platform.Foundation.DependencyInjection;

namespace Sitecore.Demo.Platform.Feature.Demo.Repositories
{
    [Service(typeof(IOnsiteBehaviorRepository))]
    public class OnsiteBehaviorRepository : IOnsiteBehaviorRepository
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

        public OnsiteBehavior Get()
        {
            return new OnsiteBehavior
                   {
                       Goals = this.pageEventRepository.GetLatest().ToArray(),
                       PageEvents = this.pageEventRepository.GetPageEvents().ToArray(),
                       Outcomes = this.outcomesRepository.GetLatest().ToArray(),
                       HistoricProfiles = this.profileRepository.GetProfiles(ProfilingTypes.Historic).ToArray(),
                       ActiveProfiles = this.profileRepository.GetProfiles(ProfilingTypes.Active).ToArray()
                   };
        }
    }
}