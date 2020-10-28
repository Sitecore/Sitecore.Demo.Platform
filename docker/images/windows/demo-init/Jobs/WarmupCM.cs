using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Model;
using Sitecore.Demo.Init.Services;
using System.Net.Http;

namespace Sitecore.Demo.Init.Jobs
{
	public class WarmupCM : WarmupBase
	{
		public static async Task Run()
		{
			try
			{
				await Start(typeof(WarmupCM).Name);

				var content = File.ReadAllText("data/warmup-config.json");
				var config = JsonConvert.DeserializeObject<WarmupConfig>(content);

				Console.WriteLine($"{DateTime.UtcNow} Warmup CM started");
				await WaitForSitecoreToStart.Run();

				var cm = Environment.GetEnvironmentVariable("HOST_CM");
				var id = Environment.GetEnvironmentVariable("HOST_ID");
				var user = Environment.GetEnvironmentVariable("ADMIN_USER_NAME");
				var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

				Console.WriteLine($"Warmup CM - URL: {cm}");
				Console.WriteLine($"Warmup CM - ID: {id}");
				Console.WriteLine($"Warmup CM - User: {user}");
				Console.WriteLine($"Warmup CM - Password: {password}");

				var client = new HttpClient();
				client.Timeout = TimeSpan.FromMinutes(10);
				Task.WaitAll(WarmupBackend(cm, id, user, password, config), WarmupFrontend(config, client, cm));

				await Stop(typeof(WarmupCM).Name);

				Console.WriteLine($"{DateTime.UtcNow} Warmup CM complete");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Warmup CM failed. " + ex);
			}
		}

		private static async Task WarmupFrontend(WarmupConfig config, HttpClient client, string cm)
		{
			foreach (var entry in config.urls[1].xp)
			{
				await LoadUrl(cm, entry.url, client);
			}
		}

		private static async Task WarmupBackend(string cm, string id, string user, string password, WarmupConfig config)
		{
			var authenticatedClient = await new SitecoreLoginService().GetSitecoreClient(cm, id, user, password);
			foreach (var entry in config.urls[0].sitecore)
			{
				await LoadUrl(cm, entry.url, authenticatedClient);
			}
		}
	}
}
