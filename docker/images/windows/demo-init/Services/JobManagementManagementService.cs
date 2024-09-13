using System;
using System.Collections.Generic;
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
		private readonly IStateService stateService;

		public JobManagementManagementService(ILoggerFactory logFactory, ILogger<JobManagementManagementService> logger, InitContext initContext, IStateService stateService)
		{
			ApplicationLogging.LoggerFactory = logFactory;
			this.logger = logger;
			this.initContext = initContext;
			this.stateService = stateService;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				var watch = System.Diagnostics.Stopwatch.StartNew();

				await stateService.SetState(InstanceState.Initializing);
				logger.LogInformation($"{DateTime.UtcNow} Init started.");

				var indexRebuildAsyncJob = new IndexRebuild(initContext);
				var experienceGeneratorAsyncJob = new ExperienceGenerator(initContext);
				await new WaitForContextDatabase(initContext).Run();
				await new PushSerialized(initContext).Run();
				await new PublishItems(initContext).Run();
				await new DeployMarketingDefinitions(initContext).Run();
				await new RebuildLinkDatabase(initContext).Run();
				await new PopulateManagedSchema(initContext).Run();
				await new WaitForSitecoreToStart(initContext).Run();
				await new DisableFallback(initContext).Run();
				await new DeactivateMobileDeviceLayout(initContext).Run();
				await new UpdateDamUri(initContext).Run();

				await stateService.SetState(InstanceState.WarmingUp);
				logger.LogInformation($"WarmingUp. Elapsed: {watch.Elapsed:m\\:ss}");

				await Task.WhenAll(new WarmupCM(initContext).Run(), new WarmupCD(initContext).Run());

				logger.LogInformation($"Preparing. Elapsed: {watch.Elapsed:m\\:ss}");
				await stateService.SetState(InstanceState.Preparing);
				await Task.WhenAll(indexRebuildAsyncJob.Run(), experienceGeneratorAsyncJob.Run());

				logger.LogInformation($"{DateTime.UtcNow} All init tasks complete. See the background jobs status below. Elapsed: {watch.Elapsed:m\\:ss}");

				var asyncJobList = new List<TaskBase>
				                   {
					                   indexRebuildAsyncJob,
					                   experienceGeneratorAsyncJob,
								   };

				var runningJobs = await JobStatus.Run();
				while (runningJobs.Any(x => x.Title.Contains("IndexRebuild") || x.Title.Contains("ExperienceGenerator")))
				{
					var completedJobs = asyncJobList.Where(
						asyncJob => runningJobs.All(runningJob => runningJob.Title != asyncJob.TaskName)).ToList();
					foreach (var completedJob in completedJobs)
					{
						logger.LogInformation($"Writing job complete file to disk - {completedJob.TaskName}");
						await completedJob.Complete();
						asyncJobList.Remove(completedJob);
					}

					await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
					runningJobs = await JobStatus.Run();
				}

				if (asyncJobList.Any())
				{
					foreach (var job in asyncJobList)
					{
						logger.LogInformation($"Writing job complete file to disk - {job.TaskName}");
						await job.Complete();
					}
				}

				await stateService.SetState(InstanceState.Ready);
				logger.LogInformation($"{DateTime.UtcNow} No jobs are running. Monitoring stopped. Elapsed: {watch.Elapsed:m\\:ss}");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error has occurred when running JobManagementManagementService");
			}
		}
	}
}
