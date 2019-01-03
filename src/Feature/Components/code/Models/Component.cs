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
    public class Component : ItemBase
    {
        private Site site;

        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Lead { get; set; }
        public string Content { get; set; }
        public Component TargetPage { get; set; }
        public string TargetUrl { get; set; }



        public Site Site
        {
            get
            {
                if (site == null)
                {
                    site = new Site();
                    site.Item = Context.Database.GetItem(Context.Site.ContentStartPath);
                }
                return site;
            }
        }



        public string Url
        {
            get
            {
                string url = TargetPage?.Url;
                if (string.IsNullOrEmpty(url))
                    url = TargetUrl;
                if (string.IsNullOrEmpty(url))
                    url = LinkManager.GetItemUrl(Item);
                return url;
            }
        }
    }
}