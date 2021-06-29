using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sitecore.Demo.Init.Services;

namespace Sitecore.Demo.Init
{
	using Microsoft.EntityFrameworkCore;

	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(
				loggingBuilder =>
				{
					loggingBuilder.AddFile("app.log", false);
					loggingBuilder.AddConsole();
					loggingBuilder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
				});

			string connectionString = Configuration["INIT_CONTEXT"];
			services.AddDbContext<InitContext>(options => options.UseSqlServer(connectionString));

			services.AddHostedService<JobManagementManagementService>();
			services.AddTransient<IStateService, StateService>();
		}
	}
}
