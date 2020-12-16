using System;

namespace Sitecore.Demo.Init.Model
{
	public class CompletedJob
	{
		public CompletedJob(string name)
		{
			Name = name;
			CompletedDate = DateTime.UtcNow;
		}

		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime CompletedDate { get; set; }
	}
}
