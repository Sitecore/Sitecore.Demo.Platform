using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Model;
using Sitecore.Demo.Init.Extensions;
using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Init.Jobs
{
	class UpdateFieldValues : TaskBase
	{
		public UpdateFieldValues(InitContext initContext)
			: base(initContext)
		{
		}

		public async Task Run()
		{
			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");
			var user = Environment.GetEnvironmentVariable("ADMIN_USER_NAME").Replace("sitecore\\", string.Empty);
			var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
			var damUrl = Environment.GetEnvironmentVariable("DAM_URL");

			Log.LogInformation($"UpdateFieldValues() started on {hostCM}");
			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/sitecore/api/ssc/auth/login")
			{
				Content = new StringContent($"{{\"domain\":\"sitecore\",\"username\":\"{user}\",\"password\":\"{password}\"}}", Encoding.UTF8, "application/json")
			};

			var response = await client.SendAsync(request);
			var contents = await response.Content.ReadAsStringAsync();
			var token = JsonConvert.DeserializeObject<SscLoginResponse>(contents).Token;

			UpdateValues(hostCM, damUrl, token);

			Log.LogInformation($"{response.StatusCode} {contents}");
			await Complete();
			Log.LogInformation("UpdateFieldValues() complete");
		}

		private static void UpdateValues(string hostCM, string damUrl, string token)
		{
			var client = new CookieWebClient();
			client.Encoding = System.Text.Encoding.UTF8;
			client.Headers.Add("token", token);

			client.Headers.Add("Content-Type", "application/json");
			client.UploadData(
				new Uri(hostCM + "/sitecore/api/ssc/item/15BE535E-1A49-4C91-A1CA-1DE14B35FF77?database=master"),
				"PATCH",
				System.Text.Encoding.UTF8.GetBytes($"{{\"DAMInstance\": \"{damUrl}\" }}"));

			client.Headers.Add("Content-Type", "application/json");
			client.UploadData(
				new Uri(hostCM + "/sitecore/api/ssc/item/15BE535E-1A49-4C91-A1CA-1DE14B35FF77?database=master"),
				"PATCH",
				System.Text.Encoding.UTF8.GetBytes(
					$"{{\"SearchPage\": \"{damUrl}/en-us/sitecore-dam-connect/approved-assets\" }}"));
		}
	}
}
