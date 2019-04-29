using System.Web.Mvc;

namespace Sitecore.HabitatHome.Feature.News.Controllers
{
    public class NewsController : Controller
    {
        public ViewResult NewsOverview()
        {
            return View("~/Areas/News/Views/NewsOverview.cshtml");
        }
    }
}