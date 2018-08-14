using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.HabitatHome.Foundation.CreativeExchange.Services;
using Sitecore.XA.Feature.CreativeExchange.Models.Export;
using Sitecore.XA.Feature.CreativeExchange.Pipelines.Export.PageProcessing;
using Sitecore.XA.Feature.CreativeExchange.Services.Export;

namespace Sitecore.HabitatHome.Foundation.CreativeExchange.Pipelines.PageProcessing
{
    public class PageRequestOverride : PageRequest
    {
        protected override IPageRequestService InitPageRequestService(PageContext context)
        {
            return (IPageRequestService)new PageRequestServiceOverride(context);
        }
    }
}