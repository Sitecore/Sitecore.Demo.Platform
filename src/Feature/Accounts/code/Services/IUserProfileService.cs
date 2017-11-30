namespace Sitecore.Feature.Accounts.Services
{
    using Sitecore.Feature.Accounts.Models;
    using Sitecore.Security;
    using Sitecore.Security.Accounts;
    using System.Collections.Generic;

    public interface IUserProfileService
    {
        string GetUserDefaultProfileId();
        EditProfile GetEmptyProfile();
        EditProfile GetProfile(User user);
        void SaveProfile(UserProfile userProfile, EditProfile model);
        IEnumerable<string> GetInterests();
    }
}