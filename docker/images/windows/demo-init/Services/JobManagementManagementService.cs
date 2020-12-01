using System;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Demo.Init.Jobs;
using Sitecore.Demo.Init.Model;

namespace Sitecore.Demo.Init.Services
{
	using Sitecore.Demo.Init.Extensions;

	public sealed class JobManagementManagementService : BackgroundService, IJobManagementService
	{
		private readonly ILogger<JobManagementManagementService> logger;

		public JobManagementManagementService(ILogger<JobManagementManagementService> logger, ILoggerFactory logFactory)
		{
			this.logger = logger;
			ApplicationLogging.LoggerFactory = logFactory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				var startTime = DateTime.UtcNow;
				logger.LogInformation($"{DateTime.UtcNow} Init started.");

				await PublishItems.Run();
				await WaitForSitecoreToStart.Run();
				await Task.WhenAll(UpdateFieldValues.Run(), DeployMarketingDefinitions.Run(), RebuildLinkDatabase.Run());
				await Task.WhenAll(WarmupCM.Run(), WarmupCD.Run());
				await Task.WhenAll(IndexRebuild.Run(), ExperienceGenerator.Run());

				logger.LogInformation($"{DateTime.UtcNow} All init tasks complete. See the background jobs status below.");
				logger.LogInformation($"Elapsed time: {(DateTime.UtcNow - startTime):c}");

				var asyncJobList = new List<string>
				                   {
					                   typeof(DeployMarketingDefinitions).Name,
					                   typeof(IndexRebuild).Name,
					                   typeof(ExperienceGenerator).Name
				                   };

				while (true)
				{
					var statusDirectory = Path.Combine(Directory.GetCurrentDirectory(), "status");
					List<SitecoreJobStatus> runningJobs = await CheckAsyncJobsStatus();
					if (runningJobs != null)
					{
						var completedJobs = asyncJobList.Where(s => !runningJobs.Any(p => p.Title == s)).ToList();
						foreach (var completedJob in completedJobs)
						{
							logger.LogInformation("Writing job complete file to disk");

							await File.WriteAllTextAsync(Path.Combine(statusDirectory, $"{completedJob}.Ready"), "Ready", stoppingToken);
							asyncJobList.Remove(completedJob);
						}
					}
					// the rare case where we need to clean up our asyncJobsList if no running jobs were found.
					// ie: what happens when runningJobs doesn't return any running jobs?
					else if (asyncJobList.Count > 0)
					{
						foreach (var job in asyncJobList)
						{
							await File.WriteAllTextAsync(Path.Combine(statusDirectory, $"{job}.Ready"), "Ready", stoppingToken);
						}

						return;
					}
					else
					{
						return;
					}

					await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error has occurred when running JobManagementManagementService");
			}
		}

		private async Task<List<SitecoreJobStatus>> CheckAsyncJobsStatus()
		{
			try
			{
				logger.LogInformation($"{DateTime.UtcNow} Job status:");
				var jobs = await JobStatus.Run();
				if (jobs.Any())
				{
					foreach (var job in jobs)
					{
						logger.LogInformation($"{job.Title} {job.Added} - {job.Progress}");
					}

					return jobs;
				}

				logger.LogInformation("No jobs are running. Monitoring stopped.");
				return null;
			}
			catch (Exception ex)
			{
				logger.LogInformation("Failed to retrieve running jobs. " + ex);
				return null;
			}
		}
	}
}
