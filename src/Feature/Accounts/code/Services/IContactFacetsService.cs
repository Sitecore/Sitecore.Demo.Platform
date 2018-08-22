using Sitecore.Security;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    public interface IContactFacetsService
    {
        void UpdateContactFacets(UserProfile profile);

        string ExportContactData();

        bool DeleteContact();
    }
}