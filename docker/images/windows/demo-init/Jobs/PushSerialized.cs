using System;
using System.Threading.Tasks;
using Sitecore.Demo.Init.Container;

namespace Sitecore.Demo.Init.Jobs
{
	using Microsoft.Extensions.Logging;

	class PushSerialized : TaskBase
	{
		public PushSerialized(InitContext initContext)
			: base(initContext)
		{
		}

		public async Task Run()
        {
            if (this.IsCompleted())
            {
                Log.LogWarning($"{this.GetType().Name} is already complete, it will not execute this time");
                return;
            }

            var ns = Environment.GetEnvironmentVariable("RELEASE_NAMESPACE");
            if (string.IsNullOrEmpty(ns))
            {
                Log.LogWarning($"{this.GetType().Name} will not execute this time, RELEASE_NAMESPACE is not configured - this job is only required on AKS");
                return;
            }

            var token = Environment.GetEnvironmentVariable("ID_SERVER_DEMO_CLIENT_SECRET");
            if (string.IsNullOrEmpty(token))
            {
                Log.LogWarning($"{this.GetType().Name} will not execute ID_SERVER_DEMO_CLIENT_SECRET is not configured");
                return;
			}

            var cm = Environment.GetEnvironmentVariable("PUBLIC_HOST_CM");
            var id = Environment.GetEnvironmentVariable("PUBLIC_HOST_ID");
            if (string.IsNullOrEmpty(cm) || string.IsNullOrEmpty(id))
            {
                Log.LogWarning($"{this.GetType().Name} will not execute, PUBLIC_HOST_CM and PUBLIC_HOST_ID are not configured");
                return;
            }

            var cmd = new WindowsCommandLine("C:\\app");
            Console.WriteLine(cmd.Run(
                $"dotnet sitecore login --client-credentials true --auth {id} --cm {cm} --allow-write true --client-id \"Demo_Automation\" --client-secret \"{token}\" -t"));

			// Run once without logging, to suppress excessive logs
            cmd.Run($"dotnet sitecore ser push");
            Console.WriteLine(cmd.Run($"dotnet sitecore ser push"));
            Console.WriteLine(cmd.Run($"dotnet sitecore publish"));

            // Publish twice to ensure all items are published correctly. There's been a few cases when API key did not get published.
            Console.WriteLine(cmd.Run($"dotnet sitecore publish"));

            await Complete();
		}
	}
}
