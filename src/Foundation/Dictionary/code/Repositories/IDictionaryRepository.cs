using Sitecore.Sites;

namespace Sitecore.Demo.Platform.Foundation.Dictionary.Repositories
{
  public interface IDictionaryRepository
  {
    Models.Dictionary Get(SiteContext site);
  }
}