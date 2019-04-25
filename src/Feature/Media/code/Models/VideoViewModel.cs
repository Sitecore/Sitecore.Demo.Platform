using Sitecore.Data.Fields;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Models;

namespace Sitecore.HabitatHome.Feature.Media.Models
{
    public class VideoViewModel : ItemBase
    {
        public string VideoEmbed { get; set; }

        public string VideoFile { get; set; }

        public string VideoFileSrc
        {
            get
            {
                if (string.IsNullOrEmpty(VideoFile)) return string.Empty;

                FileField fileField = Item.Fields[Templates.Video.Fields.VideoFile];
                if (fileField != null) return fileField.Src;

                return string.Empty;
            }
        }
    }
}