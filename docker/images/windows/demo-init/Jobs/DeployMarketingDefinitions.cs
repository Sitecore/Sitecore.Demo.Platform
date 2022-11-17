using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	using Microsoft.Extensions.Logging;

	class DeployMarketingDefinitions : TaskBase
	{
		public DeployMarketingDefinitions(InitContext initContext)
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
			var marketingDefinitionsApikey = Environment.GetEnvironmentVariable("MARKETING_DEFINITIONS_APIKEY");

			Log.LogInformation($"DeployMarketingDefinitions() started {hostCM}");

			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/DeployMarketingDefinitions.aspx?apiKey={marketingDefinitionsApikey}"))
			{
				using (var response = await client.SendAsync(request))
				{
					var contents = await response.Content.ReadAsStringAsync();
					Log.LogInformation($"{response.StatusCode} {contents}");
					Log.LogInformation("DeployMarketingDefinitions() complete");
				}
			}
		}
	}
}
