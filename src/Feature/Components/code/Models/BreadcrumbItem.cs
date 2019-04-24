namespace Sitecore.HabitatHome.Feature.Components.Models
{
    public class BreadcrumbItem
    {
        public BreadcrumbItem(string title = "", string url = "", bool active = false)
        {
            Title = title;
            Url = url;
            Active = active;
        }

        public string Title { get; set; }

        public string Url { get; set; }

        public bool Active { get; set; }

        public string ActiveClass => Active ? "active" : "";
    }
}