using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Demo.Init.Extensions;
using Sitecore.Demo.Init.Model;

namespace Sitecore.Demo.Init.Jobs
{
	public class TaskBase
	{
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

		public bool IsCompleted()
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

		public async Task Complete()
		{
			initContext.CompletedJobs.Add(new CompletedJob(TaskName, ComputeSettingsHash()));
			await initContext.SaveChangesAsync();
		}
	}
}
