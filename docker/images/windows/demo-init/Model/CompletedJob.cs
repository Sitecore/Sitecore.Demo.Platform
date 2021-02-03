using System;

namespace Sitecore.Demo.Init.Model
{
	public class CompletedJob
	{
		public CompletedJob(string name, string settingsHash)
		{
			Name = name;
			SettingsHash = settingsHash;
			CompletedDate = DateTime.UtcNow;
		}

		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime CompletedDate { get; set; }
		public string SettingsHash { get; set; }
	}
}
