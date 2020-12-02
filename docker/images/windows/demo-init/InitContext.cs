using Microsoft.EntityFrameworkCore;
using Sitecore.Demo.Init.Model;

namespace Sitecore.Demo.Init
{
	public class InitContext : DbContext
	{
		public InitContext(DbContextOptions<InitContext> options)
			: base(options)
		{
		}

		public DbSet<CompletedJob> CompletedJobs { get; set; }
	}
}
