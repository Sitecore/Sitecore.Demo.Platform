using System;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Demo.Platform.Foundation.SitecoreExtensions.Extensions
{
    public static class RenderingExtensions
    {
        public static int GetIntegerParameter(this Rendering rendering, string parameterName, int defaultValue = 0)
        {
            if (rendering == null) throw new ArgumentNullException(nameof(rendering));

            var parameter = rendering.Parameters[parameterName];
            if (string.IsNullOrEmpty(parameter)) return defaultValue;

            return !int.TryParse(parameter, out var returnValue) ? defaultValue : returnValue;
        }
    }
}