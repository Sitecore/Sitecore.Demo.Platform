using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	class WaitForPublishingServiceToStart : TaskBase
	{
		public static async Task Run()
		{
			await Start(typeof(WaitForPublishingServiceToStart).Name);

			var hostPS = Environment.GetEnvironmentVariable("HOST_PS");

			Console.WriteLine($"WaitForPublishingServiceToStart() started on {hostPS}");
			using var client = new HttpClient { BaseAddress = new Uri(hostPS) };
			var i = 0;

			while(true)
			{
				try
				{
					i++;
					Console.WriteLine($"WaitForPublishingServiceToStart attempt #{i}");
					var request = new HttpRequestMessage(HttpMethod.Get, "/api/publishing/jobqueue");
					var response = await client.SendAsync(request);
					if (response.StatusCode == HttpStatusCode.OK)
					{
						Console.WriteLine($"{response.StatusCode} Publishing Service is ready");
						break;
					}
				}
				catch
				{
					// Ignore exceptions during warmup
					Console.WriteLine("PS not ready yet, retrying...");
				}
				await Task.Delay(5000);
			}

			await Stop(typeof(WaitForPublishingServiceToStart).Name);

			Console.WriteLine("WaitForPublishingServiceToStart() complete");
		}
	}
}
