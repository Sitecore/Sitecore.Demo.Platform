using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sitecore.Demo.Init.Extensions;
using Sitecore.Demo.Init.Jobs;
using Sitecore.Demo.Init.Model;

namespace Sitecore.Demo.Init.Services
{

	public sealed class JobManagementManagementService : BackgroundService, IJobManagementService
	{
		private readonly ILogger<JobManagementManagementService> logger;
		private readonly InitContext initContext;

		public JobManagementManagementService(ILoggerFactory logFactory, ILogger<JobManagementManagementService> logger, InitContext initContext)
		{
			ApplicationLogging.LoggerFactory = logFactory;
			this.logger = logger;
			this.initContext = initContext;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				var startTime = DateTime.UtcNow;
				logger.LogInformation($"{DateTime.UtcNow} Init started.");

				var deployMarketingDefinitionsAsyncJob = new DeployMarketingDefinitions(initContext);
				var rebuildLinkDatabaseAsyncJob = new RebuildLinkDatabase(initContext);
				var indexRebuildAsyncJob = new IndexRebuild(initContext);
				var experienceGeneratorAsyncJob = new ExperienceGenerator(initContext);

				await new WaitForContextDatabase(initContext).Run();
				await new PublishItems(initContext).Run();
				await new ActivateCoveo(initContext).Run();
				await new WaitForSitecoreToStart(initContext).Run();
				await Task.WhenAll(new RemoveItems(initContext).Run());
				await Task.WhenAll(new UpdateFieldValues(initContext).Run(), deployMarketingDefinitionsAsyncJob.Run(), rebuildLinkDatabaseAsyncJob.Run());
				await Task.WhenAll(new WarmupCM(initContext).Run(), new WarmupCD(initContext).Run());
				await Task.WhenAll(indexRebuildAsyncJob.Run(), experienceGeneratorAsyncJob.Run());

				logger.LogInformation($"{DateTime.UtcNow} All init tasks complete. See the background jobs status below.");
				logger.LogInformation($"Elapsed time: {(DateTime.UtcNow - startTime):c}");

				var asyncJobList = new List<TaskBase>
				                   {
					                   deployMarketingDefinitionsAsyncJob,
					                   rebuildLinkDatabaseAsyncJob,
					                   indexRebuildAsyncJob,
					                   experienceGeneratorAsyncJob
				                   };

				while (true)
				{
					var statusDirectory = Path.Combine(Directory.GetCurrentDirectory(), "status");
					List<SitecoreJobStatus> runningJobs = await CheckAsyncJobsStatus();
					if (runningJobs != null)
					{
						var completedJobs = asyncJobList.Where(asyncJob => runningJobs.All(runningJob => runningJob.Title != asyncJob.TaskName)).ToList();
						foreach (var completedJob in completedJobs)
						{
							await LogCompletedJob(completedJob, statusDirectory);
							asyncJobList.Remove(completedJob);
						}
					}
					// the common case where we need to clean up our asyncJobsList if no running jobs were found.
					// ie: what happens when runningJobs doesn't return any running jobs?
					else if (asyncJobList.Count > 0)
					{
						foreach (var job in asyncJobList)
						{
							await LogCompletedJob(job, statusDirectory);
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

		private async Task LogCompletedJob(TaskBase completedJob, string statusDirectory)
		{
			logger.LogInformation($"Writing job complete file to disk - {completedJob.TaskName}");
			await completedJob.Stop(completedJob.TaskName);
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
