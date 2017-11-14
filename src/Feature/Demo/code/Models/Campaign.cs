namespace Sitecore.Feature.Demo.Models
{
    using Sitecore.XA.Foundation.Mvc.Models;
    using System;

    public class Campaign : RenderingModelBase
    {
        public DateTime? Date { get; set; }
        public bool IsActive { get; set; }
        public string Title { get; set; }
        public string Channel { get; set; }
    }
}