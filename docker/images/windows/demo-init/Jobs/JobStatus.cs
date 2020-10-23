using System;
using System.Threading.Tasks;
using Sitecore.Demo.Init.Services;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Sitecore.Demo.Init.Model;

namespace Sitecore.Demo.Init.Jobs
{
	public class JobStatus
	{
		public static async Task<List<SitecoreJobStatus>> Run()
		{
			var cm = Environment.GetEnvironmentVariable("HOST_CM");
			var user = Environment.GetEnvironmentVariable("ADMIN_USER_NAME");
			var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
			var id = Environment.GetEnvironmentVariable("HOST_ID");
			var authenticatedClient = await new SitecoreLoginService().GetSitecoreClient(cm, id, user, password);
			var status = await authenticatedClient.DownloadStringTaskAsync($"{cm}/sitecore/admin/jobs.aspx?refresh=5");

			var jobsPage = new HtmlAgilityPack.HtmlDocument();
			jobsPage.LoadHtml(status);
			var statusTableNode = jobsPage.DocumentNode.SelectNodes("//*[@id=\"Form1\"]/div[3]/table[1]").FirstOrDefault();

			var results = new List<SitecoreJobStatus>();
			var titleNode = statusTableNode?.PreviousSibling?.PreviousSibling?.PreviousSibling;
			if (titleNode?.InnerText == "Running jobs:")
			{
				foreach (HtmlNode row in statusTableNode.SelectNodes("tr"))
				{
					HtmlNodeCollection cells = row.SelectNodes("td");
					results.Add(
						new SitecoreJobStatus()
						{
							Title = GetJobName(cells[2].InnerText),
							Added = cells[1].InnerText,
							Progress = cells[3].InnerText
						});
				}
			}

			return results;
		}
		private static string GetJobName(string htmlJobName)
		{
			switch (htmlJobName)
			{
				case string a when a.Contains("ExperienceGenerator"):
					return typeof(ExperienceGenerator).Name;
				case string b when b.Contains("Index_Update"):
					return typeof(IndexRebuild).Name;
				case string c when c.Contains("Deploy all definitions"):
					return typeof(DeployMarketingDefinitions).Name;
				default:
					return htmlJobName;
			}
		}
	}
}
