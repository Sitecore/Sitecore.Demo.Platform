namespace Sitecore.HabitatHome.Foundation.SitecoreExtensions
{
    using Sitecore.Data;

    public struct Templates
    {
        public struct Settings
        {
            //todo: this needs to be cleaned up - Account Settings should not be in this assembly
            // public static readonly ID ID = new ID("{4C7D90D2-CEAA-4A1F-8302-63D831E8E7B6}");

            //todo: this neds to be cleaned up - this is an SXA template, and we won't to remove dependencies on SXA
            public static readonly ID TemplateID = new ID("{BE2A1204-F7BA-4BE3-933D-190C97496700}");
        }
    }
}