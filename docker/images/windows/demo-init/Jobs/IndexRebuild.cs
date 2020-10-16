using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	class IndexRebuild : TaskBase
	{
		public static async Task Run()
		{

			await Start(typeof(IndexRebuild).Name);

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");

			Console.WriteLine($"IndexRebuild() started {hostCM}");

			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };
			var indexes = new[] { "sitecore_sxa_web_index", "sitecore_sxa_master_index", "sitecore_master_index", "sitecore_web_index", "sitecore_marketingdefinitions_master", "sitecore_marketingdefinitions_web", "sitecore_testing_index" };

			foreach (var index in indexes)
			{
				using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/IndexRebuild.aspx?index={index}"))
				{
					using (var response = await client.SendAsync(request))
					{
						var contents = await response.Content.ReadAsStringAsync();
						Console.WriteLine($"Rebuilding index {index}");
						Console.WriteLine($"{response.StatusCode}");
					}
				}
			}
		}
	}
}
