using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	using Microsoft.Extensions.Logging;

	class IndexRebuild : TaskBase
	{
		public static async Task Run()
		{
			await Start(typeof(IndexRebuild).Name);

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");

			Log.LogInformation($"IndexRebuild() started {hostCM}");

			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };
			var indexes = new[] { "sitecore_sxa_web_index", "sitecore_sxa_master_index", "sitecore_master_index", "sitecore_web_index", "sitecore_marketingdefinitions_master", "sitecore_marketingdefinitions_web", "sitecore_testing_index" };

			foreach (var index in indexes)
			{
				using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/IndexRebuild.aspx?index={index}"))
				{
					using (var response = await client.SendAsync(request))
					{
						Log.LogInformation($"Rebuilding index {index}");
						Log.LogInformation($"{response.StatusCode}");
					}
				}
			}
		}
	}
}
