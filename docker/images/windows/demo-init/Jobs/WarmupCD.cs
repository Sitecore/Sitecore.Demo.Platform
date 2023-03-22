using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Model;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Init.Jobs
{
	public class WarmupCD : WarmupBase
	{
		private readonly WaitForSitecoreToStart waitForSitecoreToStart;

		public WarmupCD(InitContext initContext)
			: base(initContext)
		{
			waitForSitecoreToStart = new WaitForSitecoreToStart(initContext);
		}

		public async Task Run()
		{
			try
			{
				var watch = System.Diagnostics.Stopwatch.StartNew();
				var skipWarmupCd = Convert.ToBoolean(Environment.GetEnvironmentVariable("SKIP_WARMUP_CD"));

				if (skipWarmupCd)
				{
					Log.LogInformation($"{DateTime.UtcNow} SKIP_WARMUP_CD set to true. Skipping Warmup CD");
					return;
				}

				var content = File.ReadAllText("data/warmup-config.json");
				var config = JsonConvert.DeserializeObject<WarmupConfig>(content);

				Log.LogInformation($"{DateTime.UtcNow} Warmup CD started");
				await waitForSitecoreToStart.RunCD();

				var cd = Environment.GetEnvironmentVariable("HOST_CD");
				Log.LogInformation($"Warmup CD - URL: {cd}");

				var client = new HttpClient();
				client.Timeout = TimeSpan.FromMinutes(10);
				foreach (var entry in config.xp)
				{
					await LoadUrl(cd, entry.url, client);
				}

				await Complete();

				watch.Stop();
				Log.LogInformation($"{DateTime.UtcNow} Warmup CD complete. Elapsed: {watch.Elapsed:m\\:ss}");
			}
			catch(Exception ex)
			{
				Log.LogError(ex, "Warmup CD failed");
			}
		}
	}
}
