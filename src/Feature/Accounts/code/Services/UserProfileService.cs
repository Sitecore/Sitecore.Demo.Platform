namespace Sitecore.Feature.Accounts.Services
{
    using Sitecore.Feature.Accounts.Models;
    using Sitecore.Foundation.DependencyInjection;
    using Sitecore.Security;
    using Sitecore.Security.Accounts;
    using System.Collections.Generic;

    [Service(typeof(IUserProfileService))]
    public class UserProfileService : IUserProfileService
    {
        private readonly IProfileSettingsService _profileSettingsService;
        private readonly IUserProfileProvider _userProfileProvider;
        private readonly IUpdateContactFacetsService _updateContactFacetsService;
        private readonly IAccountTrackerService _accountTrackerService;

        public UserProfileService(IProfileSettingsService profileSettingsService, IUserProfileProvider userProfileProvider, IUpdateContactFacetsService updateContactFacetsService, IAccountTrackerService accountTrackerService)
        {
            this._profileSettingsService = profileSettingsService;
            this._userProfileProvider = userProfileProvider;
            this._updateContactFacetsService = updateContactFacetsService;
            this._accountTrackerService = accountTrackerService;
        }

        public virtual string GetUserDefaultProfileId()
        {
            return this._profileSettingsService.GetUserDefaultProfile()?.ID?.ToString();
        }

        public virtual EditProfile GetEmptyProfile()
        {
            return new EditProfile
            {
                InterestTypes = this._profileSettingsService.GetInterests()
            };
        }

        public virtual EditProfile GetProfile(User user)
        {
            this.SetProfileIfEmpty(user);

            var properties = this._userProfileProvider.GetCustomProperties(user.Profile);

            var model = new EditProfile
            {
                Email = user.Profile.Email,
                FirstName = properties.ContainsKey(Constants.UserProfile.Fields.FirstName) ? properties[Constants.UserProfile.Fields.FirstName] : "",
                LastName = properties.ContainsKey(Constants.UserProfile.Fields.LastName) ? properties[Constants.UserProfile.Fields.LastName] : "",
                PhoneNumber = properties.ContainsKey(Constants.UserProfile.Fields.PhoneNumber) ? properties[Constants.UserProfile.Fields.PhoneNumber] : "",
                Interest = properties.ContainsKey(Constants.UserProfile.Fields.Interest) ? properties[Constants.UserProfile.Fields.Interest] : "",
                InterestTypes = this._profileSettingsService.GetInterests()
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

            this._userProfileProvider.SetCustomProfile(userProfile, properties);
            this._updateContactFacetsService.UpdateContactFacets(userProfile);
            _accountTrackerService.TrackEditProfile(userProfile);
        }

        public IEnumerable<string> GetInterests()
        {
            return this._profileSettingsService.GetInterests();
        }
        private void SetProfileIfEmpty(User user)
        {
            if (Context.User.Profile.ProfileItemId != null)
                return;

            user.Profile.ProfileItemId = this.GetUserDefaultProfileId();
            user.Profile.Save();
        }
    }
}