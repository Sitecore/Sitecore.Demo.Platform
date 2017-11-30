namespace Sitecore.Feature.Accounts.Services
{
    using Sitecore.Security;
    using System.Collections.Generic;

    public interface IUserProfileProvider
    {
        IDictionary<string, string> GetCustomProperties(UserProfile userProfile);
        void SetCustomProfile(UserProfile userProfile, IDictionary<string, string> properties);
    }
}