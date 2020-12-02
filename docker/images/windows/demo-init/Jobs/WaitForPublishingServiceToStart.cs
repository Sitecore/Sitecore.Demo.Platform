using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	using Microsoft.Extensions.Logging;

	class WaitForPublishingServiceToStart : TaskBase
	{
		public static async Task Run()
		{
			await Start(typeof(WaitForPublishingServiceToStart).Name);

			var hostPS = Environment.GetEnvironmentVariable("HOST_PS");

			Log.LogInformation($"WaitForPublishingServiceToStart() started on {hostPS}");
			using var client = new HttpClient { BaseAddress = new Uri(hostPS) };
			var i = 0;

			while(true)
			{
				try
				{
					i++;
					Log.LogInformation($"WaitForPublishingServiceToStart attempt #{i}");
					var request = new HttpRequestMessage(HttpMethod.Get, "/api/publishing/jobqueue");
					var response = await client.SendAsync(request);
					if (response.StatusCode == HttpStatusCode.OK)
					{
						Log.LogInformation($"{response.StatusCode} Publishing Service is ready");
						break;
					}
				}
				catch
				{
					// Ignore exceptions during warmup
					Log.LogInformation("PS not ready yet, retrying...");
				}
				await Task.Delay(5000);
			}

			await Stop(typeof(WaitForPublishingServiceToStart).Name);

			Log.LogInformation("WaitForPublishingServiceToStart() complete");
		}
	}
}
