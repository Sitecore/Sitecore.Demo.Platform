using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Sitecore.Demo.Init.Extensions;
using Sitecore.Demo.Init.Model;
using Sitecore.Demo.Init.Services;

namespace Sitecore.Demo.Init.Jobs
{
	public class JobStatus
	{
		protected static ILogger Log = ApplicationLogging.CreateLogger<JobStatus>();

		public static async Task<List<SitecoreJobStatus>> Run()
		{
			try
			{
				var cm = Environment.GetEnvironmentVariable("HOST_CM");
				var user = Environment.GetEnvironmentVariable("ADMIN_USER_NAME");
				var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
				var id = Environment.GetEnvironmentVariable("HOST_ID");
				var authenticatedClient = new SitecoreLoginService(Log).GetSitecoreClient(cm, id, user, password);
				var status = await authenticatedClient.DownloadStringTaskAsync($"{cm}/sitecore/admin/jobs.aspx?refresh=5");

				var jobsPage = new HtmlAgilityPack.HtmlDocument();
				jobsPage.LoadHtml(status);
				var statusTableNode = jobsPage.DocumentNode.SelectNodes("//*[@id=\"Form1\"]/div[3]/table[1]")
					.FirstOrDefault();

				Log.LogInformation($"{DateTime.UtcNow} Job status:");
				var results = new List<SitecoreJobStatus>();
				var titleNode = statusTableNode?.PreviousSibling?.PreviousSibling?.PreviousSibling;
				if (titleNode?.InnerText == "Running jobs:")
				{
					foreach (HtmlNode row in statusTableNode.SelectNodes("tr"))
					{
						HtmlNodeCollection cells = row.SelectNodes("td");
						var jobStatus = new SitecoreJobStatus()
						                {
							                Title = GetJobName(cells[2].InnerText),
							                Added = cells[1].InnerText,
							                Progress = cells[3].InnerText
						                };
						results.Add(jobStatus);
						Log.LogInformation($"{jobStatus.Title} {jobStatus.Added} - {jobStatus.Progress}");
					}
				}

				return results;
			}
			catch (Exception ex)
			{
				Log.LogError("Failed to retrieve running jobs. ", ex);
				return new List<SitecoreJobStatus>();
			}
		}

		private static string GetJobName(string htmlJobName)
		{
			switch (htmlJobName)
			{
				case string a when a.Contains("ExperienceGenerator"):
					return nameof(ExperienceGenerator);
				case string b when b.Contains("Index_Update"):
					return nameof(IndexRebuild);
				case string c when c.Contains("Deploy all definitions"):
					return nameof(DeployMarketingDefinitions);
				default:
					return htmlJobName;
			}
		}
	}
}
