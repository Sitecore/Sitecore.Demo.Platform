using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	using Microsoft.Extensions.Logging;

	class DeployMarketingDefinitions : TaskBase
	{
		public static async Task Run()
		{
			await Start(typeof(DeployMarketingDefinitions).Name);

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
