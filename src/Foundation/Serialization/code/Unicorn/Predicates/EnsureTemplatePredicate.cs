using System;
using System.Collections.Generic;
using Unicorn.Predicates;
using System.Xml;
using Rainbow.Model;
using Unicorn.Configuration;
using Unicorn.Data.DataProvider;
using System.Linq;
using Sitecore.StringExtensions;

namespace Sitecore.HabitatHome.Foundation.Serialization.Unicorn.Predicates
{
    public class EnsureTemplatePredicate : SerializationPresetPredicate
    {
        public EnsureTemplatePredicate(XmlNode configNode, IUnicornDataProviderConfiguration dataProviderConfiguration, IConfiguration configuration) : base(configNode, dataProviderConfiguration, configuration)
        {
        }

        protected PredicateResult Includes(CustomPresetTreeRoot entry, IItemData itemData)
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

            // check if template items exist for those defined on the include node in serialization files
            var ensureTemplateItem = Sitecore.Configuration.Factory.GetDatabase(itemData.DatabaseName).GetItem(entry.EnsureTemplate.ToString());
            if (ensureTemplateItem == null)
            {
                return new PredicateResult(false);
            }

            // check excludes
            return ExcludeMatches(entry, itemData);
        }

        protected new CustomPresetTreeRoot CreateIncludeEntry(XmlNode configuration)
        {
            string database = GetExpectedAttribute(configuration, "database");
            string path = GetExpectedAttribute(configuration, "path");
            string ensureTemplate = GetExpectedAttribute(configuration, "ensureTemplate");

            // ReSharper disable once PossibleNullReferenceException
            var name = configuration.Attributes["name"];
            string nameValue = name == null ? path.Substring(path.LastIndexOf('/') + 1) : name.Value;

            var root = new CustomPresetTreeRoot(nameValue, path, database, ensureTemplate);

            root.Exclusions = configuration.ChildNodes
                .OfType<XmlElement>()
                .Where(element => element.Name.Equals("exclude"))
                .Select(excludeNode => CreateExcludeEntry(excludeNode, root))
                .ToList();

            return root;
        }

        private static string GetExpectedAttribute(XmlNode node, string attributeName)
        {
            // ReSharper disable once PossibleNullReferenceException
            var attribute = node.Attributes[attributeName];

            if (attribute == null) throw new InvalidOperationException("Missing expected '{0}' attribute on '{1}' node while processing predicate: {2}".FormatWith(attributeName, node.Name, node.OuterXml));

            return attribute.Value;
        }
    }
}