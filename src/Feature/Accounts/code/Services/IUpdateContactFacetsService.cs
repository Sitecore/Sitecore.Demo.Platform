using Sitecore.Security;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    public interface IUpdateContactFacetsService
    {
        void UpdateContactFacets(UserProfile profile);
    }
}