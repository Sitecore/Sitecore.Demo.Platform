using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Init.Jobs
{
	using System;

	class WaitForContextDatabase : TaskBase
	{
		private readonly InitContext initContext;

		public WaitForContextDatabase(InitContext initContext)
			: base(initContext)
		{
			this.initContext = initContext;
		}

		public async Task Run()
		{
			var i = 0;

			while (true)
			{
				try
				{
					i++;
					Log.LogInformation($"WaitForContextDatabase attempt #{i}");

					if (await initContext.Database.CanConnectAsync())
					{
						break;
					}
				}
				catch(Exception ex)
				{
					Log.LogInformation($"WaitForContextDatabase failed, retrying... " + ex);
				}

				await Task.Delay(10000);
			}

			await initContext.Database.EnsureCreatedAsync();
		}
	}
}
