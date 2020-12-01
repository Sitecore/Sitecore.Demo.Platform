using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sitecore.Demo.Init.Services;

namespace Sitecore.Demo.Init
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(loggingBuilder => {
				loggingBuilder.AddFile("app.log", false);
				loggingBuilder.AddConsole();
			});

			services.AddHostedService<JobManagementManagementService>();
		}
	}
}
