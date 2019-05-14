using System.Web;
using System.Web.Mvc;
using Sitecore.HabitatHome.Feature.Search.Models;
using Sitecore.HabitatHome.Feature.Search.Services;
using Sitecore.Links;
using Sitecore.Mvc.Controllers;
using Sitecore.Web;

namespace Sitecore.HabitatHome.Feature.Search.Controllers
{
    public class SearchController : SitecoreController
    {
        private readonly ISearchConfigurationService _searchConfigurationService;
        private readonly ISearchService _searchService;

        public SearchController(ISearchConfigurationService searchConfigurationService, ISearchService searchService)
        {
            _searchConfigurationService = searchConfigurationService;
            _searchService = searchService;
        }

        public ViewResult Search()
        {
            var model = new SearchFormViewModel();
            return View("~/Areas/Search/Views/Search.cshtml", model);
        }

        [HttpPost]
        public ActionResult Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid || Context.PageMode.IsExperienceEditor) return View("~/Areas/Search/Views/Search.cshtml", model);

            var searchPage = _searchConfigurationService.GetSearchPage();
            if (searchPage != null)
            {
                var redirectUrl = $"{LinkManager.GetItemUrl(searchPage)}?query={model.SearchTerm}";
                return Redirect(redirectUrl);
            }


            return Redirect("/");
        }

        public ViewResult SearchResults()
        {
            var pageNumber = WebUtil.GetQueryString("page", "1");
            var searchTerm = HttpUtility.HtmlDecode(WebUtil.GetQueryString("query", string.Empty));

            var defaultNumberOfItems = _searchConfigurationService.GetSearchPageDefaultNumberOfItems();
            if (int.TryParse(pageNumber, out var page))
            {
            }

            var model = _searchService.GetSearchResults(searchTerm, page, defaultNumberOfItems);

            return View("~/Areas/Search/Views/SearchResults.cshtml", model);
        }
    }
}