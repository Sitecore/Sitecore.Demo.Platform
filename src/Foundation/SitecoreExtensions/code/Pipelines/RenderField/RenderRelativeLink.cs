using System;     

namespace Sitecore.Foundation.SitecoreExtensions.Pipelines.RenderField
{
    public class RenderRelativeLink
    {
        public void Process(Sitecore.Pipelines.RenderField.RenderFieldArgs args)
        {
            if (args != null && (args.FieldTypeKey == "link" || args.FieldTypeKey == "general link"))
            {
                Sitecore.Data.Fields.LinkField linkField = args.Item.Fields[args.FieldName];
                if (!string.IsNullOrEmpty(linkField.Url) && linkField.LinkType == "rel")
                {
                    args.Parameters["href"] = linkField.Url;
                }
            }
        }
    }
}