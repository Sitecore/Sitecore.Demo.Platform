namespace Sitecore.HabitatHome.Feature.Accounts.Services
{   
    using Sitecore.HabitatHome.Foundation.Accounts.Providers;
    using Sitecore.HabitatHome.Foundation.DependencyInjection;
    using Sitecore.Security;                        
    using Sitecore.HabitatHome.Foundation.Accounts.Services;
    using Sitecore.HabitatHome.Foundation.Accounts.Models;

    [Service(typeof(IUpdateContactFacetsService))]
    public class UpdateContactFacetsService : IUpdateContactFacetsService
    {
        private readonly IContactFacetsProvider _contactFacetsProvider;
        private readonly IContactFacetService _contactFacetService;

        public UpdateContactFacetsService(IContactFacetsProvider contactFacetsProvider, IContactFacetService contactFacetService)
        {
            _contactFacetsProvider = contactFacetsProvider;
            _contactFacetService = contactFacetService;
        }

        public void UpdateContactFacets(UserProfile profile)
        {
            ContactFacetData data = new ContactFacetData();

            data.FirstName = profile[Constants.UserProfile.Fields.FirstName];
            data.MiddleName = profile[Constants.UserProfile.Fields.MiddleName];
            data.LastName = profile[Constants.UserProfile.Fields.LastName];

            data.AvatarUrl = profile[Constants.UserProfile.Fields.PictureUrl];
            data.AvatarMimeType = profile[Constants.UserProfile.Fields.PictureMimeType];
            data.EmailAddress = profile.Email;
            data.PhoneNumber = profile[Constants.UserProfile.Fields.PhoneNumber];
            data.Language = profile[Constants.UserProfile.Fields.Language];
            data.Gender = profile[Constants.UserProfile.Fields.Gender];
            data.Birthday = profile[Constants.UserProfile.Fields.Birthday];

            _contactFacetService.UpdateContactFacets(data);

        }
    }
}