using Sitecore.Mvc.Pipelines;
using Sitecore.Mvc.Pipelines.Response.GetPageItem;

namespace Sitecore.HabitatHome.Feature.News.Pipelines.MvcGetPageItem
{
    public class CheckNewsItemResolved : MvcPipelineProcessor<GetPageItemArgs>
    {
        public override void Process(GetPageItemArgs args)
        {
            var resolved = Context.Items[Constants.NewsItemResolvedKey];
            if (MainUtil.GetBool(resolved, false)) args.Result = Context.Item;
        }
    }
}