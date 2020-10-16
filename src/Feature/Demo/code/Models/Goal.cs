using System;

namespace Sitecore.Demo.Platform.Feature.Demo.Models
{
    public class PageEvent
    {
        public int EngagementValue { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public bool IsCurrentVisit { get; set; }
        public string Data { get; set; }
    }
}