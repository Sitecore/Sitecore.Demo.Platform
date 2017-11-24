namespace Sitecore.Feature.Demo.Controllers
{
    using Sitecore.Feature.Demo.Repositories;
    using Sitecore.XA.Foundation.Mvc.Controllers;
    using System.Web.Mvc;

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

        public ActionResult SidebarContent()
        {
            if (Sitecore.Context.PageMode.IsExperienceEditor)
            {
                return new EmptyResult();
            }

            var sidebarContent = GetModel();
            return this.View("_SidebarContent", sidebarContent);
        }

        protected override object GetModel()
        {
            if (Sitecore.Context.PageMode.IsExperienceEditor)
            {
                return null;
            }
            return _sidebarRepository.GetModel();
        }

        protected override string GetIndexViewName()
        {
            return "~/Views/Demo/Sidebar.cshtml";
        }
    }
}