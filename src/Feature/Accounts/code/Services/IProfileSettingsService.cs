namespace Sitecore.Feature.Accounts.Services
{
    using Sitecore.Data.Items;
    using System.Collections.Generic;

    public interface IProfileSettingsService
    {
        IEnumerable<string> GetInterests();
        Item GetUserDefaultProfile();
    }
}