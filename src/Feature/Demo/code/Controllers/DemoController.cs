namespace Sitecore.HabitatHome.Feature.Demo.Controllers
{
    using Sitecore.Analytics;
    using Sitecore.ExperienceEditor.Utils;
    using Sitecore.ExperienceExplorer.Core.State;
    using Sitecore.HabitatHome.Feature.Demo.Repositories;
    using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Attributes;
    using Sitecore.Sites;
    using Sitecore.XA.Foundation.Mvc.Controllers;
    using System.Net;
    using System.Web.Mvc;

    [SkipAnalyticsTracking]
    public class DemoController : StandardController
    {
        private readonly ISidebarRepository _sidebarRepository;
        
        public DemoController(ISidebarRepository sidebarRepository)
        {
            if (!Sitecore.Context.PageMode.IsExperienceEditor)
            {
                this._sidebarRepository = sidebarRepository;
            }
        }

        protected override object GetModel()
        {          
            if (Tracker.Current == null || Tracker.Current.Interaction == null) //todo: missing !this.DemoStateService.IsDemoEnabled
            {
                return null;
            }

            var explorerContext = DependencyResolver.Current.GetService<IExplorerContext>();
            var isInExperienceExplorer = explorerContext?.IsExplorerMode() ?? false;
            if (Context.Site.DisplayMode != DisplayMode.Normal || WebEditUtility.IsDebugActive(Context.Site) || isInExperienceExplorer)
            {
                return null;
            }
                              
            return _sidebarRepository.GetModel();
        }

        protected override string GetIndexViewName()
        {
            return "~/Views/Demo/Sidebar.cshtml";
        }

        public ActionResult ExperienceDataContent()
        {           
            var experienceData = GetModel();
            return this.View("_ExperienceDataContent", experienceData);
        }                                   

        public ActionResult EndVisit()
        {
            this.Session.Abandon();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}