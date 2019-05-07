using System.Web.Mvc;

namespace Sitecore.HabitatHome.Feature.Search.Controllers
{
    public class SearchController : Controller
    {
        public ViewResult Search()
        {
            return View("~/Areas/Search/Views/Search.cshtml");
        }
    }
}