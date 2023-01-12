using DAM = Sitecore.Connector.ContentHub.DAM;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.Demo.Platform.Website.Hotfixes
{
	public class MImageControl: DAM.Link.MImageControl

	{
		/// <summary>
		/// Handles the Browse the Sitecore DAM control - Override to set MediaId attribute to empty if an image was selected from the DAM.
		/// </summary>
		/// <param name="args"></param>
		protected new void BrowseMImage(ClientPipelineArgs args)
		{
			if (args.IsPostBack)
			{
				if (!string.IsNullOrWhiteSpace(args.Result) && args.Result != DAM.Constants.UndefinedValue)
				{
					DAM.Mapping.FieldAttribute.SetAttribute(XmlValue.SetAttribute, args.Result);
					// Set MediaId attribute value (this value for Sitecore image) to empty if the image is from Content Hub
					if (!string.IsNullOrEmpty(XmlValue.GetAttribute(DAM.Constants.StylelabsContentIdAttribute))
						&& !string.IsNullOrEmpty(XmlValue.GetAttribute(DAM.Constants.MediaIdAttribute)))
					{
						XmlValue.SetAttribute(DAM.Constants.MediaIdAttribute, string.Empty);
					}
					MUpdate();
					SetModified();
					Value = XmlValue.GetAttribute(DAM.Constants.SourceAttribute);

				}
			}
			else
			{
				SheerResponse.ShowModalDialog(DAM.Constants.MAppModalDialogUrl, DAM.Constants.MAppModalDialogWidth2, DAM.Constants.MAppModalDialogHeight2, string.Empty, true);
				args.WaitForPostBack();
			}
		}
	}
}
