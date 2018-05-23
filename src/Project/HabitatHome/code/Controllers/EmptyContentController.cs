using System;
using System.Web.Mvc;

namespace Sitecore.HabitatHome.Website.Controllers
{
    public class EmptyContentController : Controller
    {
        public ActionResult Render()
        {
            return new EmptyResult();
        }
    }
}