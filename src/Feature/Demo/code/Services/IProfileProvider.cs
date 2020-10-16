using System.Collections.Generic;
using Sitecore.Analytics.Data.Items;
using Sitecore.Demo.Platform.Feature.Demo.Models;

namespace Sitecore.Demo.Platform.Feature.Demo.Services
{
    public interface IProfileProvider
    {
        #pragma warning disable 0618

        IEnumerable<ProfileItem> GetSiteProfiles();

        bool HasMatchingPattern(ProfileItem currentProfile, ProfilingTypes type);

        IEnumerable<PatternMatch> GetPatternsWithGravityShare(ProfileItem visibleProfile, ProfilingTypes type);

        #pragma warning restore 0618
    }
}