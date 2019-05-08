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
        private ISearchService _searchService;

        public SearchController(ISearchConfigurationService searchConfigurationService, ISearchService searchService)
        {
            _searchConfigurationService = searchConfigurationService;
            _searchService = searchService;
        }

        public ViewResult Search()
        {
            var model = new SearchFormModel();
            return View("~/Areas/Search/Views/Search.cshtml", model);
        }

        [HttpPost]
        public ActionResult Search(SearchFormModel model)
        {
            if (!ModelState.IsValid) return View("~/Areas/Search/Views/Search.cshtml", model);

            var searchConfigurationSettingsItem = _searchConfigurationService.GetSearchConfigurationSettingsItem();
            if (searchConfigurationSettingsItem != null)
            {
                var targetItem = Context.Site.Database.GetItem(searchConfigurationSettingsItem[Templates.SearchConfigurationSettings.Fields.SearchPage]);
                if (targetItem != null)
                {
                    var redirectUrl = $"{LinkManager.GetItemUrl(targetItem)}?query={model.SearchTerm}";
                    return Redirect(redirectUrl);
                }
            }

            return Redirect("/");
        }

        public ViewResult SearchResults()
        {
            var model = new SearchResultsViewModel();
            model.SearchTerm = WebUtil.GetQueryString("query", string.Empty); ;
            return View("~/Areas/Search/Views/SearchResults.cshtml", model);
        }
    }
}