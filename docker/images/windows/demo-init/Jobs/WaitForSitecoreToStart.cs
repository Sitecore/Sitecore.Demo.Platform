using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Init.Jobs
{
	class WaitForSitecoreToStart : TaskBase
	{
		public WaitForSitecoreToStart(InitContext initContext)
			: base(initContext)
		{
		}

		public async Task Run()
		{
			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");
			await WaitForTheInstanceToStart(hostCM, "/sitecore");
		}

		public async Task RunCD()
		{
			var hostCD = Environment.GetEnvironmentVariable("HOST_CD");
			await WaitForTheInstanceToStart(hostCD, string.Empty);
		}

		private async Task WaitForTheInstanceToStart(string baseAddress, string path)
		{
			Log.LogInformation($"WaitForSitecoreToStart() started {baseAddress}");
			using var client = new HttpClient { BaseAddress = new Uri(baseAddress) };
			var i = 0;

			while(true)
			{
				try
				{
					i++;
					Log.LogInformation($"WaitForSitecoreToStart ({baseAddress}) attempt #{i}");
					var request = new HttpRequestMessage(HttpMethod.Get, path);
					var response = await client.SendAsync(request);
					if (response.StatusCode == HttpStatusCode.OK)
					{
						Log.LogInformation($"{response.StatusCode} Sitecore is ready - {baseAddress}");
						break;
					}

				}
				catch
				{
					// Ignore exceptions during warmup
					Log.LogInformation($"{baseAddress} not ready yet, retrying...");
				}

				await Task.Delay(10000);
			}

			Log.LogInformation($"WaitForSitecoreToStart() complete - {baseAddress}");
		}
	}
}
