using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Model;

namespace Sitecore.Demo.Init.Jobs
{
	using Microsoft.Extensions.Logging;

	class PublishItems : TaskBase
	{
		public static async Task Run()
		{
			await Start(typeof(PublishItems).Name);
			await WaitForPublishingServiceToStart.Run();

			var hostPS = Environment.GetEnvironmentVariable("HOST_PS");

			Log.LogInformation($"PublishItems() started on {hostPS}");
			using var client = new HttpClient { BaseAddress = new Uri(hostPS) };
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var content = File.ReadAllText("data/publishingservice.json");
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "/api/publishing/jobqueue")
			{
				Content = new StringContent(content, Encoding.UTF8, "application/json")
			};

			var response = await client.SendAsync(request);
			var contents = await response.Content.ReadAsStringAsync();

			var jobId = JsonConvert.DeserializeObject<PublishJobResponse>(contents).value;
			await WaitForPublishToComplete(client, jobId);

			Log.LogInformation($"{response.StatusCode} {contents}");

			await Stop(typeof(PublishItems).Name);

			Log.LogInformation("PublishItems() complete");
		}

		static async Task WaitForPublishToComplete(HttpClient client, string jobId)
		{
			for (int i = 0; i < 50; i++)
			{
				try
				{
					var request = new HttpRequestMessage(HttpMethod.Get, "/api/publishing/jobs/" + jobId);
					var response = await client.SendAsync(request);
					var contents = await response.Content.ReadAsStringAsync();
					var status = JsonConvert.DeserializeObject<PublishJobStatus>(contents);
					if (status.status <= 1)
					{
						Log.LogInformation($"#{i} {DateTime.UtcNow} Publishing is in progress...");
					}
					else
					{
						Log.LogInformation($"Publishing completed with the status: {status.statusMessage}");
						break;
					}
				}
				catch(Exception)
				{
					// Ignore exceptions during warmup
					Log.LogInformation($"Publishing is not ready yet, retrying...");
					break;
				}

				await Task.Delay(10000);
			}
		}
	}
}
