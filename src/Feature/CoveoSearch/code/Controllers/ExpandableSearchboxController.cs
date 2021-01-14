using System.Web.Mvc;
using Sitecore.XA.Foundation.Mvc.Controllers;
using Sitecore.Demo.Platform.Feature.CoveoSearch.Models;

namespace Sitecore.Demo.Platform.Feature.CoveoSearch.Controllers
{
	public class ExpandableSearchboxController : StandardController
	{
		public ActionResult ExpandableSearchbox()
		{
			return View(CreateModel());
		}

		public ExpandableSearchboxModel CreateModel()
		{
			ExpandableSearchboxModel model = new ExpandableSearchboxModel();
			try
			{
				model.IsCoveoActivated = System.IO.File.Exists("C:\\inetpub\\wwwroot\\App_Config\\Include\\Coveo\\Coveo.SearchProvider.Custom.config");
			}
			catch
			{
				model.IsCoveoActivated = false;
			}
			model.IsExperienceEditor = Sitecore.Context.PageMode.IsExperienceEditorEditing || Sitecore.Context.PageMode.IsExperienceEditor;

			return model;
		}
	}
}
