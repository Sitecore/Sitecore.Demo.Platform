namespace Sitecore.Feature.Demo.Services
{
    using Sitecore.Analytics.Data.Items;
    using Sitecore.Feature.Demo.Models;
    using System.Collections.Generic;

    public interface IProfileProvider
    {
        IEnumerable<ProfileItem> GetSiteProfiles();
        bool HasMatchingPattern(ProfileItem currentProfile, ProfilingTypes type);
        IEnumerable<PatternMatch> GetPatternsWithGravityShare(ProfileItem visibleProfile, ProfilingTypes type);
    }
}