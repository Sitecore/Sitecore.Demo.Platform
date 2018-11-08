using Sitecore.HabitatHome.Feature.Accounts.Models;
using Sitecore.HabitatHome.Foundation.Accounts.Models;
using Sitecore.HabitatHome.Foundation.Accounts.Services;
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
        private readonly IContactFacetService _contactFacetsService;
        private readonly IAccountTrackerService _accountTrackerService;

        public UserProfileService(IProfileSettingsService profileSettingsService, IUserProfileProvider userProfileProvider, IContactFacetService contactFacetsService, IAccountTrackerService accountTrackerService)
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
            var contactData = _contactFacetsService.GetContactData();

            var model = new EditProfile
                        {
                            Email = user.Profile.Email,
                            FirstName = user.Profile.GetCustomProperty(Constants.UserProfile.Fields.FirstName) ?? "",
                            LastName = user.Profile.GetCustomProperty(Constants.UserProfile.Fields.LastName) ?? "",
                            PhoneNumber = user.Profile.GetCustomProperty(Constants.UserProfile.Fields.PhoneNumber) ?? "",
                            Interest = properties.ContainsKey(Constants.UserProfile.Fields.Interest) ? properties[Constants.UserProfile.Fields.Interest] : "",
                            InterestTypes = _profileSettingsService.GetInterests(),
                            SportType = contactData.SportType,
                            SportName = contactData.SportName
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
            UpdateContactFacetData(userProfile);
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


        public void UpdateContactFacetData(UserProfile profile)
        {
            ContactFacetData data = new ContactFacetData
            {
                FirstName = profile[Constants.UserProfile.Fields.FirstName],
                MiddleName = profile[Constants.UserProfile.Fields.MiddleName],
                LastName = profile[Constants.UserProfile.Fields.LastName],
                AvatarUrl = profile[Constants.UserProfile.Fields.PictureUrl],
                AvatarMimeType = profile[Constants.UserProfile.Fields.PictureMimeType],
                EmailAddress = profile.Email,
                PhoneNumber = profile[Constants.UserProfile.Fields.PhoneNumber],
                Language = profile[Constants.UserProfile.Fields.Language],
                Gender = profile[Constants.UserProfile.Fields.Gender],
                Birthday = profile[Constants.UserProfile.Fields.Birthday]
            };

            _contactFacetsService.UpdateContactFacets(data);
        }
    }
}