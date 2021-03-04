using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	using Microsoft.Extensions.Logging;

	class RestartSitecore : TaskBase
	{
		public RestartSitecore(InitContext initContext)
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

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");
			var hostCD = Environment.GetEnvironmentVariable("HOST_CD");

			await Task.WhenAll(Restart(hostCM), Restart(hostCD));
			await Complete();
		}

		private async Task Restart(string host)
		{
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
		}
	}
}
