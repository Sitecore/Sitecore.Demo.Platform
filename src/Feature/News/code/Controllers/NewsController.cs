using System.Web.Mvc;

namespace Sitecore.HabitatHome.Feature.News.Controllers
{
    public class NewsController : Controller
    {
        public ViewResult NewsOverview()
        {
            return View("~/Areas/News/Views/NewsOverview.cshtml");
        }

        public ViewResult NewsDetailHeading()
        {
            return View("~/Areas/News/Views/NewsDetailHeading.cshtml");
        }

        public ViewResult NewsNewsDetailArticleOverview()
        {
            return View("~/Areas/News/Views/NewsDetailArticle.cshtml");
        }
    }
}