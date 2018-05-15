using System;             
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.Dialogs;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Xml;

namespace Sitecore.HabitatHome.Foundation.SitecoreExtensions.Controls
{
    public class RelativePathForm : LinkForm
    {
        protected Edit Class;
        protected Edit Text;
        protected Edit Title;
        protected Edit Url;

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (Context.ClientPage.IsEvent)
            {
                return;
            }

            var item = LinkAttributes["url"];

            if (LinkType != "rel")
            {
                item = string.Empty;
            }

            Text.Value = LinkAttributes["text"];
            Url.Value = item;
            Class.Value = LinkAttributes["class"];
            Title.Value = LinkAttributes["title"];
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            
            var packet = new Packet("link");
            SetAttribute(packet, "text", Text);
            SetAttribute(packet, "linktype", "rel");
            SetAttribute(packet, "url", Url);
            SetAttribute(packet, "anchor", string.Empty);
            SetAttribute(packet, "title", Title);
            SetAttribute(packet, "class", Class);
            SheerResponse.SetDialogValue(packet.OuterXml);
            base.OnOK(sender, args);
        }
    }
}