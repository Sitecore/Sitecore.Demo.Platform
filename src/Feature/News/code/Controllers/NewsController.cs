using System.Web.Mvc;

namespace Sitecore.HabitatHome.Feature.News.Controllers
{
    public class NewsController : Controller
    {
        private Models.News news;

        public Models.News News => news ?? (news = new Models.News {Item = Context.Item});

        public ViewResult NewsOverview()
        {
            var list = NewsRepository.GetNewsItems("0");
            return View("~/Areas/News/Views/NewsOverview.cshtml", list);
        }

        public ViewResult NewsDetailHeading()
        {
            return View("~/Areas/News/Views/NewsDetailHeading.cshtml", News);
        }

        public ViewResult NewsDetailArticle()
        {
            return View("~/Areas/News/Views/NewsDetailArticle.cshtml", News);
        }
    }
}