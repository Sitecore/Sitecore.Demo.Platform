using System.Collections.Generic;
using Sitecore.Pipelines;

namespace Sitecore.Demo.Platform.Foundation.SitecoreExtensions.Pipelines
{
    public class ExcludeUrlsFromAnalytics
    {
        private readonly List<string> _urls = new List<string>();

        public void AddUrls(string url)
        {
            _urls.Add(url);
        }

        public virtual void Process(PipelineArgs args)
        {
            var filePath = Sitecore.Context.Request?.FilePath?.ToLower();
            if (string.IsNullOrEmpty(filePath))
            {
				return;
            }

            foreach (var url in _urls)
            {
                if (filePath.Contains(url))
                {
                    args.AbortPipeline();
                }
            }
        }
    }
}
