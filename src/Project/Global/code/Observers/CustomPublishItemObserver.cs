using System;
using System.Collections.Generic;
using System.Globalization;
using Sitecore.Data;
using Sitecore.Framework.Conditions;
using Sitecore.Marketing;
using Sitecore.Marketing.Definitions;
using Sitecore.Marketing.xMgmt.Observers;
using Sitecore.Marketing.xMgmt.Observers.Activation;

namespace Sitecore.Demo.Website.Observers
{
    public class CustomPublishItemObserver<TDefinition> : PublishItemObserver<TDefinition>
        where TDefinition : IDefinition
    {
        public CustomPublishItemObserver(
            IPublishTargetResolver publishResolver,
            IItemRepositoriesSettings settings)
            : this(
                Condition.Requires<IPublishTargetResolver>(publishResolver).IsNotNull<IPublishTargetResolver>().Value,
                Condition.Requires<IItemRepositoriesSettings>(settings, nameof(settings))
                    .IsNotNull<IItemRepositoriesSettings>().Value.Database)
        {
        }

        public CustomPublishItemObserver(
            IPublishTargetResolver publishResolver,
            string authoringItemRepositoriesDatabaseName)
            : this(
                Condition.Requires<IPublishTargetResolver>(publishResolver).IsNotNull<IPublishTargetResolver>().Value,
                Condition.Requires<Database>(Database.GetDatabase(authoringItemRepositoriesDatabaseName),
                    "authoringItemRepositoriesDatabaseName != null").IsNotNull<Database>().Value)
        {
        }

        public CustomPublishItemObserver(
            IPublishTargetResolver publishResolver,
            Database authoringItemRepositoriesDatabase)
            : base(publishResolver, authoringItemRepositoriesDatabase)
        {
        }

        protected override void PublishItemsByIds(
            Database[] targetDatabases,
            Dictionary<Guid, List<CultureInfo>> itemIdsToPublish)
        {
            // Customization: disables auto-publishing for analytics items when DeployMarketingDefinitions is running
            if (Configuration.Factory.GetDatabase("core").PropertyStore.GetBoolValue("DisablePublishItemObserver", false))
            {
                return;
            }

            base.PublishItemsByIds(targetDatabases, itemIdsToPublish);
        }
    }
}