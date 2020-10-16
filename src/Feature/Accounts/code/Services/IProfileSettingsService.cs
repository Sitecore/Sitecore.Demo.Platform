using System.Collections.Generic;
using Sitecore.Data.Items;

namespace Sitecore.Demo.Platform.Feature.Accounts.Services
{
    public interface IProfileSettingsService
    {
        IEnumerable<string> GetInterests();
        string GetUserDefaultProfileId();
        Item GetUserDefaultProfile();
    }
}