using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;

namespace Sitecore.HabitatHome.Foundation.Cdn.Infrastructure.Pipelines.MediaProviders
{
  public class MediaProvider : Sitecore.Resources.Media.MediaProvider
  {
    public override string GetMediaUrl(MediaItem mediaItem)
    {
      if (Configuration.Settings.GetSetting("CDN.Enabled") != "true" || Sitecore.Context.Database.Name != "web")
      {
        return GetEmptyServerMediaUrl(mediaItem);
      }
      Assert.ArgumentNotNull(mediaItem, "mediaItem");
      return this.GetMediaUrl(mediaItem, MediaUrlOptions.Empty);
    }

    public override string GetMediaUrl(MediaItem mediaItem, MediaUrlOptions options)
    {
      if (Configuration.Settings.GetSetting("CDN.Enabled") != "true" || Sitecore.Context.Database.Name != "web")
      {
        return GetEmptyServerMediaUrl(mediaItem);
      }

      Assert.ArgumentNotNull(mediaItem, "mediaItem");
      Assert.ArgumentNotNull(options, "options");

      string oldMediaUrl = base.GetMediaUrl(mediaItem, options);
      string newMediaUrl = Sitecore.Web.WebUtil.AddQueryString(oldMediaUrl, "rev", ((Item) mediaItem).Statistics.Revision);
      newMediaUrl = Sitecore.Web.WebUtil.AddQueryString(newMediaUrl, "date",
        ((Item) mediaItem).Statistics.Updated.ToString("yymmddhhss"));

      return newMediaUrl;
    }

    private string GetEmptyServerMediaUrl(MediaItem mediaItem)
    {
      MediaUrlOptions options = new MediaUrlOptions {MediaLinkServerUrl = string.Empty};
      string mediaUrl = base.GetMediaUrl(mediaItem, options);
      return mediaUrl;
    }
  }
}