namespace Sitecore.Feature.Demo.Controllers
{
    using Sitecore.Feature.Demo.Repositories;
    using Sitecore.XA.Foundation.Mvc.Controllers;

    public class SidebarController : StandardController
    {
        private readonly ISidebarRepository _repository;

        public SidebarController(ISidebarRepository repository)
        {
            this._repository = repository;
        }

        protected override object GetModel()
        {
            return _repository.GetModel();
        }
    }
}