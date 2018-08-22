using Sitecore.HabitatHome.Foundation.Accounts.Models;
using Sitecore.HabitatHome.Foundation.Accounts.Services;
using Sitecore.HabitatHome.Foundation.DependencyInjection;
using Sitecore.Security;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    [Service(typeof(IContactFacetsService))]
    public class ContactFacetsService : IContactFacetsService
    {                                                                       
        private readonly IContactFacetService _contactFacetService;

        public ContactFacetsService(IContactFacetService contactFacetService)
        {                                                     
            _contactFacetService = contactFacetService;
        }

        public void UpdateContactFacets(UserProfile profile)
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

            _contactFacetService.UpdateContactFacets(data);       
        }

        public string ExportContactData()
        {
            return _contactFacetService.ExportContactData();
        }

        public bool DeleteContact()
        {
            return _contactFacetService.DeleteContact();
        }
    }
}