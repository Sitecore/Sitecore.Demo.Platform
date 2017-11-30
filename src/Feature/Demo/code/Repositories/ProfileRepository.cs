namespace Sitecore.Feature.Demo.Repositories
{
    using Sitecore.Analytics;
    using Sitecore.Feature.Demo.Models;
    using Sitecore.Feature.Demo.Services;
    using Sitecore.Foundation.DependencyInjection;
    using System.Collections.Generic;
    using System.Linq;

    [Service]
    public class ProfileRepository
    {
        private readonly IProfileProvider _profileProvider;

        public ProfileRepository(IProfileProvider profileProvider)
        {
            this._profileProvider = profileProvider;
        }

        public IEnumerable<Profile> GetProfiles(ProfilingTypes profiling)
        {
            if (!Tracker.IsActive)
            {
                return Enumerable.Empty<Profile>();
            }

            return this._profileProvider.GetSiteProfiles().Where(p => this._profileProvider.HasMatchingPattern(p, profiling)).Select(x => new Profile
            {
                Name = x.NameField,
                PatternMatches = this._profileProvider.GetPatternsWithGravityShare(x, profiling)
            });
        }
    }
}