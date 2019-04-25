using System.Web.Mvc;
using Sitecore.Data.Items;
using Sitecore.HabitatHome.Feature.Media.Models;
using Sitecore.Mvc.Presentation;

namespace Sitecore.HabitatHome.Feature.Media.Controllers
{
    public class MediaController : Controller
    {
        private VideoViewModel component;
        private Item dataSourceItem;

        protected VideoViewModel Component
        {
            get
            {
                if (component == null)
                {
                    var item = DataSourceItem ?? Context.Item;
                    component = new VideoViewModel {Item = item};
                }

                return component;
            }
        }

        protected Item DataSourceItem
        {
            get
            {
                var dataSource = RenderingContext.CurrentOrNull.Rendering.DataSource;
                if (dataSourceItem == null &&
                    !string.IsNullOrEmpty(dataSource))
                    dataSourceItem = Context.Database.GetItem(dataSource);
                return dataSourceItem;
            }
        }

        public ViewResult Video()
        {
            return View("~/Areas/Media/Views/Video.cshtml", Component);
        }
    }
}