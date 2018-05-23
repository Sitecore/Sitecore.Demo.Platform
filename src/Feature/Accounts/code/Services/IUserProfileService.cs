namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Sitecore.HabitatHome.Feature.Accounts.Models;
    using Sitecore.Security;
    using Sitecore.Security.Accounts;

    public interface IUserProfileService
    {
        string GetUserDefaultProfileId();
        EditProfile GetEmptyProfile();
        EditProfile GetProfile(User user);
        void SaveProfile(UserProfile userProfile, EditProfile model);
        IEnumerable<string> GetInterests();
    }
}