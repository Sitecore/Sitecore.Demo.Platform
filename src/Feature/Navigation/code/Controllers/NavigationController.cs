using Sitecore.XA.Foundation.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sitecore.Feature.Navigation.Controllers
{
    public class NavigationController : StandardController
    {
        public NavigationController()
        {

        }

        protected override object GetModel()
        {
            return base.GetModel();
        }

        protected override string GetIndexViewName()
        {
            return "~/Views/Navigation/ExtendedNavigation.cshtml";
        } 
    }
}