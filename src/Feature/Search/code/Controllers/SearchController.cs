using System.Web.Mvc;
using Sitecore.HabitatHome.Feature.Search.Models;
using Sitecore.HabitatHome.Feature.Search.Services;
using Sitecore.Links;

namespace Sitecore.HabitatHome.Feature.Search.Controllers
{
    public class SearchController : Controller
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
            return View("~/Areas/Search/Views/Search.cshtml");
        }

        [HttpPost]
        public ActionResult Index(SearchFormModel model)
        {
            if (!ModelState.IsValid) return View("~/Areas/Search/Views/Search.cshtml", model);

            // do some other stuff, like storing the name in the database
            var searchConfigurationSettingsItem = _searchConfigurationService.GetSearchConfigurationSettingsItem();
            if (searchConfigurationSettingsItem != null)
            {
                var targetItem = Context.Site.Database.GetItem(searchConfigurationSettingsItem[Templates.SearchConfigurationSettings.Fields.SearchPage]);
                if (targetItem != null)
                {
                    var redirectUrl = $"{LinkManager.GetItemUrl(targetItem)}?query={model.SearchTerm}";
                    Redirect(redirectUrl);
                }
            }

            return Redirect("/");
        }
    }
}