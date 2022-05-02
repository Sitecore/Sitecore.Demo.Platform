using System;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Web.UI.Sheer;
using DAM = Sitecore.Connector.ContentHub.DAM;

namespace Sitecore.Demo.Platform.Website.Hotfixes
{
    public class MImageCommand : DAM.Link.MImageCommand
    {
	   protected new void Run(ClientPipelineArgs args)
	   {
		  if (args.IsPostBack)
		  {
			 if (string.IsNullOrWhiteSpace(args.Result) || !(args.Result != "undefined"))
				return;
			 Item itemNotNull = Client.GetItemNotNull(args.Parameters["itemid"], Language.Parse(args.Parameters["language"]));
			 ((BaseItem)itemNotNull).Fields.ReadAll();
			 Field field = ((BaseItem)itemNotNull).Fields[args.Parameters["fieldid"]];
			 string parameter = args.Parameters["controlid"];
			 ImageField imageField = new ImageField(field, field.Value);
			 // Set MediaId attribute value (this value for Sitecore image) to empty if the image is from Content Hub
			 if (!string.IsNullOrEmpty(imageField.GetAttribute(DAM.Constants.MediaIdAttribute)))
			 {
				imageField.SetAttribute(DAM.Constants.MediaIdAttribute, string.Empty);
			 }
			 DAM.Mapping.FieldAttribute.SetAttribute(new Action<string, string>(((XmlField)imageField).SetAttribute), args.Result);
			 SheerResponse.SetAttribute("scHtmlValue", "value", WebEditImageCommand.RenderImage(args, ((CustomField)imageField).Value));
			 SheerResponse.SetAttribute("scPlainValue", "value", ((CustomField)imageField).Value);
			 SheerResponse.Eval("scSetHtmlValue('" + parameter + "')");
		  }
		  else
		  {
			 SheerResponse.ShowModalDialog("/sitecore/shell/client/Applications/MApp", "1200px", "700px", string.Empty, true);
			 args.WaitForPostBack();
		  }
	   }
    }
}
