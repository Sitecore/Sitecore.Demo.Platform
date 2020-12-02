using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Demo.Init.Extensions;

namespace Sitecore.Demo.Init.Jobs
{
	public class TaskBase
	{
		private static readonly string StatusDirectory = Path.Combine(Directory.GetCurrentDirectory(), "status");

		protected static ILogger Log = ApplicationLogging.CreateLogger<TaskBase>();

		protected static async Task Start(string theType)
		{
			if (!Directory.Exists(StatusDirectory))
			{
				Directory.CreateDirectory(StatusDirectory);
			}

			await File.WriteAllTextAsync(Path.Combine(StatusDirectory, $"{theType}.Started"), "Started");
		}

		protected static async Task Stop(string theType)
		{
			await File.WriteAllTextAsync(Path.Combine(StatusDirectory, $"{theType}.Ready"), "Ready");
		}
	}
}
