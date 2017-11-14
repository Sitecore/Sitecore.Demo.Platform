namespace Sitecore.Feature.Demo.Services
{
    using Sitecore.Configuration;
    using Sitecore.Foundation.DependencyInjection;
    using System;
    using System.Web;

    [Service]
    public class DemoStateService
    {
        public DemoStateService(HttpContextBase httpContext)
        {
            this.HttpContext = httpContext;
        }

        public HttpContextBase HttpContext { get; }

        public bool IsDemoEnabled => !this.HttpContext?.Request?.Headers["X-DisableDemo"]?.Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase) ?? Settings.GetBoolSetting("Demo.Enabled", true);
    }
}