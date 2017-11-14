namespace Sitecore.Feature.Demo.Models
{
    using Sitecore.XA.Foundation.Mvc.Models;

    public class SidebarModel : RenderingModelBase
    {
        public Visits Visits { get; set; }
        public PersonalInfo PersonalInfo { get; set; }
        public OnsiteBehavior OnsiteBehavior { get; set; }
        public Referral Referral { get; set; }
        public bool IsActive { get; set; }
    }
}