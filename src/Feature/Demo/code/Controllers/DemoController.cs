using System;
using System.Net;
using System.Web.Mvc;
using Sitecore.Analytics;
using Sitecore.Demo.Platform.Feature.Demo.Models;
using Sitecore.Demo.Platform.Feature.Demo.Services;
using Sitecore.Demo.Platform.Foundation.Alerts.Exceptions;
using Sitecore.Demo.Platform.Foundation.SitecoreExtensions.Attributes;
using Sitecore.ExperienceEditor.Utils;
using Sitecore.ExperienceExplorer.Core.State;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Presentation;
using Sitecore.Sites;
using Sitecore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.Demo.Platform.Feature.Demo.Controllers
{
    [SkipAnalyticsTracking]
    public class DemoController : SitecoreController
    {
        private IDemoStateService DemoStateService { get; }
        private IExperienceDataFactory ExperienceDataFactory { get; }
        private const string AnalyticsGlobalCookieName = "SC_ANALYTICS_GLOBAL_COOKIE";

        public DemoController(IDemoStateService demoStateService)
        {
            var role = System.Configuration.ConfigurationManager.AppSettings["role:define"];
            if (!role.Contains("ContentManagement"))
                this.ExperienceDataFactory = ServiceLocator.ServiceProvider.GetService<IExperienceDataFactory>();
            this.DemoStateService = demoStateService;
        }

        public ActionResult ExperienceData()
        {
            if (Tracker.Current == null || Tracker.Current.Interaction == null || !this.DemoStateService.IsDemoEnabled)
            {
                return null;
            }
            var explorerContext = DependencyResolver.Current.GetService<IExplorerContext>();
            var isInExperienceExplorer = explorerContext?.IsExplorerMode() ?? false;
            if (Context.Site.DisplayMode != DisplayMode.Normal || WebEditUtility.IsDebugActive(Context.Site) || isInExperienceExplorer)
            {
                return new EmptyResult();
            }

            if (this.ExperienceDataFactory == null)
                return new EmptyResult();

            var experienceData = this.ExperienceDataFactory.Get();
            return this.View(experienceData);
        }

        public ActionResult ExperienceDataContent()
        {
            if (this.ExperienceDataFactory == null)
                return new EmptyResult();

            var experienceData = this.ExperienceDataFactory.Get();
            return this.View("_ExperienceDataContent", experienceData);
        }

        public ActionResult DemoContent()
        {
            var item = RenderingContext.Current?.Rendering?.Item ?? RenderingContext.Current?.ContextItem;
            if (item == null || !item.DescendsFrom(Templates.DemoContent.ID))
            {
                throw new InvalidDataSourceItemException($"Item should be not null and derived from {nameof(Templates.DemoContent)} {Templates.DemoContent.ID} template");
            }

            var demoContent = new DemoContent(item);
            return this.View("DemoContent", demoContent);
        }

        public ActionResult EndVisit()
        {
            this.Session.Abandon();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult NewContact()
        {
            foreach (string key in Request.Cookies.AllKeys)
            {
                var cookie = Request.Cookies[key];
                cookie.Expires = DateTime.Now.AddMonths(-1);
                cookie.Value = string.Empty;
                if ((key == AnalyticsGlobalCookieName) && !string.IsNullOrEmpty(Configuration.Settings.GetSetting("Analytics.CookieDomain")))
                {
                    cookie.Domain = $".{Configuration.Settings.GetSetting("Analytics.CookieDomain")}";
                }

                Response.AppendCookie(cookie);
            }

            this.Session.Abandon();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
