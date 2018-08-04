using Sitecore.XA.Feature.CreativeExchange.Enums;
using Sitecore.XA.Feature.CreativeExchange.Extensions;
using Sitecore.XA.Feature.CreativeExchange.Models.Export;
using Sitecore.XA.Feature.CreativeExchange.Services.Export;
using System.Linq;
using System.Net;
using System.Web;

namespace Sitecore.HabitatHome.Foundation.CreativeExchange.Services
{
    public class PageRequestServiceOverride : PageRequestService
    {
        public PageRequestServiceOverride(PageContext pageContext) : base(pageContext)
        {
        }

        protected override CookieContainer GetCookieContainer(string host, HttpCookieCollection cookies)
        {
            var container = base.GetCookieContainer(host, cookies);

            if (this.PageContext.ExportContext.ExportOptions.MarkupMode != MarkupMode.EndUserSite
                && cookies.AllKeys.Any(x => x == ".AspNet.Cookies"))
            {
                cookies.TransferCookie(".AspNet.Cookies", host, container);
            }

            return container;
        }
    }
}