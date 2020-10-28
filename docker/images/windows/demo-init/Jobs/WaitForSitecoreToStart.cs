using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	class WaitForSitecoreToStart
	{
		public static async Task Run()
		{
			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");
			await WaitForTheInstanceToStart(hostCM, "/sitecore");
		}

		public static async Task RunCD()
		{
			var hostCD = Environment.GetEnvironmentVariable("HOST_CD");
			await WaitForTheInstanceToStart(hostCD, string.Empty);
		}

		private static async Task WaitForTheInstanceToStart(string baseAddress, string path)
		{
			Console.WriteLine($"WaitForSitecoreToStart() started {baseAddress}");
			using var client = new HttpClient { BaseAddress = new Uri(baseAddress) };
			var i = 0;

			while(true)
			{
				try
				{
					i++;
					Console.WriteLine($"WaitForSitecoreToStart ({baseAddress}) attempt #{i}");
					var request = new HttpRequestMessage(HttpMethod.Get, path);
					var response = await client.SendAsync(request);
					if (response.StatusCode == HttpStatusCode.OK)
					{
						Console.WriteLine($"{response.StatusCode} Sitecore is ready - {baseAddress}");
						break;
					}

				}
				catch
				{
					// Ignore exceptions during warmup
					Console.WriteLine($"{baseAddress} not ready yet, retrying...");
				}

				await Task.Delay(5000);
			}

			Console.WriteLine($"WaitForSitecoreToStart() complete - {baseAddress}");
		}
	}
}
