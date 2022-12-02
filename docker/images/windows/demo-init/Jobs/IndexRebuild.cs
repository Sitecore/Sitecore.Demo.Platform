using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Init.Jobs
{
	class IndexRebuild : CoveoTaskBase
	{
		public IndexRebuild(InitContext initContext)
			: base(initContext)
		{
		}

		public async Task Run()
		{
			var indexes = new List<string>();

			if (!this.IsCompleted())
			{
				indexes.AddRange(new List<string>() {
					"sitecore_sxa_web_index",
					"sitecore_sxa_master_index",
					"sitecore_master_index",
					"sitecore_web_index",
					"sitecore_marketingdefinitions_master",
					"sitecore_marketingdefinitions_web",
					"sitecore_testing_index",
					"sitecore_personalization_index"
				});
			}

			if (this.AreCoveoEnvironmentVariablesSet() && this.HaveSettingsChanged())
			{
				indexes.AddRange(new List<string>() {
					"Coveo_master_index",
					"Coveo_web_index"
				});
			}

			if (indexes.Count == 0)
			{
				Log.LogWarning($"{TaskName} is already complete, it will not execute this time");
				return;
			}

			var hostCM = Environment.GetEnvironmentVariable("HOST_CM");

			Log.LogInformation($"IndexRebuild() started {hostCM}");

			using var client = new HttpClient { BaseAddress = new Uri(hostCM) };

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
