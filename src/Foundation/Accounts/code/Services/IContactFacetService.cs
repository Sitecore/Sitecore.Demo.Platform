using Sitecore.HabitatHome.Foundation.Accounts.Models;

namespace Sitecore.HabitatHome.Foundation.Accounts.Services
{
    public interface IContactFacetService
    {
        ContactFacetData GetContactData();

        void UpdateContactFacets(ContactFacetData data);

        string ExportContactData();

        bool DeleteContact();
    }
}
