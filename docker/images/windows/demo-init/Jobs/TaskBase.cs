using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	public class TaskBase
	{
		private readonly static string statusDirectory = Path.Combine(Directory.GetCurrentDirectory(), "status");

		protected static async Task Start(string theType)
		{
			
			if (!Directory.Exists(statusDirectory))
			{
				Directory.CreateDirectory(statusDirectory);
			}
			await File.WriteAllTextAsync(Path.Combine(statusDirectory, $"{theType}.Started"), "Started");

		}
		protected static async Task Stop(string theType)
		{
			await File.WriteAllTextAsync(Path.Combine(statusDirectory, $"{theType}.Ready"), "Ready");
		}
	}
}
