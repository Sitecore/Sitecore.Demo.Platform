using System;
using Sitecore.Configuration;
using Sitecore.HabitatHome.Foundation.DependencyInjection;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Services;
using Sitecore.Security;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{

    [Service(typeof(IAccountTrackerService))]
    public class AccountTrackerService : IAccountTrackerService
    {
        private readonly IAccountsSettingsService _accountsSettingsService;
        private readonly ITrackerService _trackerService;

        public AccountTrackerService(IAccountsSettingsService accountsSettingsService, ITrackerService trackerService)
        {
            _accountsSettingsService = accountsSettingsService;
            _trackerService = trackerService;
        }

        public static Guid LogoutPageEventId => Guid.Parse("{D23A32CD-F893-495E-86F0-9FE852987376}");
        public static Guid RegistrationFailedPageEventId => Guid.Parse("{D98AAED9-CF5F-41D6-8A6E-109F60F1E950}");
        public static Guid LoginFailedPageEventId => Guid.Parse("{27E67C84-B055-4D57-ADEB-E73DEFCA22A8}");
        public static Guid EditProfilePageEvent => Guid.Parse("{7A2582D2-7270-4E53-8998-3934B84876C3}");
        public static Guid LoginGoalId => Guid.Parse(Settings.GetSetting("Sitecore.HabitatHome.Feature.Accounts.LoginGoalId", "{66722F52-2D13-4DCC-90FC-EA7117CF2298}"));
        public static Guid RegistrationGoalId => Guid.Parse(Settings.GetSetting("Sitecore.HabitatHome.Feature.Accounts.RegistrationGoalId", "{8FFB183B-DA1A-4C74-8F3A-9729E9FCFF6A}"));
        public static Guid ExportDataGoalId => Guid.Parse(Settings.GetSetting("Sitecore.HabitatHome.Feature.Accounts.ExportDataGoalId", "{D0EF461C-3AA0-40B8-9B5F-C1723E54F6ED}"));
        public static Guid DeleteProfileGoalId => Guid.Parse(Settings.GetSetting("Sitecore.HabitatHome.Feature.Accounts.DeleteProfileGoalId", "{83A9DF9A-D7D9-4B73-BAE0-4F77B4935462}"));


        public virtual void TrackLoginAndIdentifyContact(string source, string identifier)
        {
            _trackerService.TrackGoal(LoginGoalId, source);
            _trackerService.IdentifyContact(source, identifier);
        }

        public void TrackLogout(string userName)
        {
            _trackerService.TrackPageEvent(LogoutPageEventId, data: userName);
        }

        public virtual void TrackRegistration()
        {
            _trackerService.TrackGoal(RegistrationGoalId);
            TrackRegistrationOutcome();
        }

        public virtual void TrackRegistrationFailed(string email)
        {
            _trackerService.TrackPageEvent(RegistrationFailedPageEventId, data: email);
        }
        public virtual void TrackLoginFailed(string userName)
        {
            _trackerService.TrackPageEvent(LoginFailedPageEventId, data: userName);
        }

        public void TrackEditProfile(UserProfile userProfile)
        {
            _trackerService.TrackPageEvent(EditProfilePageEvent, data: userProfile.UserName);
        }

        public void TrackRegistrationOutcome()
        {
            var outcomeId = this._accountsSettingsService.GetRegistrationOutcome(Context.Item);
            if (outcomeId.HasValue)
            {
                _trackerService.TrackOutcome(outcomeId.Value);
            }
        }

        public void TrackExportData(UserProfile userProfile)
        {
            _trackerService.TrackPageEvent(ExportDataGoalId, data: userProfile.UserName);
        }

        public void TrackDeleteProfile(UserProfile userProfile)
        {
            _trackerService.TrackPageEvent(DeleteProfileGoalId, data: userProfile.UserName);
        }
    }
}