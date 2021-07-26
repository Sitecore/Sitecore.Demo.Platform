using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Model;
using Sitecore.Demo.Init.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Sitecore.Demo.Init.Jobs
{
	class DisableFallback : TaskBase
	{
		public DisableFallback(InitContext initContext)
			: base(initContext)
		{
		}

		public static List<string> itemIdList = new List<string>{
			// Email Messages
			"{4113C8FB-B6DD-44B2-BBE9-B89A6A86A91F}",
			"{BD9DB6A7-D129-4B8B-BCF9-568A03F4BE31}",
			"{412E7D1D-D9BC-4C2D-9793-9FF55F3E5174}",
			"{A1F62514-B183-4A76-8DEC-091694E525B4}",
			"{DC5A3C9B-5016-43C5-9AFB-7F71FB236922}",
			"{60CAFB1C-44C0-4ABA-A99E-47A6957A7B85}",
			"{1866755F-E69A-4A3E-9D00-1A36120A822D}",
			"{E44B686C-F73C-4EE4-8B57-55BD6F4BF21D}",
			"{18FB7066-2447-4524-8B9C-605F8ECED6DB}",
			"{5CAB521B-E457-4981-9C13-DD59FA16F5F0}",
			"{0A923A3B-08E3-44EE-B819-CDA5B39EFDF0}",
			"{6681343E-AC4C-4F6C-9A17-A179FFF2D466}",
			"{42F5109F-42B7-4E67-904D-E5858DB1D48D}",
			"{FFFED4DA-E1BC-4610-98F8-E27C7147628E}",
			"{D316B553-1D15-4192-BAB6-B4369231361D}"
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

			Log.LogInformation($"DisableFallback() started on {hostCM}");
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

		  	UpdateValues(hostCM, token);

		  	Log.LogInformation("DisableFallback() complete");
			await Complete();
		}

		private void UpdateValues(string hostCM, string token)
		{
			foreach (var itemId in itemIdList)
			{
				UpdateItemField(hostCM, token, itemId);
			}
		}
		
		private void UpdateItemField(string hostCM, string token, string itemId)
		{	
			try
            {
				var cookieClient = new CookieWebClient();
				cookieClient.Encoding = System.Text.Encoding.UTF8;
				cookieClient.Headers.Add("token", token);
				cookieClient.Headers.Add("Content-Type", "application/json");

				cookieClient.UploadDataAsync(
					new Uri(hostCM + $"/sitecore/api/ssc/item/{itemId}?database=master"),
					"PATCH",
					System.Text.Encoding.UTF8.GetBytes($"{{\"__Enable item fallback\": \"0\" }}"));
			}
            catch (Exception ex)
            {
				Log.LogError($"Failed to update item field, itemId: {itemId}", ex);
            }
		}
	}
}
