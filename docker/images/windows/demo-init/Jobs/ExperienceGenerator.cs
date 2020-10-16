using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Demo.Init.Jobs
{
	class ExperienceGenerator : TaskBase
	{
		public static async Task Run()
		{
			await Start(typeof(ExperienceGenerator).Name);

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");

			Console.WriteLine($"RunExperienceGenerator() started on {hostCM}");
			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var content = File.ReadAllText("data/xGenerator.json").Replace("{HOST_CM}", hostCM);
			content = content.Replace("{START_DATE}", DateTime.UtcNow.AddYears(-1).ToString("u"));
			content = content.Replace("{END_DATE}", DateTime.UtcNow.ToString("u"));

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/clientapi/xgen/jobs")
			{
				Content = new StringContent(content, Encoding.UTF8, "application/json")
			};

			var response = await client.SendAsync(request);
			var contents = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"{response.StatusCode} {contents}");
		}
	}
}
