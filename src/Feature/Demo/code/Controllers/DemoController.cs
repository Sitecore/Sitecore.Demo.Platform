namespace Sitecore.Feature.Demo.Controllers
{
    using Sitecore.Feature.Demo.Repositories;
    using Sitecore.XA.Foundation.Mvc.Controllers;
    using System.Web.Mvc;

    public class DemoController : StandardController
    {
        private readonly ISidebarRepository _sidebarRepository;
        private readonly IVisitsRepository _visitsRepository;
        
        public DemoController(ISidebarRepository sidebarRepository, IVisitsRepository visitsRepository)
        {
            this._sidebarRepository = sidebarRepository;
            this._visitsRepository = visitsRepository;
        }

        public ActionResult SidebarContent()
        {
            var sidebarContent = GetModel();
            return this.View("_SidebarContent", sidebarContent);
        }

        protected override object GetModel()
        {
            return _sidebarRepository.GetModel();
        }

        protected override string GetIndexViewName()
        {
            return "~/Views/Demo/Sidebar.cshtml";
        }
    }
}