using System.Collections.Specialized;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Install.Framework;
using Sitecore.SecurityModel;

namespace  Sitecore.HabitatHome.Global.Website.Utilities.Installation
{
    public class UpdateHostnamePostStep : IPostStep
    {
        private readonly ID _primarySiteDefinitionId = ID.Parse("{459C2D64-251D-4469-B364-1D53C165D60D}");


        public void Run(ITaskOutput output, NameValueCollection metaData)
        {
            Database masterDatabase = Factory.GetDatabase("master");

            if (masterDatabase != null)
            {
                Item siteDefinitionItem = masterDatabase.GetItem(_primarySiteDefinitionId);

                if (siteDefinitionItem != null)
                {
                    using (new SecurityDisabler())
                    {
                        using (new EditContext(siteDefinitionItem))
                        {
                            Diagnostics.Log.Info("Setting Default Site Definition To *", this);
                            siteDefinitionItem["HostName"] = "*";
                        }
                    }
                }
            }
        }
    }
}