using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Model;
using Sitecore.Demo.Init.Services;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Init.Jobs
{
	public class WarmupCM : WarmupBase
	{
		private readonly WaitForSitecoreToStart waitForSitecoreToStart;

		public WarmupCM(InitContext initContext)
			: base(initContext)
		{
			waitForSitecoreToStart = new WaitForSitecoreToStart(initContext);
		}

		public async Task Run()
		{
			try
			{
				var content = File.ReadAllText("data/warmup-config.json");
				var config = JsonConvert.DeserializeObject<WarmupConfig>(content);

				Log.LogInformation($"{DateTime.UtcNow} Warmup CM started");
				await waitForSitecoreToStart.Run();

				var cm = Environment.GetEnvironmentVariable("HOST_CM");
				var id = Environment.GetEnvironmentVariable("HOST_ID");
				var user = Environment.GetEnvironmentVariable("ADMIN_USER_NAME");
				var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

				Log.LogInformation($"Warmup CM - URL: {cm}");
				Log.LogInformation($"Warmup CM - ID: {id}");
				Log.LogInformation($"Warmup CM - User: {user}");
				Log.LogInformation($"Warmup CM - Password: {password}");

				var client = new HttpClient();
				client.Timeout = TimeSpan.FromMinutes(10);
				Task.WaitAll(WarmupBackend(cm, id, user, password, config), WarmupFrontend(config, client, cm));

				await Complete();

				Log.LogInformation($"{DateTime.UtcNow} Warmup CM complete");
			}
			catch (Exception ex)
			{
				Log.LogError(ex, "Warmup CM failed");
			}
		}

		private async Task WarmupFrontend(WarmupConfig config, HttpClient client, string cm)
		{
			foreach (var entry in config.urls[1].xp)
			{
				await LoadUrl(cm, entry.url, client);
			}
		}

		private async Task WarmupBackend(string cm, string id, string user, string password, WarmupConfig config)
		{
			var authenticatedClient = await new SitecoreLoginService(Log).GetSitecoreClient(cm, id, user, password);
			foreach (var entry in config.urls[0].sitecore)
			{
				await LoadUrl(cm, entry.url, authenticatedClient);
			}
		}
	}
}
