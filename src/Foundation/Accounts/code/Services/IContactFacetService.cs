using Sitecore.Demo.Platform.Foundation.Accounts.Models;

namespace Sitecore.Demo.Platform.Foundation.Accounts.Services
{
    public interface IContactFacetService
    {
        ContactFacetData GetContactData();

        void UpdateContactFacets(ContactFacetData data);

        string ExportContactData();

        bool DeleteContact();
    }
}
