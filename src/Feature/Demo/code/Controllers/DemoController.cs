namespace Sitecore.Feature.Demo.Controllers
{
    using Sitecore.Feature.Demo.Repositories;
    using Sitecore.XA.Foundation.Mvc.Controllers;

    public class DemoController : StandardController
    {
        private readonly ISidebarRepository _sidebarRepository;
        private readonly IVisitsRepository _visitsRepository;
        
        public DemoController(ISidebarRepository sidebarRepository, IVisitsRepository visitsRepository)
        {
            this._sidebarRepository = sidebarRepository;
            this._visitsRepository = visitsRepository;
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