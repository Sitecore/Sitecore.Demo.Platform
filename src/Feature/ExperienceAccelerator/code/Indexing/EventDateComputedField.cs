using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Sitecore.HabitatHome.Feature.ExperienceAccelerator.Indexing
{
	public class EventDateComputedField : IComputedIndexField
	{
		public string FieldName { get; set; }

		public string ReturnType { get; set; }

		public object ComputeFieldValue(IIndexable indexable)
		{
			var indexItem = indexable as SitecoreIndexableItem;

			if (indexItem?.Item == null)
			{
				return null;
			}

			string date = null;

			Item item = indexItem.Item;

			if (Context.Site == null)
			{
				Context.SetActiveSite("shell");
			}

			if (item.TemplateID.Equals(Templates.EventPage.ID))
			{
				Sitecore.Data.Database master =
				Sitecore.Configuration.Factory.GetDatabase("master");
				Item eventItem = master.GetItem(item.Fields[Templates.EventPage.Fields.Event].Value);
				date = eventItem.Fields[Templates.Event.Fields.EventStart].Value;
			}

			return date;
		}
	}
}
