using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sitecore.Demo.Init.Model;

namespace Sitecore.Demo.Init.Jobs
{
    using Sitecore.Demo.Init.Extensions;

    public class WarmupCD : WarmupBase
    {
        public static async Task Run()
        {
            var skipWarmupCd = Convert.ToBoolean(Environment.GetEnvironmentVariable("SKIP_WARMUP_CD"));

            if (skipWarmupCd)
            {
                Console.WriteLine($"{DateTime.UtcNow} SKIP_WARMUP_CD set to true. Skipping Warmup CD");
                return;
            }
            await Start(typeof(WarmupCD).Name);

            var content = File.ReadAllText("data/warmup-config.json");
            var config = JsonConvert.DeserializeObject<WarmupConfig>(content);

            Console.WriteLine($"{DateTime.UtcNow} Warmup CD started");
            await WaitForSitecoreToStart.RunCD();

            var cd = Environment.GetEnvironmentVariable("HOST_CD");
            var client = new CookieWebClient();
            client.BaseAddress = cd;

            Console.WriteLine($"Warmup CD - URL: {cd}");

            foreach (var entry in config.urls[1].xp)
            {
                await LoadUrl(cd, entry.url, client);
            }

            await Stop(typeof(WarmupCD).Name);

            Console.WriteLine($"{DateTime.UtcNow} Warmup CD complete");
        }
    }
}
