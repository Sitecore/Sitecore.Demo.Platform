using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;

namespace Sitecore.HabitatHome.Feature.ExperienceAccelerator
{
	public struct Templates
	{
		public struct EventPage
		{
			public static readonly ID ID = new ID("{9B87D09A-3A73-4E0A-8EE8-C192719D256F}");
			public struct Fields
			{
				public static readonly ID Event = new ID("{10362885-CEDB-401B-AF25-1C0D847C8690}");
			}
		}

		public struct Event
		{
			public static readonly ID ID = new ID("{62C2B7D0-1C34-4AA6-B3D3-5D612C0891BD}");

			public struct Fields
			{
				public static readonly ID EventStart = new ID("{9E47D308-2E9C-413A-AC93-3C51C52E74D2}");
			}
		}
	}
}
