using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	class DeployMarketingDefinitions : TaskBase
	{
		public static async Task Run()
		{
			await Start(typeof(DeployMarketingDefinitions).Name);

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");
			var marketingDefinitionsApikey = Environment.GetEnvironmentVariable("MARKETING_DEFINITIONS_APIKEY");

			Console.WriteLine($"DeployMarketingDefinitions() started {hostCM}");

			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };

			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/DeployMarketingDefinitions.aspx?apiKey={marketingDefinitionsApikey}"))
			{
				using (var response = await client.SendAsync(request))
				{
					var contents = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"{response.StatusCode} {contents}");
					Console.WriteLine("DeployMarketingDefinitions() complete");
				}
			}
		}
	}
}
