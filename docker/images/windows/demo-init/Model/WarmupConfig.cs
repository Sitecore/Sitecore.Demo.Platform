using System.Collections.Generic;

namespace Sitecore.Demo.Init.Model
{
  public class Sitecore

  {
    public string url { get; set; }
  }


  public class Xp

  {
    public string url { get; set; }
  }


  public class Xc

  {
    public string url { get; set; }
  }


  public class Url

  {
    public IList<Sitecore> sitecore { get; set; }

    public IList<Xp> xp { get; set; }

    public IList<Xc> xc { get; set; }
  }


  public class WarmupConfig
  {

    public IList<Url> urls { get; set; }

  }
}