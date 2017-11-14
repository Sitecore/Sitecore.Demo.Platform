using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.Feature.Demo.Controllers
{
    using Sitecore.DependencyInjection;
    using Sitecore.Feature.Demo.Repositories;
    using Sitecore.XA.Foundation.Mvc.Controllers;

    public class DemoController : StandardController
    {
        private readonly ISidebarRepository _repository;

        //public DemoController()
        //{
        //    this._repository = ServiceLocator.ServiceProvider.GetService<SidebarRepository>();
        //}

        public DemoController(ISidebarRepository repository)
        {
            this._repository = repository;
        }

        protected override object GetModel()
        {
            return _repository.GetModel();
        }

        protected override string GetIndexViewName()
        {
            return "~/Views/Demo/Sidebar.cshtml";
        }
    }
}