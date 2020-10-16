using System;
using System.Collections.Generic;

namespace Sitecore.Demo.Init.Model
{
	class PublishJobStatus
	{
		public string id { get; set; }

		public int status { get; set; }

		public string statusMessage { get; set; }

		public IList<string> manifests { get; set; }

		public DateTime? queued { get; set; }

		public DateTime? started { get; set; }

		public DateTime? stopped { get; set; }

		public int affectedItems { get; set; }
	}
}
