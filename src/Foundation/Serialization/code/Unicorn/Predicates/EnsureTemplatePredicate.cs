using System;
using System.Collections.Generic;
using Unicorn.Predicates;
using System.Xml;
using Rainbow.Model;
using Unicorn.Configuration;
using Unicorn.Data.DataProvider;

namespace Sitecore.HabitatHome.Foundation.Serialization.Unicorn.Predicates
{
    public class EnsureTemplatePredicate : SerializationPresetPredicate
    {
        public EnsureTemplatePredicate(XmlNode configNode, IUnicornDataProviderConfiguration dataProviderConfiguration, IConfiguration configuration) : base(configNode, dataProviderConfiguration, configuration)
        {
        }

        protected new PredicateResult Includes(PresetTreeRoot entry, IItemData itemData)
        {
            // check for db match
            if (!itemData.DatabaseName.Equals(entry.DatabaseName, StringComparison.OrdinalIgnoreCase)) return new PredicateResult(false);

            // check for path match
            var unescapedPath = entry.Path.Replace(@"\*", "*");
            if (!itemData.Path.StartsWith(unescapedPath + "/", StringComparison.OrdinalIgnoreCase) && !itemData.Path.Equals(unescapedPath, StringComparison.OrdinalIgnoreCase))
            {
                return new PredicateResult(false);
            }

            // check that the item's template exists
            var templateId = itemData.TemplateId;
            var templateItem = Sitecore.Configuration.Factory.GetDatabase(itemData.DatabaseName).GetItem(templateId.ToString());
            if (templateItem == null)
            {
                return new PredicateResult(false);
            }

            // check excludes
            return ExcludeMatches(entry, itemData);
        }
    }
}