using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Init.Jobs
{
	class IndexRebuild : TaskBase
	{
		public IndexRebuild(InitContext initContext)
			: base(initContext)
		{
		}

		public async Task Run()
		{
			if (this.IsCompleted())
			{
				Log.LogWarning($"{this.GetType().Name} is already complete, it will not execute this time");
				return;
			}

			await Start(nameof(IndexRebuild));

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");

			Log.LogInformation($"IndexRebuild() started {hostCM}");

			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };
			var indexes = new List<string>() { "sitecore_sxa_web_index", "sitecore_sxa_master_index", "sitecore_master_index", "sitecore_web_index", "sitecore_marketingdefinitions_master", "sitecore_marketingdefinitions_web", "sitecore_testing_index", "sitecore_personalization_index" };

			if (AreCoveoEnvironmentVariablesSet())
			{
				indexes.Add("Coveo_master_index");
				indexes.Add("Coveo_web_index");
			}

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
