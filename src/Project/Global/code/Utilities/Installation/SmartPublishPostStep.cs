using System.Collections.Specialized;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Install.Framework;

namespace  Sitecore.HabitatHome.Global.Website.Utilities.Installation
{
    public class SmartPublishPostStep : IPostStep
    {
        private static readonly Database _masterDatabase = Factory.GetDatabase("master");
        private static readonly Database _webDatabase = Factory.GetDatabase("web");

        public void Run(ITaskOutput output, NameValueCollection metaData)
        {
            //try
            //{
            //    Assert.IsNotNull(_masterDatabase, "Master Database can't be null");
            //    Assert.IsNotNull(_webDatabase, "Web Database can't be null.");

            //    using (new SecurityModel.SecurityDisabler())
            //    {
            //        Item sitecoreRootItem = _masterDatabase.GetItem("/sitecore");

            //        if (sitecoreRootItem == null)
            //        {
            //            throw new NullReferenceException("The '/sitecore' item does not exist in Sitecore.");
            //        }

            //        Language[] languages = _masterDatabase.Languages;

            //        foreach (Language language in languages)
            //        {
            //            PublishOptions publishOptions = new PublishOptions(
            //                _masterDatabase,
            //                _webDatabase,
            //                PublishMode.Smart,
            //                language,
            //                DateTime.Now)
            //            {
            //                Deep = true
            //            };


            //            Publisher publisher = new Publisher(publishOptions);
            //            publisher.Options.RootItem = sitecoreRootItem;
            //            publisher.PublishWithResult();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Error(ex.Message, ex, this);
            //}
        }
    }
}