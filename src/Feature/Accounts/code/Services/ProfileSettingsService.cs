using System.Collections.Generic;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.HabitatHome.Foundation.DependencyInjection;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Extensions;
using Sitecore.SecurityModel;

namespace Sitecore.HabitatHome.Feature.Accounts.Services
{
    [Service(typeof(IProfileSettingsService))]
    public class ProfileSettingsService : IProfileSettingsService
    {
        public virtual Item GetUserDefaultProfile()
        {
            using (new SecurityDisabler())
            {
                Item item = GetSettingsItem();
                Assert.IsNotNull(item, "Page with profile settings isn't specified");
                var database = Database.GetDatabase(Settings.ProfileItemDatabase);
                var profileField = item.Fields[Templates.ProfileSettigs.Fields.UserProfile];
                var targetItem = database.GetItem(profileField.Value);

                return targetItem;
            }
        }

        public virtual IEnumerable<string> GetInterests()
        {
            var item = GetSettingsItem();

            return item?.TargetItem(Templates.ProfileSettigs.Fields.InterestsFolder)?.Children.Where(i => i.IsDerived(Templates.Interest.ID))?.Select(i => i.Fields[Templates.Interest.Fields.Title].Value) ?? Enumerable.Empty<string>();
        }

        private static Item GetSettingsItem()
        {
            return Context.Site.GetSettingsItem()?.Children.FirstOrDefault(x => x.IsDerived(Templates.ProfileSettigs.ID));
        }
    }
}