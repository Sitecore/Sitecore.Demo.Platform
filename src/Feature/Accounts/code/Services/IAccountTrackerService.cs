using Sitecore.Security;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    public interface IAccountTrackerService
    {
        void TrackRegistration();

        void TrackRegistrationOutcome();

        void TrackLoginAndIdentifyContact(string source, string identifier);

        void TrackLogout(string userName);

        void TrackRegistrationFailed(string email);

        void TrackLoginFailed(string userName);

        void TrackEditProfile(UserProfile userProfile);

        void TrackExportData(UserProfile userProfile);

        void TrackDeleteProfile(UserProfile userProfile);
    }
}