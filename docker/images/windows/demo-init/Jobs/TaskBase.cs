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

		public string TaskName
		{
			get
			{
				return GetType().Name;
			}
		}

		public TaskBase(InitContext initContext)
		{
			this.initContext = initContext;
		}

		protected virtual ILogger Log
		{
			get
			{
				return ApplicationLogging.CreateLogger(TaskName);
			}
		}

		protected CompletedJob GetLastCompletedJob()
		{
			return initContext.CompletedJobs.Where(completedJob => completedJob.Name == TaskName).OrderByDescending(completedJob => completedJob.Id).FirstOrDefault();
		}

		protected bool IsCompleted()
		{
			var completedJob = GetLastCompletedJob();
			return completedJob != null;
		}

		protected bool HaveSettingsChanged()
		{
			var completedJob = GetLastCompletedJob();
			if (completedJob == null)
			{
				return true;
			}

			return completedJob.SettingsHash != ComputeSettingsHash();
		}

		public virtual string ComputeSettingsHash()
		{
			return null;
		}

		protected async Task Start(string theType)
		{
			if (!Directory.Exists(StatusDirectory))
			{
				Directory.CreateDirectory(StatusDirectory);
			}

			await File.WriteAllTextAsync(Path.Combine(StatusDirectory, $"{theType}.Started"), "Started");
		}

		public async Task Stop(string theType)
		{
			initContext.CompletedJobs.Add(new CompletedJob(TaskName, ComputeSettingsHash()));
			await initContext.SaveChangesAsync();
			await File.WriteAllTextAsync(Path.Combine(StatusDirectory, $"{theType}.Ready"), "Ready");
		}
	}
}
