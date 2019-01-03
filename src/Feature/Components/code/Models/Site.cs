using Sitecore.Data.Items;
using Sitecore.Links;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Sitecore.HabitatHome.Feature.Components.Models
{
    public class Site : ItemBase
    {
        private Component home;
        private Item settings;



        public Component Home
        {
            get
            {
                if (home == null)
                {
                    home = new Component();
                    home.Item = Context.Database.GetItem(Context.Site.StartPath);
                }
                return home;
            }
        }



        public Item Settings
        {
            get
            {
                var sitePath = Context.Site.ContentStartPath;
                if (settings == null && Item != null)
                    settings = Context.Database.GetItem(sitePath + "/" + Item["Settings"]);
                return settings;
            }
        }



        public T GetSettings<T>() where T : ItemBase, new()
        {
            return new T() { Item = Settings };
        }
    }
}