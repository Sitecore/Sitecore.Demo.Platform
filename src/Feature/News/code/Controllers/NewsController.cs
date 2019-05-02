using System.Web.Mvc;
using Sitecore.Web;

namespace Sitecore.HabitatHome.Feature.News.Controllers
{
    public class NewsController : Controller
    {
        private Models.News news;

        public Models.News News => news ?? (news = new Models.News {Item = Context.Item});

        public ViewResult NewsOverview()
        {
            var queryStringValue = WebUtil.GetQueryString("page", "10");
            if (int.TryParse(queryStringValue, out var page))
            {
            }

            var list = NewsRepository.GetNewsItems(page, 1);
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