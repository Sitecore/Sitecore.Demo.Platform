using System.Collections.Specialized;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.Demo.Platform.Foundation.SitecoreExtensions.Controls
{
    public class ExtendedGeneralLink : Link
    {
        public ExtendedGeneralLink()
        {
            Class = "scContentControl";
            Activation = true;
        }

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            base.HandleMessage(message);

            if (message["id"] != ID)
                return;

            switch (message.Name)
            {
                case "contentlink:relativelink":
                    {
                        var url = new UrlString(UIUtil.GetUri("control:RelativePathForm"));
                        Insert(url.ToString(), new NameValueCollection
                    {
                        {
                            "height",
                            "425"
                        }
                    });
                        break;
                    }
                default:
                    {
                        return;
                    }
            }
        }
    }
}