using Sitecore.Data;

namespace Sitecore.HabitatHome.Feature.News
{
    public struct Templates
    {
        public struct News
        {
            public static readonly ID ID = new ID("{B9718AE9-0113-43AE-8C86-E29830B6CDB7}");
        }

        public struct NewsSettings
        {
            public static readonly ID ID = new ID("{9B479B57-AAE9-4601-ABB4-E48ADC72B368}");

            public struct Fields
            {
                public static readonly ID NewsWildcardItem = new ID("{C6B8EB78-8193-4728-B1D6-5A9185463AF1}");
            }
        }
    }
}