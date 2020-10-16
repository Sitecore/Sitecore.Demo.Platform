using System;
using Sitecore.Jobs;

namespace Sitecore.Demo.Platform.Global.Website.Utilities
{
	public partial class RebuildLinkDatabase : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Run();
		}

		public void Run()
		{
			JobManager.Start(CreateNewJobOptions());
		}

		protected virtual DefaultJobOptions CreateNewJobOptions()
		{
			return new DefaultJobOptions("RebuildLinkDatabase", "index", Sitecore.Context.Site.Name, this, "Rebuild");
		}

		protected virtual void Rebuild()
		{
			var job = Sitecore.Context.Job;
			try
			{
				var database = Data.Database.GetDatabase("master");
				Globals.LinkDatabase.Rebuild(database);
			}
			catch (Exception ex)
			{
				job.Status.Failed = true;
				job.Status.Messages.Add(ex.ToString());
			}

			job.Status.State = JobState.Finished;
		}
	}
}
