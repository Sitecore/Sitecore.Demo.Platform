using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Init.Jobs
{
	class PublishItems : TaskBase
	{
		public PublishItems(InitContext initContext)
			: base(initContext)
		{
		}

		public async Task Run()
		{
			if (this.IsCompleted())
			{
				Log.LogWarning($"{this.GetType().Name} is already complete, it will not execute this time");
				return;
			}

			var ns = Environment.GetEnvironmentVariable("RELEASE_NAMESPACE");
			if (string.IsNullOrEmpty(ns))
			{
				Log.LogWarning(
					$"{this.GetType().Name} will not execute this time, RELEASE_NAMESPACE is not configured - this job is only required on AKS");
				return;
			}

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");
			Log.LogInformation($"PublishItems() started {hostCM}");

			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };
			client.Timeout = TimeSpan.FromMinutes(15);
			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/Publish.aspx"))
			{
				using (var response = await client.SendAsync(request))
				{
					var contents = await response.Content.ReadAsStringAsync();
					Log.LogInformation($"{response.StatusCode} {contents}");
					Log.LogInformation("PublishItems() complete");
				}
			}

			await Complete();
		}
	}
}
