using System.Collections.Generic;
using Sitecore.Data.Items;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    public interface IProfileSettingsService
    {
        IEnumerable<string> GetInterests();
        Item GetUserDefaultProfile();
    }
}