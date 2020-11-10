using System;
using System.Threading;
using System.Threading.Tasks;
using Sitecore.Demo.Init.Jobs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Demo.Init.Model;

namespace Sitecore.Demo.Init
{
	class Program
	{
		private static readonly AutoResetEvent _closing = new AutoResetEvent(false);

		private static bool CheckStatus = true;

		public static async Task Main(string[] args)
		{
			var startTime = DateTime.UtcNow;
			Console.WriteLine($"{DateTime.UtcNow} Init started.");

			await Task.WhenAll(PublishItems.Run(), UpdateFieldValues.Run(), DeployMarketingDefinitions.Run(), RebuildLinkDatabase.Run());
			await Task.WhenAll(WarmupCM.Run(), WarmupCD.Run());
			await Task.WhenAll(IndexRebuild.Run(), ExperienceGenerator.Run());

			Console.WriteLine($"{DateTime.UtcNow} All init tasks complete. See the background jobs status below.");
			Console.WriteLine($"Elapsed time: {(DateTime.UtcNow - startTime):c}");

			var asyncJobList = new List<string>
			                   {
				                   typeof(DeployMarketingDefinitions).Name,
				                   typeof(IndexRebuild).Name,
				                   typeof(ExperienceGenerator).Name
			                   };

			// Prevent container from exiting. Otherwise it will get re-created and ran on each subsequent 'dc up -d' locally
			await Task.Factory.StartNew(
				async () =>
				{
					while (true)
					{
						var statusDirectory = Path.Combine(Directory.GetCurrentDirectory(), "status");

						Console.WriteLine();
						List<SitecoreJobStatus> runningJobs = await CheckAsyncJobsStatus();
						if (runningJobs != null)
						{
							var completedJobs = asyncJobList.Where(s => !runningJobs.Any(p => p.Title == s)).ToList();
							foreach (var completedJob in completedJobs)
							{
								Console.WriteLine("Writing job complete file to disk");

								await File.WriteAllTextAsync(Path.Combine(statusDirectory, $"{completedJob}.Ready"), "Ready");
								asyncJobList.Remove(completedJob);
							}
						}
						// the rare case where we need to clean up our asyncJobsList if no running jobs were found.
						// ie: what happens when runningJobs doesn't return any running jobs?
						else if (asyncJobList.Count > 0)
						{
							foreach (var job in asyncJobList)
							{
								await File.WriteAllTextAsync(Path.Combine(statusDirectory, $"{job}.Ready"), "Ready");
							}
						}

						Thread.Sleep(60000);
					}
				});

			Console.CancelKeyPress += OnExit;
			_closing.WaitOne();
		}

		private static async Task<List<SitecoreJobStatus>> CheckAsyncJobsStatus()
		{
			try
			{
				if (!CheckStatus)
				{
					return null;
				}

				Console.WriteLine($"{DateTime.UtcNow} Job status:");
				var jobs = await JobStatus.Run();
				if (jobs.Any())
				{
					foreach (var job in jobs)
					{
						Console.WriteLine($"{job.Title} {job.Added} - {job.Progress}");
					}

					return jobs;
				}

				CheckStatus = false;
				Console.WriteLine("No jobs are running. Monitoring stopped.");
				return null;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to retrieve running jobs. " + ex);
				return null;
			}
		}

		protected static void OnExit(object sender, ConsoleCancelEventArgs args)
		{
			Console.WriteLine("Exit");
			_closing.Set();
		}
	}
}
