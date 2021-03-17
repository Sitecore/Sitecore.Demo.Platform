using System.Collections.Generic;

namespace Sitecore.Demo.Init.Model
{
  public class Url
  {
    public string url { get; set; }
  }

  public class WarmupConfig
  {
    public IList<Url> sitecore { get; set; }
    public IList<Url> xp { get; set; }
    public IList<Url> xc { get; set; }
    public IList<Url> coveo { get; set; }
  }
}