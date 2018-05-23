namespace Sitecore.HabitatHome.Feature.Demo.Services
{
    using Sitecore.Analytics.Data.Items;
    using Sitecore.HabitatHome.Feature.Demo.Models;
    using System.Collections.Generic;

    public interface IProfileProvider
    {
        #pragma warning disable 0618

        IEnumerable<ProfileItem> GetSiteProfiles();

        bool HasMatchingPattern(ProfileItem currentProfile, ProfilingTypes type);

        IEnumerable<PatternMatch> GetPatternsWithGravityShare(ProfileItem visibleProfile, ProfilingTypes type);

        #pragma warning restore 0618
    }
}