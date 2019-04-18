using System.Collections.Generic;

namespace Sitecore.HabitatHome.Feature.Components.Models
{
    public class CarouselModel : ItemBase
    {
        public IEnumerable<CarouselSlideModel> Slides { get; set; }

        public string CarouselModelSignature => string.Format("carousel-{0}", Item.Name.ToLower().Replace(" ", "-"));
    }
}