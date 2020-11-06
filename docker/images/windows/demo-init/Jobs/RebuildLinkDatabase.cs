using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	class RebuildLinkDatabase : TaskBase
	{
		public static async Task Run()
		{
			await Start(typeof(RebuildLinkDatabase).Name);
			await WaitForSitecoreToStart.Run();
			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");
			Console.WriteLine($"RebuildLinkDatabase() started {hostCM}");

			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };
			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/Utilities/RebuildLinkDatabase.aspx"))
			{
				using (var response = await client.SendAsync(request))
				{
					var contents = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"{response.StatusCode} {contents}");
					Console.WriteLine("RebuildLinkDatabase() complete");
				}
			}
		}
	}
}
