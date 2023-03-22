using System.Web.Mvc;
using Sitecore.XA.Foundation.Mvc.Controllers;
using Sitecore.Demo.Platform.Feature.ExperienceAccelerator.Models;

namespace Sitecore.Demo.Platform.Feature.ExperienceAccelerator.Controllers
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
			model.IsExperienceEditor = Sitecore.Context.PageMode.IsExperienceEditorEditing || Sitecore.Context.PageMode.IsExperienceEditor;

			return model;
		}
	}
}
