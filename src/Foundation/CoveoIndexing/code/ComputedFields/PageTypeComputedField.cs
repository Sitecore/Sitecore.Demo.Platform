using System;
using System.Xml;
using Coveo.Framework.Databases;
using Coveo.SearchProvider.ComputedFields;
using Newtonsoft.Json;
using Sitecore.ContentSearch;
using Sitecore.Demo.Platform.Foundation.CoveoIndexing.Models;
using Sitecore.Globalization;

namespace Sitecore.Demo.Platform.Foundation.CoveoIndexing.ComputedFields
{
	public class PageTypeComputedField : ConfigurableComputedField
	{
		[ThreadStatic]
		private const string MAPPINGS_ATTRIBUTE_NAME = "mappings";
		private const string DICTIONARY_DOMAIN_ATTRIBUTE_NAME = "dictionaryDomain";

		public new string ReturnType
		{
			get
			{
				return "string";
			}
			set
			{
			}
		}

		public PageTypeComputedField(XmlNode p_Configuration) : base(p_Configuration)
		{
		}

		public PageTypeComputedField(ISitecoreFactory p_SitecoreFactory) : base(p_SitecoreFactory)
		{
		}

		public override object ComputeFieldValue(IIndexable p_Indexable)
		{
			if (p_Indexable == null)
			{
				return null;
			}

			IIndexableBuiltinFields fields = (IIndexableBuiltinFields)p_Indexable;
			string currentPageTemplateId = fields.TemplateId.ToString().ToUpper();

			foreach (PageTypeMapping pageType in GetMappings().PageTypes)
			{
				foreach (string templateId in pageType.Templates)
				{
					if (templateId.ToUpper() == currentPageTemplateId)
					{
						string pageTypeName = pageType.Name;

						string dictionaryDomain = GetAttributeValue(DICTIONARY_DOMAIN_ATTRIBUTE_NAME);
						if (!string.IsNullOrEmpty(dictionaryDomain))
						{
							Language itemLanguage;
							if (Language.TryParse(p_Indexable.Culture.Name, out itemLanguage))
							{
								pageTypeName = Translate.TextByLanguage(dictionaryDomain, TranslateOptions.Default, pageTypeName, itemLanguage, pageTypeName);
							}
						}

						return pageTypeName;
					}
				}
			}

			return null;
		}

		private PageTypeMappings GetMappings()
		{
			string rawMappings = GetAttributeValue(MAPPINGS_ATTRIBUTE_NAME);
			return (PageTypeMappings)JsonConvert.DeserializeObject(rawMappings, typeof(PageTypeMappings));
		}
	}
}
