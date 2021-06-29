using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	using Microsoft.Extensions.Logging;

	class RestartCD : TaskBase
	{
		public RestartCD(InitContext initContext)
			: base(initContext)
		{
		}

		public async Task Run()
		{
			var skipWarmupCd = Convert.ToBoolean(Environment.GetEnvironmentVariable("SKIP_WARMUP_CD"));

			if (skipWarmupCd)
			{
				Log.LogInformation($"{DateTime.UtcNow} SKIP_WARMUP_CD set to true. Skipping Restart CD as well");
				return;
			}

			if (this.IsCompleted())
			{
				Log.LogWarning($"{this.GetType().Name} is already complete, it will not execute this time");
				return;
			}

			var host = Environment.GetEnvironmentVariable("HOST_CD");

			using var client = new HttpClient { BaseAddress = new Uri(host) };
			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/Restart.aspx"))
			{
				using (var response = await client.SendAsync(request))
				{
					Log.LogInformation($"RestartSitecore() {host} started");
					var contents = await response.Content.ReadAsStringAsync();
					Log.LogInformation($"{response.StatusCode} {contents}");
					Log.LogInformation($"RestartSitecore() {host} complete");
				}
			}

			await Complete();
		}
	}
}
