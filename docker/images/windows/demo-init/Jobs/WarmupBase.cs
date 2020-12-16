namespace Sitecore.Demo.Init.Jobs
{
	using System;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Microsoft.Extensions.Logging;

	public class WarmupBase: TaskBase
	{
		public WarmupBase(InitContext initContext)
			: base(initContext)
		{
		}

		protected async Task LoadUrl(string baseUrl, string path, WebClient client)
		{
			for (int i = 0; i < 10; i++)
			{
				try
				{
					Log.LogInformation($"{DateTime.UtcNow} Loading {baseUrl}{path}");
					await client.DownloadStringTaskAsync($"{baseUrl}/{path}");
					return;
				}
				catch (Exception ex)
				{
					// Ignore exceptions during warmup
					Log.LogInformation($"{DateTime.UtcNow} Failed to load {baseUrl}{path}: \r\n {ex}");
				}

				await Task.Delay(1000);
			}
		}

		protected async Task LoadUrl(string baseUrl, string path, HttpClient client)
		{
			try
			{
				Log.LogInformation($"{DateTime.UtcNow} Loading {baseUrl}{path}");
				await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"{baseUrl}{path}"));
			}
			catch (Exception ex)
			{
				// Ignore exceptions during warmup
				Log.LogInformation($"{DateTime.UtcNow} Failed to load {baseUrl}{path}: \r\n {ex}");
			}
		}
	}
}
