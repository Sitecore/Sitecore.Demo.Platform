using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Demo.Init.Services;

namespace Sitecore.Demo.Init.Jobs
{
	class PopulateManagedSchema : TaskBase
	{
		public PopulateManagedSchema(InitContext initContext)
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

			var ns = Environment.GetEnvironmentVariable("RELEASE_NAMESPACE");
			if (string.IsNullOrEmpty(ns))
			{
				Log.LogWarning(
					$"{this.GetType().Name} will not execute this time, RELEASE_NAMESPACE is not configured - this job is only required on AKS");
				return;
			}

			var cm = Environment.GetEnvironmentVariable("HOST_CM");
			var user = Environment.GetEnvironmentVariable("ADMIN_USER_NAME");
			var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
			var id = Environment.GetEnvironmentVariable("HOST_ID");
			var authenticatedClient = new SitecoreLoginService(Log).GetSitecoreClient(cm, id, user, password);

			Log.LogInformation($"PopulateManagedSchema() starting {cm}");
			var status =
				await authenticatedClient.DownloadStringTaskAsync(
					$"{cm}/sitecore/admin/PopulateManagedSchema.aspx?indexes=all");
			Log.LogInformation($"PopulateManagedSchema() status: {status}");

			// Run PopulateManagedSchema second time as sometimes it does not update Solr properly from the first time
			status = await authenticatedClient.DownloadStringTaskAsync(
				         $"{cm}/sitecore/admin/PopulateManagedSchema.aspx?indexes=all");
			Log.LogInformation($"PopulateManagedSchema() status: {status}");

			// Wait for PopulateManagedSchema to complete before restarting the sites
			var jobs = await JobStatus.Run();
			while (jobs.Any(x => x.Title.Contains("IndexRebuild") || x.Title.Contains("Publish")))
			{
				Log.LogInformation($"PopulateManagedSchema still running: {DateTime.UtcNow}");
				await Task.Delay(TimeSpan.FromSeconds(30));
				jobs = await JobStatus.Run();
			}

			await Complete();
		}
	}
}
