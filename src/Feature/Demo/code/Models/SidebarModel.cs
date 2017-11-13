namespace Sitecore.Feature.Demo.Models
{
    using Sitecore.XA.Foundation.Mvc.Models;

    public class SidebarModel : RenderingModelBase
    {
        public string HtmlContent
        {
            get
            {
                var content = this.Item[Templates.SidebarContent.Fields.HtmlContent];
                return content;
            }
        }
    }
}