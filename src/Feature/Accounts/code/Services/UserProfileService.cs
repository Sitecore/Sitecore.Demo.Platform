using Sitecore.HabitatHome.Feature.Accounts.Models;
using Sitecore.HabitatHome.Foundation.DependencyInjection;
using Sitecore.Security;
using Sitecore.Security.Accounts;
using System.Collections.Generic;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    [Service(typeof(IUserProfileService))]
    public class UserProfileService : IUserProfileService
    {
        private readonly IProfileSettingsService _profileSettingsService;
        private readonly IUserProfileProvider _userProfileProvider;
        private readonly IContactFacetsService _contactFacetsService;
        private readonly IAccountTrackerService _accountTrackerService;

        public UserProfileService(IProfileSettingsService profileSettingsService, IUserProfileProvider userProfileProvider, IContactFacetsService contactFacetsService, IAccountTrackerService accountTrackerService)
        {
            _profileSettingsService = profileSettingsService;
            _userProfileProvider = userProfileProvider;
            _contactFacetsService = contactFacetsService;
            _accountTrackerService = accountTrackerService;
        }

        public virtual string GetUserDefaultProfileId()
        {
            return _profileSettingsService.GetUserDefaultProfile()?.ID?.ToString();
        }

        public virtual EditProfile GetEmptyProfile()
        {
            return new EditProfile
                   {
                       InterestTypes = _profileSettingsService.GetInterests()
                   };
        }

        public virtual EditProfile GetProfile(User user)
        {
            SetProfileIfEmpty(user);

            var properties = _userProfileProvider.GetCustomProperties(user.Profile);

            var model = new EditProfile
                        {
                            Email = user.Profile.Email,
                            FirstName = user.Profile.Name ?? "",
                            LastName = user.Profile.GetCustomProperty("LastName") ?? "",
                            PhoneNumber = user.Profile.GetCustomProperty("Phone") ?? "",
                            Interest = properties.ContainsKey(Constants.UserProfile.Fields.Interest) ? properties[Constants.UserProfile.Fields.Interest] : "",
                            InterestTypes = _profileSettingsService.GetInterests()
                        };

            return model;
        }

        public virtual void SaveProfile(UserProfile userProfile, EditProfile model)
        {
            var properties = new Dictionary<string, string>
                             {
                                 [Constants.UserProfile.Fields.FirstName] = model.FirstName,
                                 [Constants.UserProfile.Fields.LastName] = model.LastName,
                                 [Constants.UserProfile.Fields.PhoneNumber] = model.PhoneNumber,
                                 [Constants.UserProfile.Fields.Interest] = model.Interest,
                                 [nameof(userProfile.Name)] = model.FirstName,
                                 [nameof(userProfile.FullName)] = $"{model.FirstName} {model.LastName}".Trim()
                             };

            _userProfileProvider.SetCustomProfile(userProfile, properties);
            _contactFacetsService.UpdateContactFacets(userProfile);
            _accountTrackerService.TrackEditProfile(userProfile);
        }

        public IEnumerable<string> GetInterests()
        {
            return _profileSettingsService.GetInterests();
        }

        public virtual string ExportData(UserProfile userProfile)
        {
            _accountTrackerService.TrackExportData(userProfile);
            return _contactFacetsService.ExportContactData();
        }


        public virtual void DeleteProfile(UserProfile userProfile)
        {
            _contactFacetsService.DeleteContact();
            userProfile.ProfileUser.Delete();
            _accountTrackerService.TrackDeleteProfile(userProfile);
        }

        private void SetProfileIfEmpty(User user)
        {
            if (Context.User.Profile.ProfileItemId != null)
            {
                return;
            }

            user.Profile.ProfileItemId = GetUserDefaultProfileId();
            user.Profile.Save();
        }
    }
}