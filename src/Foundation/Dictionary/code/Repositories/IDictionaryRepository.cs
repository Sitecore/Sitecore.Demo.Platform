using Sitecore.HabitatHome.Foundation.Dictionary.Models;
using Sitecore.Sites;

namespace Sitecore.HabitatHome.Foundation.Dictionary.Repositories
{
  public interface IDictionaryRepository
  {
    Models.Dictionary Get(SiteContext site);
  }
}