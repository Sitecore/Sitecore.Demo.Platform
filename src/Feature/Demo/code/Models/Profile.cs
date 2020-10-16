using System.Collections.Generic;

namespace Sitecore.Demo.Platform.Feature.Demo.Models
{
    public class Profile
  {
    public string Name { get; set; }
    public IEnumerable<PatternMatch> PatternMatches { get; set; }
  }
}