using System.Web.Mvc;
using Sitecore.Web;

namespace Sitecore.HabitatHome.Feature.News.Controllers
{
    public class NewsController : Controller
    {
        private Models.News news;

        public Models.News News => news ?? (news = new Models.News {Item = Context.Item});

        public int NewsOverviewDefaultNumberOfItems
        {
            get
            {
                var defaultNumber = 10;
                var sitePath = Context.Site.ContentStartPath;
                var newsSettingsItem = Context.Database.GetItem($"{sitePath}/Settings/News Settings");
                if (newsSettingsItem != null)
                {
                    var number = newsSettingsItem[Templates.NewsSettings.Fields.NewsOverviewDefaultNumberOfItems];
                    if (!string.IsNullOrEmpty(number) && int.TryParse(number, out defaultNumber)) return defaultNumber;
                }

                return defaultNumber;
            }
        }

        public ViewResult NewsOverview()
        {
            var queryStringValue = WebUtil.GetQueryString("page", "1");
            if (int.TryParse(queryStringValue, out var page))
            {
            }

            var list = NewsRepository.GetNewsItems(page, NewsOverviewDefaultNumberOfItems);
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