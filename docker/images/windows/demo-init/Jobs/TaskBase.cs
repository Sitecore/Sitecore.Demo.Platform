using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Demo.Init.Extensions;
using Sitecore.Demo.Init.Model;

namespace Sitecore.Demo.Init.Jobs
{
	public class TaskBase
	{
		private static readonly string StatusDirectory = Path.Combine(Directory.GetCurrentDirectory(), "status");
		private readonly InitContext initContext;

		public TaskBase(InitContext initContext)
		{
			this.initContext = initContext;
		}

		protected virtual ILogger Log
		{
			get
			{
				return ApplicationLogging.CreateLogger(this.GetType().Name);
			}
		}

		protected bool IsCompleted()
		{
			var completedJobs = initContext.CompletedJobs.Where(x => x.Name == this.GetType().Name);
			if (completedJobs.Any())
			{
				return true;
			}

			return false;
		}

		protected async Task Start(string theType)
		{
			if (!Directory.Exists(StatusDirectory))
			{
				Directory.CreateDirectory(StatusDirectory);
			}

			await File.WriteAllTextAsync(Path.Combine(StatusDirectory, $"{theType}.Started"), "Started");
		}

		protected async Task Stop(string theType)
		{
			initContext.CompletedJobs.Add(new CompletedJob(this.GetType().Name));
			await initContext.SaveChangesAsync();
			await File.WriteAllTextAsync(Path.Combine(StatusDirectory, $"{theType}.Ready"), "Ready");
		}

		protected bool AreCoveoEnvironmentVariablesSet()
		{
			var organizationId = Environment.GetEnvironmentVariable("COVEO_ORGANIZATION_ID");
			return !string.IsNullOrEmpty(organizationId);
		}
	}
}
