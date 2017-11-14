using Sitecore.Foundation.DependencyInjection;

namespace Sitecore.Feature.Demo.Repositories
{
    using Sitecore.Feature.Demo.Models;
    using Sitecore.XA.Foundation.Mvc.Repositories.Base;

    [Service(typeof(ISidebarRepository))]
    public class SidebarRepository : ModelRepository, ISidebarRepository
    {
        public override IRenderingModelBase GetModel()
        {
            SidebarModel model = new SidebarModel();
            FillBaseProperties(model);                                                             
            return model;
        }
    }
}