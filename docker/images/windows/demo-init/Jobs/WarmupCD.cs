using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Model;
using System.Net.Http;

namespace Sitecore.Demo.Init.Jobs
{
	public class WarmupCD : WarmupBase
	{
		public static async Task Run()
		{
			try
			{
				var skipWarmupCd = Convert.ToBoolean(Environment.GetEnvironmentVariable("SKIP_WARMUP_CD"));

				if (skipWarmupCd)
				{
					Console.WriteLine($"{DateTime.UtcNow} SKIP_WARMUP_CD set to true. Skipping Warmup CD");
					return;
				}

				await Start(typeof(WarmupCD).Name);

				var content = File.ReadAllText("data/warmup-config.json");
				var config = JsonConvert.DeserializeObject<WarmupConfig>(content);

				Console.WriteLine($"{DateTime.UtcNow} Warmup CD started");
				await WaitForSitecoreToStart.RunCD();

				var cd = Environment.GetEnvironmentVariable("HOST_CD");
				Console.WriteLine($"Warmup CD - URL: {cd}");

				var client = new HttpClient();
				client.Timeout = TimeSpan.FromMinutes(10);
				foreach (var entry in config.urls[1].xp)
				{
					await LoadUrl(cd, entry.url, client);
				}

				await Stop(typeof(WarmupCD).Name);

				Console.WriteLine($"{DateTime.UtcNow} Warmup CD complete");
			}
			catch(Exception ex)
			{
				Console.WriteLine($"Warmup CD failed. " + ex);
			}
		}
	}
}
