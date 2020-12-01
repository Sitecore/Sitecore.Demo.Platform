using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Sitecore.Demo.Init.Extensions;

namespace Sitecore.Demo.Init
{
	class Program
	{
		public static async Task Main(string[] args)
		{
			var host = CreateHostBuilder(args).Build();
			await host.RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}
