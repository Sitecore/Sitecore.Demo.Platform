using Sitecore.HabitatHome.Feature.Accounts.Models;
using Sitecore.Security.Accounts;

namespace Sitecore.HabitatHome.Feature.Accounts.Repositories
{
    public interface IAccountRepository
    {
        /// <summary>
        ///   Method changes the password for the user to a random string,
        ///   and returns that string.
        /// </summary>
        /// <param name="userName">Username that should have new password</param>
        /// <returns>New generated password</returns>
        string RestorePassword(string userName);

        void RegisterUser(RegistrationInfo registrationInfo, string profileId);

        bool Exists(string userName);

        void Logout();

        User Login(string userName, string password);

        bool ChangePassword(string userName, string oldPassword, string newPassword);
    }
}