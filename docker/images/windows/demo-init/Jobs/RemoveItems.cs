using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Extensions;
using Sitecore.Demo.Init.Model;

namespace Sitecore.Demo.Init.Jobs
{
	class RemoveItems : TaskBase
	{
		public RemoveItems(InitContext initContext)
			: base(initContext)
		{
		}

		public string[] itemsToRemoveArray = new string[]
		{
			"73D4FFF4-6D9E-40E4-8B84-A495A838E0E8", //sitecore/layout/Simulators/Feature Phone
			"69B47A34-D529-4245-90C0-0858C66582BB", //sitecore/layout/Simulators/HD TV
			"E1DC505A-F86F-4C05-B409-AE2246AD3441", //sitecore/layout/Simulators/Blackberry
			"618DA108-0F8B-44D6-9384-C3E0B29B8878"  //sitecore/layout/Simulators/Windows Phone
		};

		public async Task Run()
		{
			if (this.IsCompleted())
			{
				Log.LogWarning($"{this.GetType().Name} is already complete, it will not execute this time");
				return;
			}

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");
			var user = Environment.GetEnvironmentVariable("ADMIN_USER_NAME").Replace("sitecore\\", string.Empty);
			var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

			Log.LogInformation($"RemoveItems() started on {hostCM}");
			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/sitecore/api/ssc/auth/login")
			{
				Content = new StringContent($"{{\"domain\":\"sitecore\",\"username\":\"{user}\",\"password\":\"{password}\"}}", Encoding.UTF8, "application/json")
			};

			var response = await client.SendAsync(request);
			var contents = await response.Content.ReadAsStringAsync();
			Log.LogInformation($"{response.StatusCode} {contents}");
			var token = JsonConvert.DeserializeObject<SscLoginResponse>(contents).Token;

			DeleteItems(hostCM, token);

			await Complete();
			Log.LogInformation("RemoveItems() complete");
		}

		private void DeleteItems(string hostCM, string token)
		{
			var client = new CookieWebClient();
			client.Encoding = System.Text.Encoding.UTF8;
			client.Headers.Add("token", token);
			client.Headers.Add("Content-Type", "application/json");

			foreach (var itemToRemove in itemsToRemoveArray)
			{
				try
				{
					client.UploadData(
						new Uri(hostCM + $"/sitecore/api/ssc/item/{itemToRemove}?database=master"),
						"DELETE",
						new byte[0]
					);
					Log.LogInformation("RemoveItems() succesfully removed an item. Item ID: " + itemToRemove);
				}
				catch
				{
					Log.LogInformation("RemoveItems() failed with a non-successful response code. The item may have already been removed. Item ID: " + itemToRemove);
				}
			}
		}
	}
}
